using System.Numerics;

namespace cfEngine.DataStructure.Tests
{
    [TestFixture]
    public class GridMapTests
    {
        private sealed class RefItem { public int Value; public RefItem(int v) => Value = v; }

        [Test]
        public void Indexer_GetSet_WithinBounds()
        {
            var grid = new GridMap<int>(new Vector3(3, 2, 4), () => 0);

            var world = new Vector3(1, 1, 2);
            grid[world] = 42;

            Assert.That(grid[world], Is.EqualTo(42));
        }

        [Test]
        public void IsOutOfBounds_TrueForOutside_FalseForInside()
        {
            var grid = new GridMap<int>(new Vector3(3, 2, 4), () => 0);

            Assert.That(grid.IsOutOfBounds(new Vector3(-1, 0, 0)), Is.True);
            Assert.That(grid.IsOutOfBounds(new Vector3(3, 0, 0)), Is.True);
            Assert.That(grid.IsOutOfBounds(new Vector3(2, 1, 3)), Is.False);
        }

        [Test]
        public void GetIndex_Throws_WhenOutOfBounds()
        {
            var grid = new GridMap<int>(new Vector3(3, 2, 4), () => 0);

            Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetIndex(new Vector3(3, 0, 0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = grid[new Vector3(-1, 0, 0)]; });
        }

        [Test]
        public void StartPosition_Offset_Applies_ForIndexer_And_RoundTripIndex()
        {
            var dims = new Vector3(3, 2, 4);
            var start = new Vector3(10, -5, 7);
            var grid = new GridMap<int>(dims, () => 0, start);

            var local = new Vector3(2, 1, 3);
            var world = start + local;

            grid[world] = 99;
            Assert.That(grid[world], Is.EqualTo(99));

            var idx = grid.GetIndex(world);                 // world in
            // Re-derive world via enumeration order (i maps to world)
            var (posWorld, _) = grid.ElementAt(idx);
            Assert.That(posWorld, Is.EqualTo(world));
        }

        [Test]
        public void GetIndexUnsafe_EqualsGetIndex_ForValidWorldPositions_WithStart()
        {
            var dims = new Vector3(4, 3, 2);
            var start = new Vector3(5, 6, 7);
            var grid = new GridMap<int>(dims, () => 0, start);

            var world = start + new Vector3(3, 2, 1);
            Assert.That(grid.GetIndexUnsafe(world), Is.EqualTo(grid.GetIndex(world)));
        }

        [Test]
        public void Enumerator_OrderAndPositions_MatchExpectedXYZScan()
        {
            var dims = new Vector3(3, 2, 2);
            var start = new Vector3(4, 5, 6);
            var grid = new GridMap<int>(dims, () => 0, start);

            // Fill uniquely to check mapping
            int counter = 1;
            for (int z = 0; z < dims.Z; z++)
            for (int y = 0; y < dims.Y; y++)
            for (int x = 0; x < dims.X; x++)
            {
                grid[start + new Vector3(x, y, z)] = counter++;
            }

            // Expected order: x fastest, then y, then z (matches GetIndexUnsafe formula)
            var expected = new List<Vector3>();
            for (int z = 0; z < dims.Z; z++)
            for (int y = 0; y < dims.Y; y++)
            for (int x = 0; x < dims.X; x++)
                expected.Add(start + new Vector3(x, y, z));

            var enumerated = grid.Select(t => t.position).ToList();
            Assert.That(enumerated.Count, Is.EqualTo(expected.Count));
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.That(enumerated[i], Is.EqualTo(expected[i]));
                Assert.That(grid.GetIndex(enumerated[i]), Is.EqualTo(i)); // inverse via index
            }
        }

        [Test]
        public void Remove_Works_ForReferenceType_DefaultIsNull()
        {
            var grid = new GridMap<RefItem>(new Vector3(2, 2, 1), () => null);

            var world = new Vector3(1, 1, 0);
            Assert.That(grid.Remove(world), Is.False);

            grid[world] = new RefItem(7);
            Assert.That(grid.Remove(world), Is.True);
            Assert.That(grid[world], Is.Null);
        }

        [Test]
        public void Remove_Works_ForValueType_DefaultIsZero()
        {
            var grid = new GridMap<int>(new Vector3(2, 2, 1), () => 0);

            var world = new Vector3(0, 1, 0);
            Assert.That(grid.Remove(world), Is.False);

            grid[world] = 123;
            Assert.That(grid.Remove(world), Is.True);
            Assert.That(grid[world], Is.EqualTo(0));
        }

        [Test]
        public void Dimensions_AreClamped_ToAtLeastZero_AndStorageMatchesProduct()
        {
            var requested = new Vector3(0, -5, 2);
            var clamped = new Vector3(0, 0, 2);
            var expectedCount = (int)(clamped.X * clamped.Y * clamped.Z); // may be 0

            var grid = new GridMap<int>(requested, () => 0);

            Assert.That(grid.dimensions, Is.EqualTo(clamped));
            Assert.That(grid.Count(), Is.EqualTo(expectedCount));
        }

        [Test]
        public void ZeroDimensions_ProducesEmptyGrid_AndEnumeratorIsEmpty()
        {
            var dims = new Vector3(0, 0, 0);
            var grid = new GridMap<int>(dims, () => 123);

            Assert.That(grid.dimensions, Is.EqualTo(dims));
            Assert.That(grid.Any(), Is.False);
        }

        [Test]
        public void Misusing_GetIndex_WithLocalCoords_AndNonZeroStart_Throws()
        {
            var grid = new GridMap<int>(new Vector3(1, 1, 1), () => 0, new Vector3(10, 0, 0));

            var local = new Vector3(0, 0, 0); // not a world coord
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetIndex(local));
        }
    }
}
