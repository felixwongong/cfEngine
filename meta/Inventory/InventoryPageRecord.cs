using System;
using cfEngine.Rt;
using StackId = System.Guid;

namespace cfEngine.Meta
{
    public class InventoryPageRecord: RtList<StackId>
    {
        public InventoryPageRecord(int pageSize): base()
        {
            for (int i = 0; i < pageSize; i++)
            {
                Add(Guid.Empty);
            }
        }

        public bool TryAddToEmptySlot(StackId stackId)
        {
            var firstEmpty = this.IndexOf(Guid.Empty);
            if (firstEmpty != -1)
            {
                Update(firstEmpty, stackId);
                return true;
            }

            return false;
        }

        public void RemoveItem(int index)
        {
            this.Update(index, Guid.Empty);
        }
    }
}