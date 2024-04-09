using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{

    private GridSystem<PathNode> grid;
    public int x;
    public int y;

    // Walking Cost from the Start Node
    public int gCost;
    // Heuristic Cost to reach End Node
    public int hCost;
    // final cost
    public int fCost;

    public PathNode cameFromNode;

    public PathNode(GridSystem<PathNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public override string ToString()
    {
        return x+","+y;
    }

}
