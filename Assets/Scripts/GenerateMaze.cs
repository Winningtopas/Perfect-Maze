using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMaze : MonoBehaviour
{
    #region Variables
    [Header("Maze Variables")]
    [SerializeField]
    [Range(10, 250)] private int columsCount, rowsCount = 10;

    [Header("Cell Variables")]
    public static List<Cell> cells = new List<Cell>();
    public static List<Cell> stack = new List<Cell>();
    public static List<GameObject> junkCells = new List<GameObject>();

    [SerializeField]
    private GameObject cellPrefab, cellContainer;
    private Cell currentCell, nextCell;
    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        SpawnMazeGrid();

        while (true)
        {
            MazeLogic();
            if (stack.Count == 0)
            {
                break;
            }
        }
    }

    private void SpawnMazeGrid()
    {
        // Add a cell for each row multiplied by columns
        for (int i = 0; i < rowsCount; i++)
        {
            for (int j = 0; j < columsCount; j++)
            {
                Cell cell = new Cell();

                // currently all cells are 1 unit, so this will place all cells next to each other
                cell.x = j;
                cell.z = i;

                cells.Add(cell);
            }
        }

        // Instantiate a visual element for the cellgrid and store it in the cell
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].cellPrefab = Instantiate(cellPrefab, new Vector3(cells[i].x, transform.position.y, cells[i].z), Quaternion.identity, cellContainer.transform);
        }

        currentCell = cells[Random.Range(0, cells.Count)];
    }

    private void MazeLogic()
    {
        currentCell.visited = true;

        nextCell = currentCell.CheckNeighbors(columsCount, rowsCount);

        // empty the neighbours of the current cell so we can start fresh with the search if we ever look for neighbours of this cell again
        currentCell.availableNeighbourCells = new List<Cell>();

        if (nextCell != null)
        {
            nextCell.visited = true;

            stack.Add(currentCell);

            RemoveWalls(currentCell, nextCell);

            currentCell = nextCell;
        }
        else if (stack.Count > 0)
        {
            currentCell = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);

        }
        else
        {
            foreach (GameObject go in junkCells)
            {
                Destroy(go);
            }
        }
    }

    private void RemoveWalls(Cell a, Cell b)
    {
        int x = a.x - b.x;
        if (x == 1)
        {
            a.walls[0] = false;
            b.walls[2] = false;
        }
        else if (x == -1)
        {
            a.walls[2] = false;
            b.walls[0] = false;
        }

        int z = a.z - b.z;
        if (z == 1)
        {
            a.walls[3] = false;
            b.walls[1] = false;
        }
        else if (z == -1)
        {
            a.walls[1] = false;
            b.walls[3] = false;
        }

        a.CalculateWalls();
        b.CalculateWalls();
    }
}