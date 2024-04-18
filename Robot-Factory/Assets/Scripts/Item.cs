using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Rock,
    Wood,
    TowerBase,
    TowerBarrel,
    TowerProjectile,
    Tower
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
        [ItemType.Wood] = null,
        [ItemType.TowerBase] = new Dictionary<ItemType, int>() { [ItemType.Wood] = 2 },
        [ItemType.TowerBarrel] = new Dictionary<ItemType, int>() { [ItemType.Rock] = 2, [ItemType.Wood] = 1 },
        [ItemType.TowerProjectile] = new Dictionary<ItemType, int>() { [ItemType.Rock] = 3 },
        [ItemType.Tower] = new Dictionary<ItemType, int>() { [ItemType.TowerBase] = 1, [ItemType.TowerBarrel] = 1, [ItemType.TowerProjectile] = 1 }
    };
}
