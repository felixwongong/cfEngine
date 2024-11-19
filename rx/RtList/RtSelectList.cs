using System;
using System.Collections.Generic;

namespace cfEngine.Rt
{
    public class RtSelectList<TOrig, TNew>: RtMutatedListBase<TOrig, TNew>
    {
        private Func<TOrig, TNew> _selectFn;

        public RtSelectList(RtReadOnlyList<TOrig> source, Func<TOrig, TNew> selectFn) : base(source.Events, out var mutated)
        {
            _selectFn = selectFn;
            
            mutated.Capacity = source.Count;
            foreach (var item in source)
            {
                mutated.Add(selectFn(item));
            }
        }

        protected override void _OnSourceUpdate(List<TNew> mutated, (int index, TOrig item) oldItem, (int index, TOrig item) newItem)
        {
            if (oldItem.index != newItem.index)
            {
                throw new InvalidOperationException("RtSelectList._OnSourceUpdate: index mismatch on update items");
            }

            if (oldItem.index < 0 || oldItem.index >= mutated.Count)
            {
                throw new IndexOutOfRangeException("RtSelectList._OnSourceUpdate: index out of range");
            }

            var oldSelected = mutated[oldItem.index];
            var newSelected = _selectFn(newItem.item);
            mutated[oldItem.index] = newSelected;
            CollectionEvents.OnUpdateRelay.Dispatch(
                (oldItem.index, oldSelected),
                (oldItem.index, newSelected)
                );
        }

        protected override void _OnSourceRemove(List<TNew> mutated, (int index, TOrig item) item)
        {
            if(item.index < 0 || item.index >= mutated.Count)
            {
                throw new IndexOutOfRangeException("RtSelectList._OnSourceRemove: index out of range");
            }
            
            mutated.RemoveAt(item.index);
            CollectionEvents.OnRemoveRelay.Dispatch((item.index, _selectFn(item.item)));
        }

        protected override void _OnSourceAdd(List<TNew> mutated, (int index, TOrig item) item)
        {
            if (item.index < 0 || item.index > mutated.Count + 1)
            {
                throw new IndexOutOfRangeException("RtSelectList._OnSourceAdd: index out of range");
            }
            
            mutated.Insert(item.index, _selectFn(item.item));
        }
    }
}