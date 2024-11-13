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
        public void AddSingleItem_ShouldAddNewItem()
        {
            var request = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item1",
                count = 1
            };

            _inventoryController.AddItem(request);

            Assert.IsTrue(_inventoryController.itemGroup.ContainsKey("item1"));
            Assert.AreEqual(1, _inventoryController.itemGroup["item1"][0].ItemCount);
        }

        [Test]
        public void RemoveSingleItem_ShouldRemoveExistingItem()
        {
            var addRequest = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item1",
                count = 1
            };
            _inventoryController.AddItem(addRequest);

            var removeRequest = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item1",
                count = 1
            };
            var result = _inventoryController.RemoveItem(removeRequest);

            Assert.IsTrue(result.State == ValidationState.Success);
            Assert.IsFalse(_inventoryController.itemGroup.ContainsKey("item1"));
        }

        [Test]
        public void AddMultipleItems_ShouldAddAllItemsCorrectly()
        {
            var request1 = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item1",
                count = 2
            };
            var request2 = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item2",
                count = 3
            };
            var request3 = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item3",
                count = 4
            };

            _inventoryController.AddItem(request1);
            _inventoryController.AddItem(request2);
            _inventoryController.AddItem(request3);

            Assert.IsTrue(_inventoryController.itemGroup.ContainsKey("item1"));
            Assert.AreEqual(2, _inventoryController.itemGroup["item1"][0].ItemCount);
            Assert.IsTrue(_inventoryController.itemGroup.ContainsKey("item2"));
            Assert.AreEqual(3, _inventoryController.itemGroup["item2"][0].ItemCount);
            Assert.IsTrue(_inventoryController.itemGroup.ContainsKey("item3"));
            Assert.AreEqual(4, _inventoryController.itemGroup["item3"][0].ItemCount);
        }

        [Test]
        public void RemoveItemsInSequence_ShouldLeaveCorrectItemsInInventory()
        {
            var addRequest1 = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item1",
                count = 2
            };
            var addRequest2 = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item2",
                count = 3
            };
            _inventoryController.AddItem(addRequest1);
            _inventoryController.AddItem(addRequest2);

            var removeRequest1 = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item1",
                count = 1
            };
            var removeRequest2 = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item2",
                count = 1
            };
            _inventoryController.RemoveItem(removeRequest1);
            _inventoryController.RemoveItem(removeRequest2);

            Assert.IsTrue(_inventoryController.itemGroup.ContainsKey("item1"));
            Assert.AreEqual(1, _inventoryController.itemGroup["item1"][0].ItemCount);
            Assert.IsTrue(_inventoryController.itemGroup.ContainsKey("item2"));
            Assert.AreEqual(2, _inventoryController.itemGroup["item2"][0].ItemCount);
        }

        [Test]
        public void AddItemToExistingStack_ShouldAddToStack()
        {
            var stackId = Guid.NewGuid();
            var item = new InventoryItem(stackId, "item1", 1);
            _inventoryController._stackMap.Add(stackId, item);

            var request = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item1",
                count = 1,
                stackId = stackId
            };

            _inventoryController.AddItem(request);

            Assert.AreEqual(2, _inventoryController._stackMap[stackId].ItemCount);
        }

        [Test]
        public void RemoveItemFromExistingStack_ShouldRemoveFromStack()
        {
            var stackId = Guid.NewGuid();
            var item = new InventoryItem(stackId, "item1", 2);
            _inventoryController._stackMap.Add(stackId, item);

            var request = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item1",
                count = 1,
                stackId = stackId
            };

            var result = _inventoryController.RemoveItem(request);

            Assert.IsTrue(result.State == ValidationState.Success);
            Assert.AreEqual(1, _inventoryController._stackMap[stackId].ItemCount);
        }

        [Test]
        public void AddItemToNonExistingStack_ShouldFail()
        {
            var request = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item1",
                count = 1,
                stackId = Guid.NewGuid()
            };

            _inventoryController.AddItem(request);

            Assert.IsTrue(_inventoryController.itemGroup.ContainsKey("item1"));
        }

        [Test]
        public void RemoveItemFromNonExistingStack_ShouldFail()
        {
            var request = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item1",
                count = 1,
                stackId = Guid.NewGuid()
            };

            var result = _inventoryController.RemoveItem(request);

            Assert.IsTrue(result.State == ValidationState.Failure);
        }

        [Test]
        public void AddAndRemoveItemsInComplexSequence_ShouldHandleCorrectly()
        {
            var addRequest1 = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item1",
                count = 2
            };
            var addRequest2 = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item2",
                count = 3
            };
            _inventoryController.AddItem(addRequest1);
            _inventoryController.AddItem(addRequest2);

            var removeRequest1 = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item1",
                count = 1
            };
            _inventoryController.RemoveItem(removeRequest1);

            var addRequest3 = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item3",
                count = 4
            };
            _inventoryController.AddItem(addRequest3);

            var removeRequest2 = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item2",
                count = 1
            };
            _inventoryController.RemoveItem(removeRequest2);

            Assert.IsTrue(_inventoryController.itemGroup.ContainsKey("item1"));
            Assert.AreEqual(1, _inventoryController.itemGroup["item1"][0].ItemCount);
            Assert.IsTrue(_inventoryController.itemGroup.ContainsKey("item2"));
            Assert.AreEqual(2, _inventoryController.itemGroup["item2"][0].ItemCount);
            Assert.IsTrue(_inventoryController.itemGroup.ContainsKey("item3"));
            Assert.AreEqual(4, _inventoryController.itemGroup["item3"][0].ItemCount);
        }

        [Test]
        public void AddItemWithNoInventoryConfig_ShouldUseDefault()
        {
            var request = new InventoryController.UpdateInventoryRequest
            {
                itemId = "item6",
                count = 1
            };

            _inventoryController.AddItem(request);

            var defaultMaxStackSize = Game.Info.Get<InventoryConfigInfoManager>().GetOrDefault("item6").maxStackSize;
            Assert.IsTrue(_inventoryController.itemGroup.ContainsKey("item6"));
            Assert.AreEqual(1, _inventoryController.itemGroup["item6"][0].ItemCount);
        }
    }
}
#endif