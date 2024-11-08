using System;

namespace cfEngine.Rx
{
    public static class RxDictionaryExtension
    {
        public static RxDictionary<TKey, TNewValue> SelectValueRx<TKey, TValue, TNewValue>(this RxReadOnlyDictionary<TKey, TValue> rxDict, Func<TValue, TNewValue> selectFn)
        {
            var selectedDictionary = new RxDictionary<TKey, TNewValue>();
        }
    }
}