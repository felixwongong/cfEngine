using System.Collections.Generic;

namespace cfEngine.Pooling
{
    public class ListPool<T>: ObjectPool<List<T>>
    {
        public ListPool() : base(CreateList, ReleaseList)
        {
        }

        private static List<T> CreateList()
        {
            return new List<T>();
        }

        private static void ReleaseList(List<T> list)
        {
            list.Clear();
        }
    }
}