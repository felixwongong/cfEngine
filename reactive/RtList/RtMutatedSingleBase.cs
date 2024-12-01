using System;
using System.Collections.Generic;

namespace cfEngine.Rt
{
    public abstract class RtMutatedSingleBase<TOrig, TNew>: RtMutatedLocalListBase<TOrig, TNew>
    {
        public TNew Value { get; protected set; }

        public RtMutatedSingleBase(ICollectionEvents<(int index, TOrig item)> sourceEvents) : base(sourceEvents)
        {
        }

        public override void Dispose()
        {
            base.Dispose();

            if (Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        public override IEnumerator<TNew> GetEnumerator()
        {
            yield return Value;
        }

        public override int Count => 1;

        public override TNew this[int index] => Value;
    }
}