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
#if CF_REACTIVE_DEBUG
            result.__SetDebugName(nameof(selectKey));
#endif
            return result;
        }
        
        public static RxSelectValueDictionary<TKey, TValue, TSelectValue> selectValue<TKey, TValue, TSelectValue>(
            this RxReadOnlyDictionary<TKey, TValue> source, Func<TValue, TSelectValue> selectFn)
        {
            var result = new RxSelectValueDictionary<TKey, TValue, TSelectValue>(source, selectFn);
#if CF_REACTIVE_DEBUG
            result.__SetDebugName(nameof(selectValue));
#endif
            return result;
        }
        
        public static RxFilteredDictionary<TKey, TValue> where<TKey, TValue>(
            this RxReadOnlyDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> filterFn
            )
        {
            var result = new RxFilteredDictionary<TKey, TValue>(source, filterFn);
#if CF_REACTIVE_DEBUG
            result.__SetDebugName(nameof(where));
#endif
            return result;
        }

        public static RxGroup<TKey, TValue> groupBy<TKey, TValue>(this RxReadOnlyList<TValue> rxList, Func<TValue, TKey> keyFn)
        {
            var result =  new RxGroup<TKey, TValue>(rxList, keyFn);
#if CF_REACTIVE_DEBUG
            result.__SetDebugName(nameof(groupBy));
#endif
            return result;
        }
        
        public static RxSelectLocalList<T, TSelect> select<T, TSelect>(this RxReadOnlyList<T> source, Func<T, TSelect> selectFn)
        {
            var result = new RxSelectLocalList<T, TSelect>(source, selectFn);
#if CF_REACTIVE_DEBUG
            result.__SetDebugName(nameof(select));
#endif
            return result;
        }
        
        public static RxSelectList<T, TSelect> selectNew<T, TSelect>(this RxReadOnlyList<T> source, Func<T, TSelect> selectFn)
        {
            var result = new RxSelectList<T, TSelect>(source, selectFn);
#if CF_REACTIVE_DEBUG
            result.__SetDebugName(nameof(selectNew));
#endif
            return result;
        }
        
        public static RxCount<T> count<T>(this RxReadOnlyList<T> source)
        {
            var result = new RxCount<T>(source);
#if CF_REACTIVE_DEBUG
            result.__SetDebugName(nameof(count));
#endif
            return result;
        }
    }
}