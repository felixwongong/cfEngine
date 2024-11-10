using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace cfEngine.Rx
{
    public static class Linq
    {
        public static RtSelectKeyDictionary<TOrigKey, TSelectedKey, TValue> SelectKey<TOrigKey, TSelectedKey, TValue>(
            this RtReadOnlyDictionary<TOrigKey, TValue> source, Func<TOrigKey, TSelectedKey> selectFn)
        {
            return new RtSelectKeyDictionary<TOrigKey, TSelectedKey, TValue>(source, selectFn);
        }
        
        public static RtSelectValueDictionary<TKey, TValue, TSelectValue> SelectValue<TKey, TValue, TSelectValue>(
            this RtReadOnlyDictionary<TKey, TValue> source, Func<TValue, TSelectValue> selectFn)
        {
            return new RtSelectValueDictionary<TKey, TValue, TSelectValue>(source, selectFn);
        }
    }
}