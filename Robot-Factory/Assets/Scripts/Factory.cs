using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Factory : MonoBehaviour
{
    public ItemType itemToProduce = ItemType.TowerBase;
    public int producedNum = 0;

    public float craftTime = 2;
    float craftTimer = 0;

    public int numInProgress = 0;

    Dictionary<ItemType, int> recipe = new Dictionary<ItemType, int>();

    Dictionary<ItemType, int> heldItems = new Dictionary<ItemType, int>();

    Dictionary<ItemType, bool> hasEnoughItems = new Dictionary<ItemType, bool>();

    bool canCraft = false;

    [SerializeField]
    Slider progressBar;

    public Vector2 gridPositon = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        recipe = Item.recipes[itemToProduce];
        craftTimer = craftTime;

        progressBar.value = 0;
        progressBar.gameObject.SetActive(false);

        foreach (var pair in recipe)
        {
            hasEnoughItems.Add(pair.Key, false);
            heldItems.Add(pair.Key, 0);
        }

        var pathfinder = Pathfinding.Instance;

        var pos = pathfinder.GetGrid().GetWorldPosition(Mathf.FloorToInt(this.gridPositon.x), Mathf.FloorToInt(this.gridPositon.y));
        pos.x += pathfinder.GetGrid().GetCellSize() / 2;
        pos.y += pathfinder.GetGrid().GetCellSize() / 2;
        this.transform.position = pos;
    }


    // Update is called once per frame
    void Update()
    {
        if (recipe is null)
        {
            return;
        }

        if (canCraft)
        {
            if (craftTimer <= 0)
            {
                craftTimer = craftTime;
                foreach (var (itemType, numNeeded) in recipe)
                {
                    heldItems[itemType] -= numNeeded;
                    if (heldItems[itemType] < numNeeded)
                    {
                        hasEnoughItems[itemType] = false;
                        canCraft = false;
                    }
                }

                producedNum += 1;
                numInProgress--;
            }

            craftTimer -= Time.deltaTime;

            progressBar.value = (craftTime - craftTimer) / craftTime;
        }
        else
        {
            progressBar.gameObject.SetActive(false);
            progressBar.value = 0;
        }
    }

    public bool AddItems(ItemType itemType, int amount)
    {
        Debug.Log(itemType + ": " + amount);
        // Check if the recipe needs this item
        if (recipe.ContainsKey(itemType) && amount > 0)
        {
            // If it's needed then add it to the factory's inventory
            heldItems[itemType] += amount;
            if (heldItems[itemType] >= recipe[itemType])
                hasEnoughItems[itemType] = true;

            canCraft = true;
            foreach (var (_, hasEnough) in hasEnoughItems)
            {
                if (!hasEnough)
                {
                    canCraft = false;
                    break;
                }
            }

            if (canCraft)
            {
                numInProgress++;
                progressBar.gameObject.SetActive(true);
            }

            return true;
        }

        // If it's not needed then the adding failed
        return false;
    }

    public int GetNumProduced(int numToGet)
    {
        int num = Math.Min(producedNum, numToGet);
        producedNum -= num;
        return num;
    }
}
