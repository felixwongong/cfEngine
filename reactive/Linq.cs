using System;
using System.Collections.Generic;

namespace cfEngine.Rx
{
    public static class Linq
    {
        public static RtSelectKeyDictionary<TOrigKey, TSelectedKey, TValue> selectKey<TOrigKey, TSelectedKey, TValue>(
            this RtReadOnlyDictionary<TOrigKey, TValue> source, Func<TOrigKey, TSelectedKey> selectFn)
        {
            var result = new RtSelectKeyDictionary<TOrigKey, TSelectedKey, TValue>(source, selectFn);
#if CF_REACTIVE_DEBUG
            result.__SetDebugName(nameof(selectKey));
#endif
            return result;
        }
        
        public static RtSelectValueDictionary<TKey, TValue, TSelectValue> selectValue<TKey, TValue, TSelectValue>(
            this RtReadOnlyDictionary<TKey, TValue> source, Func<TValue, TSelectValue> selectFn)
        {
            var result = new RtSelectValueDictionary<TKey, TValue, TSelectValue>(source, selectFn);
#if CF_REACTIVE_DEBUG
            result.__SetDebugName(nameof(selectValue));
#endif
            return result;
        }
        
        public static RtFilteredDictionary<TKey, TValue> where<TKey, TValue>(
            this RtReadOnlyDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> filterFn
            )
        {
            var result = new RtFilteredDictionary<TKey, TValue>(source, filterFn);
#if CF_REACTIVE_DEBUG
            result.__SetDebugName(nameof(where));
#endif
            return result;
        }

        public static RtGroup<TKey, TValue> groupBy<TKey, TValue>(this RtReadOnlyList<TValue> rtList, Func<TValue, TKey> keyFn)
        {
            var result =  new RtGroup<TKey, TValue>(rtList, keyFn);
#if CF_REACTIVE_DEBUG
            result.__SetDebugName(nameof(groupBy));
#endif
            return result;
        }
        
        public static RtSelectLocalList<T, TSelect> select<T, TSelect>(this RtReadOnlyList<T> source, Func<T, TSelect> selectFn)
        {
            var result = new RtSelectLocalList<T, TSelect>(source, selectFn);
#if CF_REACTIVE_DEBUG
            result.__SetDebugName(nameof(select));
#endif
            return result;
        }
        
        public static RtSelectList<T, TSelect> selectNew<T, TSelect>(this RtReadOnlyList<T> source, Func<T, TSelect> selectFn)
        {
            var result = new RtSelectList<T, TSelect>(source, selectFn);
#if CF_REACTIVE_DEBUG
            result.__SetDebugName(nameof(selectNew));
#endif
            return result;
        }
        
        public static RtCount<T> count<T>(this RtReadOnlyList<T> source)
        {
            var result = new RtCount<T>(source);
#if CF_REACTIVE_DEBUG
            result.__SetDebugName(nameof(count));
#endif
            return result;
        }
    }
}