using System;
using System.Collections.Generic;

namespace cfEngine.Rt
{
    // Select suppose not for creating new objects, but for selecting from existing
    public class RtSelectLocalList<T, TSelect>: RtMutatedLocalListBase<T, TSelect>
    {
        private readonly RtReadOnlyList<T> _source;
        private readonly Func<T, TSelect> _selectFn;

        public RtSelectLocalList(RtReadOnlyList<T> source, Func<T, TSelect> selectFn): base(source.Events)
        {
            _source = source;
            _selectFn = selectFn;
        }

        protected override void _OnSourceUpdate((int index, T item) oldItem, (int index, T item) newItem)
        {
            CollectionEvents.OnUpdateRelay.Dispatch((oldItem.index, _selectFn(oldItem.item)), (newItem.index, _selectFn(newItem.item)));
        }

        protected override void _OnSourceRemove((int index, T item) listItem)
        {
            CollectionEvents.OnRemoveRelay.Dispatch((listItem.index, _selectFn(listItem.item)));
        }

        protected override void _OnSourceAdd((int index, T item) listItem)
        {
            CollectionEvents.OnAddRelay.Dispatch((listItem.index, _selectFn(listItem.item)));
        }

        public override IEnumerator<TSelect> GetEnumerator()
        {
            foreach (var item in _source)
            {
                yield return _selectFn(item);
            }
        }

        public override int Count => _source.Count;

        public override TSelect this[int index] => _selectFn(_source[index]);
    }
}