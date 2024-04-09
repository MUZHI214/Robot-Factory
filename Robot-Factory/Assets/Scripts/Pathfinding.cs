using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private GridSystem<PathNode> grid;
    private List<PathNode> openList;
    private List<PathNode> closedList;


    public Pathfinding(int width, int height)
    {
        // set up grid
        grid = new GridSystem<PathNode>(width, height, 10f, Vector3.zero,(GridSystem<PathNode> grid, int x, int y) => new PathNode(grid,x,y));
    }


    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        // Get the coordinates of the current location
        PathNode startNode = grid.GetValue(startX,startY);
        // Get the coordinates of the target location
        PathNode endNode = grid.GetValue(endX, endY);
        //set up OpenList and ClosedList
        openList = new List<PathNode> { startNode };        // openList.Add(startNode);
        closedList = new List<PathNode>();

        // 
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode pathNode = grid.GetValue(x,y);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistance(startNode,endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                // Reached final node
                return CalculatePath(endNode);
            }
            // current node has already been searched
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                // check if th neighbor node is already on the Closed List
                if (closedList.Contains(neighbourNode)) continue;

                int tentativeGCost = currentNode.gCost + CalculateDistance(currentNode,neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistance(neighbourNode,endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        
        }

        // Out of nodes on the Open List       
        return null;
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();
        
        if (currentNode.x -1 >= 0) {
            // Left
            neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
            // Left Down
            if (currentNode.y - 1 >= 0)
            {
                neighbourList.Add(GetNode(currentNode.x -1, currentNode.y -1));
            }
            // Left Up
            if (currentNode.y + 1 < grid.GetHeight())
            {
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
            }
        }
        if (currentNode.x + 1 < grid.GetWidth())
        {
            // Right
            neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
            // Right Down
            if (currentNode.y - 1 >= 0)
            {
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
            }
            // Right Up
            if (currentNode.y + 1 < grid.GetHeight())
            {
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
            }
        }
        // Down
        if (currentNode.y - 1 >= 0) {
            neighbourList.Add(GetNode(currentNode.x, currentNode.y -1));
        }
        // Up
        if (currentNode.y + 1 < grid.GetHeight())
        {
            neighbourList.Add(GetNode(currentNode.x, currentNode.y +1));
        }

        return neighbourList;
    }

   

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistance(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    public GridSystem<PathNode> GetGrid()
    {
        return grid;
    }

    private PathNode GetNode(int x, int y)
    {
        return grid.GetValue(x,y);
    }
    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i =1; i < pathNodeList.Count; i ++)
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }
}
