using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDebugTest : MonoBehaviour
{
    Grid grid;
    Camera camera;

    void Start()
    {
        grid = GetComponent<Grid>();
        camera = FindFirstObjectByType<Camera>();
    }

    void Update()
    {
        Vector2 ray = camera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero);
        if (hit.collider != null)
        {
            Debug.Log(grid.WorldToCell(hit.point)); 
        }
    }
}
