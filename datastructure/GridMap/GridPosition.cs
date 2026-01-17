using System;

namespace cfEngine.DataStructure;

/// <summary>
/// Represents a 2D grid position using integer coordinates.
/// Pure C# value type with no external dependencies.
/// </summary>
public readonly struct GridPosition : IEquatable<GridPosition>
{
    public int X { get; }
    public int Y { get; }
    public int Z { get; }

    public GridPosition(int x, int y, int z = 0)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public bool Equals(GridPosition other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override bool Equals(object obj)
    {
        return obj is GridPosition other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public override string ToString()
    {
        return Z == 0 ? $"({X}, {Y})" : $"({X}, {Y}, {Z})";
    }

    public static bool operator ==(GridPosition left, GridPosition right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GridPosition left, GridPosition right)
    {
        return !left.Equals(right);
    }

    public static GridPosition operator +(GridPosition left, GridPosition right)
    {
        return new GridPosition(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    public static GridPosition operator -(GridPosition left, GridPosition right)
    {
        return new GridPosition(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }
}
