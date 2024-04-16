using System;
using UnityEngine;

public class ItemMine : MonoBehaviour
{
    public ItemType itemType;

    public float mineTime = 1.0f;

    public Vector2 gridPositon = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        var pathfinder = Pathfinding.Instance;

        var pos = pathfinder.GetGrid().GetWorldPosition(Mathf.FloorToInt(this.gridPositon.x), Mathf.FloorToInt(this.gridPositon.y));
        pos.x += pathfinder.GetGrid().GetCellSize() / 2;
        pos.y += pathfinder.GetGrid().GetCellSize() / 2;
        this.transform.position = pos;
    }

    // Update is called once per frame
    void Update()
    {
    }

}
