using System;
using cfEngine.Info;

namespace cfEngine.Meta
{
    public class InventoryInfoManager: ExcelInfoManager<string, InventoryInfo>
    {
        public override string InfoDirectory => nameof(InventoryInfo);
        protected override Func<InventoryInfo, string> KeyFn => info => info.itemId;

        public InventoryInfo GetOrDefault(string itemId)
        {
            if (_valueMap.TryGetValue(itemId, out var info))
            {
                return info;
            }

            return new InventoryInfo()
            {
                itemId = itemId,
                maxStackSize = int.MaxValue,
                iconKey = string.Empty
            };
        }
    }

    public class InventoryInfo
    {
        public string itemId;
        public int maxStackSize;
        public string iconKey;
    }
}