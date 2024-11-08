﻿using System;

[Serializable]
public class InventoryItem
{
    public readonly string Id;

    public readonly int ItemCount;
    
    public InventoryItem(string id, int itemCount)
    {
        Id = id;
        ItemCount = itemCount;
    }

    public InventoryItem CloneNewCount(int itemCount)
    {
        return new InventoryItem(Id, itemCount);
    }
}