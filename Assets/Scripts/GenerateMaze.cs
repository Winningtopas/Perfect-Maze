using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMaze : MonoBehaviour
{
    #region Variables
    [Header("Maze Variables")]
    [SerializeField]
    private bool spawnMaze;
    private int columsCount, rowsCount = 10;

    [Header("Cell Variables")]
    public static List<Cell> cells = new List<Cell>();
    public static List<Cell> stack = new List<Cell>();
    public static List<GameObject> junkObjects = new List<GameObject>();

    [SerializeField]
    private GameObject cellPrefab, cellContainerPrefab, cellContainer;
    [SerializeField]
    private float cellHeightOffset = 0.1f, cellSpawnDelay = .1f;
    private float previousCellHeight;
    private Cell previousCell, currentCell, nextCell;

    [Header("UI references")]
    [SerializeField]
    private AdjustSliderValue columnSliderText, rowSliderText;
    #endregion

    public void SpawnMaze()
    {
        columsCount = columnSliderText.sliderValue;
        rowsCount = rowSliderText.sliderValue;
        DestroyCurrentMaze();
        SpawnMazeGrid();
        StartCoroutine(NextMazeCell());
    }

    private void DestroyCurrentMaze()
    {
        StopAllCoroutines();
        Destroy(cellContainer);
        ResetVariables();

        cellContainer = Instantiate(cellContainerPrefab, Vector3.zero, Quaternion.identity, transform);
        cellContainer.name = "CellContainer";
    }

    private void ResetVariables()
    {
        cells.Clear();
        stack.Clear();
        junkObjects.Clear();
    }

    private IEnumerator NextMazeCell()
    {
        while (true)
        {
            currentCell.visited = true;

            nextCell = currentCell.CheckNeighbors(columsCount, rowsCount);

            // empty the neighbours of the current cell so we can start fresh with the search if we ever look for neighbours of this cell again
            currentCell.availableNeighbourCells = new List<Cell>();

            if (nextCell != null) // if there is a neighbouring cell that's unvisited
            {
                nextCell.visited = true;

                stack.Add(currentCell);

                RemoveWalls(currentCell, nextCell);

                previousCell = currentCell;
                currentCell = nextCell;

                AdjustCellVisual();
                yield return new WaitForSeconds(cellSpawnDelay);
            }
            else if (stack.Count > 0) // if there are still unvisited cells in the stack
            {
                currentCell = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
            }
            else // if every cell has been visited
            {
                foreach (GameObject go in junkObjects) // destroy junkObjects that we won't be needing anymore (example: removed walls)
                {
                    Destroy(go);
                }
                yield break;
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

        // Instantiate a visual element for the cellgrid and store it in the cell, set it inactive for now
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].cellPrefab = Instantiate(cellPrefab, new Vector3(cells[i].x, transform.position.y, cells[i].z), Quaternion.identity, cellContainer.transform);
            cells[i].cellPrefab.name = cellPrefab.name;
            cells[i].cellPrefab.SetActive(false);
        }

        currentCell = cells[Random.Range(0, cells.Count)];
    }

    private void AdjustCellVisual()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i] == currentCell)
            {
                currentCell.cellPrefab.SetActive(true);

                // change the cell ground color
                currentCell.cellPrefab.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.color = Color.red;

                StartCoroutine(LerpPosition(.2f));
                break;
            }
        }
    }

    IEnumerator LerpPosition(float duration)
    {
        Vector3 cellPostion = currentCell.cellPrefab.transform.position;
        currentCell.cellPrefab.transform.position = new Vector3(cellPostion.x, 0f, cellPostion.z);

        float previousCellHeight = previousCell.cellPrefab.transform.position.y;

        // change the cell height, the further down the maze the lower it will be
        Vector3 targetPosition = new Vector3(cellPostion.x, previousCellHeight - cellHeightOffset, cellPostion.z);
        Vector3 startPosition = currentCell.cellPrefab.transform.position;

        float time = 0;

        while (time < duration)
        {
            currentCell.cellPrefab.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        currentCell.cellPrefab.transform.position = targetPosition;
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