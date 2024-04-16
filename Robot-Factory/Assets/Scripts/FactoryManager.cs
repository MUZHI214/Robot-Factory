using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryManager : MonoBehaviour
{
    public PlayerController player;
    public List<Robot> robots;

    [SerializeField]
    private List<ItemMine> allMines;
    [SerializeField]
    private List<Factory> allFactories;

    public Dictionary<ItemType, List<ItemMine>> itemMines = new Dictionary<ItemType, List<ItemMine>>();
    public Dictionary<ItemType, List<Factory>> factories = new Dictionary<ItemType, List<Factory>>();

    private Pathfinding pathfinding;

    public static FactoryManager Instance { get; private set; }


    // Start is called before the first frame update
    void Start()
    {
        pathfinding = new Pathfinding(10, 10);

        if (Instance is null)
            Instance = this;

        foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
        {
            itemMines.Add(itemType, new List<ItemMine>());
            factories.Add(itemType, new List<Factory>());
        }

        foreach (var mine in allMines)
        {
            itemMines[mine.itemType].Add(mine);
        }

        foreach (var factory in allFactories)
        {
            factories[factory.itemToProduce].Add(factory);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
