using System;
using cfEngine.Info;

namespace cfEngine.Service
{
    public class InventoryInfoManager: ConfigInfoManager<string, InventoryInfo>
    {
        public override string infoKey => nameof(InventoryInfoManager);
        public override string infoDirectory => nameof(InventoryInfo);
        protected override Func<InventoryInfo, string> keyFn => info => info.itemId;

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