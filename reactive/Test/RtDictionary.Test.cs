using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace cfEngine.Rx.Test
{
    [TestFixture]
    public class RtDictionary_Test 
    {
        [Test]
        public void RtDictionary_AddRemove()
        {
            string[] added = new string[2];
            var rtDictionary = new RtDictionary<int, string>();
            var addSub = rtDictionary.Events.SubscribeOnAdd(kvp =>
            {
                added[kvp.Key - 1] = kvp.Value;
            });
            
            rtDictionary.Add(1, "one");
            rtDictionary.Add(2, "two");
            Assert.Equals(rtDictionary.ContainsKey(1), true);
            Assert.Equals(rtDictionary.ContainsKey(2), true);
            Assert.Equals(rtDictionary[1], "one");
            Assert.Equals(rtDictionary[2], "two");
            Assert.Equals(added[0], "one");
            Assert.Equals(added[1], "two");
        }

        [Test]
        public void RtDictionary_Remove()
        {
            string[] removed = new string[2];
            var rtDictionary = new RtDictionary<int, string>();
            var removeSub = rtDictionary.Events.SubscribeOnRemove(kvp =>
            {
                removed[kvp.Key - 1] = kvp.Value;
            });          
            
            rtDictionary.Add(1, "one");
            rtDictionary.Add(2, "two");

            rtDictionary.Remove(1);
            rtDictionary.Remove(new KeyValuePair<int, string>(2, "two"));
            Assert.Equals(rtDictionary.ContainsKey(1), false);
            Assert.Equals(rtDictionary.ContainsKey(2), false);   
            Assert.Equals(removed[0], "one");
            Assert.Equals(removed[1], "two");
        }

        [Test]
        public void RtDictionary_Upsert()
        {
            string added = string.Empty;
            string updated = string.Empty;
            var rtDictionary = new RtDictionary<int, string>();
            var sub = rtDictionary.Events.Subscribe(
                onAdd: kvp => { if(kvp.Key == 1) added = kvp.Value; },
                onUpdate: (old, kvp) => { if(kvp.Key == 1) updated = kvp.Value; }
            );
            
            rtDictionary.Upsert(1, "one");
            rtDictionary.Upsert(1, "new one");
            Assert.Equals(added, "one");
            Assert.Equals(updated, "new one");
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
            
            Assert.Equals(disposed, true);
            Assert.Equals(rtDictionary.Count, 0);
        }

        [Test]
        public void RtDictionary_NotCacheSubscription()
        {
            string[] added = new string[2];
            var rtDictionary = new RtDictionary<int, string>();
            rtDictionary.Events.SubscribeOnAdd(kvp =>
            {
                added[kvp.Key - 1] = kvp.Value;
            });
            
            GC.Collect();
            
            rtDictionary.Add(1, "one");
            rtDictionary.Add(2, "two");
            Assert.Equals(added[0], "one");
            Assert.Equals(added[1], "two");
        }

        [Test]
        public void RtReadOnlyDictionary_RtPairs()
        {
            var rtDictionary = new RtDictionary<int, string>();
            rtDictionary.Add(1, "one");
            var rtPairs = rtDictionary.RtPairs;
            rtDictionary.Add(2, "two");
            
            Assert.Equals(rtPairs.Count >= 2, true);
            Assert.Equals(rtPairs[0].Equals(new KeyValuePair<int,string>(1, "one")), true);
            Assert.Equals(rtPairs[1].Equals(new KeyValuePair<int,string>(2, "two")), true);
        }

        [Test]
        public void RtReadOnlyDictionary_RtKeys()
        {
            var rtDictionary = new RtDictionary<int, string>();
            rtDictionary.Add(1, "one");
            var rtKeys = rtDictionary.RtKeys;
            rtDictionary.Add(2, "two");
            
            Assert.Equals(rtKeys.Count >= 2, true);
            Assert.Equals(rtKeys[0].Equals(1), true);
            Assert.Equals(rtKeys[1].Equals(2), true);
        }

        [Test]
        public void RtReadOnlyDictionary_RtValues()
        {
            var rtDictionary = new RtDictionary<int, string>();
            rtDictionary.Add(1, "one");
            var rtValues = rtDictionary.RtValues;
            rtDictionary.Add(2, "two");
            
            Assert.Equals(rtValues.Count >= 2, true);
            Assert.Equals(rtValues[0].Equals("one"), true);
            Assert.Equals(rtValues[1].Equals("two"), true);
        }
    }
}