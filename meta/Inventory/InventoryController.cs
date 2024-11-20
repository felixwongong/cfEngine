using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using cfEngine.Core;
using cfEngine.Rt;
using cfEngine.Util;
using ItemId = System.String;
using StackId = System.Guid;

namespace cfEngine.Core
{
    public partial class UserDataKey
    {
        public const string Inventory = "Inventory";
    }
}

namespace cfEngine.Meta
{
    public partial class InventoryController : IRuntimeSavable, IDisposable
    {
        public class UpdateInventoryRequest
        {
            public string ItemId;
            public int Count;
            public Guid StackId = Guid.Empty;
        }

        private RtDictionary<StackId, InventoryItem> _stackMap = new();
        public RtReadOnlyDictionary<StackId, InventoryItem> StackMap => _stackMap;
        public RtGroup<string, InventoryItem> ItemGroup;
        public RtGroup<string, InventoryItem> VacantItemGroup;

        public InventoryController()
        {
            ItemGroup = _stackMap.RtValues.GroupBy(item => item.Id);
            VacantItemGroup = _stackMap
                .Where(kvp => kvp.Value.GetVacancies() > 0).RtValues
                .GroupBy(item => item.Id);
        }

        public void Initialize(IReadOnlyDictionary<string, JsonObject> dataMap)
        {
            if (dataMap.TryGetValue(UserDataKey.Inventory, out var data))
            {
                var saved = data.GetValue<Dictionary<StackId, InventoryItem>>();
                foreach (var kvp in saved)
                {
                    _stackMap.Add(kvp);
                }
            }
        }
        
        public void Save(Dictionary<string, object> dataMap)
        {
            dataMap[UserDataKey.Inventory] = _stackMap;
        }

        public void AddItem(UpdateInventoryRequest request)
        {
            var itemCount = request.Count;
            if (request.StackId != Guid.Empty)
            {
                TryAddToStack(request.StackId, itemCount, out itemCount);
            }
            
            if(itemCount <= 0) return;

            if (VacantItemGroup.TryGetValue(request.ItemId, out var vacantItems))
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
            var maxStackSize = Game.Info.Get<InventoryConfigInfoManager>().GetOrDefault(itemId).maxStackSize;
            while (count > 0)
            {
                var itemCount = Math.Min(count, maxStackSize);
                var stackId = Guid.NewGuid();
                _stackMap.Add(stackId, new InventoryItem(stackId, itemId, itemCount));
                count -= itemCount;
            }
        }

        public Validation<bool> RemoveItem(UpdateInventoryRequest request)
        {
            if (!ItemGroup.TryGetValue(request.ItemId, out var group))
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

        public void Dispose()
        {
            ItemGroup.Dispose();
            VacantItemGroup.Dispose();
            _stackMap.Dispose();
        }
    }
}