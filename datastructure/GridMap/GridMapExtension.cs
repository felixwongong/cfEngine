using System.Collections.Generic;
using System.Numerics;

namespace cfEngine.DataStructure
{
    public static class GridMapExtension
    {
        static readonly (int dx, int dy)[] NeighborOffsets =
        {
            (-1,-1),(0,-1),(1,-1),
            (-1, 0),        (1, 0),
            (-1, 1),(0, 1),(1, 1),
        };
    
        public static IEnumerable<Vector3> GetNeighbors<T>(this IReadOnlyGridMap<T> gridMap, Vector3 position)
        {
            foreach (var (dx, dy) in NeighborOffsets)
            {
                var neighbourPosition = new Vector3(position.X + dx, position.Y + dy, 0);
                if (!gridMap.IsOutOfBounds(neighbourPosition))
                    yield return neighbourPosition;
            }
        }
    }
}