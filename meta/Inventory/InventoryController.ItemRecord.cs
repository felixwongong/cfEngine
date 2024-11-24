using System;

namespace cfEngine.Meta.Inventory
{
    public partial class InventoryController
    {
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
                    return Game.Info.Get<InventoryInfoManager>().GetOrDefault(Id).maxStackSize - ItemCount;
                }
            
                public InventoryItemRecord CloneNewCount(int itemCount)
                {
                    return new InventoryItemRecord(StackId, Id, itemCount);
                }
            }
    }
}
