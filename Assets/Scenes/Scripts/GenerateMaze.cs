using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMaze : MonoBehaviour
{
    #region Variables
    [Header("Maze Variables")]
    [SerializeField]
    [Range(10, 250)] private int columsCount, rowsCount = 10;

    public static List<Cell> cells = new List<Cell>();
    [SerializeField]
    private GameObject cellPrefab, cellContainer;
    private Cell currentCell;
    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        SpawnMazeGrid();
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
}