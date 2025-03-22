using System;

namespace cfEngine.Service.Inventory
{
    public partial class InventoryService
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
                    return int.MaxValue;
                }
            
                public StackRecord CloneNewCount(int itemCount)
                {
                    return new StackRecord(StackId, Id, itemCount);
                }
            }
    }
}
