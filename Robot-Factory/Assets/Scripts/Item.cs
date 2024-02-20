using UnityEngine;

public enum ItemType
{
    Rock,
    Metal,
    Wood
}

public class Item
{
    public string Name { get; set; }

    public Item(ItemType type)
    {
        Name = type.ToString();
        Debug.Log(Name);
    }

}
