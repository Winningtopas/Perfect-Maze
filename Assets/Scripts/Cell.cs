using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : IPooledObject
{
    #region Variables
    public int x, z;
    public GameObject cellPrefab;
    public bool visited = false;
    public List<Cell> availableNeighbourCells = new List<Cell>();
    public bool[] walls = { true, true, true, true };

    private Cell[] neighbourCells = new Cell[4];
    private int columnAmount, rowAmount;

    #endregion

    public void OnObjectSpawn()
    {
        for (int i = 0; i < 4; i++)
        {
                GameObject wall = cellPrefab.transform.GetChild(1).GetChild(i).gameObject;
                wall.SetActive(true);
        }
    }

    private void CheckNeighbourCells()
    {
        if (Index(x, z - 1) != -1)
            neighbourCells[0] = GenerateMaze.cells[Index(x, z - 1)];
        if (Index(x + 1, z) != -1)
            neighbourCells[1] = GenerateMaze.cells[Index(x + 1, z)];
        if (Index(x, z + 1) != -1)
            neighbourCells[2] = GenerateMaze.cells[Index(x, z + 1)];
        if (Index(x - 1, z) != -1)
            neighbourCells[3] = GenerateMaze.cells[Index(x - 1, z)];

        for (int i = 0; i < 4; i++)
        {
            if (neighbourCells[i] != null && neighbourCells[i].visited == false)
                availableNeighbourCells.Add(neighbourCells[i]);
        }
    }

    public Cell CheckNeighbors(int columns, int rows)
    {
        columnAmount = columns;
        rowAmount = rows;
        CheckNeighbourCells();

        if (availableNeighbourCells.Count > 0)
        {
            int r = Random.Range(0, availableNeighbourCells.Count);
            return availableNeighbourCells[r];
        }
        else
            return null;
    }

    public int Index(int i, int j)
    {
        if (i < 0 || j < 0 || i > columnAmount - 1 || j > rowAmount - 1)
        {
            return -1;
        }
        return i + j * columnAmount;
    }
    public void CalculateWalls()
    {
        for (int i = 0; i < 4; i++)
        {
            if (walls[i] == false)
            {
                GameObject wall = cellPrefab.transform.GetChild(1).GetChild(i).gameObject;
                wall.SetActive(false);
            }
        }
    }
}