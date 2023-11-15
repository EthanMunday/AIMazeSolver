using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlacementTesting : MonoBehaviour
{
    Grid globalGrid;
    GridSlot[,] wallGrid;
    Camera camera;
    public GameObject wall;
    public LayerMask wallMask;

    void Start()
    {
        globalGrid = GetComponent<Grid>();
        camera = FindFirstObjectByType<Camera>();
        wallGrid = new GridSlot[33, 18];

        for (int i = 0; i <  wallGrid.GetLength(0); i++)
        {
            for (int j = 0; j < wallGrid.GetLength(1); j++)
            {
                wallGrid[i, j] = new GridSlot();
            }
        }
    }

    void Update()
    {
        Vector2 ray = camera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, 100f, wallMask);
        if (hit.collider != null && Input.GetMouseButton(0))
        {
            Vector3Int hitPoint = globalGrid.WorldToCell(hit.point);
            if (hitPoint.x < 33 && hitPoint.y < 18)
            {
                if (!wallGrid[hitPoint.x, hitPoint.y].wallData[0])
                {
                    GameObject newblock = Instantiate(wall, globalGrid.CellToWorld(hitPoint) + new Vector3(0.5f, 0.5f), Quaternion.Euler(Vector3.zero));
                    newblock.transform.parent = transform;
                    wallGrid[hitPoint.x, hitPoint.y].wallObjects[0] = newblock;
                    wallGrid[hitPoint.x, hitPoint.y].wallData[0] = true;
                    UpdateGrid();
                }
                
            }
        }

        else if (hit.collider != null && Input.GetMouseButton(1))
        {
            Vector3Int hitPoint = globalGrid.WorldToCell(hit.point);
            if (hitPoint.x < 33 && hitPoint.y < 18)
            {
                if (wallGrid[hitPoint.x, hitPoint.y].wallData[0])
                {
                    Destroy(wallGrid[hitPoint.x, hitPoint.y].wallObjects[0]);
                    wallGrid[hitPoint.x, hitPoint.y].wallData[0] = false;
                    UpdateGrid();
                }
            }
        }
    }

    void UpdateGrid()
    {
        for(int x = 0; x < wallGrid.GetLength(0); x++)
        {
            for (int y = 0; y < wallGrid.GetLength(1); y++)
            {
                if (wallGrid[x, y].wallData[0])
                { 
                    if (Mathf.Clamp(x + 1, 0, 32) == x + 1)
                    {
                        if (wallGrid[x + 1, y].wallData[0] && !wallGrid[x, y].wallData[1])
                        {
                            GameObject newblock = Instantiate(wall, new Vector3(x + 0.5f, y), Quaternion.Euler(Vector3.zero));
                            newblock.transform.parent = transform;
                            wallGrid[x, y].wallObjects[1] = newblock;
                            wallGrid[x, y].wallData[1] = true;
                        }
                        if (!wallGrid[x + 1, y].wallData[0] && wallGrid[x, y].wallData[1])
                        {
                            Destroy(wallGrid[x, y].wallObjects[1]);
                            wallGrid[x, y].wallData[1] = false;
                        }
                    }
                    if (Mathf.Clamp(y + 1, 0, 16) == y + 1)
                    {
                        if (wallGrid[x, y + 1].wallData[0] && !wallGrid[x, y].wallData[2])
                        {
                            GameObject newblock = Instantiate(wall, new Vector3(x, y + 0.5f), Quaternion.Euler(Vector3.zero));
                            newblock.transform.parent = transform;
                            wallGrid[x, y].wallObjects[2] = newblock;
                            wallGrid[x, y].wallData[2] = true;
                        }
                        if (!wallGrid[x, y + 1].wallData[0] && wallGrid[x, y].wallData[2])
                        {
                            Destroy(wallGrid[x, y].wallObjects[2]);
                            wallGrid[x, y].wallData[2] = false;
                        }
                    }
                }

                else
                {
                    if (wallGrid[x, y].wallData[1])
                    {
                        Destroy(wallGrid[x, y].wallObjects[1]);
                        wallGrid[x, y].wallData[1] = false;
                    }
                    if (wallGrid[x, y].wallData[2])
                    {
                        Destroy(wallGrid[x, y].wallObjects[2]);
                        wallGrid[x, y].wallData[2] = false;
                    }
                }
            }
        }
    }

    void UpdateGrid2()
    {
        List<List<GridSlot>> horizontalLists = new List<List<GridSlot>>();
        List<List<GridSlot>> verticalLists = new List<List<GridSlot>>();
        GridSlot[,] copiedSlots = wallGrid;
        for (int x = 0; x < wallGrid.GetLength(0); x++)
        {
            for (int y = 0; y < wallGrid.GetLength(1); y++)
            {
                
            }
        }
    }
}
