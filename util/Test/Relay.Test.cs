using System;
using System.Collections.Generic;
using System.Linq; // May be useful later
using NUnit.Framework;
using cfEngine.Rx; // Namespace for Relay and Subscription classes
// Potentially using cfEngine.Logging; if we need to mock Log.LogError

namespace cfEngine.Util.Test // Assuming a new namespace for util tests
{
    [TestFixture]
    public class Relay_Test
    {
        // Helper for Mocking Log.LogError
        private static Action<object> _originalLogError;
        private static List<string> _errorLogMessages;

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            _errorLogMessages = new List<string>();
            // Assuming Log.LogError takes an object or string. Adjust if necessary.
            // This is a simplified way to "mock" a static logger.
            // If Log.LogError is an event or can be set via a provider, that's better.
            // For now, let's assume we can't directly swap Log.LogError.
            // The test for duplicate listener logging might be hard to verify without actual logger redirection.
            // I will proceed assuming direct verification of Log.LogError is out of scope for now,
            // and focus on the behavior (e.g., AddListener returning null).
        }

        // If direct mocking of Log.LogError is possible, it would look like:
        // [SetUp] public void InterceptLogging() { _originalLogError = Log.LogError; Log.LogError = msg => _errorLogMessages.Add(msg.ToString()); }
        // [TearDown] public void RestoreLogging() { Log.LogError = _originalLogError; _errorLogMessages.Clear(); }


        // Test methods will be added here in subsequent steps.

        // Helper class for owner object, as Relay constructors require an 'owner'
        private class TestOwner { }

        // Example listener methods for testing
        private bool _actionCalled = false;
        private void TestAction() => _actionCalled = true;

        private int _actionT1Value = 0;
        private void TestActionT1(int val) => _actionT1Value = val;
        
        private string _actionT1T2_Val1 = null;
        private int _actionT1T2_Val2 = 0;
        private void TestActionT1T2(string val1, int val2)
        {
            _actionT1T2_Val1 = val1;
            _actionT1T2_Val2 = val2;
        }

        // Reset helper for stateful listener checks
        [SetUp]
        public void Setup()
        {
            _actionCalled = false;
            _actionT1Value = 0;
            _actionT1T2_Val1 = null;
            _actionT1T2_Val2 = 0;
            // Any other per-test setup
        }

        // Helper Mock/Stub for RelayBase for SubscriptionBinding tests
        private class MockRelayForBindingTests : RelayBase<Action>
        {
            public Action ListenerPassedToRemove { get; private set; }
            public bool RemoveListenerCalled { get; private set; } = false;

            public MockRelayForBindingTests() : base(new TestOwner()) { }

            // This 'new' keyword hides the base class's RemoveListener.
            // For the purpose of testing SubscriptionBinding, we only care that 
            // SubscriptionBinding calls *a* RemoveListener method on the relay object
            // it holds, and passes the correct listener.
            public new bool RemoveListener(Action listener) 
            {
                RemoveListenerCalled = true;
                ListenerPassedToRemove = listener;
                // We don't call base.RemoveListener(listener) here because
                // the listener was never actually added to this mock relay's internal list.
                // The test is about the SubscriptionBinding's behavior of calling this method.
                return true; // Simulate successful removal
            }
        }

        [Test]
        public void SubscriptionBinding_IsListener_CorrectlyIdentifiesListener()
        {
            Action listener1 = TestAction;
            Action listener2 = () => { /* Another action */ };
            var dummyRelay = new Relay(new TestOwner()); // Can use a real Relay, its state doesn't matter here

            var binding = new SubscriptionBinding<Action>(listener1, dummyRelay);

            Assert.IsTrue(binding.IsListener(listener1), "IsListener should be true for the bound listener.");
            Assert.IsFalse(binding.IsListener(listener2), "IsListener should be false for a different listener.");
            Assert.IsFalse(binding.IsListener(null), "IsListener should be false for null.");
        }

        [Test]
        public void SubscriptionBinding_Unsubscribe_CallsRemoveListenerOnRelay()
        {
            Action listener = TestAction;
            var mockRelay = new MockRelayForBindingTests();
            
            var binding = new SubscriptionBinding<Action>(listener, mockRelay);
            binding.Unsubscribe();

            Assert.IsTrue(mockRelay.RemoveListenerCalled, "RemoveListener should have been called on the relay.");
            Assert.AreSame(listener, mockRelay.ListenerPassedToRemove, "RemoveListener should have been called with the correct listener.");
        }
    
        [Test]
        public void SubscriptionBinding_Unsubscribe_WithNullRelay_DoesNotThrow()
        {
            Action listener = TestAction;
            // The SubscriptionBinding constructor takes RelayBase<TDelegate>.
            // The Unsubscribe method has a null check: _relay?.RemoveListener(Listener);
            // So, if _relay is null, it should not throw.
            var binding = new SubscriptionBinding<Action>(listener, null); // Pass null for the relay

            Assert.DoesNotThrow(() => binding.Unsubscribe(), "Unsubscribe with a null relay should not throw.");
        }

        // Helper Mock for Subscription tests
        private class MockSubscription : Subscription
        {
            public bool UnsubscribedCalled { get; private set; } = false;
            public int UnsubscribeCallCount { get; private set; } = 0;

            public override void Unsubscribe()
            {
                UnsubscribedCalled = true;
                UnsubscribeCallCount++;
            }
        }

        [Test]
        public void SubscriptionGroup_Add_And_Unsubscribe_CallsUnsubscribeOnChildren()
        {
            var subGroup = new SubscriptionGroup();
            var mockSub1 = new MockSubscription();
            var mockSub2 = new MockSubscription();
            var mockSub3 = new MockSubscription();

            subGroup.Add(mockSub1);
            subGroup.Add(mockSub2);
            subGroup.Add(mockSub3);

            subGroup.Unsubscribe();

            Assert.IsTrue(mockSub1.UnsubscribedCalled, "MockSub1 should have been unsubscribed.");
            Assert.AreEqual(1, mockSub1.UnsubscribeCallCount, "MockSub1.Unsubscribe should be called once.");
            Assert.IsTrue(mockSub2.UnsubscribedCalled, "MockSub2 should have been unsubscribed.");
            Assert.AreEqual(1, mockSub2.UnsubscribeCallCount, "MockSub2.Unsubscribe should be called once.");
            Assert.IsTrue(mockSub3.UnsubscribedCalled, "MockSub3 should have been unsubscribed.");
            Assert.AreEqual(1, mockSub3.UnsubscribeCallCount, "MockSub3.Unsubscribe should be called once.");
            
            // Test that the internal list is cleared.
            var freshSub = new MockSubscription();
            subGroup.Add(freshSub); // Should be added to an empty list now if Unsubscribe clears it
            subGroup.Unsubscribe(); // This will call Unsubscribe on freshSub
            
            Assert.IsTrue(freshSub.UnsubscribedCalled, "FreshSub should have been unsubscribed on second group Unsubscribe.");
            Assert.AreEqual(1, freshSub.UnsubscribeCallCount, "FreshSub.Unsubscribe should be called once.");
            // Crucially, check that the original subscriptions were not called again
            Assert.AreEqual(1, mockSub1.UnsubscribeCallCount, "MockSub1.Unsubscribe should still only be called once after second group Unsubscribe.");
            Assert.AreEqual(1, mockSub2.UnsubscribeCallCount, "MockSub2.Unsubscribe should still only be called once after second group Unsubscribe.");
            Assert.AreEqual(1, mockSub3.UnsubscribeCallCount, "MockSub3.Unsubscribe should still only be called once after second group Unsubscribe.");
        }

        [Test]
        public void SubscriptionGroup_Unsubscribe_EmptyGroup_DoesNotThrow()
        {
            var subGroup = new SubscriptionGroup();
            Assert.DoesNotThrow(() => subGroup.Unsubscribe());
        }

        [Test]
        public void SubscriptionGroup_Add_NullSubscription_DoesNotThrow_And_UnsubscribeHandles()
        {
            var subGroup = new SubscriptionGroup();
            // List<T>.Add(null) is allowed.
            Assert.DoesNotThrow(() => subGroup.Add(null), "Adding null to SubscriptionGroup should not throw."); 
            
            var mockSub = new MockSubscription();
            subGroup.Add(mockSub);

            // Unsubscribe should handle null entries gracefully.
            Assert.DoesNotThrow(() => subGroup.Unsubscribe(), "Unsubscribe with null entries should not throw.");
            Assert.IsTrue(mockSub.UnsubscribedCalled, "Valid mock subscription should still be unsubscribed.");
            Assert.AreEqual(1, mockSub.UnsubscribeCallCount, "Valid mock subscription Unsubscribe should be called once.");
        }

        [Test]
        public void RelayBase_AddListener_Basic()
        {
            var owner = new TestOwner();
            var relay = new Relay(owner);
            Action listener = TestAction;

            Assert.AreEqual(0, relay.listenerCount);
            var sub = relay.AddListener(listener);

            Assert.IsNotNull(sub);
            Assert.AreEqual(1, relay.listenerCount);
            Assert.IsTrue(relay.Contains(listener));
            
            sub.Unsubscribe(); // Cleanup
            Assert.AreEqual(0, relay.listenerCount);
        }

        [Test]
        public void RelayBase_AddListener_DuplicateReturnsNullOrSameSubscriptionAndDoesNotAddAgain()
        {
            var owner = new TestOwner();
            var relay = new Relay(owner);
            Action listener = TestAction;

            var sub1 = relay.AddListener(listener);
            Assert.IsNotNull(sub1);
            Assert.AreEqual(1, relay.listenerCount);

            var sub2 = relay.AddListener(listener); // Try adding the same listener
            
            // Current RelayBase logs error and returns null.
            Assert.IsNull(sub2, "Adding a duplicate listener should return null.");
            Assert.AreEqual(1, relay.listenerCount, "Listener count should not increase for duplicate.");
            
            sub1.Unsubscribe();
        }

        [Test]
        public void RelayBase_RemoveListener_Basic()
        {
            var owner = new TestOwner();
            var relay = new Relay(owner);
            Action listener = TestAction;

            relay.AddListener(listener);
            Assert.AreEqual(1, relay.listenerCount);

            bool removed = relay.RemoveListener(listener);
            Assert.IsTrue(removed);
            Assert.AreEqual(0, relay.listenerCount);
            Assert.IsFalse(relay.Contains(listener));
        }

        [Test]
        public void RelayBase_RemoveListener_NotAdded_ReturnsFalse()
        {
            var owner = new TestOwner();
            var relay = new Relay(owner);
            Action listener = TestAction;
            bool removed = relay.RemoveListener(listener);
            Assert.IsFalse(removed);
        }

        [Test]
        public void RelayBase_Contains_PositiveAndNegative()
        {
            var owner = new TestOwner();
            var relay = new Relay(owner);
            Action listener1 = TestAction;
            Action listener2 = () => { /* another */};

            relay.AddListener(listener1);
            Assert.IsTrue(relay.Contains(listener1));
            Assert.IsFalse(relay.Contains(listener2));
            relay.RemoveListener(listener1);
            Assert.IsFalse(relay.Contains(listener1));
        }

        [Test]
        public void RelayBase_RemoveAll_RemovesAllListeners()
        {
            var owner = new TestOwner();
            var relay = new Relay(owner, defaultSize: 2); // Start with capacity for a few
            Action listener1 = TestAction;
            Action listener2 = () => _actionT1Value = 100; // Another listener

            relay.AddListener(listener1);
            relay.AddListener(listener2);
            Assert.AreEqual(2, relay.listenerCount);

            relay.RemoveAll();
            // RelayBase.RemoveAll sets slots to null. listenerCount is not reset by RemoveAll itself.
            // Compaction happens during Dispatch or Expand.
            // The primary check is that Contains is false and dispatch doesn't happen.
            Assert.IsFalse(relay.Contains(listener1));
            Assert.IsFalse(relay.Contains(listener2));
            
            // Check listenerCount after a dispatch (which should compact)
            ((Relay)relay).Dispatch(); // Cast to concrete type to Dispatch
            Assert.AreEqual(0, relay.listenerCount, "Listener count should be 0 after RemoveAll and Dispatch.");
        }

        [Test]
        public void RelayBase_Expand_IncreasesCapacityAndRetainsListeners()
        {
            var owner = new TestOwner();
            var relay = new Relay(owner, defaultSize: 1); // Start with size 1
            Action listener1 = TestAction;
            Action listener2 = () => _actionT1Value = 200;

            var sub1 = relay.AddListener(listener1); // count = 1, cap = 1
            Assert.AreEqual(1, relay.listenerCount);

            var sub2 = relay.AddListener(listener2); // Triggers Expand. count = 2, cap = 2
            Assert.IsNotNull(sub2);
            Assert.AreEqual(2, relay.listenerCount);
            Assert.IsTrue(relay.Contains(listener1));
            Assert.IsTrue(relay.Contains(listener2));

            // Verify they are callable after expand
            _actionCalled = false;
            _actionT1Value = 0;
            ((Relay)relay).Dispatch();
            Assert.IsTrue(_actionCalled);
            Assert.AreEqual(200, _actionT1Value);
            
            sub1.Unsubscribe();
            sub2.Unsubscribe();
        }

        [Test]
        public void Relay_Dispatch_NoListeners_DoesNotThrow()
        {
            var owner = new TestOwner();
            var relay = new Relay(owner);
            Assert.DoesNotThrow(() => relay.Dispatch());
        }

        [Test]
        public void Relay_Dispatch_SingleListener_IsCalled()
        {
            var owner = new TestOwner();
            var relay = new Relay(owner);
            _actionCalled = false; // Reset from Setup
            var sub = relay.AddListener(TestAction);

            relay.Dispatch();
            Assert.IsTrue(_actionCalled, "Listener should have been called.");
            sub.Unsubscribe();
        }

        [Test]
        public void Relay_Dispatch_MultipleListeners_AllCalled()
        {
            var owner = new TestOwner();
            var relay = new Relay(owner);
            bool listener1Called = false;
            Action listener1 = () => listener1Called = true;
            bool listener2Called = false;
            Action listener2 = () => listener2Called = true;

            var sub1 = relay.AddListener(listener1);
            var sub2 = relay.AddListener(listener2);

            relay.Dispatch();
            Assert.IsTrue(listener1Called, "Listener 1 should have been called.");
            Assert.IsTrue(listener2Called, "Listener 2 should have been called.");
            
            sub1.Unsubscribe();
            sub2.Unsubscribe();
        }

        [Test]
        public void Relay_Dispatch_ListenerRemoved_NotCalled()
        {
            var owner = new TestOwner();
            var relay = new Relay(owner);
            _actionCalled = false; // For listener1 (TestAction)
            bool listener2Called = false;
            Action listener2 = () => listener2Called = true;

            var sub1 = relay.AddListener(TestAction);
            var sub2 = relay.AddListener(listener2);

            relay.RemoveListener(TestAction); // Remove listener1

            relay.Dispatch();
            Assert.IsFalse(_actionCalled, "Removed listener (TestAction) should not have been called.");
            Assert.IsTrue(listener2Called, "Remaining listener (listener2) should have been called.");
            
            sub2.Unsubscribe();
        }

        [Test]
        public void Relay_Dispatch_AfterRemoveAll_NoListenersCalled()
        {
            var owner = new TestOwner();
            var relay = new Relay(owner);
            _actionCalled = false;
            relay.AddListener(TestAction);
            
            relay.RemoveAll();
            relay.Dispatch();
            Assert.IsFalse(_actionCalled, "Listener should not be called after RemoveAll.");
        }

        [Test]
        public void Relay_Dispatch_SubscriptionUnsubscribed_ListenerNotCalled()
        {
            var owner = new TestOwner();
            var relay = new Relay(owner);
            _actionCalled = false;
            var sub = relay.AddListener(TestAction);

            sub.Unsubscribe(); // Unsubscribe via the subscription object

            relay.Dispatch();
            Assert.IsFalse(_actionCalled, "Listener should not be called after its subscription is unsubscribed.");
        }

        [Test]
        public void Relay_Dispatch_CompactsAndUpdatesCountForCollectedListeners()
        {
            var owner = new TestOwner();
            var relay = new Relay(owner, defaultSize: 2); // Start with capacity for a few

            // Listener 1: Will be strongly referenced by the test
            bool liveListenerCalled = false;
            Action liveListener = () => liveListenerCalled = true;
            var liveSub = relay.AddListener(liveListener);
            
            // Listener 2: Attempt to make it eligible for GC
            AddWeaklyReferencedListener(relay); 

            Assert.AreEqual(2, relay.listenerCount, "Initial listener count should be 2.");

            // Force GC
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect(); 

            liveListenerCalled = false;
            relay.Dispatch();

            Assert.IsTrue(liveListenerCalled, "Strongly referenced listener should still be called.");
            Assert.AreEqual(1, relay.listenerCount, "Listener count should be 1 after dispatch collected a weak ref.");
            
            liveSub.Unsubscribe();
        }

        // Helper method to add a listener that can be GC'd
        private void AddWeaklyReferencedListener(Relay relay)
        {
            Action gcCandidateListener = () => { /* This should not be called if GC'd */ Assert.Fail("GC Candidate Listener was called."); };
            var sub = relay.AddListener(gcCandidateListener);
            // By not storing 'sub' or 'gcCandidateListener' as fields or returning them,
            // they become candidates for GC when this method scope is exited,
            // assuming the Relay's WeakReference is the only thing keeping them (indirectly).
        }

        [Test]
        public void RelayT_Dispatch_NoListeners_DoesNotThrow()
        {
            var owner = new TestOwner();
            var relay = new Relay<int>(owner);
            Assert.DoesNotThrow(() => relay.Dispatch(123));
        }

        [Test]
        public void RelayT_Dispatch_SingleListener_IsCalledWithCorrectArg()
        {
            var owner = new TestOwner();
            var relay = new Relay<int>(owner);
            _actionT1Value = 0; // Reset from Setup
            var sub = relay.AddListener(TestActionT1);
            int testValue = 42;

            relay.Dispatch(testValue);
            Assert.AreEqual(testValue, _actionT1Value, "Listener should have been called with the correct argument.");
            sub.Unsubscribe();
        }

        [Test]
        public void RelayT_Dispatch_MultipleListeners_AllCalledWithCorrectArg()
        {
            var owner = new TestOwner();
            var relay = new Relay<int>(owner);
            int listener1Value = 0;
            Action<int> listener1 = val => listener1Value = val;
            int listener2Value = 0;
            Action<int> listener2 = val => listener2Value = val;
            int testValue = 77;

            var sub1 = relay.AddListener(listener1);
            var sub2 = relay.AddListener(listener2);

            relay.Dispatch(testValue);
            Assert.AreEqual(testValue, listener1Value, "Listener 1 should have been called with the correct argument.");
            Assert.AreEqual(testValue, listener2Value, "Listener 2 should have been called with the correct argument.");
            
            sub1.Unsubscribe();
            sub2.Unsubscribe();
        }

        [Test]
        public void RelayT_Dispatch_ListenerRemoved_NotCalled()
        {
            var owner = new TestOwner();
            var relay = new Relay<int>(owner);
            _actionT1Value = 0; // For listener1 (TestActionT1)
            int listener2Value = 0;
            Action<int> listener2 = val => listener2Value = val;
            int testValue = 88;

            var sub1 = relay.AddListener(TestActionT1);
            var sub2 = relay.AddListener(listener2);

            relay.RemoveListener(TestActionT1); // Remove listener1

            relay.Dispatch(testValue);
            Assert.AreEqual(0, _actionT1Value, "Removed listener (TestActionT1) should not have been called.");
            Assert.AreEqual(testValue, listener2Value, "Remaining listener (listener2) should have been called.");
            
            sub2.Unsubscribe();
        }

        [Test]
        public void RelayT_Dispatch_AfterRemoveAll_NoListenersCalled()
        {
            var owner = new TestOwner();
            var relay = new Relay<int>(owner);
            _actionT1Value = 0;
            relay.AddListener(TestActionT1);
            
            relay.RemoveAll();
            relay.Dispatch(100);
            Assert.AreEqual(0, _actionT1Value, "Listener should not be called after RemoveAll.");
        }

        [Test]
        public void RelayT_Dispatch_SubscriptionUnsubscribed_ListenerNotCalled()
        {
            var owner = new TestOwner();
            var relay = new Relay<int>(owner);
            _actionT1Value = 0;
            var sub = relay.AddListener(TestActionT1);

            sub.Unsubscribe();

            relay.Dispatch(200);
            Assert.AreEqual(0, _actionT1Value, "Listener should not be called after its subscription is unsubscribed.");
        }

        [Test]
        public void RelayT_Dispatch_CompactsAndUpdatesCountForCollectedListeners()
        {
            var owner = new TestOwner();
            var relay = new Relay<int>(owner, defaultSize: 2);

            int liveListenerReceivedValue = 0;
            Action<int> liveListener = val => liveListenerReceivedValue = val;
            var liveSub = relay.AddListener(liveListener);
            
            AddWeaklyReferencedListenerT(relay);

            Assert.AreEqual(2, relay.listenerCount, "Initial listener count should be 2.");

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            liveListenerReceivedValue = 0;
            int dispatchValue = 333;
            relay.Dispatch(dispatchValue);

            Assert.AreEqual(dispatchValue, liveListenerReceivedValue, "Strongly referenced listener should still be called with correct value.");
            Assert.AreEqual(1, relay.listenerCount, "Listener count should be 1 after dispatch collected a weak ref.");
            
            liveSub.Unsubscribe();
        }

        // Helper method for Relay<T>
        private void AddWeaklyReferencedListenerT(Relay<int> relay)
        {
            Action<int> gcCandidateListener = val => { /* This should not be called if GC'd */ Assert.Fail($"GC Candidate Listener for Relay<T> was called with {val}."); };
            var sub = relay.AddListener(gcCandidateListener);
        }

        [Test]
        public void RelayT1T2_Dispatch_NoListeners_DoesNotThrow()
        {
            var owner = new TestOwner();
            var relay = new Relay<string, int>(owner);
            Assert.DoesNotThrow(() => relay.Dispatch("test", 123));
        }

        [Test]
        public void RelayT1T2_Dispatch_SingleListener_IsCalledWithCorrectArgs()
        {
            var owner = new TestOwner();
            var relay = new Relay<string, int>(owner);
            // Reset from Setup is implicit via [SetUp]
            var sub = relay.AddListener(TestActionT1T2);
            string testVal1 = "hello";
            int testVal2 = 42;

            relay.Dispatch(testVal1, testVal2);
            Assert.AreEqual(testVal1, _actionT1T2_Val1, "Listener should have been called with the correct string argument.");
            Assert.AreEqual(testVal2, _actionT1T2_Val2, "Listener should have been called with the correct int argument.");
            sub.Unsubscribe();
        }

        [Test]
        public void RelayT1T2_Dispatch_MultipleListeners_AllCalledWithCorrectArgs()
        {
            var owner = new TestOwner();
            var relay = new Relay<string, int>(owner);
            string l1Val1 = null; int l1Val2 = 0;
            Action<string, int> listener1 = (s, i) => { l1Val1 = s; l1Val2 = i; };
            string l2Val1 = null; int l2Val2 = 0;
            Action<string, int> listener2 = (s, i) => { l2Val1 = s; l2Val2 = i; };
            
            string testDispatchVal1 = "dispatch";
            int testDispatchVal2 = 77;

            var sub1 = relay.AddListener(listener1);
            var sub2 = relay.AddListener(listener2);

            relay.Dispatch(testDispatchVal1, testDispatchVal2);
            Assert.AreEqual(testDispatchVal1, l1Val1);
            Assert.AreEqual(testDispatchVal2, l1Val2);
            Assert.AreEqual(testDispatchVal1, l2Val1);
            Assert.AreEqual(testDispatchVal2, l2Val2);
            
            sub1.Unsubscribe();
            sub2.Unsubscribe();
        }

        [Test]
        public void RelayT1T2_Dispatch_ListenerRemoved_NotCalled()
        {
            var owner = new TestOwner();
            var relay = new Relay<string, int>(owner);
            // _actionT1T2_Val1/2 for TestActionT1T2 are reset by Setup
            string l2Val1 = null; int l2Val2 = 0;
            Action<string, int> listener2 = (s, i) => { l2Val1 = s; l2Val2 = i; };
            
            string testDispatchVal1 = "removedTest";
            int testDispatchVal2 = 88;

            var sub1 = relay.AddListener(TestActionT1T2);
            var sub2 = relay.AddListener(listener2);

            relay.RemoveListener(TestActionT1T2); 

            relay.Dispatch(testDispatchVal1, testDispatchVal2);
            Assert.IsNull(_actionT1T2_Val1, "Removed listener (TestActionT1T2) string arg should be null.");
            Assert.AreEqual(0, _actionT1T2_Val2, "Removed listener (TestActionT1T2) int arg should be 0.");
            Assert.AreEqual(testDispatchVal1, l2Val1);
            Assert.AreEqual(testDispatchVal2, l2Val2);
            
            sub2.Unsubscribe();
        }

        [Test]
        public void RelayT1T2_Dispatch_AfterRemoveAll_NoListenersCalled()
        {
            var owner = new TestOwner();
            var relay = new Relay<string, int>(owner);
            relay.AddListener(TestActionT1T2); // _actionT1T2_Val1/2 are reset by Setup
            
            relay.RemoveAll();
            relay.Dispatch("afterRemove", 100);
            Assert.IsNull(_actionT1T2_Val1);
            Assert.AreEqual(0, _actionT1T2_Val2);
        }

        [Test]
        public void RelayT1T2_Dispatch_SubscriptionUnsubscribed_ListenerNotCalled()
        {
            var owner = new TestOwner();
            var relay = new Relay<string, int>(owner);
            var sub = relay.AddListener(TestActionT1T2); // _actionT1T2_Val1/2 are reset by Setup

            sub.Unsubscribe();

            relay.Dispatch("unsubscribed", 200);
            Assert.IsNull(_actionT1T2_Val1);
            Assert.AreEqual(0, _actionT1T2_Val2);
        }

        [Test]
        public void RelayT1T2_Dispatch_CompactsAndUpdatesCountForCollectedListeners()
        {
            var owner = new TestOwner();
            var relay = new Relay<string, int>(owner, defaultSize: 2);

            string liveListenerStrVal = null;
            int liveListenerIntVal = 0;
            Action<string, int> liveListener = (s, i) => { liveListenerStrVal = s; liveListenerIntVal = i; };
            var liveSub = relay.AddListener(liveListener);
            
            AddWeaklyReferencedListenerT1T2(relay);

            Assert.AreEqual(2, relay.listenerCount, "Initial listener count should be 2.");

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            string dispatchStr = "gcDispatch";
            int dispatchInt = 444;
            relay.Dispatch(dispatchStr, dispatchInt);

            Assert.AreEqual(dispatchStr, liveListenerStrVal);
            Assert.AreEqual(dispatchInt, liveListenerIntVal);
            Assert.AreEqual(1, relay.listenerCount, "Listener count should be 1 after dispatch collected a weak ref.");
            
            liveSub.Unsubscribe();
        }

        // Helper method for Relay<T1, T2>
        private void AddWeaklyReferencedListenerT1T2(Relay<string, int> relay)
        {
            Action<string, int> gcCandidateListener = (s, i) => { /* This should not be called if GC'd */ Assert.Fail($"GC Candidate Listener for Relay<T1,T2> was called with {s}, {i}."); };
            var sub = relay.AddListener(gcCandidateListener);
        }
    }
}
