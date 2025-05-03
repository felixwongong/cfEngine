using System;
using System.Collections.Generic;
using cfEngine.Core;
using cfEngine.Service.Inventory;
using cfEngine.Util;
using ItemId = System.String;

namespace cfEngine.Core
{
    public static partial class ServiceName
    {
        public const string Inventory = "Inventory";
    }
    
    public static partial class GameExtension
    {
        public static Game WithInventory(this Game game, IInventoryService service)
        {
            game.Register(service, ServiceName.Inventory);
            return game;
        }
        
        public static InventoryService GetInventory(this Game game) => game.GetService<InventoryService>(ServiceName.Inventory);
    }
}

namespace cfEngine.Service.Inventory
{
    public interface IInventoryService : IModelService 
    {
    } 
    
    public partial class InventoryService : IInventoryService
    {
        private readonly InventoryModel _model;
        IServiceModel IModelService.GetModel => _model;

        public InventoryService(InventoryModel model)
        {
            _model = model;
        }

        public void Dispose()
        {
        }
    }
}