using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using cfEngine.Core;
using cfEngine.Logging;
using cfEngine.Util;
using ItemId = System.String;
using StackId = System.Guid;

namespace cfEngine.Meta
{
    public partial class InventoryController : IRuntimeSavable, IDisposable
    {
        private Dictionary<ItemId, HashSet<StackId>> itemStackRegistry = new();
        private Dictionary<StackId, InventoryItem> _itemMap = new();

        public void Initialize(IReadOnlyDictionary<string, JsonObject> dataMap)
        {
        }

        public class UpdateInventoryRequest
        {
            public string itemId;
            public int count;
            public Guid stackId = Guid.Empty;
        }
        
        public Validation<bool> AddItem(UpdateInventoryRequest req)
        {
            var configInfo = Game.Info.Get<InventoryConfigInfoManager>().GetOrDefault(req.itemId);

            var itemId = req.itemId;
            var requestItemAddCount = req.count;

            if (!itemStackRegistry.TryGetValue(itemId, out var stackRegistry))
            {
                stackRegistry = new HashSet<Guid>(1);
                itemStackRegistry.Add(itemId, stackRegistry);
            }
            
            if(requestItemAddCount <=  0) return Validation<bool>.Success(false);

            var remaining = TryPutInExistingStack(requestItemAddCount);
            if (remaining <= 0)
            {
                return Validation<bool>.Success(false);
            }

            while (remaining > 0)
            {
                var newStackItemCount = Math.Min(remaining, configInfo.maxStackSize);
                CreateNewStackItem(newStackItemCount);
                remaining -= newStackItemCount;
            }

            return Validation<bool>.Success(true);  //add inventory by creating a new stack

            void CreateNewStackItem(int itemCount)
            {
                var stackId = Guid.NewGuid();
                stackRegistry.Add(stackId);
                _itemMap.Add(stackId, new InventoryItem(itemId, itemCount));
            }

            int TryPutInExistingStack(int count)
            {
                var remaining = count;
                foreach (var stackId in stackRegistry)
                {
                    if (!_itemMap.TryGetValue(stackId, out var item))
                    {
                        Log.LogException(new KeyNotFoundException($"Unexpected missing stackId {stackId} in itemMap."));
                        continue;
                    }

                    var stackGain = Math.Min(configInfo.maxStackSize - item.ItemCount, count);
                    if(stackGain <= 0) continue;

                    _itemMap[stackId] = item.CloneNewCount(item.ItemCount + stackGain);
                    remaining -= stackGain;
                }

                return remaining;
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