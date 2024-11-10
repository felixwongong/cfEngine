using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using cfEngine.Core;
using cfEngine.Logging;
using cfEngine.Rt;
using cfEngine.Util;
using ItemId = System.String;
using StackId = System.Guid;

namespace cfEngine.Meta
{
    public partial class InventoryController : IRuntimeSavable, IDisposable
    {
        private RtDictionary<StackId, InventoryItem> _itemMap = new();
        private RtGroup<string, InventoryItem> _itemStackGroup;

        public InventoryController()
        {
            _itemStackGroup = _itemMap.RtValues.GroupBy(item => item.Id);
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
        
        public Validation<bool> AddItem(UpdateInventoryRequest req)
        {
            var configInfo = Game.Info.Get<InventoryConfigInfoManager>().GetOrDefault(req.itemId);

            var itemId = req.itemId;
            var requestItemAddCount = req.count;

            if(requestItemAddCount <=  0) return Validation<bool>.Success(false);

            var pendingAddStacks = new List<InventoryItem>();
            var pendingAddCount = 0;
            

            InventoryItem CreateNewStackInMap(string itemId)
            {
                var item = new InventoryItem(itemId, 0);
                _itemMap.Add(Guid.NewGuid(), item);
                return item;
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