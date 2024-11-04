using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using cfEngine.Core;

namespace cfEngine.Meta
{
    public partial class InventoryController : IRuntimeSavable, IDisposable
    {
        private Dictionary<string, (Guid, InventoryItem)> _itemMap = new();

        public void Initialize(IReadOnlyDictionary<string, JsonObject> dataMap)
        {
        }

        public void AddItem(string itemId, int count)
        {
            
        }

        public void Save(Dictionary<string, object> dataMap)
        {
        }

        public void Dispose()
        {
        }
    }
}