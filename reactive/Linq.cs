using System;
using System.Collections.Generic;

namespace cfEngine.Rt
{
    public static class Linq
    {
        public static RtSelectKeyDictionary<TOrigKey, TSelectedKey, TValue> selectKey<TOrigKey, TSelectedKey, TValue>(
            this RtReadOnlyDictionary<TOrigKey, TValue> source, Func<TOrigKey, TSelectedKey> selectFn)
        {
            return new RtSelectKeyDictionary<TOrigKey, TSelectedKey, TValue>(source, selectFn);
        }
        
        public static RtSelectValueDictionary<TKey, TValue, TSelectValue> selectValue<TKey, TValue, TSelectValue>(
            this RtReadOnlyDictionary<TKey, TValue> source, Func<TValue, TSelectValue> selectFn)
        {
            return new RtSelectValueDictionary<TKey, TValue, TSelectValue>(source, selectFn);
        }
        
        public static RtFilteredDictionary<TKey, TValue> where<TKey, TValue>(
            this RtReadOnlyDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> filterFn
            )
        {
            return new RtFilteredDictionary<TKey, TValue>(source, filterFn);
        }

        public static RtGroup<TKey, TValue> groupBy<TKey, TValue>(this RtReadOnlyList<TValue> rtList, Func<TValue, TKey> keyFn)
        {
            return new RtGroup<TKey, TValue>(rtList, keyFn);
        }
        
        public static RtSelectLocalList<T, TSelect> select<T, TSelect>(this RtReadOnlyList<T> source, Func<T, TSelect> selectFn)
        {
            return new RtSelectLocalList<T, TSelect>(source, selectFn);
        }
        
        public static RtSelectList<T, TSelect> selectNew<T, TSelect>(this RtReadOnlyList<T> source, Func<T, TSelect> selectFn)
        {
            return new RtSelectList<T, TSelect>(source, selectFn);
        }
        
        public static RtCount<T> count<T>(this RtReadOnlyList<T> source)
        {
            return new RtCount<T>(source);
        }
    }
}