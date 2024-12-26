using System;

namespace cfEngine.Meta.Inventory
{
    public partial class InventoryController
    {
            [Serializable]
            public class StackRecord
            {
                public readonly Guid StackId;
                public readonly string Id;
                public readonly int ItemCount;
                
                public StackRecord(Guid stackId, string id, int itemCount)
                {
                    StackId = stackId;
                    Id = id;
                    ItemCount = itemCount;
                }
            
                public int GetVacancies()
                {
                    return Game.Info.Get<InventoryInfoManager>().GetOrDefault(Id).maxStackSize - ItemCount;
                }
            
                public StackRecord CloneNewCount(int itemCount)
                {
                    return new StackRecord(StackId, Id, itemCount);
                }
            }
    }
}
