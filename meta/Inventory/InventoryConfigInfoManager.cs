using System;
using cfEngine.Info;

namespace cfEngine.Meta
{
    public class InventoryConfigInfoManager: ExcelInfoManager<string, InventoryConfigInfo>
    {
        public override string InfoDirectory => nameof(InventoryConfigInfo);
        protected override Func<InventoryConfigInfo, string> KeyFn => info => info.itemId;

        public InventoryConfigInfo GetOrDefault(string itemId)
        {
            if (_valueMap.TryGetValue(itemId, out var info))
            {
                return info;
            }

            return new InventoryConfigInfo()
            {
                itemId = itemId,
                maxStackSize = -1,
            };
        }
    }

    public class InventoryConfigInfo
    {
        public string itemId;
        public int maxStackSize;
    }
}