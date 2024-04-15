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

    public Dictionary<ItemType, List<ItemMine>> itemMines;
    public Dictionary<ItemType, List<Factory>> factories;

    private Pathfinding pathfinding;

    public static FactoryManager Instance { get; private set; }


    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        itemMines = new Dictionary<ItemType, List<ItemMine>>();
        factories = new Dictionary<ItemType, List<Factory>>();

        foreach (var mine in allMines)
        {
            if (!itemMines.ContainsKey(mine.itemType))
                itemMines.Add(mine.itemType, new List<ItemMine>());

            itemMines[mine.itemType].Add(mine);
        }

        foreach (var factory in allFactories)
        {
            if (!factories.ContainsKey(factory.itemToProduce))
                factories.Add(factory.itemToProduce, new List<Factory>());

            factories[factory.itemToProduce].Add(factory);
        }

        pathfinding = new Pathfinding(10, 10);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
