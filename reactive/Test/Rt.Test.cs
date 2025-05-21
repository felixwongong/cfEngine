using NUnit.Framework;
using cfEngine.Rx;
using System;

namespace cfEngine.Rx.Tests
{
    [TestFixture]
    public class Rt_Test
    {
        [Test]
        public void Test_DefaultConstructor()
        {
            var rt = new Rt<int>();
            Assert.AreEqual(default(int), rt.Value);
        }

        [Test]
        public void Test_ValueConstructor()
        {
            var rtString = new Rt<string>("testValue");
            Assert.AreEqual("testValue", rtString.Value);

            var rtInt = new Rt<int>(123);
            Assert.AreEqual(123, rtInt.Value);
        }

        [Test]
        public void Test_Set_UpdatesValue()
        {
            var rt = new Rt<string>("initial");
            rt.Set("new");
            Assert.AreEqual("new", rt.Value);
        }

        [Test]
        public void Test_Set_TriggersUpdateEvent()
        {
            // Test with int (value type)
            var rtInt = new Rt<int>(10);
            int oldValueActualInt = -1;
            int newValueActualInt = -1;
            bool eventTriggeredInt = false;

            var updateSubInt = rtInt.Events.SubscribeOnUpdate((oldVal, newVal) =>
            {
                oldValueActualInt = oldVal;
                newValueActualInt = newVal;
                eventTriggeredInt = true;
            });

            rtInt.Set(20);

            Assert.IsTrue(eventTriggeredInt, "Event was not triggered for int.");
            Assert.AreEqual(10, oldValueActualInt, "Old value for int is incorrect.");
            Assert.AreEqual(20, newValueActualInt, "New value for int is incorrect.");

            // Test with string (reference type)
            var rtString = new Rt<string>("oldS");
            string oldValueActualString = null;
            string newValueActualString = null;
            bool eventTriggeredString = false;

            var updateSubString = rtString.Events.SubscribeOnUpdate((oldVal, newVal) =>
            {
                oldValueActualString = oldVal;
                newValueActualString = newVal;
                eventTriggeredString = true;
            });

            rtString.Set("newS");

            Assert.IsTrue(eventTriggeredString, "Event was not triggered for string.");
            Assert.AreEqual("oldS", oldValueActualString, "Old value for string is incorrect.");
            Assert.AreEqual("newS", newValueActualString, "New value for string is incorrect.");

            updateSubInt.Dispose();
            updateSubString.Dispose();
        }

        [Test]
        public void Test_SetNoTrigger_UpdatesValue()
        {
            var rt = new Rt<string>("initial");
            rt.SetNoTrigger("newNoTrigger");
            Assert.AreEqual("newNoTrigger", rt.Value);
        }

        [Test]
        public void Test_SetNoTrigger_DoesNotTriggerUpdateEvent()
        {
            // Test with int (value type)
            var rtInt = new Rt<int>(50);
            bool eventTriggeredInt = false;
            var subUpdateInt = rtInt.Events.SubscribeOnUpdate((_, __) => eventTriggeredInt = true);
            // Also test the general Subscribe just in case
            var subGeneralInt = rtInt.Events.Subscribe(_ => eventTriggeredInt = true);


            rtInt.SetNoTrigger(60);

            Assert.IsFalse(eventTriggeredInt, "Event was triggered for int with SetNoTrigger.");
            Assert.AreEqual(60, rtInt.Value, "Value was not updated for int with SetNoTrigger.");
            subUpdateInt.Dispose();
            subGeneralInt.Dispose();

            // Test with string (reference type)
            var rtString = new Rt<string>("oldS_noTrigger");
            bool eventTriggeredString = false;
            var subUpdateString = rtString.Events.SubscribeOnUpdate((_, __) => eventTriggeredString = true);
            // Also test the general Subscribe just in case
            var subGeneralString = rtString.Events.Subscribe(_ => eventTriggeredString = true);

            rtString.SetNoTrigger("newS_noTrigger");

            Assert.IsFalse(eventTriggeredString, "Event was triggered for string with SetNoTrigger.");
            Assert.AreEqual("newS_noTrigger", rtString.Value, "Value was not updated for string with SetNoTrigger.");
            subUpdateString.Dispose();
            subGeneralString.Dispose();
        }

        public class MockDisposable : System.IDisposable
        {
            public bool IsDisposed { get; private set; } = false;
            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        [Test]
        public void Test_Dispose_TriggersDisposeEvent()
        {
            var rt = new Rt<int>();
            bool disposeEventTriggered = false;
            var disposeSub = rt.Events.SubscribeOnDispose(() => disposeEventTriggered = true);
            rt.Dispose();
            Assert.IsTrue(disposeEventTriggered, "Dispose event was not triggered.");
            disposeSub.Dispose();
        }

        [Test]
        public void Test_Dispose_DisposesDisposableValue()
        {
            var mockDisposable = new MockDisposable();
            var rt = new Rt<MockDisposable>(mockDisposable);
            rt.Dispose();
            Assert.IsTrue(mockDisposable.IsDisposed, "Disposable value was not disposed.");
        }

        [Test]
        public void Test_Dispose_HandlesNonDisposableValue()
        {
            var rt = new Rt<int>(123);
            rt.Dispose();
            Assert.Pass("Dispose called on non-disposable value without error.");
        }

        public class MyTestClass
        {
            public int Id { get; set; }
            // Optional: Override Equals and GetHashCode if you were using Assert.AreEqual for object instances
            // For Assert.AreSame, this is not strictly necessary.
        }

        [Test]
        public void Test_ImplicitConversion_ReturnsValue()
        {
            // String Test
            var rtString = new Rt<string>("helloConversion");
            string resultString = rtString;
            Assert.AreEqual("helloConversion", resultString, "String implicit conversion failed.");

            // Integer Test
            var rtInt = new Rt<int>(12345);
            int resultInt = rtInt;
            Assert.AreEqual(12345, resultInt, "Integer implicit conversion failed.");

            // Null Test (for reference types)
            var rtNull = new Rt<string>(null);
            string resultNull = rtNull;
            Assert.IsNull(resultNull, "Null string implicit conversion failed.");

            // Object Test (custom class)
            var testObj = new MyTestClass { Id = 100 };
            var rtObject = new Rt<MyTestClass>(testObj);
            MyTestClass resultObject = rtObject;
            Assert.AreSame(testObj, resultObject, "Object implicit conversion did not return the same instance.");
            Assert.AreEqual(100, resultObject.Id, "Object's property via implicit conversion failed.");
        }

        [Test]
        public void Test_Count_IsAlwaysOne()
        {
            var rtInt = new Rt<int>(7);
            Assert.AreEqual(1, rtInt.Count, "Count should be 1 for Rt<int>.");

            var rtString = new Rt<string>("test");
            Assert.AreEqual(1, rtString.Count, "Count should be 1 for Rt<string>.");

            var rtNull = new Rt<string>(null);
            Assert.AreEqual(1, rtNull.Count, "Count should be 1 for Rt<string> with null value.");
        }

        [Test]
        public void Test_GetEnumerator_YieldsValue()
        {
            var rtString = new Rt<string>("enumTest");
            int itemsIterated = 0;
            foreach (var item in rtString)
            {
                itemsIterated++;
                Assert.AreEqual("enumTest", item, "Item from enumerator is incorrect.");
            }
            Assert.AreEqual(1, itemsIterated, "Should iterate exactly one item.");

            // Direct Enumerator Check
            var rtInt = new Rt<int>(99);
            using (var enumerator = rtInt.GetEnumerator())
            {
                Assert.IsTrue(enumerator.MoveNext(), "MoveNext should return true for the first item.");
                Assert.AreEqual(99, enumerator.Current, "Current item from enumerator is incorrect.");
                Assert.IsFalse(enumerator.MoveNext(), "MoveNext should return false after the first item.");
            }
        }

        [Test]
        public void Test_Indexer_AccessesValue()
        {
            var rtDouble = new Rt<double>(3.14159);
            Assert.AreEqual(3.14159, rtDouble[0], "Indexer at 0 failed.");
            Assert.AreEqual(3.14159, rtDouble[1], "Indexer at 1 should return Value as per implementation.");
            Assert.AreEqual(3.14159, rtDouble[-1], "Indexer at -1 should return Value as per implementation.");
            Assert.AreEqual(3.14159, rtDouble[100], "Indexer at 100 should return Value as per implementation.");
        }
    }
}
