using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class AStar : MonoBehaviour
{
    [Header("Tiles & Tilemap References")]
    [Header("Options")]
    private GridNodes gridNodes;

    private Node startNode;
    private Node targetNode;
    private int gridWidth = 25;
    private int gridHeight = 16;
    private int originX;
    private int originY;
    private Camera mainCamera;
    [SerializeField] private Grid grid = null;
    [SerializeField] private Tilemap pathTilemap = null;
    [SerializeField] private Tilemap directionTilemap = null;
    [SerializeField] private TileBase startTile = null;
    [SerializeField] private TileBase finishTile = null;
    [SerializeField] private TileBase obstacleTile = null;
    [SerializeField] private TileBase blueTile = null;
    [SerializeField] private TileBase redTile = null;
    [SerializeField] private TileBase greenTile = null;
    [SerializeField] private TileBase arrowUpTile = null;
    [SerializeField] private TileBase arrowDownTile = null;
    [SerializeField] private TileBase arrowLeftTile = null;
    [SerializeField] private TileBase arrowRightTile = null;
    [SerializeField] private TileBase arrowUpRightTile = null;
    [SerializeField] private TileBase arrowUpLeftTile = null;
    [SerializeField] private TileBase arrowDownRightTile = null;
    [SerializeField] private TileBase arrowDownLeftTile = null;

    [SerializeField] private GameObject displayCostsPrefab = null;

    private List<Node> openNodeList;
    private HashSet<Node> closedNodeList;

    private bool findPathTriggered = false;

    private void Awake()
    {
        // Cache main camera
        mainCamera = Camera.main;

        // Create gridNodes instance
        gridNodes = new GridNodes(gridWidth, gridHeight);

        // Create open node list
        openNodeList = new List<Node>();

        // Create closed node list
        closedNodeList = new HashSet<Node>();
    }

    private void Update()
    {
        // Set start node
        if (Input.GetKeyDown(KeyCode.S) && findPathTriggered == false)
        {
            SetStartNode();
        }

        // Set finish node
        if (Input.GetKeyDown(KeyCode.F) && findPathTriggered == false)
        {
            SetTargetNode();
        }

        // Set Obstacle
        if (Input.GetKey(KeyCode.O) && findPathTriggered == false)
        {
            SetObstacle();
        }

        // Clear Obstacle
        if (Input.GetKey(KeyCode.P) && findPathTriggered == false)
        {
            ClearObstacle();
        }

        // calculate shortest path
        if (startNode != null && targetNode != null && Input.GetKeyDown(KeyCode.B) && findPathTriggered == false)
        {
            findPathTriggered = true;
            StartCoroutine(FindShorrtestPath());
        }

        // Display path back to start
        if (startNode != null && targetNode != null && Input.GetKey(KeyCode.D) && findPathTriggered == true)
        {
            // Display path
            DisplayPath();
        }

        // Clear path display back to start
        if (startNode != null && targetNode != null && Input.GetKeyUp(KeyCode.D) && findPathTriggered == true)
        {
            // Clear path
            ClearDisplayPath();
        }

        // Restart
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(0);

        //Quit
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Application.Quit();
        }
    }

    private Vector3Int GetTilePosition(Vector3 worldPosition)
    {
        // Convert world position to grid position
        return grid.WorldToCell(mainCamera.ScreenToWorldPoint(worldPosition));
    }

    // Set Start Node
    private void SetStartNode()
    {
        Vector3Int tilePosition = GetTilePosition(Input.mousePosition);

        if (!IsWithinTilemap(tilePosition))
            return;

        // Clear start node if already exists
        if (startNode != null)
        {
            // remove tile
            pathTilemap.SetTile(new Vector3Int(startNode.gridPosition.x, startNode.gridPosition.y, 0), null);

            // clear node
            ClearNode(startNode);
            startNode.displayCosts.GetComponent<DisplayCosts>().ClearText();
        }

        // Set Tilemap Tile
        pathTilemap.SetTile(tilePosition, startTile);

        // Set Node
        startNode = gridNodes.GetGridNode(tilePosition.x, tilePosition.y);
        startNode.displayCosts = Instantiate(displayCostsPrefab);
        startNode.displayCosts.transform.position = new Vector3(startNode.gridPosition.x, startNode.gridPosition.y, 0);
        DisplayCosts displayCosts = startNode.displayCosts.GetComponent<DisplayCosts>();
        displayCosts.ClearText();
        displayCosts.SetFCost("Start");
    }

    // Set Finish Node
    private void SetTargetNode()
    {
        Vector3Int tilePosition = GetTilePosition(Input.mousePosition);

        if (!IsWithinTilemap(tilePosition))
            return;

        // Clear finish node if already exists
        if (targetNode != null)
        {
            // remove tile
            pathTilemap.SetTile(new Vector3Int(targetNode.gridPosition.x, targetNode.gridPosition.y, 0), null);

            // clear node
            ClearNode(targetNode);
            targetNode.displayCosts.GetComponent<DisplayCosts>().ClearText();
        }

        // Set Tilemap Tile
        pathTilemap.SetTile(tilePosition, finishTile);

        // Set Node
        targetNode = gridNodes.GetGridNode(tilePosition.x, tilePosition.y);
        targetNode.displayCosts = Instantiate(displayCostsPrefab);
        targetNode.displayCosts.transform.position = new Vector3(targetNode.gridPosition.x, targetNode.gridPosition.y, 0);
        DisplayCosts displayCosts = targetNode.displayCosts.GetComponent<DisplayCosts>();
        displayCosts.ClearText();
        displayCosts.SetFCost("Finish");
    }

    // Set Obstacle
    private void SetObstacle()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3Int tilePosition = GetTilePosition(mousePosition);

        if (!IsWithinTilemap(tilePosition))
            return;

        // Get tile at mouse position
        TileBase tile = pathTilemap.GetTile(tilePosition);

        if (tile != obstacleTile)
        {
            // Set Tilemap Tile
            pathTilemap.SetTile(tilePosition, obstacleTile);

            // Set Node
            Node node = gridNodes.GetGridNode(tilePosition.x, tilePosition.y);
            node.isObstacle = true;
        }
    }

    // Clear Obstacle
    private void ClearObstacle()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3Int tilePosition = GetTilePosition(mousePosition);

        if (!IsWithinTilemap(tilePosition))
            return;

        // Get tile at mouse position
        TileBase tile = pathTilemap.GetTile(tilePosition);

        if (tile == obstacleTile)
        {
            // Clear Tile
            pathTilemap.SetTile(tilePosition, null);

            // Clear Node
            ClearNode(gridNodes.GetGridNode(tilePosition.x, tilePosition.y));
        }
    }

    private bool IsWithinTilemap(Vector3Int position)
    {
        if (position.x >= 0 && position.x < gridWidth && position.y >= 0 && position.y < gridHeight)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    ///  Returns true if a path has been found
    /// </summary>
    private IEnumerator FindShorrtestPath()
    {
        // Add start node to open list
        openNodeList.Add(startNode);

        // Loop through open node list until empty
        while (openNodeList.Count > 0)
        {
            // Sort List
            openNodeList.Sort();

            //  current node = the node in the open list with the lowest fCost
            Node currentNode = openNodeList[0];
            openNodeList.RemoveAt(0);

            // add current node to the closed list
            closedNodeList.Add(currentNode);

            // if the current node = target node
            //      then finish

            if (currentNode == targetNode)
            {
                break;
            }

            // evaluate fcost for each neighbour of the current node
            EvaluateCurrentNodeNeighbours(currentNode);

            // Colour open nodes
            ColourOpenNodes();

            // colour closed nodes
            ColourClosedNodes();

            do
            {
                yield return null;
            } while (!Input.GetKeyDown(KeyCode.Space) && !Input.GetKey(KeyCode.LeftAlt));
        }

        DisplayShortestPath();

    }

    private void ColourClosedNodes()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Node node = gridNodes.GetGridNode(x, y);

                if (node == targetNode || node == startNode || node.isObstacle)
                    continue;

                if (closedNodeList.Contains(node))
                {
                    // colour red
                    pathTilemap.SetTile(new Vector3Int(node.gridPosition.x, node.gridPosition.y, 0), redTile);
                }
            }
        }
    }

    private void ColourOpenNodes()
    {
        foreach (Node node in openNodeList)
        {
            if (node != null)
            {
                if (node == targetNode || node == startNode || node.isObstacle)
                    continue;

                // colour green
                pathTilemap.SetTile(new Vector3Int(node.gridPosition.x, node.gridPosition.y, 0), greenTile);
            }
        }
    }

    private void EvaluateCurrentNodeNeighbours(Node currentNode)
    {
        Vector2Int currentNodeGridPosition = currentNode.gridPosition;

        Node validNeighbourNode;

        // Loop through all directions
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                validNeighbourNode = GetValidNodeNeighbour(currentNodeGridPosition.x + i, currentNodeGridPosition.y + j);

                if (validNeighbourNode != null)
                {
                    // Calculate new gcost for neighbour
                    int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighbourNode);

                    if (newCostToNeighbour < validNeighbourNode.gCost || !openNodeList.Contains(validNeighbourNode))
                    {
                        validNeighbourNode.gCost = newCostToNeighbour;
                        validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);

                        validNeighbourNode.parentNode = currentNode;

                        if (!openNodeList.Contains(validNeighbourNode))
                        {
                            openNodeList.Add(validNeighbourNode);
                        }
                    }
                    // Display costs
                    DisplayCosts(validNeighbourNode);
                }
            }
        }
    }

    private void DisplayCosts(Node node)
    {
        if (node == targetNode || node == startNode || node.isObstacle)
            return;

        // Create gameobject to display costs
        if (node.displayCosts == null)
        {
            node.displayCosts = Instantiate(displayCostsPrefab);
        }
        node.displayCosts.transform.position = new Vector3(node.gridPosition.x, node.gridPosition.y, 0);
        DisplayCosts displayCosts = node.displayCosts.GetComponent<DisplayCosts>();
        displayCosts.ClearText();
        displayCosts.SetGCost(node.gCost.ToString());
        displayCosts.SetHCost(node.hCost.ToString());
        displayCosts.SetFCost((node.gCost + node.hCost).ToString());
    }

    private void DisplayPath()
    {
        ClearDisplayPath();

        Vector3 mousePosition = Input.mousePosition;
        Vector3Int tilePosition = GetTilePosition(mousePosition);

        // Check is within tilemap
        if (!IsWithinTilemap(tilePosition))
            return;

        // Loop through nodes and display appropriate tile
        // Get node at tileposition
        Node node = gridNodes.GetGridNode(tilePosition.x, tilePosition.y);

        // If node is start, finish, obstacle, or null then return
        if (node == null)
            return;

        if (node == startNode || node == targetNode || node.isObstacle == true)
            return;

        while (node.parentNode != null)
        {
            NodeDirection nodeDirection = GetNodeDirection(node, node.parentNode);

            SetDirectionTile(tilePosition, nodeDirection);

            node = node.parentNode;
            tilePosition = new Vector3Int(node.gridPosition.x, node.gridPosition.y, 0);
        }
    }

    private NodeDirection GetNodeDirection(Node startNode, Node targetNode)
    {
        if (targetNode.gridPosition.x < startNode.gridPosition.x)
        {
            if (targetNode.gridPosition.y < startNode.gridPosition.y)
            {
                return NodeDirection.DownLeft;
            }
            else if (targetNode.gridPosition.y > startNode.gridPosition.y)
            {
                return NodeDirection.UpLeft;
            }
            else return NodeDirection.Left;
        }

        if (targetNode.gridPosition.x > startNode.gridPosition.x)
        {
            if (targetNode.gridPosition.y < startNode.gridPosition.y)
            {
                return NodeDirection.DownRight;
            }
            else if (targetNode.gridPosition.y > startNode.gridPosition.y)
            {
                return NodeDirection.UpRight;
            }
            else return NodeDirection.Right;
        }

        if (targetNode.gridPosition.y < startNode.gridPosition.y)
        {
            return NodeDirection.Down;
        }
        else if (targetNode.gridPosition.y > startNode.gridPosition.y)
        {
            return NodeDirection.Up;
        }

        return NodeDirection.Same;
    }

    private void SetDirectionTile(Vector3Int tilePosition, NodeDirection nodeDirection)
    {
        switch (nodeDirection)
        {
            case NodeDirection.Up:
                directionTilemap.SetTile(tilePosition, arrowUpTile);
                break;

            case NodeDirection.Down:
                directionTilemap.SetTile(tilePosition, arrowDownTile);
                break;

            case NodeDirection.Right:
                directionTilemap.SetTile(tilePosition, arrowRightTile);
                break;

            case NodeDirection.Left:
                directionTilemap.SetTile(tilePosition, arrowLeftTile);
                break;

            case NodeDirection.UpLeft:
                directionTilemap.SetTile(tilePosition, arrowUpLeftTile);
                break;

            case NodeDirection.UpRight:
                directionTilemap.SetTile(tilePosition, arrowUpRightTile);
                break;

            case NodeDirection.DownRight:
                directionTilemap.SetTile(tilePosition, arrowDownRightTile);
                break;

            case NodeDirection.DownLeft:
                directionTilemap.SetTile(tilePosition, arrowDownLeftTile);
                break;

            default:
                break;
        }
    }

    private void ClearDisplayPath()
    {
        directionTilemap.ClearAllTiles();
        directionTilemap.RefreshAllTiles();
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    private Node GetValidNodeNeighbour(int neighboutNodeXPosition, int neighbourNodeYPosition)
    {
        // If neighbour node position is beyond grid then return null
        if (neighboutNodeXPosition >= gridWidth || neighboutNodeXPosition < 0 || neighbourNodeYPosition >= gridHeight || neighbourNodeYPosition < 0)
        {
            return null;
        }

        // if neighbour is an obstacle or neighbour is in the closed list then skip
        Node neighbourNode = gridNodes.GetGridNode(neighboutNodeXPosition, neighbourNodeYPosition);

        if (neighbourNode.isObstacle || closedNodeList.Contains(neighbourNode))
        {
            return null;
        }
        else
        {
            return neighbourNode;
        }
    }

    private void DisplayShortestPath()
    {
        //  Loop through all parent nodes from target node

        Node currentNode = targetNode.parentNode;

        while (currentNode != null)
        {
            if (currentNode != startNode)
            {
                // Set tile colour to blue
                pathTilemap.SetTile(new Vector3Int(currentNode.gridPosition.x, currentNode.gridPosition.y, 0), blueTile);
            }

            currentNode = currentNode.parentNode;
        }
    }

    private void ClearNode(Node node)
    {
        node.hCost = 0;
        node.gCost = 0;
        node.isObstacle = false;
        node.parentNode = null;
        node.movementPenalty = 0;
    }
}