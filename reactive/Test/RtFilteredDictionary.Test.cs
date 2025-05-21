using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using cfEngine.Rx;

namespace cfEngine.Rx.Test
{
    [TestFixture]
    public class RtFilteredDictionary_Test
    {
        private RtDictionary<int, string> CreateTestSource()
        {
            var source = new RtDictionary<int, string>();
            source.Add(1, "apple");    // Matches filter: starts with 'a'
            source.Add(2, "banana");   // No match
            source.Add(3, "avocado");  // Matches filter
            source.Add(4, "cherry");   // No match
            source.Add(5, "apricot");  // Matches filter
            return source;
        }

        private Func<KeyValuePair<int, string>, bool> filterPredicate = kvp => kvp.Value.StartsWith("a");

        [Test]
        public void Test_InitialFiltering()
        {
            var source = CreateTestSource();
            var filteredDict = new RtFilteredDictionary<int, string>(source, filterPredicate);

            Assert.AreEqual(3, filteredDict.Count, "Initial count of filtered dictionary is wrong.");
            Assert.IsTrue(filteredDict.ContainsKey(1), "Filtered dictionary should contain key 1.");
            Assert.AreEqual("apple", filteredDict[1], "Value for key 1 in filtered dictionary is wrong.");
            Assert.IsTrue(filteredDict.ContainsKey(3), "Filtered dictionary should contain key 3.");
            Assert.AreEqual("avocado", filteredDict[3], "Value for key 3 in filtered dictionary is wrong.");
            Assert.IsTrue(filteredDict.ContainsKey(5), "Filtered dictionary should contain key 5.");
            Assert.AreEqual("apricot", filteredDict[5], "Value for key 5 in filtered dictionary is wrong.");

            Assert.IsFalse(filteredDict.ContainsKey(2), "Filtered dictionary should not contain key 2.");
            Assert.IsFalse(filteredDict.ContainsKey(4), "Filtered dictionary should not contain key 4.");

            filteredDict.Dispose();
            source.Dispose();
        }

        [Test]
        public void Test_Source_AddItem_MatchesFilter()
        {
            var source = CreateTestSource();
            var filteredDict = new RtFilteredDictionary<int, string>(source, filterPredicate);
            
            var addedToFiltered = new List<KeyValuePair<int, string>>();
            var addSub = filteredDict.Events.SubscribeOnAdd(kvp => addedToFiltered.Add(kvp));

            source.Add(6, "agenda"); // Matches filter

            Assert.AreEqual(4, filteredDict.Count, "Count after adding matching item to source is wrong.");
            Assert.IsTrue(filteredDict.ContainsKey(6), "Filtered dictionary should contain key 6 after source add.");
            Assert.AreEqual("agenda", filteredDict[6], "Value for key 6 in filtered dictionary is wrong.");

            Assert.AreEqual(1, addedToFiltered.Count, "OnAdd event count for filtered dictionary is wrong.");
            Assert.AreEqual(6, addedToFiltered[0].Key, "Added item key in event is wrong.");
            Assert.AreEqual("agenda", addedToFiltered[0].Value, "Added item value in event is wrong.");

            addSub.Dispose();
            filteredDict.Dispose();
            source.Dispose();
        }

        [Test]
        public void Test_Source_AddItem_NoMatchFilter()
        {
            var source = CreateTestSource();
            var filteredDict = new RtFilteredDictionary<int, string>(source, filterPredicate);

            var addedToFiltered = new List<KeyValuePair<int, string>>();
            var addSub = filteredDict.Events.SubscribeOnAdd(kvp => addedToFiltered.Add(kvp));

            source.Add(7, "blueberry"); // No match

            Assert.AreEqual(3, filteredDict.Count, "Count after adding non-matching item to source should not change.");
            Assert.IsFalse(filteredDict.ContainsKey(7), "Filtered dictionary should not contain key 7.");
            Assert.IsEmpty(addedToFiltered, "OnAdd event should not fire for non-matching item.");

            addSub.Dispose();
            filteredDict.Dispose();
            source.Dispose();
        }

        [Test]
        public void Test_Source_RemoveItem_MatchesFilter()
        {
            var source = CreateTestSource();
            var filteredDict = new RtFilteredDictionary<int, string>(source, filterPredicate);

            var removedFromFiltered = new List<KeyValuePair<int, string>>();
            var removeSub = filteredDict.Events.SubscribeOnRemove(kvp => removedFromFiltered.Add(kvp));

            source.Remove(1); // "apple", was in filtered

            Assert.AreEqual(2, filteredDict.Count, "Count after removing matching item from source is wrong.");
            Assert.IsFalse(filteredDict.ContainsKey(1), "Filtered dictionary should not contain key 1 after source remove.");
            
            Assert.AreEqual(1, removedFromFiltered.Count, "OnRemove event count for filtered dictionary is wrong.");
            Assert.AreEqual(1, removedFromFiltered[0].Key, "Removed item key in event is wrong.");
            Assert.AreEqual("apple", removedFromFiltered[0].Value, "Removed item value in event is wrong.");

            removeSub.Dispose();
            filteredDict.Dispose();
            source.Dispose();
        }

        [Test]
        public void Test_Source_RemoveItem_NoMatchFilter()
        {
            var source = CreateTestSource();
            var filteredDict = new RtFilteredDictionary<int, string>(source, filterPredicate);

            var removedFromFiltered = new List<KeyValuePair<int, string>>();
            var removeSub = filteredDict.Events.SubscribeOnRemove(kvp => removedFromFiltered.Add(kvp));

            source.Remove(2); // "banana", was not in filtered

            Assert.AreEqual(3, filteredDict.Count, "Count after removing non-matching item from source should not change.");
            Assert.IsEmpty(removedFromFiltered, "OnRemove event should not fire for non-matching item removal from source.");

            removeSub.Dispose();
            filteredDict.Dispose();
            source.Dispose();
        }

        [Test]
        public void Test_Source_UpdateItem_FromNoMatch_ToMatch()
        {
            var source = CreateTestSource();
            var filteredDict = new RtFilteredDictionary<int, string>(source, filterPredicate);
            
            var addedToFiltered = new List<KeyValuePair<int, string>>();
            var addSub = filteredDict.Events.SubscribeOnAdd(kvp => addedToFiltered.Add(kvp));

            // Key 2 ("banana") initially didn't match, now "anchovy" does.
            source[2] = "anchovy"; 

            Assert.AreEqual(4, filteredDict.Count, "Count after update (no-match to match) is wrong.");
            Assert.IsTrue(filteredDict.ContainsKey(2), "Filtered dictionary should contain key 2 after update.");
            Assert.AreEqual("anchovy", filteredDict[2], "Value for key 2 in filtered dictionary is wrong.");

            Assert.AreEqual(1, addedToFiltered.Count, "OnAdd event count for filtered dictionary is wrong.");
            Assert.AreEqual(2, addedToFiltered[0].Key, "Added item key in event is wrong.");
            Assert.AreEqual("anchovy", addedToFiltered[0].Value, "Added item value in event is wrong.");

            addSub.Dispose();
            filteredDict.Dispose();
            source.Dispose();
        }

        [Test]
        public void Test_Source_UpdateItem_MatchesFilter_ValueChange()
        {
            var source = CreateTestSource();
            var filteredDict = new RtFilteredDictionary<int, string>(source, filterPredicate);
            
            var updatedInFiltered = new List<(KeyValuePair<int, string> oldKvp, KeyValuePair<int, string> newKvp)>();
            var updateSub = filteredDict.Events.SubscribeOnUpdate((oldKvp, newKvp) => updatedInFiltered.Add((oldKvp, newKvp)));

            // Key 1 ("apple") initially matched, "artichoke" still matches.
            source[1] = "artichoke"; 

            Assert.AreEqual(3, filteredDict.Count, "Count after update (match to match) should remain the same.");
            Assert.IsTrue(filteredDict.ContainsKey(1), "Filtered dictionary should still contain key 1.");
            Assert.AreEqual("artichoke", filteredDict[1], "Value for key 1 in filtered dictionary is wrong after update.");

            Assert.AreEqual(1, updatedInFiltered.Count, "OnUpdate event count for filtered dictionary is wrong.");
            Assert.AreEqual(1, updatedInFiltered[0].newKvp.Key, "Updated item key in event is wrong.");
            Assert.AreEqual("apple", updatedInFiltered[0].oldKvp.Value, "Old value in event is wrong.");
            Assert.AreEqual("artichoke", updatedInFiltered[0].newKvp.Value, "New value in event is wrong.");

            updateSub.Dispose();
            filteredDict.Dispose();
            source.Dispose();
        }

        [Test]
        public void Test_Source_UpdateItem_FromMatch_ToNoMatch()
        {
            var source = CreateTestSource();
            var filteredDict = new RtFilteredDictionary<int, string>(source, filterPredicate);
            
            var removedFromFiltered = new List<KeyValuePair<int, string>>();
            var removeSub = filteredDict.Events.SubscribeOnRemove(kvp => removedFromFiltered.Add(kvp));

            // Key 1 ("apple") initially matched, "blueberry" does not.
            source[1] = "blueberry"; 

            Assert.AreEqual(2, filteredDict.Count, "Count after update (match to no-match) is wrong.");
            Assert.IsFalse(filteredDict.ContainsKey(1), "Filtered dictionary should not contain key 1 after update.");

            Assert.AreEqual(1, removedFromFiltered.Count, "OnRemove event count for filtered dictionary is wrong.");
            Assert.AreEqual(1, removedFromFiltered[0].Key, "Removed item key in event is wrong.");
            // The value removed from the filtered dictionary is the one that matched the filter ("apple")
            Assert.AreEqual("apple", removedFromFiltered[0].Value, "Removed item value in event is wrong.");

            removeSub.Dispose();
            filteredDict.Dispose();
            source.Dispose();
        }

        [Test]
        public void Test_Source_Clear_ClearsFilteredDictionary()
        {
            var source = CreateTestSource();
            var filteredDict = new RtFilteredDictionary<int, string>(source, filterPredicate);
            
            bool clearEventFired = false;
            var clearSub = filteredDict.Events.SubscribeOnClear(() => clearEventFired = true);

            source.Clear();

            Assert.AreEqual(0, filteredDict.Count, "Filtered dictionary count after source clear is wrong.");
            Assert.IsTrue(clearEventFired, "OnClear event for filtered dictionary did not fire.");

            clearSub.Dispose();
            filteredDict.Dispose();
            source.Dispose(); // Source is already cleared and disposed by its own Clear method, but good practice.
        }

        [Test]
        public void Test_Dispose_FilteredDictionary()
        {
            var source = CreateTestSource();
            var initialSourceCount = source.Count;
            var filteredDict = new RtFilteredDictionary<int, string>(source, filterPredicate);
            
            bool disposeEventFired = false;
            var disposeSub = filteredDict.Events.SubscribeOnDispose(() => disposeEventFired = true);

            filteredDict.Dispose();

            Assert.IsTrue(disposeEventFired, "OnDispose event for filtered dictionary did not fire.");
            Assert.AreEqual(0, filteredDict.Count, "Filtered dictionary count after its dispose is wrong.");
            // Check that operations throw ObjectDisposedException
            Assert.Throws<ObjectDisposedException>(() => { var _ = filteredDict.ContainsKey(1); });
            Assert.Throws<ObjectDisposedException>(() => { var _ = filteredDict.Count; }); // Accessing Count might also throw

            Assert.AreEqual(initialSourceCount, source.Count, "Source dictionary count should not be affected by filtered dictionary's dispose.");
            Assert.IsTrue(source.ContainsKey(1), "Source dictionary should still contain its items.");

            disposeSub.Dispose(); // Dispose the subscription
            source.Dispose();
        }
        
        [Test]
        public void Test_Source_Dispose_DisposesFilteredDictionary()
        {
            var source = CreateTestSource();
            var filteredDict = new RtFilteredDictionary<int, string>(source, filterPredicate);
            
            bool filteredDisposeEventFired = false;
            var filteredDisposeSub = filteredDict.Events.SubscribeOnDispose(() => filteredDisposeEventFired = true);

            source.Dispose(); // Dispose the source

            Assert.IsTrue(filteredDisposeEventFired, "OnDispose event for filtered dictionary did not fire after source dispose.");
            Assert.AreEqual(0, filteredDict.Count, "Filtered dictionary count after source dispose should be 0.");
            // Operations on filteredDict should ideally throw ObjectDisposedException
            Assert.Throws<ObjectDisposedException>(() => { var _ = filteredDict.ContainsKey(1); });
            Assert.Throws<ObjectDisposedException>(() => { var _ = filteredDict.Count; });

            filteredDisposeSub.Dispose();
            // filteredDict is already disposed by source.Dispose(), but calling again should be safe.
            filteredDict.Dispose(); 
        }
    }
}
