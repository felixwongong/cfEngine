using System;
using System.Collections.Generic;

namespace cfEngine.Rx
{
    public static class Linq
    {
        public static RxSelectKeyDictionary<TOrigKey, TSelectedKey, TValue> selectKey<TOrigKey, TSelectedKey, TValue>(
            this RxReadOnlyDictionary<TOrigKey, TValue> source, Func<TOrigKey, TSelectedKey> selectFn)
        {
            var result = new RxSelectKeyDictionary<TOrigKey, TSelectedKey, TValue>(source, selectFn);
            return result;
        }
        
        public static RxSelectValueDictionary<TKey, TValue, TSelectValue> selectValue<TKey, TValue, TSelectValue>(
            this RxReadOnlyDictionary<TKey, TValue> source, Func<TValue, TSelectValue> selectFn)
        {
            var result = new RxSelectValueDictionary<TKey, TValue, TSelectValue>(source, selectFn);
            return result;
        }
        
        public static RxFilteredDictionary<TKey, TValue> where<TKey, TValue>(
            this RxReadOnlyDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> filterFn
            )
        {
            var result = new RxFilteredDictionary<TKey, TValue>(source, filterFn);
            return result;
        }

        public static RxGroup<TKey, TValue> groupBy<TKey, TValue>(this RxReadOnlyList<TValue> rxList, Func<TValue, TKey> keyFn)
        {
            var result =  new RxGroup<TKey, TValue>(rxList, keyFn);
            return result;
        }
        
        public static RxSelectLocalList<T, TSelect> select<T, TSelect>(this RxReadOnlyList<T> source, Func<T, TSelect> selectFn)
        {
            var result = new RxSelectLocalList<T, TSelect>(source, selectFn);
            return result;
        }
        
        public static RxSelectList<T, TSelect> selectNew<T, TSelect>(this RxReadOnlyList<T> source, Func<T, TSelect> selectFn)
        {
            var result = new RxSelectList<T, TSelect>(source, selectFn);
            return result;
        }
        
        public static RxCount<T> count<T>(this RxReadOnlyList<T> source)
        {
            var result = new RxCount<T>(source);
            return result;
        }
    }
}