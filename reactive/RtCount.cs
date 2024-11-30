using System;
using System.Collections.Generic;

namespace cfEngine.Rt
{
    public class RtCount<T>: RtMutatedLocalListBase<T, int>
    {
        public int Value { get; private set; }

        private CollectionEvents<int> CollectionEvents;
        public ICollectionEvents<int> Events => CollectionEvents ??= new();
        public override IEnumerator<int> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override int Count { get; }

        public override int this[int index] => throw new NotImplementedException();

        private SubscriptionHandle _sourceSub;

        public RtCount(ICollectionEvents<(int index, T item)> sourceEvents) : base(sourceEvents)
        {
        }

        protected override void _OnSourceUpdate((int index, T item) oldItem, (int index, T item) newItem)
        {
            throw new NotImplementedException();
        }

        protected override void _OnSourceRemove((int index, T item) item)
        {
            throw new NotImplementedException();
        }

        protected override void _OnSourceAdd((int index, T item) item)
        {
            throw new NotImplementedException();
        }
    }
}