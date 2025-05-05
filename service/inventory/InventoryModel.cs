using System.Collections.Generic;
using cfEngine.Core;
using StackId = System.Guid;

namespace cfEngine.Core
{
    public partial class UserDataKey
    {
        public const string Inventory = "Inventory";
    }
}

namespace cfEngine.Service.Inventory
{
    public class InventoryModel: IServiceModel
    {
        public void Initialize(IUserData userData)
        {
        }

        public void SetSaveData(Dictionary<string, object> dataMap)
        {
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}