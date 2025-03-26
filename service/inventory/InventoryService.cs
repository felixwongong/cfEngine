using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using cfEngine.Core;
using cfEngine.Logging;
using cfEngine.Rx;
using cfEngine.Service.Inventory;
using cfEngine.Util;
using ItemId = System.String;
using StackId = System.Guid;

namespace cfEngine.Core
{
    public partial class UserDataKey
    {
        public const string Inventory = "Inventory";
    }
    
    public static partial class ServiceName
    {
        public const string Inventory = "Inventory";
    }
    
    public static partial class GameExtension
    {
        public static Game WithInventory(this Game game, IInventoryService service)
        {
            game.Register(service, ServiceName.Inventory);
            return game;
        }
        
        public static InventoryService GetInventory(this Game game) => game.GetService<InventoryService>(ServiceName.Inventory);
    }
}

namespace cfEngine.Service.Inventory
{
    public interface IInventoryService : IService
    {
        
    } 
    
    public partial class InventoryService : IRuntimeSavable, IInventoryService
    {
        public class UpdateRequest
        {
            public string ItemId;
            public int Count;
            public Guid StackId = Guid.Empty;
        }
        
        private const int PAGE_SIZE = 16;
        
        private RtDictionary<StackId, StackRecord> _stackMap = new();
        public RtReadOnlyDictionary<StackId, StackRecord> StackMap => _stackMap;
        public RtGroup<string, StackRecord> ItemStackGroup;
        public RtGroup<string, StackRecord> VacantItemStackGroup;
        
        private RtList<PageRecord> _pages = new();
        public RtReadOnlyList<PageRecord> Pages => _pages;
        
        Subscription _stackMapChangeSub;

        public InventoryService()
        {
            ItemStackGroup = _stackMap.RtValues.groupBy(item => item.Id);
            VacantItemStackGroup = _stackMap
                .where(kvp => kvp.Value.GetVacancies() > 0).RtValues
                .groupBy(item => item.Id);

            _stackMapChangeSub = _stackMap.Events.Subscribe(OnNewStackAdded, OnStackRemoved, OnStackUpdated);

#if CF_REACTIVE_DEBUG
            _stackMap.__SetDebugName(nameof(_stackMap));
            _pages.__SetDebugName(nameof(_pages));
#endif
        }

        public void Dispose()
        {
            _stackMapChangeSub.UnsubscribeIfNotNull();
            
            _stackMap.Dispose();
            
            foreach (var pageRecord in _pages)
            {
                pageRecord.Dispose();
            }
            _pages.Dispose();
            _pages.Clear();
        }

        public void Initialize(IReadOnlyDictionary<string, JsonObject> dataMap)
        {
            if (dataMap.TryGetValue(UserDataKey.Inventory, out var data))
            {
                var saved = data.GetValue<Dictionary<StackId, StackRecord>>();
                foreach (var kvp in saved)
                {
                    _stackMap.Add(kvp);
                }
            }
            
            if (_pages.Count <= 0)
            {
                _pages.Add(CreatePage());
            }
        }
        
        public void Save(Dictionary<string, object> dataMap)
        {
            dataMap[UserDataKey.Inventory] = _stackMap;
        }

        private PageRecord CreatePage()
        {
            var page = new PageRecord(PAGE_SIZE);
#if CF_REACTIVE_DEBUG
            page.__SetDebugName($"{nameof(_pages)}/{_pages.Count}");
#endif
            return page;
        }

        private void OnNewStackAdded(KeyValuePair<Guid, StackRecord> kvp)
        {
            var stackId = kvp.Key;
            foreach (var page in _pages)
            {
                if (page.TryAddToEmptySlot(stackId))
                {
                    return;
                }
            }

            var newPage = CreatePage();
            newPage.TryAddToEmptySlot(kvp.Key);
            
            _pages.Add(newPage);
        }
        
        private void OnStackRemoved(KeyValuePair<Guid, StackRecord> kvp)
        {
            var stackId = kvp.Key;
            foreach (var page in _pages)
            {
                var index = page.IndexOf(stackId);
                if (index != -1)
                {
                    page.RemoveItem(index);
                }
            }
            
            Log.LogError("StackId not found in any page, something went wrong.");
        }
        
        private void OnStackUpdated(KeyValuePair<Guid, StackRecord> oldItem, KeyValuePair<Guid, StackRecord> newItem)
        {
            if(oldItem.Key != newItem.Key)
            {
                Log.LogError("StackId changed, something went wrong.");
                return;
            }
            
            foreach (var page in _pages)
            {
                var index = page.IndexOf(oldItem.Key);
                if (index != -1)
                {
                    page.Update(index, newItem.Key);
                    return;
                }
            }
        }

        public void AddItem(UpdateRequest request)
        {
            var itemCount = request.Count;
            if (request.StackId != Guid.Empty)
            {
                TryAddToStack(request.StackId, itemCount, out itemCount);
            }
            
            if(itemCount <= 0) return;
        
            if (VacantItemStackGroup.TryGetValue(request.ItemId, out var vacantItems))
            {
                Span<(int, int)> itemAddCounts = stackalloc (int, int)[vacantItems.Count];
        
                for (var i = 0; i < vacantItems.Count && itemCount > 0; i++)
                {
                    var stackAddCount = Math.Min(vacantItems[i].GetVacancies(), itemCount);
                    itemAddCounts[i] = (i, stackAddCount);
                    itemCount -= stackAddCount;
                }
        
                foreach (var (idx, addCount) in itemAddCounts)
                {
                    var item = vacantItems[idx];
                    _stackMap.Upsert(item.StackId, item.CloneNewCount(item.ItemCount + addCount));
                }
        
                if (itemCount <= 0) return;
            }
            
            AddAllToNewStacks(request.ItemId, itemCount);
        }

        public bool TryAddToStack(StackId stackId, int count, out int remain)
        {
            remain = count;
            if (!_stackMap.TryGetValue(stackId, out var stack) || stack.GetVacancies() <= 0)
            {
                return false;
            }
        
            var stackAddCount = Math.Min(stack.GetVacancies(), count);
            _stackMap.Upsert(stackId, stack.CloneNewCount(stack.ItemCount + stackAddCount));
            remain -= stackAddCount;
            
            return true;
        }
        
        public void AddAllToNewStacks(ItemId itemId, int count)
        {
            var maxStackSize = Game.Current.GetInfo().Get<InventoryInfoManager>().GetOrDefault(itemId).maxStackSize;
            while (count > 0)
            {
                var itemCount = Math.Min(count, maxStackSize);
                var stackId = Guid.NewGuid();
                _stackMap.Add(stackId, new StackRecord(stackId, itemId, itemCount));
                count -= itemCount;
            }
        }
        
        public Validation<bool> RemoveItem(UpdateRequest request)
        {
            if (!ItemStackGroup.TryGetValue(request.ItemId, out var group))
            {
                return Validation<bool>.Failure(new InvalidOperationException($"Item {request.ItemId} not found, cannot remove."));
            }
        
            var sum = group.Sum(item => item.ItemCount);
            if (sum < request.Count)
            {
                return Validation<bool>.Failure(new InvalidOperationException($"Item owned ({sum}) less than requested ({request.Count}), cannot remove"));
            }
        
            var remain = request.Count;
            if (request.StackId != Guid.Empty && TryRemoveFromStack(request.StackId, request.Count, out remain))
            {
                return Validation<bool>.Success(true);
            }
            
            for (var i = group.Count - 1; i >= 0 && remain > 0; i--)
            {
                var stack = group[i];
                var stackRemoveCount = Math.Min(remain, stack.ItemCount);
                var stackRemain = stack.ItemCount - stackRemoveCount;
        
                if (stackRemain <= 0)
                {
                    _stackMap.Remove(stack.StackId);
                }
                else
                {
                    _stackMap.Upsert(stack.StackId, stack.CloneNewCount(stackRemain));
                }
        
                remain -= stackRemoveCount;
            }
        
            if (remain > 0)
            {
                return Validation<bool>.Failure(new InvalidOperationException($"removal request remain {remain} at the end, somethings go wrong."));
            }
        
            return Validation<bool>.Success(false);
        }
        
        public bool TryRemoveFromStack(StackId stackId, int count, out int remain)
        {
            remain = count;
        
            if (!_stackMap.TryGetValue(stackId, out var stack))
            {
                return false;
            }
        
            if (stack.ItemCount <= remain)
            {
                _stackMap.Remove(stackId);
                remain -= stack.ItemCount;
                return stack.ItemCount == remain;
            }
            else
            {
                var stackRemoveCount = stack.ItemCount - remain;
                _stackMap.Upsert(stackId, stack.CloneNewCount(stack.ItemCount - stackRemoveCount));
                remain -= stackRemoveCount;
                return true;
            }
        }
        
        public PageRecord GetPage(int index)
        {
            return _pages[index];
        }
    }
}