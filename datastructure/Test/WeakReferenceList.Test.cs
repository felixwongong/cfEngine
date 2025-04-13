using System.Linq;
using NUnit.Framework;

namespace cfEngine.DataStructure.test
{
    [TestFixture]
    public class TestWeakReferenceList
    {
        public class TestObject
        {
        }
        
        [Test]
        public void Test_Create()
        {
            var list = WeakReferenceList<TestObject>.Create();
            Assert.IsNotNull(list);
        }
        
        [Test]
        public void Test_Add()
        {
            var list = WeakReferenceList<TestObject>.Create();
            var obj = new TestObject();
            list.Add(obj);
            Assert.AreEqual(1, list.Count);
            Assert.IsTrue(list.First().TryGetTarget(out var target));
            Assert.AreEqual(obj, target);
        }
    }
}