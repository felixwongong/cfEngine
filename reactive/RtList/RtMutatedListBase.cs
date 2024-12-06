using System;
using System.Collections.Generic;
using cfEngine.Util;

namespace cfEngine.Rt
{
    public abstract class RtMutatedLocalListBase<TOrig, TNew> : RtReadOnlyList<TNew>
    {
        private readonly ICollectionEvents<(int, TOrig)> _sourceEvents;
        
        Subscription _sourceChangeSubscription;

        protected RtMutatedLocalListBase(ICollectionEvents<(int index, TOrig item)> sourceEvents): base()
        {
            _sourceEvents = sourceEvents;
            _sourceChangeSubscription = _sourceEvents.Subscribe(_OnSourceAdd, _OnSourceRemove, _OnSourceUpdate, Dispose);
        }

        public override void Dispose()
        {
            base.Dispose();
            _sourceChangeSubscription.UnsubscribeIfNotNull();
        }

        protected abstract void _OnSourceUpdate((int index, TOrig item) oldItem, (int index, TOrig item) newItem);

        protected abstract void _OnSourceRemove((int index, TOrig item) item);

        protected abstract void _OnSourceAdd((int index, TOrig item) item);
    }

    public abstract class RtMutatedListBase<TOrig, TNew>: RtReadOnlyList<TNew>
    {
        private readonly ICollectionEvents<(int, TOrig)> _sourceEvents;
        private readonly List<TNew> _mutated = new();

        private Subscription _sourceChangeSubscription;
        protected RtMutatedListBase(ICollectionEvents<(int index, TOrig item)> sourceEvents, out List<TNew> mutated): base()
        {
            mutated = _mutated;
            
            _sourceEvents = sourceEvents;
            _sourceChangeSubscription = _sourceEvents.Subscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);
        }
        
        public override void Dispose()
        {
            base.Dispose();
            _sourceChangeSubscription.UnsubscribeIfNotNull();
            foreach (var item in _mutated)
            {
                if (item is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _mutated.Clear();
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
    }
}