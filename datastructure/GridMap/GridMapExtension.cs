using System.Collections.Generic;

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
    
        public static IEnumerable<GridPosition> GetNeighbors<T>(this IReadOnlyGridMap<T> gridMap, GridPosition position)
        {
            foreach (var (dx, dy) in NeighborOffsets)
            {
                var neighbourPosition = new GridPosition(position.X + dx, position.Y + dy, 0);
                if (!gridMap.IsOutOfBounds(neighbourPosition))
                    yield return neighbourPosition;
            }
        }
    }
}