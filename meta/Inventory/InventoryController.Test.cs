#if UNITY_EDITOR
using System;
using NUnit.Framework;
using cfEngine.Util;

namespace cfEngine.Meta.Inventory
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
            var request = new UpdateRequest
            {
                ItemId = "item1",
                Count = 1
            };
    
            _inventoryController.AddItem(request);
    
            Assert.IsTrue(_inventoryController.ItemStackGroup.ContainsKey("item1"));
            Assert.AreEqual(1, _inventoryController.ItemStackGroup["item1"][0].ItemCount);
        }
    
        [Test]
        public void RemoveSingleItem_ShouldRemoveExistingItem()
        {
            var addRequest = new UpdateRequest
            {
                ItemId = "item1",
                Count = 1
            };
            _inventoryController.AddItem(addRequest);
    
            var removeRequest = new UpdateRequest
            {
                ItemId = "item1",
                Count = 1
            };
            var result = _inventoryController.RemoveItem(removeRequest);
    
            Assert.IsTrue(result.State == ValidationState.Success);
            Assert.IsFalse(_inventoryController.ItemStackGroup.ContainsKey("item1"));
        }
    
        [Test]
        public void AddMultipleItems_ShouldAddAllItemsCorrectly()
        {
            var request1 = new UpdateRequest
            {
                ItemId = "item1",
                Count = 2
            };
            var request2 = new UpdateRequest
            {
                ItemId = "item2",
                Count = 3
            };
            var request3 = new UpdateRequest
            {
                ItemId = "item3",
                Count = 4
            };
    
            _inventoryController.AddItem(request1);
            _inventoryController.AddItem(request2);
            _inventoryController.AddItem(request3);
    
            Assert.IsTrue(_inventoryController.ItemStackGroup.ContainsKey("item1"));
            Assert.AreEqual(2, _inventoryController.ItemStackGroup["item1"][0].ItemCount);
            Assert.IsTrue(_inventoryController.ItemStackGroup.ContainsKey("item2"));
            Assert.AreEqual(3, _inventoryController.ItemStackGroup["item2"][0].ItemCount);
            Assert.IsTrue(_inventoryController.ItemStackGroup.ContainsKey("item3"));
            Assert.AreEqual(4, _inventoryController.ItemStackGroup["item3"][0].ItemCount);
        }
    
        [Test]
        public void RemoveItemsInSequence_ShouldLeaveCorrectItemsInInventory()
        {
            var addRequest1 = new UpdateRequest
            {
                ItemId = "item1",
                Count = 2
            };
            var addRequest2 = new UpdateRequest
            {
                ItemId = "item2",
                Count = 3
            };
            _inventoryController.AddItem(addRequest1);
            _inventoryController.AddItem(addRequest2);
    
            var removeRequest1 = new UpdateRequest
            {
                ItemId = "item1",
                Count = 1
            };
            var removeRequest2 = new UpdateRequest
            {
                ItemId = "item2",
                Count = 1
            };
            _inventoryController.RemoveItem(removeRequest1);
            _inventoryController.RemoveItem(removeRequest2);
    
            Assert.IsTrue(_inventoryController.ItemStackGroup.ContainsKey("item1"));
            Assert.AreEqual(1, _inventoryController.ItemStackGroup["item1"][0].ItemCount);
            Assert.IsTrue(_inventoryController.ItemStackGroup.ContainsKey("item2"));
            Assert.AreEqual(2, _inventoryController.ItemStackGroup["item2"][0].ItemCount);
        }
    
        [Test]
        public void AddItemToExistingStack_ShouldAddToStack()
        {
            var stackId = Guid.NewGuid();
            var item = new StackRecord(stackId, "item1", 1);
            _inventoryController._stackMap.Add(stackId, item);
    
            var request = new UpdateRequest
            {
                ItemId = "item1",
                Count = 1,
                StackId = stackId
            };
    
            _inventoryController.AddItem(request);
    
            Assert.AreEqual(2, _inventoryController._stackMap[stackId].ItemCount);
        }
    
        [Test]
        public void RemoveItemFromExistingStack_ShouldRemoveFromStack()
        {
            var stackId = Guid.NewGuid();
            var item = new StackRecord(stackId, "item1", 2);
            _inventoryController._stackMap.Add(stackId, item);
    
            var request = new UpdateRequest
            {
                ItemId = "item1",
                Count = 1,
                StackId = stackId
            };
    
            var result = _inventoryController.RemoveItem(request);
    
            Assert.IsTrue(result.State == ValidationState.Success);
            Assert.AreEqual(1, _inventoryController._stackMap[stackId].ItemCount);
        }
    
        [Test]
        public void AddItemToNonExistingStack_ShouldFail()
        {
            var request = new UpdateRequest
            {
                ItemId = "item1",
                Count = 1,
                StackId = Guid.NewGuid()
            };
    
            _inventoryController.AddItem(request);
    
            Assert.IsTrue(_inventoryController.ItemStackGroup.ContainsKey("item1"));
        }
    
        [Test]
        public void RemoveItemFromNonExistingStack_ShouldFail()
        {
            var request = new UpdateRequest
            {
                ItemId = "item1",
                Count = 1,
                StackId = Guid.NewGuid()
            };
    
            var result = _inventoryController.RemoveItem(request);
    
            Assert.IsTrue(result.State == ValidationState.Failure);
        }
    
        [Test]
        public void AddAndRemoveItemsInComplexSequence_ShouldHandleCorrectly()
        {
            var addRequest1 = new UpdateRequest
            {
                ItemId = "item1",
                Count = 2
            };
            var addRequest2 = new UpdateRequest
            {
                ItemId = "item2",
                Count = 3
            };
            _inventoryController.AddItem(addRequest1);
            _inventoryController.AddItem(addRequest2);
    
            var removeRequest1 = new UpdateRequest
            {
                ItemId = "item1",
                Count = 1
            };
            _inventoryController.RemoveItem(removeRequest1);
    
            var addRequest3 = new UpdateRequest
            {
                ItemId = "item3",
                Count = 4
            };
            _inventoryController.AddItem(addRequest3);
    
            var removeRequest2 = new UpdateRequest
            {
                ItemId = "item2",
                Count = 1
            };
            _inventoryController.RemoveItem(removeRequest2);
    
            Assert.IsTrue(_inventoryController.ItemStackGroup.ContainsKey("item1"));
            Assert.AreEqual(1, _inventoryController.ItemStackGroup["item1"][0].ItemCount);
            Assert.IsTrue(_inventoryController.ItemStackGroup.ContainsKey("item2"));
            Assert.AreEqual(2, _inventoryController.ItemStackGroup["item2"][0].ItemCount);
            Assert.IsTrue(_inventoryController.ItemStackGroup.ContainsKey("item3"));
            Assert.AreEqual(4, _inventoryController.ItemStackGroup["item3"][0].ItemCount);
        }
    
        [Test]
        public void AddItemWithNoInventoryConfig_ShouldUseDefault()
        {
            var request = new UpdateRequest
            {
                ItemId = "item6",
                Count = 1
            };
    
            _inventoryController.AddItem(request);
    
            var defaultMaxStackSize = Game.Info.Get<InventoryInfoManager>().GetOrDefault("item6").maxStackSize;
            Assert.IsTrue(_inventoryController.ItemStackGroup.ContainsKey("item6"));
            Assert.AreEqual(1, _inventoryController.ItemStackGroup["item6"][0].ItemCount);
        }
    }
}
#endif