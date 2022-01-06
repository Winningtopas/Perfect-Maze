using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositioning : MonoBehaviour
{
    public void CenterCameraOnMaze(int mazeColumns,int mazeRows)
    {
        float mazeScale = Mathf.Max(mazeColumns, mazeRows);

        float x = mazeColumns * .5f;
        float y = mazeScale * 0.966666667f + 3.03333f;
        float z = mazeRows * 0.505555556f - 0.55555556f;

        transform.position = new Vector3(x, y, z);
    }
}
