using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using cfEngine.Core;

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
        
        public void TryAddItem(UpdateInventoryRequest req)
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
                    targetItem.ItemCount < configInfo.maxStackSize)
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

                    if (targetStack == Guid.Empty || targetItem == null)
                    {
                        targetStack = Guid.NewGuid();
                        targetItem = new InventoryItem(req.itemId, 0);
                    }
                }

                var stackGain = targetItem.ItemCount + count > configInfo.maxStackSize
                    ? configInfo.maxStackSize - targetItem.ItemCount
                    : count;

                stackMap[targetStack] = new InventoryItem(req.itemId, targetItem.ItemCount + stackGain);
                count -= stackGain;
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