using System.Collections.Generic;
using NUnit.Framework;

namespace cfEngine.Rt.Test
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

            rtList.Events.SubscribeOnDispose(() => disposed = true);
            rtList.Dispose();
            
            Assert.IsTrue(disposed);
            Assert.IsTrue(rtList.Count == 0);
        }

        [Test]
        public void RtList_Add()
        {
            var added = new List<int>(5);

            var rtList = new RtList<int>();
            for (int i = 0; i < 10; i++)
            {
                rtList.Add(i);
                if (i == 4)
                {
                    var sub = rtList.Events.SubscribeOnAdd(x =>
                    {
                        added.Add(x.item);
                    });
                }
            }

            for (int i = 5; i < 10; i++)
            {
                Assert.IsTrue(added.Contains(i));
            }
        }
    }
}