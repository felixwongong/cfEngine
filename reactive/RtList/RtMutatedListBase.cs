using System;
using System.Collections.Generic;

namespace cfEngine.Rt
{
    public abstract class RtMutatedLocalListBase<TOrig, TNew> : RtReadOnlyList<TNew>
    {
        private readonly ICollectionEvents<(int, TOrig)> _sourceEvents;

        protected RtMutatedLocalListBase(ICollectionEvents<(int index, TOrig item)> sourceEvents)
        {
            _sourceEvents = sourceEvents;
            _sourceEvents.Subscribe(_OnSourceAdd, _OnSourceRemove, _OnSourceUpdate, Dispose);
        }

        protected abstract void _OnSourceUpdate((int index, TOrig item) oldItem, (int index, TOrig item) newItem);

        protected abstract void _OnSourceRemove((int index, TOrig item) item);

        protected abstract void _OnSourceAdd((int index, TOrig item) item);

        public override void Dispose()
        {
            base.Dispose();
            _sourceEvents.Unsubscribe(_OnSourceAdd, _OnSourceRemove, _OnSourceUpdate, Dispose);
        }
    }

    public abstract class RtMutatedListBase<TOrig, TNew>: RtReadOnlyList<TNew>
    {
        private readonly ICollectionEvents<(int, TOrig)> _sourceEvents;
        private readonly List<TNew> _mutated = new();

        protected RtMutatedListBase(ICollectionEvents<(int index, TOrig item)> sourceEvents, out List<TNew> mutated)
        {
            mutated = _mutated;
            
            _sourceEvents = sourceEvents;
            _sourceEvents.Subscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);
        }

        private void OnSourceUpdate((int index, TOrig item) oldItem, (int index, TOrig item) newItem)
        {
            _OnSourceUpdate(_mutated, oldItem, newItem);
        }
        
        protected abstract void _OnSourceUpdate(List<TNew> mutated, (int index, TOrig item) oldItem, (int index, TOrig item) newItem);

        private void OnSourceRemove((int index, TOrig item) item)
        {
            _OnSourceRemove(_mutated, item);
        }
        
        protected abstract void _OnSourceRemove(List<TNew> mutated, (int index, TOrig item) item);

        private void OnSourceAdd((int index, TOrig item) item)
        {
            _OnSourceAdd(_mutated, item);
        }
        
        protected abstract void _OnSourceAdd(List<TNew> mutated, (int index, TOrig item) item);

        public override IEnumerator<TNew> GetEnumerator()
        {
            return _mutated.GetEnumerator();
        }

        public override int Count => _mutated.Count;

        public override TNew this[int index] => _mutated[index];

        public override void Dispose()
        {
            base.Dispose();
            _sourceEvents.Unsubscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);
            _mutated.Clear();
        }
    }
}