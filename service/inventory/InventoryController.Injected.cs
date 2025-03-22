namespace cfEngine.Service.Inventory
{
    public partial class InventoryService
    {
        private InventoryInfoManager InfoManager { get; set; }
        
        public struct InjectContent
        {
            public InventoryInfoManager InfoManager;
        }
        
        public void Inject(InjectContent content)
        {
            InfoManager = content.InfoManager;
        }
    }
}