using System;
using System.Collections.Generic;

namespace cfEngine.Rx
{
    /// <summary>
    /// Represents a list that projects each element of a source list into a new form.
    /// </summary>
    /// <typeparam name="TOrig">The type of elements in the source list.</typeparam>
    /// <typeparam name="TNew">The type of elements in the projected list.</typeparam>
    public class RxSelectList<TOrig, TNew> : RxMutatedListBase<TOrig, TNew>
    {
        private readonly Func<TOrig, TNew> _selectFn;

        public RxSelectList(RxReadOnlyList<TOrig> source, Func<TOrig, TNew> selectFn) : base(source.Events)
        {
            _selectFn = selectFn ?? throw new ArgumentNullException(nameof(selectFn));

            _mutated.Capacity = source.Count;
            foreach (var item in source)
            {
                _mutated.Add(selectFn(item));
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
            if (item.index < 0 || item.index >= mutated.Count)
            {
                throw new IndexOutOfRangeException("RtSelectList._OnSourceRemove: index out of range");
            }

            var selected = mutated[item.index];
            mutated.RemoveAt(item.index);
            CollectionEvents.OnRemoveRelay.Dispatch((item.index, selected));
        }

        protected override void _OnSourceAdd(List<TNew> mutated, (int index, TOrig item) item)
        {
            if (item.index < 0 || item.index > mutated.Count)
            {
                throw new IndexOutOfRangeException("RtSelectList._OnSourceAdd: index out of range");
            }

            var selected = _selectFn(item.item);
            mutated.Insert(item.index, selected);
            CollectionEvents.OnAddRelay.Dispatch((item.index, selected));
        }
    }
}
