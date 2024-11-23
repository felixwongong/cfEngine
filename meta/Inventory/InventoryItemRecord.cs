using System;
using cfEngine.Meta;

[Serializable]
public class InventoryItemRecord
{
    public readonly Guid StackId;
    public readonly string Id;
    public readonly int ItemCount;
    
    public InventoryItemRecord(Guid stackId, string id, int itemCount)
    {
        StackId = stackId;
        Id = id;
        ItemCount = itemCount;
    }

    public int GetVacancies()
    {
        return Game.Info.Get<InventoryConfigInfoManager>().GetOrDefault(Id).maxStackSize - ItemCount;
    }

    public InventoryItemRecord CloneNewCount(int itemCount)
    {
        return new InventoryItemRecord(StackId, Id, itemCount);
    }
}
