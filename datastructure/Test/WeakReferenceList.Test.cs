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
		}
		
		[Test]
		public void Test_Add()
		{
			var list = WeakReferenceList<TestObject>.Create();
			var obj = new TestObject();
			list.Add(obj);
		}
	}
}
