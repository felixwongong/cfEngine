using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using cfEngine.Rx;

namespace cfEngine.Rx.Test
{
    [TestFixture]
    public class RtSelectList_Test
    {
        private RtList<int> CreateSourceList()
        {
            var source = new RtList<int>();
            source.Add(1);
            source.Add(2);
            source.Add(3);
            return source;
        }

        private Func<int, string> selector = num => "item-" + num.ToString();

        public class SelectableItem
        {
            public int Id { get; set; }
            public string Value { get; set; }
            public SelectableItem(int id, string val) { Id = id; Value = val; }
            public override bool Equals(object obj) => obj is SelectableItem item && Id == item.Id && Value == item.Value;
            public override int GetHashCode() => (Id, Value).GetHashCode();
        }
        private Func<int, SelectableItem> objectSelector = num => new SelectableItem(num, "value-" + num);

        [Test]
        public void Test_InitialTransformation()
        {
            var source = CreateSourceList();
            var selectList = new RtSelectList<int, string>(source, selector);

            Assert.AreEqual(3, selectList.Count, "Initial count of select list is wrong.");
            Assert.AreEqual("item-1", selectList[0], "Value for index 0 in select list is wrong.");
            Assert.AreEqual("item-2", selectList[1], "Value for index 1 in select list is wrong.");
            Assert.AreEqual("item-3", selectList[2], "Value for index 2 in select list is wrong.");

            selectList.Dispose();
            source.Dispose();
        }

        [Test]
        public void Test_Source_AddItem_TransformsAndAdds()
        {
            var source = CreateSourceList();
            var selectList = new RtSelectList<int, string>(source, selector);
            
            var addedToSelect = new List<string>();
            var addSub = selectList.Events.SubscribeOnAdd(args => addedToSelect.Add(args.item));

            source.Add(4);

            Assert.AreEqual(4, selectList.Count, "Count after adding item to source is wrong.");
            Assert.AreEqual("item-4", selectList[3], "Value for new item in select list is wrong.");

            Assert.AreEqual(1, addedToSelect.Count, "OnAdd event count for select list is wrong.");
            Assert.AreEqual("item-4", addedToSelect[0], "Added item in event is wrong.");

            addSub.Dispose();
            selectList.Dispose();
            source.Dispose();
        }

        [Test]
        public void Test_Source_RemoveItem_RemovesTransformed()
        {
            var source = CreateSourceList();
            source.Add(4);
            source.Add(5); // Source is 1,2,3,4,5
            var selectList = new RtSelectList<int, string>(source, selector); // item-1, item-2, item-3, item-4, item-5
            
            (string item, int index) removedEvent = default;
            bool eventFired = false;
            var removeSub = selectList.Events.SubscribeOnRemoveAt(args => 
            {
                removedEvent = (args.item, args.index);
                eventFired = true;
            });

            source.RemoveAt(1); // removes 2 from source, so "item-2" should be removed from selectList at index 1

            Assert.AreEqual(4, selectList.Count, "Count after removing item from source is wrong.");
            Assert.IsTrue(selectList.SequenceEqual(new[] { "item-1", "item-3", "item-4", "item-5" }), "Select list content after remove is wrong.");
            
            Assert.IsTrue(eventFired, "OnRemoveAt event did not fire.");
            Assert.AreEqual("item-2", removedEvent.item, "Removed item in event is wrong.");
            Assert.AreEqual(1, removedEvent.index, "Removed item index in event is wrong.");

            removeSub.Dispose();
            selectList.Dispose();
            source.Dispose();
        }

        [Test]
        public void Test_Source_UpdateItem_UpdatesTransformed_IfSelectorReflectsChange()
        {
            // Scenario 1: Selector produces new distinct result on re-application
            var sourceForUpdate = new RtList<int>();
            sourceForUpdate.Add(10);
            sourceForUpdate.Add(20);
            
            Func<int, string> dynamicSelector = val => val > 15 ? $"Big-{val}" : $"Small-{val}";
            var selectListUpdated = new RtSelectList<int, string>(sourceForUpdate, dynamicSelector); // Initial: Small-10, Big-20

            (string oldItem, string newItem, int index) updatedEvent = default;
            bool eventFired = false;
            // Assuming OnUpdate or OnReplace. Let's assume OnUpdate based on RtList's indexer behavior
            var updateSub = selectListUpdated.Events.SubscribeOnUpdate(args => 
            {
                updatedEvent = (args.oldItem, args.newItem, args.index);
                eventFired = true;
            });

            sourceForUpdate[0] = 25; // Was Small-10 (source 10), now source is 25 -> should be Big-25

            Assert.AreEqual("Big-25", selectListUpdated[0], "Updated value in select list is wrong.");
            Assert.IsTrue(eventFired, "OnUpdate event did not fire.");
            Assert.AreEqual("Small-10", updatedEvent.oldItem, "Old item in event is wrong.");
            Assert.AreEqual("Big-25", updatedEvent.newItem, "New item in event is wrong.");
            Assert.AreEqual(0, updatedEvent.index, "Index in event is wrong.");

            updateSub.Dispose();
            selectListUpdated.Dispose();
            sourceForUpdate.Dispose();
        }

        [Test]
        public void Test_Source_Clear_ClearsSelectList()
        {
            var source = CreateSourceList();
            var selectList = new RtSelectList<int, string>(source, selector);
            
            bool clearFired = false;
            var clearSub = selectList.Events.SubscribeOnClear(() => clearFired = true);

            source.Clear();

            Assert.AreEqual(0, selectList.Count, "Select list count after source clear is wrong.");
            Assert.IsTrue(clearFired, "OnClear event for select list did not fire.");

            clearSub.Dispose();
            selectList.Dispose();
            source.Dispose();
        }

        [Test]
        public void Test_Dispose_SelectList()
        {
            var source = CreateSourceList();
            var selectList = new RtSelectList<int, string>(source, selector);
            
            bool disposeFired = false;
            var disposeSub = selectList.Events.SubscribeOnDispose(() => disposeFired = true);

            selectList.Dispose();

            Assert.IsTrue(disposeFired, "OnDispose event for select list did not fire.");
            Assert.Throws<ObjectDisposedException>(() => { var _ = selectList.Count; }, "Accessing Count after dispose should throw ObjectDisposedException.");
            Assert.Throws<ObjectDisposedException>(() => { var _ = selectList[0]; }, "Accessing indexer after dispose should throw ObjectDisposedException.");

            Assert.AreEqual(3, source.Count, "Source list count should not be affected by select list's dispose.");
            Assert.AreEqual("item-1", selector(source[0]), "Source list content should be intact."); // Simple check on source

            disposeSub.Dispose();
            source.Dispose();
        }

        [Test]
        public void Test_Source_Dispose_DisposesSelectList()
        {
            var source = CreateSourceList();
            var selectList = new RtSelectList<int, string>(source, selector);
            
            bool selectListDisposeFired = false;
            var selectListDisposeSub = selectList.Events.SubscribeOnDispose(() => selectListDisposeFired = true);

            source.Dispose();

            Assert.IsTrue(selectListDisposeFired, "OnDispose event for select list did not fire after source dispose.");
            Assert.Throws<ObjectDisposedException>(() => { var _ = selectList.Count; }, "Accessing Count after source dispose should throw ObjectDisposedException.");
            Assert.Throws<ObjectDisposedException>(() => { var _ = selectList[0]; }, "Accessing indexer after source dispose should throw ObjectDisposedException.");

            selectListDisposeSub.Dispose();
            // selectList is already disposed by source.Dispose(), but calling again should be safe.
            selectList.Dispose(); 
        }

        [Test]
        public void Test_Source_MoveItem_MovesTransformedItem_And_FiresOnMove()
        {
            var source = CreateSourceList(); // 1, 2, 3
            source.Add(4);
            source.Add(5); // Source: 1, 2, 3, 4, 5
            
            var selectList = new RtSelectList<int, string>(source, selector); // item-1, item-2, item-3, item-4, item-5
            
            (string item, int oldIdx, int newIdx) movedItemArgs = default;
            bool eventFired = false;
            var moveSub = selectList.Events.SubscribeOnMove(args => 
            {
                movedItemArgs = (args.item, args.oldIndex, args.newIndex);
                eventFired = true;
            });

            source.Move(1, 3); // Moves 2 (item-2) from index 1 to index 3. 
                               // Source becomes: 1, 3, 4, 2, 5
                               // SelectList should become: item-1, item-3, item-4, item-2, item-5

            Assert.AreEqual(5, selectList.Count, "Select list count after source move is wrong.");
            Assert.IsTrue(selectList.SequenceEqual(new[] { "item-1", "item-3", "item-4", "item-2", "item-5" }), "Select list content after source move is wrong.");
            
            Assert.IsTrue(eventFired, "OnMove event did not fire on select list.");
            Assert.AreEqual("item-2", movedItemArgs.item, "Moved item in event is wrong.");
            Assert.AreEqual(1, movedItemArgs.oldIdx, "Old index in event is wrong.");
            Assert.AreEqual(3, movedItemArgs.newIdx, "New index in event is wrong.");

            moveSub.Dispose();
            selectList.Dispose();
            source.Dispose();
        }
        
        [Test]
        public void Test_Source_SetAt_ReplacesTransformedItem_And_FiresEvent()
        {
            var source = CreateSourceList(); // Source: 1, 2, 3
            var selectList = new RtSelectList<int, string>(source, selector); // SelectList: item-1, item-2, item-3

            var eventArgsList = new List<(string oldItem, string newItem, int index)>();
            // RtSelectList's OnUpdate event is the one that should fire when an item is replaced and the selector produces a new value.
            // If the selector produced the same value, no event would fire from RtSelectList.
            var updateSub = selectList.Events.SubscribeOnUpdate(args => 
                eventArgsList.Add((args.oldItem, args.newItem, args.index)));
            
            // If RtList.SetAt fires OnReplace, and RtSelectList listens to OnReplace from source
            // and then fires its own OnReplace. However, RtList.SetAt actually fires OnUpdate for the source list.
            // RtSelectList should then re-select and fire OnUpdate if the selected value changes.
            // If RtSelectList has an OnReplace, it would be more specific.
            // Let's assume RtSelectList fires OnUpdate for this.

            source.SetAt(1, 10); // Replaces 2 with 10. Source: 1, 10, 3. 
                                 // SelectList: item-1, item-10, item-3.

            Assert.AreEqual(3, selectList.Count, "Select list count after source SetAt is wrong.");
            Assert.IsTrue(selectList.SequenceEqual(new[] { "item-1", "item-10", "item-3" }), "Select list content after source SetAt is wrong.");
            
            Assert.AreEqual(1, eventArgsList.Count, "Incorrect number of events fired.");
            Assert.AreEqual("item-2", eventArgsList[0].oldItem, "Old item in event is wrong.");
            Assert.AreEqual("item-10", eventArgsList[0].newItem, "New item in event is wrong.");
            Assert.AreEqual(1, eventArgsList[0].index, "Index in event is wrong.");

            updateSub.Dispose();
            selectList.Dispose();
            source.Dispose();
        }
    }
}
