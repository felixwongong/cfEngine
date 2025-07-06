using System.Collections.Generic;

namespace cfEngine.Pooling
{
    public class ListPool<T>: ObjectPool<List<T>>
    {
        private static ListPool<T> _default;
        public static ListPool<T> Default => _default ??= new ListPool<T>();

        public ListPool() : base(CreateList, null, ReleaseList)
        {
        }

        private static List<T> CreateList()
        {
            return new List<T>();
        }

        private static void ReleaseList(List<T> list)
        {
            list?.Clear();
        }
    }
}