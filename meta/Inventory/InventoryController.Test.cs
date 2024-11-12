#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using NUnit.Framework;
using cfEngine.Meta;
using cfEngine.Rt;
using cfEngine.Util;

namespace cfEngine.Meta
{
    [TestFixture]
    public partial class InventoryController
    {
        private InventoryController _inventoryController;

        [SetUp]
        public void SetUp()
        {
            _inventoryController = new InventoryController();
        }

        [Test]
        public void AddItem_ShouldAddNewItem()
        {
            var request = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item1",
                count = 10
            };

            _inventoryController.AddItem(request);

            Assert.IsTrue(_inventoryController.itemGroup.ContainsKey("item1"));
            Assert.AreEqual(10, _inventoryController.itemGroup["item1"][0].ItemCount);
        }

        [Test]
        public void RemoveItem_ShouldRemoveExistingItem()
        {
            var addRequest = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item1",
                count = 10
            };
            _inventoryController.AddItem(addRequest);

            var removeRequest = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item1",
                count = 5
            };
            var result = _inventoryController.RemoveItem(removeRequest);

            Assert.IsTrue(result.State == ValidationState.Success);
            Assert.AreEqual(5, _inventoryController.itemGroup["item1"][0].ItemCount);
        }

        [Test]
        public void TryAddToStack_ShouldAddToExistingStack()
        {
            var stackId = Guid.NewGuid();
            var item = new InventoryItem(stackId, "item1", 5);
            _inventoryController._stackMap.Add(stackId, item);

            var result = _inventoryController.TryAddToStack(stackId, 5, out var remain);

            Assert.IsTrue(result);
            Assert.AreEqual(0, remain);
            Assert.AreEqual(10, _inventoryController._stackMap[stackId].ItemCount);
        }

        [Test]
        public void TryRemoveFromStack_ShouldRemoveFromExistingStack()
        {
            var stackId = Guid.NewGuid();
            var item = new InventoryItem(stackId, "item1", 10);
            _inventoryController._stackMap.Add(stackId, item);

            var result = _inventoryController.TryRemoveFromStack(stackId, 5, out var remain);

            Assert.IsTrue(result);
            Assert.AreEqual(5, remain);
            Assert.AreEqual(5, _inventoryController._stackMap[stackId].ItemCount);
        }
    }
}
#endif
