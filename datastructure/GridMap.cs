using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace cfEngine.DataStructure;

public class GridMap<T> : IEnumerable<(Vector3 position, T item)>
{
    private readonly Vector3 _dimensions;
    private readonly Vector3 _startPosition;
    private readonly List<T> _list;
    private readonly Func<T> _createFn;

    public Vector3 dimensions => _dimensions;
    public Vector3 startPosition => _startPosition;

    public GridMap(Vector3 dimensions, Func<T> createFn, Vector3 startPosition = default)
    {
        _createFn = createFn;
        _startPosition = startPosition;
        _dimensions = new Vector3(
            Math.Max(0, dimensions.X),
            Math.Max(0, dimensions.Y),
            Math.Max(0, dimensions.Z));
        var dimension = (int)(dimensions.X * dimensions.Y * dimensions.Z);
        _list = new List<T>(dimension);
        for (int i = 0; i < dimension; i++) {
            _list.Add(createFn());
        }
    }

    public T this[Vector3 worldPosition]
    {
        get => _list[GetIndex(worldPosition)];
        set => _list[GetIndex(worldPosition)] = value;
    }

    public bool Remove(Vector3 position)
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

    public bool IsOutOfBounds(Vector3 worldPosition)
    {
        worldPosition = ToLocal(worldPosition);
        return worldPosition.X < 0 || worldPosition.Y < 0 || worldPosition.Z < 0 ||
               worldPosition.X >= _dimensions.X || worldPosition.Y >= _dimensions.Y || worldPosition.Z >= _dimensions.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetIndexUnsafe(Vector3 worldPosition)
    {
        worldPosition = ToLocal(worldPosition);
        return (int)(worldPosition.X + worldPosition.Y * _dimensions.X + worldPosition.Z * _dimensions.X * _dimensions.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetIndex(Vector3 worldPosition)
    {
        if (IsOutOfBounds(worldPosition)) {
            throw new ArgumentOutOfRangeException(nameof(worldPosition),
                $"Position ({worldPosition.ToString()}) is out of bounds of the grid map dimension ({_dimensions.ToString()}).");
        }

        return GetIndexUnsafe(worldPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector3 GetPositionUnsafe(int index)
    {
        int z = (int)(index / (_dimensions.X * _dimensions.Y));
        int y = (int)((index - z * _dimensions.X * _dimensions.Y) / _dimensions.X);
        int x = (int)(index - z * _dimensions.X * _dimensions.Y - y * _dimensions.X);
        return ToWorld(new Vector3(x, y, z));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector3 ToLocal(Vector3 position)
    {
        return position - _startPosition;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector3 ToWorld(Vector3 position)
    {
        return position + _startPosition;
    }

    public IEnumerator<(Vector3 position, T item)> GetEnumerator()
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
}