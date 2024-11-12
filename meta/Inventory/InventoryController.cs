using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using cfEngine.Core;
using cfEngine.Rt;
using ItemId = System.String;
using StackId = System.Guid;

namespace cfEngine.Meta
{
    public partial class InventoryController : IRuntimeSavable, IDisposable
    {
        private RtDictionary<StackId, InventoryItem> _stackMap = new();
        public RtGroup<string, InventoryItem> _itemGroup;
        public RtGroup<string, InventoryItem> _vacantItemGroup;

        public InventoryController()
        {
            _itemGroup = _stackMap.RtValues.GroupBy(item => item.Id);
            _vacantItemGroup = _stackMap
                .Where(kvp => kvp.Value.GetVacancies() > 0).RtValues
                .GroupBy(item => item.Id);

        }

        public void Initialize(IReadOnlyDictionary<string, JsonObject> dataMap)
        {
        }

        public class UpdateInventoryRequest
        {
            public string itemId;
            public int count;
            public Guid stackId = Guid.Empty;
        }

        public void AddItem(UpdateInventoryRequest request)
        {
            var itemCount = request.count;
            if (request.stackId != Guid.Empty)
            {
                TryAddToStack(request.stackId, itemCount, out itemCount);
            }
            
            if(itemCount <= 0) return;

            var vacantItems = _vacantItemGroup[request.itemId];
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
            
            if(itemCount <= 0) return;
            
            AddAllToNewStacks(request.itemId, itemCount);
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
        
        public void Save(Dictionary<string, object> dataMap)
        {
            
        }

        public void Dispose()
        {
        }
    }
}