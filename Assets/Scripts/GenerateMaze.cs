using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMaze : MonoBehaviour
{
    #region Variables
    [HideInInspector]
    public bool threeDimensionalMaze = true, dropDownAnimation = true;

    [SerializeField]
    public static List<Cell> cells = new List<Cell>();
    public static List<Cell> stack = new List<Cell>();
    //public static List<GameObject> junkObjects = new List<GameObject>();

    private List<Cell> unoptimisedWallsCell = new List<Cell>();

    [Header("Cell Variables")]
    [SerializeField]
    private float cellSpawnDelay = .2f;
    [SerializeField]
    private float cellHeightOffset = 0.1f, cellFallHeight = 3f;

    private Cell previousCell, currentCell, nextCell, endCell;
    private int columsCount, rowsCount = 10;
    private bool isFirstCell = true, coloredLastCell = false;
    private int startCellIndex;
    [SerializeField]
    private List<MeshFilter> cellGroundMeshFilters, cellWallMeshFilters = new List<MeshFilter>();

    [Header("UI references")]
    [SerializeField]
    private AdjustSliderValue columnSliderText;
    [SerializeField]
    private AdjustSliderValue rowSliderText;

    [Header("Object references")]
    [SerializeField]
    private CameraPositioning mainCamera;
    [SerializeField]
    private GameObject cellPrefab, cellContainerPrefab;

    private GameObject cellContainer;
    [SerializeField]
    private MeshFilter mazeGroundMesh, mazeWallMesh;
    #endregion

    private void Update()
    {
        if (cellGroundMeshFilters.Count >= 50)
        {
            CombineMazeMeshes();
        }
    }

    public void SpawnMaze()
    {
        columsCount = columnSliderText.sliderValue;
        rowsCount = rowSliderText.sliderValue;
        DestroyCurrentMaze();
        SpawnMazeGrid();
        mainCamera.CenterCameraOnMaze(columsCount, rowsCount);

        StartCoroutine(NextMazeCell());
    }

    public void ThreeDimensionalMazeToggle()
    {
        threeDimensionalMaze = !threeDimensionalMaze;
    }

    public void DropDownAnimationToggle()
    {
        dropDownAnimation = !dropDownAnimation;
        if (dropDownAnimation)
            cellSpawnDelay = .2f;
        else
            cellSpawnDelay = 0f;
    }

    private void DestroyCurrentMaze()
    {
        StopAllCoroutines();

        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].cellPrefab.SetActive(false);
        }
        ResetVariables();
    }

    private void ResetVariables()
    {
        cells.Clear();
        stack.Clear();
        cellGroundMeshFilters.Clear();
        cellWallMeshFilters.Clear();
        unoptimisedWallsCell.Clear();

        isFirstCell = true;
        coloredLastCell = false;
        previousCell = null;
        currentCell = null;
        nextCell = null;
        mazeGroundMesh.mesh = null;
        mazeWallMesh.mesh = null;

        if (cellContainer)
            Destroy(cellContainer);
        cellContainer = Instantiate(cellContainerPrefab, transform.position, Quaternion.identity, transform);
        cellContainer.name = "CellContainer";
    }

    private IEnumerator NextMazeCell()
    {
        while (true)
        {
            if (isFirstCell)
            {
                ColorCell(currentCell, Color.green);
                AdjustCellVisual();
                isFirstCell = false;
            }

            currentCell.visited = true;

            nextCell = currentCell.CheckNeighbors(columsCount, rowsCount);

            // empty the neighbours of the current cell so we can start fresh with the search if we ever look for neighbours of this cell again
            currentCell.availableNeighbourCells = new List<Cell>();

            if (nextCell != null) // if there is a neighbouring cell that's unvisited
            {
                nextCell.visited = true;

                stack.Add(currentCell);

                RemoveWalls(currentCell, nextCell);

                if (currentCell.GeneratedAllNeighbours())
                    CombineWallMeshes();
                else
                    unoptimisedWallsCell.Add(currentCell);

                previousCell = currentCell;
                currentCell = nextCell;

                AdjustCellVisual();
                yield return new WaitForSeconds(cellSpawnDelay);
            }
            else if (!coloredLastCell)
            {
                currentCell.cellPrefab.name = "endCell";
                endCell = currentCell;
                currentCell.isEndCell = true;
                coloredLastCell = true;
                AdjustCellVisual();
            }
            else if (stack.Count > 0) // if there are still un(re)visited cells in the stack
            {
                currentCell = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
            }
            else // if every cell has been visited
            {
                ColorCell(endCell, Color.yellow);
                endCell.cellPrefab.transform.GetChild(0).gameObject.SetActive(true);
                endCell.cellPrefab.transform.position = new Vector3(endCell.cellPrefab.transform.position.x, endCell.cellPrefab.transform.position.y + 0.05f, endCell.cellPrefab.transform.position.z);

                CombineMazeMeshes();
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

        for (int i = 0; i < cells.Count; i++)
        {
            // Instantiate a visual element for the cellgrid and store it in the cell, set it inactive for now
            cells[i].cellPrefab = Instantiate(cellPrefab, new Vector3(cells[i].x, transform.position.y, cells[i].z), transform.rotation, cellContainer.transform);
            cells[i].cellPrefab.SetActive(false);

            cells[i].OnObjectSpawn();
        }

        startCellIndex = UnityEngine.Random.Range(0, cells.Count);
        currentCell = cells[startCellIndex];
        currentCell.isStartCell = true;
    }

    private void AddToWallMesh()
    {
        for (int i = 0; i < unoptimisedWallsCell.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (unoptimisedWallsCell[i] != null)
                {
                    if (unoptimisedWallsCell[i].walls[j] == true)
                        cellWallMeshFilters.Add(unoptimisedWallsCell[i].cellPrefab.transform.GetChild(1).GetChild(j).gameObject.GetComponent<MeshFilter>());
                }
            }
            unoptimisedWallsCell.RemoveAt(i);
            i--;
        }
    }

    private void CombineWallMeshes()
    {
        for (int i = 0; i < 4; i++)
        {
            if (currentCell.walls[i] == true)
                cellWallMeshFilters.Add(currentCell.cellPrefab.transform.GetChild(1).GetChild(i).gameObject.GetComponent<MeshFilter>());
        }
    }

    private void CombineMazeMeshes()
    {
        cellGroundMeshFilters.Add(mazeGroundMesh);
        AddToWallMesh();
        cellWallMeshFilters.Add(mazeWallMesh);
        CombineInstance[] combineGround = new CombineInstance[cellGroundMeshFilters.Count];
        CombineInstance[] combineWalls = new CombineInstance[cellWallMeshFilters.Count];

        int i = 0;
        while (i < cellGroundMeshFilters.Count)
        {
            combineGround[i].mesh = cellGroundMeshFilters[i].sharedMesh;
            combineGround[i].transform = cellGroundMeshFilters[i].transform.localToWorldMatrix;

            cellGroundMeshFilters[i].gameObject.SetActive(false);

            i++;
        }
        cellGroundMeshFilters.Clear();

        int j = 0;
        while (j < cellWallMeshFilters.Count)
        {
            combineWalls[j].mesh = cellWallMeshFilters[j].sharedMesh;
            combineWalls[j].transform = cellWallMeshFilters[j].transform.localToWorldMatrix;
            cellWallMeshFilters[j].gameObject.SetActive(false);

            j++;
        }
        cellWallMeshFilters.Clear();

        mazeGroundMesh.mesh = new Mesh();
        mazeGroundMesh.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mazeGroundMesh.mesh.CombineMeshes(combineGround);
        mazeGroundMesh.gameObject.SetActive(true);
        AutoWeld(mazeGroundMesh.mesh, 0f, 1f);

        mazeWallMesh.mesh = new Mesh();
        mazeWallMesh.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mazeWallMesh.mesh.CombineMeshes(combineWalls);
        mazeWallMesh.gameObject.SetActive(true);
        AutoWeld(mazeWallMesh.mesh, 0f, 1f);
    }

    public static void AutoWeld(Mesh mesh, float threshold, float bucketStep)
    {
        Vector3[] oldVertices = mesh.vertices;
        Vector3[] newVertices = new Vector3[oldVertices.Length];
        int[] old2new = new int[oldVertices.Length];
        int newSize = 0;

        // Find AABB
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < oldVertices.Length; i++)
        {
            if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
            if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
            if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
            if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
            if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
            if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
        }

        // Make cubic buckets, each with dimensions "bucketStep"
        int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
        int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
        int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
        List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

        // Make new vertices
        for (int i = 0; i < oldVertices.Length; i++)
        {
            // Determine which bucket it belongs to
            int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
            int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
            int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

            // Check to see if it's already been added
            if (buckets[x, y, z] == null)
                buckets[x, y, z] = new List<int>(); // Make buckets lazily

            for (int j = 0; j < buckets[x, y, z].Count; j++)
            {
                Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                if (Vector3.SqrMagnitude(to) < threshold)
                {
                    old2new[i] = buckets[x, y, z][j];
                    goto skip; // Skip to next old vertex if this one is already there
                }
            }

            // Add new vertex
            newVertices[newSize] = oldVertices[i];
            buckets[x, y, z].Add(newSize);
            old2new[i] = newSize;
            newSize++;

        skip:;
        }

        // Make new triangles
        int[] oldTris = mesh.triangles;
        int[] newTris = new int[oldTris.Length];
        for (int i = 0; i < oldTris.Length; i++)
        {
            newTris[i] = old2new[oldTris[i]];
        }

        Vector3[] finalVertices = new Vector3[newSize];
        for (int i = 0; i < newSize; i++)
            finalVertices[i] = newVertices[i];

        mesh.Clear();
        mesh.vertices = finalVertices;
        mesh.triangles = newTris;
        mesh.RecalculateNormals();
        mesh.Optimize();
    }

    private void AdjustCellVisual()
    {
        if (!currentCell.adjustedCell)
        {
            currentCell.adjustedCell = true;
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i] == currentCell)
                {
                    currentCell.cellPrefab.SetActive(true);
                    StartCoroutine(LerpPosition(.2f, currentCell));
                    break;
                }
            }
        }
    }

    private void ColorCell(Cell cell, Color cellColor)
    {
        // change the cell ground color
        cell.cellPrefab.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.color = cellColor;
    }

    IEnumerator LerpPosition(float duration, Cell cell)
    {
        Vector3 cellPostion = cell.cellPrefab.transform.position;
        cell.cellPrefab.transform.position = new Vector3(cellPostion.x, cellFallHeight, cellPostion.z);

        float previousCellHeight;
        if (previousCell != null)
            previousCellHeight = previousCell.cellPrefab.transform.position.y;
        else
            previousCellHeight = cellFallHeight;

        float targetHeight;
        if (threeDimensionalMaze)
            targetHeight = previousCellHeight - cellHeightOffset;
        else
            targetHeight = -cellFallHeight;

        // change the cell height, the further down the maze the lower it will be
        Vector3 targetPosition = new Vector3(cellPostion.x, targetHeight, cellPostion.z);
        Vector3 startPosition = cell.cellPrefab.transform.position;

        float time = 0;

        // this coroutine only runs once when the cellSpawnDelay is a small value, the following line ensures that the cells will still get their correct height
        if (cellSpawnDelay < .2f && threeDimensionalMaze || !dropDownAnimation)
        {
            cell.cellPrefab.transform.position = targetPosition;
            if (!cell.isStartCell && !cell.isEndCell && cell != endCell)
                cellGroundMeshFilters.Add(cell.cellPrefab.transform.GetChild(0).gameObject.GetComponent<MeshFilter>());

        }
        else
        {
            while (time < duration)
            {
                cell.cellPrefab.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
                time += Time.deltaTime;

                yield return null;
            }
            cell.cellPrefab.transform.position = targetPosition;
            if (!cell.isStartCell && !cell.isEndCell && cell != endCell)
                cellGroundMeshFilters.Add(cell.cellPrefab.transform.GetChild(0).gameObject.GetComponent<MeshFilter>());
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