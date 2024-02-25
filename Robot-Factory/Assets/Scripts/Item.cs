using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Rock,
    Metal,
    Wood,
    Tool,
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
        [ItemType.Tool] = new Dictionary<ItemType, int>() { [ItemType.Wood] = 5 },
    };
}
