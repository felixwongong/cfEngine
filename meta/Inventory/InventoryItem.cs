using System;
using cfEngine.Meta;

[Serializable]
public class InventoryItem
{
    public readonly string Id;
    public readonly InventoryConfigInfo Config;

    private int _count;
    
    public InventoryItem(string id)
    {
        Id = id;
        Config = Game.Info.Get<InventoryConfigInfoManager>().GetOrDefault(Id);
    }

    public void Gain(int count)
    {
        _count += count;
    }
}
