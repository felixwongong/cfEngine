using System;
using cfEngine.Info;

namespace cfEngine.Service
{
    public class InventoryInfoManager: ConfigInfoManager<string, InventoryInfo>
    {
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

        public InventoryInfoManager(IValueLoader<InventoryInfo> loader) : base(loader) { }
    }

    public class InventoryInfo
    {
        public string itemId { get; set; }
        public int maxStackSize { get; set; }
        public string iconKey { get; set; }
    }
}