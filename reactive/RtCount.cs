namespace cfEngine.Rx
{
    public class RtCount<T>: RtMutatedSingleBase<T, int>
    {
        public RtCount(RtReadOnlyList<T> source) : base(source.Events)
        {
            Value = source.Count;
        }
        
        protected override void _OnSourceUpdate((int index, T item) oldItem, (int index, T item) newItem)
        {
        }

        protected override void _OnSourceRemove((int index, T item) item)
        {
            var oldCount = Value--;
            CollectionEvents.OnUpdateRelay.Dispatch((0, oldCount), (0, Value));
        }

        protected override void _OnSourceAdd((int index, T item) item)
        {
            var oldCount = Value++;
            CollectionEvents.OnUpdateRelay.Dispatch((0, oldCount), (0, Value));
        }
    }
}