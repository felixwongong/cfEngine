using System;
using System.Collections.Generic;
using System.Linq; // Added for Contains and other LINQ methods if needed
using NUnit.Framework;

namespace cfEngine.Rx.Test
{
    [TestFixture]
    public class RtDictionary_Test 
    {
        [Test]
        public void RtDictionary_AddRemove() // Renamed for clarity as it tests Add and its event
        {
            var addedEvents = new List<KeyValuePair<int, string>>();
            var rtDictionary = new RtDictionary<int, string>();
            var addSub = rtDictionary.Events.SubscribeOnAdd(kvp =>
            {
                addedEvents.Add(kvp);
            });
            
            rtDictionary.Add(1, "one");
            rtDictionary.Add(2, "two");

            Assert.IsTrue(rtDictionary.ContainsKey(1), "Dictionary should contain key 1 after Add.");
            Assert.IsTrue(rtDictionary.ContainsKey(2), "Dictionary should contain key 2 after Add.");
            Assert.AreEqual("one", rtDictionary[1], "Value for key 1 is incorrect.");
            Assert.AreEqual("two", rtDictionary[2], "Value for key 2 is incorrect.");
            
            Assert.AreEqual(2, addedEvents.Count, "Incorrect number of OnAdd events captured.");
            Assert.IsTrue(addedEvents.Contains(new KeyValuePair<int, string>(1, "one")), "OnAdd event for (1, 'one') not captured.");
            Assert.IsTrue(addedEvents.Contains(new KeyValuePair<int, string>(2, "two")), "OnAdd event for (2, 'two') not captured.");
            
            addSub.Dispose();
        }

        [Test]
        public void RtDictionary_Remove()
        {
            var removedEvents = new List<KeyValuePair<int, string>>();
            var rtDictionary = new RtDictionary<int, string>();
            
            // Add items first
            rtDictionary.Add(1, "one");
            rtDictionary.Add(2, "two");
            rtDictionary.Add(3, "three"); // For KVP remove test

            var removeSub = rtDictionary.Events.SubscribeOnRemove(kvp =>
            {
                removedEvents.Add(kvp);
            });          
            
            rtDictionary.Remove(1); // Remove by key
            rtDictionary.Remove(new KeyValuePair<int, string>(2, "two")); // Remove by KVP (assuming this overload exists or is the target)

            Assert.IsFalse(rtDictionary.ContainsKey(1), "Key 1 should be removed.");
            Assert.IsFalse(rtDictionary.ContainsKey(2), "Key 2 should be removed via KVP.");
            Assert.IsTrue(rtDictionary.ContainsKey(3), "Key 3 should still exist."); // Ensure other items are not affected
            
            Assert.AreEqual(2, removedEvents.Count, "Incorrect number of OnRemove events captured.");
            Assert.IsTrue(removedEvents.Contains(new KeyValuePair<int, string>(1, "one")), "OnRemove event for (1, 'one') not captured.");
            Assert.IsTrue(removedEvents.Contains(new KeyValuePair<int, string>(2, "two")), "OnRemove event for (2, 'two') not captured.");
            
            removeSub.Dispose();
        }

        [Test]
        public void RtDictionary_Upsert()
        {
            var addedEvent = default(KeyValuePair<int, string>);
            var updatedEvent = default((KeyValuePair<int, string> oldKvp, KeyValuePair<int, string> newKvp));
            bool addFired = false;
            bool updateFired = false;

            var rtDictionary = new RtDictionary<int, string>();
            var sub = rtDictionary.Events.Subscribe(
                onAdd: kvp => { addedEvent = kvp; addFired = true; },
                onUpdate: (oldKvp, newKvp) => { updatedEvent = (oldKvp, newKvp); updateFired = true; }
            );
            
            // Test Add via Upsert
            rtDictionary.Upsert(1, "one");
            Assert.IsTrue(addFired, "OnAdd did not fire for new key in Upsert.");
            Assert.AreEqual(new KeyValuePair<int, string>(1, "one"), addedEvent, "Added KVP in Upsert is incorrect.");
            Assert.AreEqual("one", rtDictionary[1], "Value after Upsert (add) is incorrect.");

            addFired = false; // Reset for next operation

            // Test Update via Upsert
            rtDictionary.Upsert(1, "new one");
            Assert.IsTrue(updateFired, "OnUpdate did not fire for existing key in Upsert.");
            Assert.IsFalse(addFired, "OnAdd should not fire for existing key in Upsert."); // Ensure Add doesn't fire on update
            Assert.AreEqual(new KeyValuePair<int, string>(1, "one"), updatedEvent.oldKvp, "Old KVP in Upsert (update) is incorrect.");
            Assert.AreEqual(new KeyValuePair<int, string>(1, "new one"), updatedEvent.newKvp, "New KVP in Upsert (update) is incorrect.");
            Assert.AreEqual("new one", rtDictionary[1], "Value after Upsert (update) is incorrect.");
            
            sub.Dispose();
        }

        [Test]
        public void RtDictionary_Dispose()
        {
            bool disposed = false;
            var rtDictionary = new RtDictionary<int, string>();
            var sub = rtDictionary.Events.SubscribeOnDispose(() =>
            {
                disposed = true;
            });
            
            rtDictionary.Add(1, "one");
            rtDictionary.Add(2, "two");
            
            rtDictionary.Dispose();
            
            Assert.IsTrue(disposed, "OnDispose event did not fire.");
            Assert.IsTrue(rtDictionary.Count == 0, "Dictionary count should be 0 after dispose.");
            // Optionally, check if TryGetValue or indexer throws ObjectDisposedException or similar
            Assert.Throws<ObjectDisposedException>(() => rtDictionary.Add(3, "three"), "Add after Dispose should throw.");
            Assert.Throws<ObjectDisposedException>(() => { var _ = rtDictionary.ContainsKey(1); }, "Access after Dispose should throw.");


            sub.Dispose();
        }

        [Test]
        public void RtDictionary_NotCacheSubscription()
        {
            var addedEvents = new List<KeyValuePair<int, string>>();
            var rtDictionary = new RtDictionary<int, string>();
            // The key here is that the lambda is the only reference to the subscription
            rtDictionary.Events.SubscribeOnAdd(kvp =>
            {
                addedEvents.Add(kvp);
            });
            
            GC.Collect();
            GC.WaitForPendingFinalizers(); // Ensure finalizers run
            
            rtDictionary.Add(1, "one");
            rtDictionary.Add(2, "two");

            // If subscription was collected, addedEvents list will be empty
            Assert.IsEmpty(addedEvents, "Subscription was not collected, events were still captured.");
        }

        [Test]
        public void RtReadOnlyDictionary_RtPairs()
        {
            var rtDictionary = new RtDictionary<int, string>();
            rtDictionary.Add(1, "one");
            var rtPairs = rtDictionary.RtPairs; // Assuming RtPairs is a snapshot or live view
            rtDictionary.Add(2, "two");
            
            // Assertions might need adjustment based on RtPairs behavior (snapshot vs. live)
            // For a live view, both should be present. For a snapshot, only 'one'.
            // Assuming live view for this test based on typical reactive collection behavior.
            Assert.AreEqual(2, rtPairs.Count, "RtPairs count is incorrect.");
            Assert.IsTrue(rtPairs.Any(p => p.Key == 1 && p.Value == "one"), "RtPairs missing (1, 'one').");
            Assert.IsTrue(rtPairs.Any(p => p.Key == 2 && p.Value == "two"), "RtPairs missing (2, 'two').");
        }

        [Test]
        public void RtReadOnlyDictionary_RtKeys()
        {
            var rtDictionary = new RtDictionary<int, string>();
            rtDictionary.Add(1, "one");
            var rtKeys = rtDictionary.RtKeys; // Assuming live view
            rtDictionary.Add(2, "two");
            
            Assert.AreEqual(2, rtKeys.Count, "RtKeys count is incorrect.");
            Assert.IsTrue(rtKeys.Contains(1), "RtKeys missing key 1.");
            Assert.IsTrue(rtKeys.Contains(2), "RtKeys missing key 2.");
        }

        [Test]
        public void RtReadOnlyDictionary_RtValues()
        {
            var rtDictionary = new RtDictionary<int, string>();
            rtDictionary.Add(1, "one");
            var rtValues = rtDictionary.RtValues; // Assuming live view
            rtDictionary.Add(2, "two");
            
            Assert.AreEqual(2, rtValues.Count, "RtValues count is incorrect.");
            Assert.IsTrue(rtValues.Contains("one"), "RtValues missing 'one'.");
            Assert.IsTrue(rtValues.Contains("two"), "RtValues missing 'two'.");
        }

        [Test]
        public void Test_Clear_And_OnClearEvent()
        {
            var rtDictionary = new RtDictionary<int, string> { { 1, "a" }, { 2, "b" } };
            bool clearEventFired = false;
            var clearSub = rtDictionary.Events.SubscribeOnClear(() => clearEventFired = true);

            rtDictionary.Clear();

            Assert.AreEqual(0, rtDictionary.Count, "Dictionary count after Clear is incorrect.");
            Assert.IsTrue(clearEventFired, "OnClear event did not fire.");
            Assert.IsFalse(rtDictionary.ContainsKey(1), "Key 1 should not exist after Clear.");
            Assert.IsFalse(rtDictionary.ContainsKey(2), "Key 2 should not exist after Clear.");

            clearSub.Dispose();
        }

        [Test]
        public void Test_TryGetValue()
        {
            var rtDictionary = new RtDictionary<int, string> { { 1, "one" }, { 2, "two" } };

            // Existing Key
            bool result = rtDictionary.TryGetValue(1, out string value);
            Assert.IsTrue(result, "TryGetValue for existing key 1 should return true.");
            Assert.AreEqual("one", value, "Value from TryGetValue for key 1 is incorrect.");

            // Non-Existing Key
            bool resultNonExisting = rtDictionary.TryGetValue(3, out string valueNonExisting);
            Assert.IsFalse(resultNonExisting, "TryGetValue for non-existing key 3 should return false.");
            Assert.AreEqual(default(string), valueNonExisting, "Value from TryGetValue for non-existing key should be default.");

            // Key with Null Value (if dictionary supports null values)
            rtDictionary.Add(4, null); // Assuming string can be null
            bool resultNull = rtDictionary.TryGetValue(4, out string valueNull);
            Assert.IsTrue(resultNull, "TryGetValue for key 4 (with null value) should return true.");
            Assert.IsNull(valueNull, "Value from TryGetValue for key 4 should be null.");
        }

        [Test]
        public void Test_Indexer_Set_ForUpdate_And_OnUpdateEvent()
        {
            var rtDictionary = new RtDictionary<int, string> { { 1, "initialOne" }, { 2, "initialTwo" } };
            (KeyValuePair<int, string> oldKvp, KeyValuePair<int, string> newKvp) capturedUpdate = default;
            bool updateEventFired = false;
            bool addEventFired = false; // To check indexer does not fire Add on update

            var updateSub = rtDictionary.Events.SubscribeOnUpdate((oldVal, newVal) => 
            {
                capturedUpdate = (oldVal, newVal);
                updateEventFired = true;
            });
            var addSub = rtDictionary.Events.SubscribeOnAdd(kvp => addEventFired = true); // Auxiliary to check no add event

            // Test Update
            rtDictionary[1] = "updatedOne";
            Assert.AreEqual("updatedOne", rtDictionary[1], "Value for key 1 after indexer set is incorrect.");
            Assert.AreEqual(2, rtDictionary.Count, "Count should remain 2 after updating existing key.");
            Assert.IsTrue(updateEventFired, "OnUpdate event did not fire for indexer set.");
            Assert.IsFalse(addEventFired, "OnAdd event should not fire when indexer updates an existing key.");
            Assert.AreEqual(new KeyValuePair<int, string>(1, "initialOne"), capturedUpdate.oldKvp, "Captured oldKvp in OnUpdate is incorrect.");
            Assert.AreEqual(new KeyValuePair<int, string>(1, "updatedOne"), capturedUpdate.newKvp, "Captured newKvp in OnUpdate is incorrect.");
            
            updateEventFired = false; // Reset for next check

            // Test Add via Indexer
            rtDictionary[3] = "third";
            Assert.AreEqual("third", rtDictionary[3], "Value for new key 3 after indexer set is incorrect.");
            Assert.AreEqual(3, rtDictionary.Count, "Count should be 3 after adding new key via indexer.");
            Assert.IsTrue(addEventFired, "OnAdd event did not fire when indexer adds a new key.");
            Assert.IsFalse(updateEventFired, "OnUpdate event should not fire when indexer adds a new key.");

            updateSub.Dispose();
            addSub.Dispose();
        }

        [Test]
        public void Test_Indexer_Get()
        {
            var rtDictionary = new RtDictionary<int, string> { { 1, "one" } };
            Assert.AreEqual("one", rtDictionary[1], "Indexer get for key 1 failed.");
            Assert.Throws<KeyNotFoundException>(() => { var _ = rtDictionary[2]; }, "Indexer get for non-existent key 2 should throw KeyNotFoundException.");
        }

        [Test]
        public void Test_ContainsKey()
        {
            var rtDictionary = new RtDictionary<int, string> { { 1, "one" } };
            Assert.IsTrue(rtDictionary.ContainsKey(1), "ContainsKey(1) should be true.");
            Assert.IsFalse(rtDictionary.ContainsKey(2), "ContainsKey(2) should be false.");
        }

        [Test]
        public void Test_Count_Property()
        {
            var rtDictionary = new RtDictionary<int, string>();
            Assert.AreEqual(0, rtDictionary.Count, "Initial count should be 0.");

            rtDictionary.Add(1, "item1");
            Assert.AreEqual(1, rtDictionary.Count, "Count should be 1 after one Add.");

            rtDictionary.Add(2, "item2");
            Assert.AreEqual(2, rtDictionary.Count, "Count should be 2 after second Add.");

            rtDictionary.Remove(1);
            Assert.AreEqual(1, rtDictionary.Count, "Count should be 1 after one Remove.");

            rtDictionary.Clear();
            Assert.AreEqual(0, rtDictionary.Count, "Count should be 0 after Clear.");
        }
    }
}