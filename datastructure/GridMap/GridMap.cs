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
        ReadOnlySpan<T> AsSpan();
    }

    public class GridMap<T> : IReadOnlyGridMap<T>, IDisposable
    {
        private readonly GridPosition _dimensions;
        private readonly GridPosition _startPosition;
        private T[] _data;
        private readonly T _defaultValue;
        private readonly Func<T> _createFn;
        private readonly int _dimXY;
        private readonly int _totalSize;
    
        public GridPosition dimensions => _dimensions;
        public int count => _totalSize;
        public GridPosition startPosition => _startPosition;

        private GridMap(GridPosition dimensions, GridPosition startPosition = default)
        {
            _startPosition = startPosition;
            _dimensions = new GridPosition(
                Math.Max(0, dimensions.X),
                Math.Max(0, dimensions.Y),
                Math.Max(0, dimensions.Z));
            _dimXY = _dimensions.X * _dimensions.Y;
            _totalSize = _dimXY * _dimensions.Z;
        }

        public GridMap(GridPosition dimensions, T defaultValue, GridPosition startPosition = default): this(dimensions, startPosition)
        {
            _defaultValue = defaultValue;
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _data = new T[_totalSize];
                Array.Fill(_data, defaultValue);
            }
            else
            {
                _data = GC.AllocateUninitializedArray<T>(_totalSize);
                Array.Fill(_data, defaultValue);
            }
        }

        public GridMap(GridPosition dimensions, Func<T> createFn, GridPosition startPosition = default): this(dimensions, startPosition)
        {
            _createFn = createFn;
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _data = new T[_totalSize];
            }
            else
            {
                _data = GC.AllocateUninitializedArray<T>(_totalSize);
            }
            for (int i = 0; i < _totalSize; i++) {
                _data[i] = createFn();
            }
        }

        public T this[GridPosition worldPosition]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data[GetIndex(worldPosition)];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _data[GetIndex(worldPosition)] = value;
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _data[index] = value;
        }

        /// <summary>
        /// Gets a reference to the element at the specified world position.
        /// This allows zero-copy access and modification of grid cells.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(GridPosition worldPosition)
        {
            return ref _data[GetIndex(worldPosition)];
        }

        /// <summary>
        /// Gets a reference to the element at the specified world position without bounds checking.
        /// This allows zero-copy access and modification of grid cells.
        /// Use only when you are certain the position is within bounds.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRefUnsafe(GridPosition worldPosition)
        {
            return ref _data[GetIndexUnsafe(worldPosition)];
        }

        public bool Remove(GridPosition position)
        {
            var defaultValue = _createFn != null ? _createFn() : _defaultValue;
            var localPosition = ToLocal(position);
            ref var currentValue = ref GetRef(localPosition);
            
            if (currentValue == null || EqualityComparer<T>.Default.Equals(currentValue, defaultValue))
            {
                return false;
            }

            currentValue = defaultValue;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOutOfBounds(GridPosition worldPosition)
        {
            var local = ToLocal(worldPosition);
            return (uint)local.X >= (uint)_dimensions.X ||
                   (uint)local.Y >= (uint)_dimensions.Y ||
                   (uint)local.Z >= (uint)_dimensions.Z;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndexUnsafe(GridPosition worldPosition)
        {
            worldPosition = ToLocal(worldPosition);
            return worldPosition.X + worldPosition.Y * _dimensions.X + worldPosition.Z * _dimXY;
        }

        public int GetIndex(GridPosition worldPosition)
        {
            if (IsOutOfBounds(worldPosition)) {
                throw new ArgumentOutOfRangeException(nameof(worldPosition), $"Position ({worldPosition.ToString()}) is out of bounds of the grid map dimension ({_dimensions.ToString()}).");
            }

            return GetIndexUnsafe(worldPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GridPosition GetPositionUnsafe(int index)
        {
            int z = index / _dimXY;
            int remainder = index - z * _dimXY;
            int y = remainder / _dimensions.X;
            int x = remainder - y * _dimensions.X;
            return ToWorld(new GridPosition(x, y, z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GridPosition ToLocal(GridPosition position) => position - _startPosition;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GridPosition ToWorld(GridPosition position) => position + _startPosition;

        /// <summary>
        /// Struct-based enumerator for zero-allocation iteration.
        /// </summary>
        public struct Enumerator : IEnumerator<(GridPosition position, T item)>
        {
            private readonly GridMap<T> _map;
            private int _index;
            private (GridPosition position, T item) _current;

            internal Enumerator(GridMap<T> map)
            {
                _map = map;
                _index = 0;
                _current = default;
            }

            public bool MoveNext()
            {
                if (_index < _map._totalSize)
                {
                    var position = _map.GetPositionUnsafe(_index);
                    _current = (position, _map._data[_index]);
                    _index++;
                    return true;
                }
                return false;
            }

            public (GridPosition position, T item) Current => _current;
            object IEnumerator.Current => Current;
            
            public void Reset()
            {
                _index = 0;
                _current = default;
            }
            
            public void Dispose() { }
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<(GridPosition position, T item)> IEnumerable<(GridPosition position, T item)>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            if (_createFn != null)
            {
                for (int i = 0; i < _totalSize; i++)
                {
                    _data[i] = _createFn();
                }
            }
            else
            {
                Array.Fill(_data, _defaultValue);
            }
        }

        public void CopyTo(GridMap<T> target)
        {
            if (target.dimensions != dimensions || target.startPosition != startPosition)
            {
                throw new ArgumentException("Target grid map must have the same dimensions and start position.");
            }
            
            Array.Copy(_data, target._data, _totalSize);
        }
        
        public GridPosition GetRandomPosition()
        {
            var rand = new Random();
            int index = rand.Next(0, _totalSize);
            return GetPositionUnsafe(index);
        }
    
        public GridMap<T> Clone()
        {
            GridMap<T> map;
            if (_createFn != null)
            {
                map = new GridMap<T>(dimensions, _createFn, startPosition);
            }
            else
            {
                map = new GridMap<T>(dimensions, _defaultValue, startPosition);
            }
            CopyTo(map);
            return map;
        }

        /// <summary>
        /// Returns a read-only span view of the underlying grid data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsSpan() => _data.AsSpan();

        /// <summary>
        /// Returns a writable span view of the underlying grid data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsWritableSpan() => _data.AsSpan();

        /// <summary>
        /// Disposes of resources. Currently a no-op, but enables future ArrayPool support.
        /// </summary>
        public void Dispose()
        {
            // Future: Return _data to ArrayPool if we switch to pooled arrays
        }
    }
}