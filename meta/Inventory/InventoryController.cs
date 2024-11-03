using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using cfEngine.Core;
using ItemIdStr = System.String;

public class InventoryController: IRuntimeSavable, IDisposable
{
    private Dictionary<ItemIdStr, InventoryItem> _itemMap = new();

    public void Initialize(IReadOnlyDictionary<string, JsonObject> dataMap)
    {
    }

    public void Save(Dictionary<string, object> dataMap)
    {
    }

    public GetInventoryResult GetInventory(GetInventoryOption option)
    {
        var resultItems = new List<InventoryItem>(option.fetchItemIds.Count);
        
        var fetchItemIds = option.fetchItemIds;
        foreach (var itemId in fetchItemIds)
        {
            if (_itemMap.TryGetValue(itemId, out var item))
            {
                resultItems.Add(item);
            }
        }

        return new GetInventoryResult()
        {
            inventoryItems = resultItems
        };
    }

    public void Dispose()
    {
    }
}

public class GetInventoryResult
{
    public IReadOnlyList<InventoryItem> inventoryItems;
}

public class GetInventoryOption
{
    public IReadOnlyList<string> fetchItemIds;
    public int itemsPerFetch;
}