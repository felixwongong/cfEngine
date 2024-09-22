using System;

namespace cfEngine.Extension
{
    public static class EnumExtension
    {
        public static bool hasFlag(this Enum target, Enum flag)
        {
            return (Convert.ToInt32(target) & Convert.ToInt32(flag)) != 0;
        }
    }
}