using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Rock,
    Metal,
    Wood,
    Tool,
    StoneTool,
    WoodTool,
    MultiTool
}

public class Item
{
    public string Name { get; set; }

    public Item(ItemType type)
    {
        Name = type.ToString();
    }

    public static Dictionary<ItemType, Dictionary<ItemType, int>> recipes = new Dictionary<ItemType, Dictionary<ItemType, int>>()
    {
        [ItemType.Rock] = null,
        [ItemType.Metal] = null,
        [ItemType.Wood] = null,
        [ItemType.Tool] = new Dictionary<ItemType, int>() { [ItemType.Wood] = 2 },
        [ItemType.StoneTool] = new Dictionary<ItemType, int>() { [ItemType.Rock] = 2, [ItemType.Tool] = 1 },
        [ItemType.WoodTool] = new Dictionary<ItemType, int>() { [ItemType.Wood] = 1, [ItemType.Tool] = 1 },
        [ItemType.MultiTool] = new Dictionary<ItemType, int>() { [ItemType.WoodTool] = 1, [ItemType.StoneTool] = 1 },
    };
}
