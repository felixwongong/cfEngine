using System;
using cfEngine.Meta;

[Serializable]
public class InventoryItem
{
    public readonly Guid StackId;
    public readonly string Id;
    public readonly int ItemCount;
    
    public InventoryItem(Guid stackId, string id, int itemCount)
    {
        StackId = stackId;
        Id = id;
        ItemCount = itemCount;
    }

    public int GetVacancies()
    {
        return Game.Info.Get<InventoryConfigInfoManager>().GetOrDefault(Id).maxStackSize - ItemCount;
    }

    public InventoryItem CloneNewCount(int itemCount)
    {
        return new InventoryItem(StackId, Id, itemCount);
    }
}
