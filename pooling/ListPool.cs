using System.Collections.Generic;

namespace cfEngine.Pooling
{
    public class ListPool<T>: ObjectPool<List<T>>
    {
        private static ListPool<T> _default;
        public static ListPool<T> Default => _default ??= new ListPool<T>();

        public ListPool() : base(CreateList, static _ => {}, ReleaseList, static _ => {})
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
    
    public class DictionaryPool<TKey, TValue>: ObjectPool<Dictionary<TKey, TValue>> where TKey : notnull
    {
        private static DictionaryPool<TKey, TValue> _default;
        public static DictionaryPool<TKey, TValue> Default => _default ??= new DictionaryPool<TKey, TValue>();

        public DictionaryPool() : base(CreateDictionary, static _ => {}, ReleaseDictionary, static _ => {})
        {
        }

        private static Dictionary<TKey, TValue> CreateDictionary()
        {
            return new Dictionary<TKey, TValue>();
        }

        private static void ReleaseDictionary(Dictionary<TKey, TValue> dictionary)
        {
            dictionary?.Clear();
        }
    }
}