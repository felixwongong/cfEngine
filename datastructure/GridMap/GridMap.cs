using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace cfEngine.DataStructure
{
    public interface IReadOnlyGridMap<T>: IEnumerable<(GridPosition position, T item)>
    {
        GridPosition dimensions { get; }
        int count { get; }
        GridPosition startPosition { get; }
        T this[GridPosition worldPosition] { get; }
        T this[int index] { get; }
        bool IsOutOfBounds(GridPosition worldPosition);
        int GetIndexUnsafe(GridPosition worldPosition);
        int GetIndex(GridPosition worldPosition);
    }

    public class GridMap<T> : IReadOnlyGridMap<T>
    {
        private readonly GridPosition _dimensions;
        private readonly GridPosition _startPosition;
        private readonly List<T> _list;
        private readonly T _defaultValue;
        private readonly Func<T> _createFn;
    
        public GridPosition dimensions => _dimensions;
        public int count => _list.Count;
        public GridPosition startPosition => _startPosition;

        private GridMap(GridPosition dimensions, GridPosition startPosition = default)
        {
            _startPosition = startPosition;
            _dimensions = new GridPosition(
                Math.Max(0, dimensions.X),
                Math.Max(0, dimensions.Y),
                Math.Max(0, dimensions.Z));
        }

        public GridMap(GridPosition dimensions, T defaultValue, GridPosition startPosition = default): this(dimensions, startPosition)
        {
            _defaultValue = defaultValue;
            var dimension = dimensions.X * dimensions.Y * dimensions.Z;
            _list = new List<T>(dimension);
            for (int i = 0; i < dimension; i++) {
                _list.Add(defaultValue);
            }
        }

        public GridMap(GridPosition dimensions, Func<T> createFn, GridPosition startPosition = default): this(dimensions, startPosition)
        {
            var dimension = dimensions.X * dimensions.Y * dimensions.Z;
            _list = new List<T>(dimension);
            _createFn = createFn;
            for (int i = 0; i < dimension; i++) {
                _list.Add(createFn());
            }
        }

        public T this[GridPosition worldPosition]
        {
            get => _list[GetIndex(worldPosition)];
            set => _list[GetIndex(worldPosition)] = value;
        }

        public T this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }

        public bool Remove(GridPosition position)
        {
            var defaultValue = _createFn();
            var localPosition = ToLocal(position);
            var currentValue = this[localPosition];
            if (currentValue == null || EqualityComparer<T>.Default.Equals(currentValue, defaultValue)) {
                return false;
            }

            this[localPosition] = defaultValue;
            return true;
        }

        public bool IsOutOfBounds(GridPosition worldPosition)
        {
            worldPosition = ToLocal(worldPosition);
            return worldPosition.X < 0 || worldPosition.Y < 0 || worldPosition.Z < 0 ||
                   worldPosition.X >= _dimensions.X || worldPosition.Y >= _dimensions.Y || worldPosition.Z >= _dimensions.Z;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndexUnsafe(GridPosition worldPosition)
        {
            worldPosition = ToLocal(worldPosition);
            return worldPosition.X + worldPosition.Y * _dimensions.X + worldPosition.Z * _dimensions.X * _dimensions.Y;
        }

        public int GetIndex(GridPosition worldPosition)
        {
            if (IsOutOfBounds(worldPosition)) {
                throw new ArgumentOutOfRangeException(nameof(worldPosition), $"Position ({worldPosition.ToString()}) is out of bounds of the grid map dimension ({_dimensions.ToString()}).");
            }

            return GetIndexUnsafe(worldPosition);
        }

        private GridPosition GetPositionUnsafe(int index)
        {
            int z = index / (_dimensions.X * _dimensions.Y);
            int y = (index - z * _dimensions.X * _dimensions.Y) / _dimensions.X;
            int x = index - z * _dimensions.X * _dimensions.Y - y * _dimensions.X;
            return ToWorld(new GridPosition(x, y, z));
        }

        private GridPosition ToLocal(GridPosition position) => position - _startPosition;
        private GridPosition ToWorld(GridPosition position) => position + _startPosition;

        public IEnumerator<(GridPosition position, T item)> GetEnumerator()
        {
            for (var i = 0; i < _list.Count; i++) {
                var position = GetPositionUnsafe(i);
                yield return (position, _list[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            for (int i = 0; i < _list.Count; i++) {
                if(_createFn != null)
                    _list.Add(_createFn());
                else
                    _list.Add(_defaultValue);
            }
        }

        public void CopyTo(GridMap<T> target)
        {
            if(target.dimensions != dimensions || target.startPosition != startPosition)
                throw new ArgumentException("Target grid map must have the same dimensions and start position.");

            for (int i = 0; i < _list.Count; i++)
            {
                target._list[i] = _list[i];
            }
        }
        
        public GridPosition GetRandomPosition()
        {
            var rand = new Random();
            int index = rand.Next(0, _list.Count);
            return GetPositionUnsafe(index);
        }
    
        public GridMap<T> Clone()
        {
            var map = new GridMap<T>(dimensions, _createFn, startPosition);
            CopyTo(map);
            return map;
        }
    }
}