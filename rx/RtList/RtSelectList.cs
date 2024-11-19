using System;
using System.Collections.Generic;

namespace cfEngine.Rt
{
    /// <summary>
    /// Represents a list that projects each element of a source list into a new form.
    /// </summary>
    /// <typeparam name="TOrig">The type of elements in the source list.</typeparam>
    /// <typeparam name="TNew">The type of elements in the projected list.</typeparam>
    public class RtSelectList<TOrig, TNew> : RtMutatedListBase<TOrig, TNew>
    {
        private readonly Func<TOrig, TNew> _selectFn;

        /// <summary>
        /// Initializes a new instance of the <see cref="RtSelectList{TOrig, TNew}"/> class.
        /// </summary>
        /// <param name="source">The source list.</param>
        /// <param name="selectFn">The function to project each element of the source list.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="selectFn"/> is null.</exception>
        public RtSelectList(RtReadOnlyList<TOrig> source, Func<TOrig, TNew> selectFn) : base(source.Events, out var mutated)
        {
            _selectFn = selectFn ?? throw new ArgumentNullException(nameof(selectFn));

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
