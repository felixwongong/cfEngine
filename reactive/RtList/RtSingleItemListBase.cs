using System.Collections.Generic;

namespace cfEngine.Rt
{
    public abstract class RtSingleItemListBase<TOrig, TNew>: RtMutatedLocalListBase<TOrig, TNew>
    {
        public TNew Value { get; protected set; }

        public RtSingleItemListBase(ICollectionEvents<(int index, TOrig item)> sourceEvents) : base(sourceEvents)
        {
        }

        public override IEnumerator<TNew> GetEnumerator()
        {
            yield return Value;
        }

        public override int Count => 1;

        public override TNew this[int index] => Value;
    }
}