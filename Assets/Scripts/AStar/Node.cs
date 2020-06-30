using UnityEngine;

public class Node : IHeapItem<Node>
{
    public Vector2Int gridPosition;
    public int gCost = 0; // distance from starting node
    public int hCost = 0; // distance from finishing node
    public bool isObstacle = false;
    public int movementPenalty;
    public Node parentNode;
    public GameObject displayCosts;

    private int _heapIndex;

    public Node(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;

        parentNode = null;

    }

    public int FCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get
        {
            return _heapIndex;
        }
        set
        {
            _heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = FCost.CompareTo(nodeToCompare.FCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}