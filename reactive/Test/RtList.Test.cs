using System.Collections.Generic;
using System.Linq; // Added using System.Linq
using NUnit.Framework;

namespace cfEngine.Rx.Test
{
    [TestFixture]
    public class RtList_Test
    {
        [Test]
        public void RtList_Dispose()
        {
            var disposed = false;
            var rtList = new RtList<int>();
            for (int i = 0; i < 10; i++)
            {
                rtList.Add(i);
            }

            var disposeSub = rtList.Events.SubscribeOnDispose(() => disposed = true); // Cache subscription
            rtList.Dispose();
            
            Assert.IsTrue(disposed, "Dispose event did not fire.");
            Assert.IsTrue(rtList.Count == 0, "List count should be 0 after dispose.");
            disposeSub.Dispose(); // Dispose subscription
        }

        [Test]
        public void RtList_Add()
        {
            var added = new List<int>(5);
            var rtList = new RtList<int>();
            System.IDisposable sub = null; // Declare sub here to access it later

            for (int i = 0; i < 10; i++)
            {
                rtList.Add(i);
                if (i == 4) // Subscribe after adding some items
                {
                    // Capture items added *after* subscription
                    sub = rtList.Events.SubscribeOnAdd(x =>
                    {
                        added.Add(x.item);
                    });
                }
            }

            // Check that items added after subscription were captured
            // The original test was checking items 5 through 9
            for (int i = 5; i < 10; i++)
            {
                Assert.IsTrue(added.Contains(i), $"Item {i} was not captured by OnAdd event.");
            }
            Assert.AreEqual(5, added.Count, "Incorrect number of items captured by OnAdd event.");

            sub?.Dispose(); // Dispose subscription
        }

        [Test]
        public void Test_Insert_And_OnInsertEvent()
        {
            var rtList = new RtList<string> { "a", "c" };
            var capturedEvents = new List<(string item, int index)>();
            var onInsertSub = rtList.Events.SubscribeOnInsert(args => capturedEvents.Add((args.item, args.index)));

            rtList.Insert(1, "b");

            Assert.AreEqual(3, rtList.Count, "List count after insert is incorrect.");
            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, rtList.ToList(), "List contents after insert are incorrect.");
            Assert.AreEqual(1, capturedEvents.Count, "Incorrect number of OnInsert events captured.");
            Assert.AreEqual("b", capturedEvents[0].item, "Inserted item in event is incorrect.");
            Assert.AreEqual(1, capturedEvents[0].index, "Inserted index in event is incorrect.");

            onInsertSub.Dispose();
        }

        [Test]
        public void Test_RemoveAt_And_OnRemoveAtEvent()
        {
            var rtList = new RtList<string> { "a", "b", "c" };
            (string item, int index) capturedEvent = default;
            bool eventFired = false;
            var onRemoveAtSub = rtList.Events.SubscribeOnRemoveAt(args => 
            {
                capturedEvent = (args.item, args.index);
                eventFired = true;
            });

            rtList.RemoveAt(1); // removes "b"

            Assert.AreEqual(2, rtList.Count, "List count after RemoveAt is incorrect.");
            CollectionAssert.AreEqual(new[] { "a", "c" }, rtList.ToList(), "List contents after RemoveAt are incorrect.");
            Assert.IsTrue(eventFired, "OnRemoveAt event did not fire.");
            Assert.AreEqual("b", capturedEvent.item, "Removed item in event is incorrect.");
            Assert.AreEqual(1, capturedEvent.index, "Removed index in event is incorrect.");

            onRemoveAtSub.Dispose();
        }

        [Test]
        public void Test_Remove_Item_And_OnRemoveEvent()
        {
            var rtList = new RtList<string> { "x", "y", "z" };
            (string item, int index) capturedEvent = default;
            bool eventFired = false;
            var onRemoveSub = rtList.Events.SubscribeOnRemove(args => 
            {
                capturedEvent = (args.item, args.index);
                eventFired = true;
            });

            bool result = rtList.Remove("y");

            Assert.IsTrue(result, "Remove(\"y\") should return true.");
            Assert.AreEqual(2, rtList.Count, "List count after Remove is incorrect.");
            CollectionAssert.AreEqual(new[] { "x", "z" }, rtList.ToList(), "List contents after Remove are incorrect.");
            Assert.IsTrue(eventFired, "OnRemove event did not fire for existing item.");
            Assert.AreEqual("y", capturedEvent.item, "Removed item in event is incorrect.");
            Assert.AreEqual(1, capturedEvent.index, "Removed index in event is incorrect.");

            eventFired = false; // Reset for next check
            bool resultNonExistent = rtList.Remove("nonexistent");
            Assert.IsFalse(resultNonExistent, "Remove(\"nonexistent\") should return false.");
            Assert.IsFalse(eventFired, "OnRemove event should not fire for non-existent item.");

            onRemoveSub.Dispose();
        }

        [Test]
        public void Test_Clear_And_OnClearEvent()
        {
            var rtList = new RtList<int> { 1, 2, 3 };
            bool clearEventFired = false;
            var onClearSub = rtList.Events.SubscribeOnClear(() => clearEventFired = true);

            rtList.Clear();

            Assert.AreEqual(0, rtList.Count, "List count after Clear is incorrect.");
            Assert.IsTrue(clearEventFired, "OnClear event did not fire.");

            onClearSub.Dispose();
        }

        [Test]
        public void Test_Contains()
        {
            var rtList = new RtList<string> { "apple", "banana", "cherry" };
            Assert.IsTrue(rtList.Contains("banana"), "Contains should find 'banana'.");
            Assert.IsFalse(rtList.Contains("grape"), "Contains should not find 'grape'.");

            // Test with null
            var rtListWithNull = new RtList<string> { "item1", null, "item2" };
            Assert.IsTrue(rtListWithNull.Contains(null), "Contains should find null.");
            Assert.IsFalse(rtListWithNull.Contains("nonexistent_null_test"), "Contains should not find non-existent item in list with null.");
        }

        [Test]
        public void Test_IndexOf()
        {
            var rtList = new RtList<string> { "apple", "banana", "apple", "cherry" };
            Assert.AreEqual(0, rtList.IndexOf("apple"), "IndexOf 'apple' should be 0.");
            Assert.AreEqual(1, rtList.IndexOf("banana"), "IndexOf 'banana' should be 1.");
            Assert.AreEqual(3, rtList.IndexOf("cherry"), "IndexOf 'cherry' should be 3.");
            Assert.AreEqual(-1, rtList.IndexOf("grape"), "IndexOf 'grape' should be -1.");

            // Test with null
            var rtListWithNull = new RtList<string> { "first", null, "third", null };
            Assert.AreEqual(1, rtListWithNull.IndexOf(null), "IndexOf null should be 1.");
            Assert.AreEqual(-1, rtListWithNull.IndexOf("nonexistent_null_test"), "IndexOf non-existent item in list with null should be -1.");
        }

        [Test]
        public void Test_CopyTo()
        {
            var rtList = new RtList<int> { 10, 20, 30 };
            var array = new int[5];
            rtList.CopyTo(array, 1);
            CollectionAssert.AreEqual(new[] { 0, 10, 20, 30, 0 }, array, "CopyTo basic case failed.");

            // Test with array exactly fitting
            var fittingArray = new int[3];
            rtList.CopyTo(fittingArray, 0);
            CollectionAssert.AreEqual(new[] { 10, 20, 30 }, fittingArray, "CopyTo fitting array failed.");

            // Test with empty list
            var emptyList = new RtList<int>();
            var emptyTargetArray = new int[3];
            emptyList.CopyTo(emptyTargetArray, 0);
            CollectionAssert.AreEqual(new[] { 0, 0, 0 }, emptyTargetArray, "CopyTo empty list failed.");

            // Test exceptions
            Assert.Throws<System.ArgumentNullException>(() => rtList.CopyTo(null, 0), "CopyTo null array should throw ArgumentNullException.");
            Assert.Throws<System.ArgumentOutOfRangeException>(() => rtList.CopyTo(array, -1), "CopyTo negative index should throw ArgumentOutOfRangeException.");
            Assert.Throws<System.ArgumentOutOfRangeException>(() => rtList.CopyTo(array, array.Length), "CopyTo index equal to array length should throw ArgumentOutOfRangeException."); // index is out of bounds for destination
            Assert.Throws<System.ArgumentException>(() => rtList.CopyTo(new int[2], 0), "CopyTo array too small should throw ArgumentException.");
            Assert.Throws<System.ArgumentException>(() => rtList.CopyTo(array, 3), "CopyTo with insufficient space in array should throw ArgumentException."); // 3 items won't fit starting at index 3 in an array of 5
        }

        [Test]
        public void Test_Indexer_Get_And_Set_And_OnUpdateEvent()
        {
            // Test Get
            var rtListGet = new RtList<string> { "first", "second", "third" };
            Assert.AreEqual("first", rtListGet[0], "Get index [0] failed.");
            Assert.AreEqual("second", rtListGet[1], "Get index [1] failed.");
            Assert.AreEqual("third", rtListGet[2], "Get index [2] failed.");
            Assert.Throws<System.ArgumentOutOfRangeException>(() => { var _ = rtListGet[3]; }, "Get index [3] (out of bounds) should throw.");
            Assert.Throws<System.ArgumentOutOfRangeException>(() => { var _ = rtListGet[-1]; }, "Get index [-1] (out of bounds) should throw.");

            // Test Set
            var rtListSet = new RtList<string> { "oldA", "oldB", "oldC" };
            (string oldItem, string newItem, int index) capturedEvent = default;
            bool eventFired = false;

            var updateSub = rtListSet.Events.SubscribeOnUpdate(args =>
            {
                capturedEvent = (args.oldItem, args.newItem, args.index);
                eventFired = true;
            });

            rtListSet[1] = "newB";

            Assert.AreEqual("newB", rtListSet[1], "Set index [1] failed to update value.");
            CollectionAssert.AreEqual(new[] { "oldA", "newB", "oldC" }, rtListSet.ToList(), "List contents after set are incorrect.");
            
            Assert.IsTrue(eventFired, "OnUpdate event did not fire.");
            Assert.AreEqual("oldB", capturedEvent.oldItem, "Captured oldItem in event is incorrect.");
            Assert.AreEqual("newB", capturedEvent.newItem, "Captured newItem in event is incorrect.");
            Assert.AreEqual(1, capturedEvent.index, "Captured index in event is incorrect.");

            Assert.Throws<System.ArgumentOutOfRangeException>(() => { rtListSet[3] = "invalid"; }, "Set index [3] (out of bounds) should throw.");
            Assert.Throws<System.ArgumentOutOfRangeException>(() => { rtListSet[-1] = "invalid"; }, "Set index [-1] (out of bounds) should throw.");

            updateSub.Dispose();
        }

        [Test]
        public void Test_SetAt_And_OnReplaceEvent()
        {
            var rtList = new RtList<string> { "one", "two", "three" };
            (string oldItem, string newItem, int index) capturedEvent = default;
            bool eventFired = false;

            var replaceSub = rtList.Events.SubscribeOnReplace(args =>
            {
                capturedEvent = (args.oldItem, args.newItem, args.index);
                eventFired = true;
            });

            rtList.SetAt(1, "newTwo");

            Assert.AreEqual("newTwo", rtList[1], "rtList[1] was not updated by SetAt.");
            CollectionAssert.AreEqual(new[] { "one", "newTwo", "three" }, rtList.ToList(), "List contents after SetAt are incorrect.");
            
            Assert.IsTrue(eventFired, "OnReplace event did not fire.");
            Assert.AreEqual("two", capturedEvent.oldItem, "Captured oldItem in OnReplace event is incorrect.");
            Assert.AreEqual("newTwo", capturedEvent.newItem, "Captured newItem in OnReplace event is incorrect.");
            Assert.AreEqual(1, capturedEvent.index, "Captured index in OnReplace event is incorrect.");

            Assert.Throws<System.ArgumentOutOfRangeException>(() => { rtList.SetAt(3, "invalid"); }, "SetAt with index 3 (out of bounds) should throw.");
            Assert.Throws<System.ArgumentOutOfRangeException>(() => { rtList.SetAt(-1, "invalid"); }, "SetAt with index -1 (out of bounds) should throw.");

            replaceSub.Dispose();
        }

        [Test]
        public void Test_Move_And_OnMoveEvent()
        {
            var rtList = new RtList<string> { "a", "b", "c", "d" };
            (string item, int oldIndex, int newIndex) capturedEvent = default;
            bool eventFired = false;

            var moveSub = rtList.Events.SubscribeOnMove(args =>
            {
                capturedEvent = (args.item, args.oldIndex, args.newIndex);
                eventFired = true;
            });

            // Scenario 1: Move "b" from index 1 to index 2
            rtList.Move(1, 2);
            Assert.AreEqual("b", rtList[2], "Item 'b' not at new index 2 after Move(1,2).");
            CollectionAssert.AreEqual(new[] { "a", "c", "b", "d" }, rtList.ToList(), "List contents after Move(1,2) are incorrect.");
            Assert.IsTrue(eventFired, "OnMove event did not fire for Move(1,2).");
            Assert.AreEqual("b", capturedEvent.item, "Captured item in OnMove event for Move(1,2) is incorrect.");
            Assert.AreEqual(1, capturedEvent.oldIndex, "Captured oldIndex in OnMove event for Move(1,2) is incorrect.");
            Assert.AreEqual(2, capturedEvent.newIndex, "Captured newIndex in OnMove event for Move(1,2) is incorrect.");

            // Scenario 2: Move item to the beginning ("b" from index 2 to 0)
            // Current list: "a", "c", "b", "d"
            eventFired = false; // Reset for next event capture
            rtList.Move(2, 0); // Moves "b" from index 2 to 0
            Assert.AreEqual("b", rtList[0], "Item 'b' not at new index 0 after Move(2,0).");
            CollectionAssert.AreEqual(new[] { "b", "a", "c", "d" }, rtList.ToList(), "List contents after Move(2,0) are incorrect.");
            Assert.IsTrue(eventFired, "OnMove event did not fire for Move(2,0).");
            Assert.AreEqual("b", capturedEvent.item, "Captured item in OnMove event for Move(2,0) is incorrect.");
            Assert.AreEqual(2, capturedEvent.oldIndex, "Captured oldIndex in OnMove event for Move(2,0) is incorrect.");
            Assert.AreEqual(0, capturedEvent.newIndex, "Captured newIndex in OnMove event for Move(2,0) is incorrect.");

            // Scenario 3: Move item to the end ("b" from index 0 to Count - 1)
            // Current list: "b", "a", "c", "d"
            eventFired = false; // Reset for next event capture
            rtList.Move(0, rtList.Count - 1); // Moves "b" from index 0 to 3
            Assert.AreEqual("b", rtList[rtList.Count-1], "Item 'b' not at new index Count-1 after Move(0, Count-1).");
            CollectionAssert.AreEqual(new[] { "a", "c", "d", "b" }, rtList.ToList(), "List contents after Move(0, Count-1) are incorrect.");
            Assert.IsTrue(eventFired, "OnMove event did not fire for Move(0, Count-1).");
            Assert.AreEqual("b", capturedEvent.item, "Captured item in OnMove event for Move(0, Count-1) is incorrect.");
            Assert.AreEqual(0, capturedEvent.oldIndex, "Captured oldIndex in OnMove event for Move(0, Count-1) is incorrect.");
            Assert.AreEqual(rtList.Count - 1, capturedEvent.newIndex, "Captured newIndex in OnMove event for Move(0, Count-1) is incorrect.");

            // Test exceptions
            Assert.Throws<System.ArgumentOutOfRangeException>(() => rtList.Move(0, rtList.Count), "Move with newIndex == rtList.Count should throw.");
            Assert.Throws<System.ArgumentOutOfRangeException>(() => rtList.Move(rtList.Count, 0), "Move with oldIndex == rtList.Count should throw.");
            Assert.Throws<System.ArgumentOutOfRangeException>(() => rtList.Move(-1, 0), "Move with oldIndex < 0 should throw.");
            Assert.Throws<System.ArgumentOutOfRangeException>(() => rtList.Move(0, -1), "Move with newIndex < 0 should throw.");

            moveSub.Dispose();
        }
    }
}