using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using cfEngine.Core;
using cfEngine.Util;

namespace cfEngine.Meta
{
    public partial class InventoryController : IRuntimeSavable, IDisposable
    {
        private Dictionary<string, Dictionary<Guid, InventoryItem>> _itemMap = new();

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

            if (!_itemMap.TryGetValue(req.itemId, out var stackMap))
            {
                stackMap = new Dictionary<Guid, InventoryItem>();
                _itemMap.Add(req.itemId, stackMap);
            }

            var count = req.count;

            while (count > 0)
            {
                var targetStack = Guid.Empty;
                InventoryItem targetItem = null;
                if (!req.stackId.Equals(Guid.Empty) && stackMap.TryGetValue(req.stackId, out targetItem) &&
                    (configInfo.maxStackSize == -1 || targetItem.ItemCount < configInfo.maxStackSize))
                {
                    targetStack = req.stackId;
                }
                else
                {
                    foreach (var (stackId, item) in stackMap)
                    {
                        if (item.ItemCount < configInfo.maxStackSize)
                        {
                            targetStack = stackId;
                            targetItem = item;
                            break;
                        }
                    }

                    if (targetStack.Equals(Guid.Empty) || targetItem == null)
                    {
                        if (stackMap.Count >= configInfo.maxStackCount)
                        {
                            return Validation<bool>.Failure(new InvalidOperationException($"{req.itemId} stack count has reached config maximum ({configInfo.maxStackCount})"));
                        }
                        
                        targetStack = Guid.NewGuid();
                        targetItem = new InventoryItem(req.itemId, 0);
                    }
                }

                if (targetItem.ItemCount + count <= configInfo.maxStackSize)
                {
                    stackMap[targetStack] = new InventoryItem(req.itemId, targetItem.ItemCount + count);
                    return Validation<bool>.Success(false);  //add inventory without creating a new stack
                }

                count -= configInfo.maxStackSize - targetItem.ItemCount;
            }
            
            return Validation<bool>.Success(true);  //add inventory by creating a new stack
        }

        public void Save(Dictionary<string, object> dataMap)
        {
            
        }

        public void Dispose()
        {
        }
    }
}