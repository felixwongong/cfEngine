using System.Runtime.CompilerServices;
using cfEngine.Logging;

namespace cfEngine.Util
{
    public static class SanityCheck
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WhenNull<T>(T target, string message = "") where T: class
        {
            if (target == null)
            {
                Log.LogException(string.IsNullOrEmpty(message)
                    ? new SanityCheckException()
                    : new SanityCheckException(message));
                return true;
            }

            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WhenTrue(bool condition, string message = "")
        {
            if (condition)
            {
                Log.LogException(string.IsNullOrEmpty(message)
                    ? new SanityCheckException()
                    : new SanityCheckException(message));
                return true;
            }

            return false;
        }
    }
}