using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlacementTesting : MonoBehaviour
{
    Grid globalGrid;
    GridSlot[,] wallGrid;
    Camera camera;
    public GameObject wall;
    public LayerMask wallMask;

    ControlBindings bindings;
    InputAction lmb;
    InputAction rmb;
    Vector2 input;

    void Start()
    {
        globalGrid = GetComponent<Grid>();
        camera = FindFirstObjectByType<Camera>();
        bindings = ControlManager.inputs;
        wallGrid = new GridSlot[33, 18];

        for (int i = 0; i <  wallGrid.GetLength(0); i++)
        {
            for (int j = 0; j < wallGrid.GetLength(1); j++)
            {
                wallGrid[i, j] = new GridSlot();
            }
        }

        AddInput(lmb, bindings.Player.LeftClick);
        AddInput(rmb, bindings.Player.RightClick);
    }

    private void Update()
    {
        if (input.sqrMagnitude == 0) return;
        Vector2 ray = camera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, 100f, wallMask);
        if (hit.collider == null) return;
        Vector3Int hitPoint = globalGrid.WorldToCell(hit.point);
        if (!(hitPoint.x < 33 && hitPoint.y < 18)) return;
        if (input.x == 1.0f && !wallGrid[hitPoint.x, hitPoint.y].wallData[0])
        {
            if (hitPoint.x < 33 && hitPoint.y < 18)
            {
                CreateWall(hitPoint);
                UpdateGrid();
            }
        }

        else if (input.y == 1.0f && wallGrid[hitPoint.x, hitPoint.y].wallData[0])
        {
            if (hitPoint.x < 33 && hitPoint.y < 18)
            {
                Destroy(wallGrid[hitPoint.x, hitPoint.y].wallObjects[0]);
                wallGrid[hitPoint.x, hitPoint.y].wallData[0] = false;
                UpdateGrid();
            }
        }
    }

    private void CreateWall(Vector3Int _hitPoint)
    {
        GameObject newblock = Instantiate(wall, globalGrid.CellToWorld(_hitPoint) + new Vector3(0.5f, 0.5f), Quaternion.Euler(Vector3.zero));
        newblock.transform.parent = transform;
        wallGrid[_hitPoint.x, _hitPoint.y].wallObjects[0] = newblock;
        wallGrid[_hitPoint.x, _hitPoint.y].wallData[0] = true;
        UpdateGrid2();
    }

    private void AddInput(InputAction newInput, InputAction correspondingButton)
    {
        newInput = correspondingButton;
        newInput.Enable();
        newInput.performed += OnInput;
        newInput.canceled += OnInput;
    }

    void OnInput(InputAction.CallbackContext button)
    {
        switch (button.action.name)
        {
            case "LeftClick":
                if (button.performed) input.x = 1;
                else if (button.canceled) input.x = 0;
                break;
            case "RightClick":
                if (button.performed) input.y = 1;
                else if (button.canceled) input.y = 0;
                break;
            default: break;
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
        List<List<Vector2Int>> horizontalLists = new List<List<Vector2Int>>();
        List<List<Vector2Int>> verticalLists = new List<List<Vector2Int>>();

        for (int y = 0; y < wallGrid.GetLength(1); y++)
        {
            for (int x = 0; x < wallGrid.GetLength(0); x++)
            {
                if (!wallGrid[x, y].wallData[0]) continue;
                List<Vector2Int> newHorizontalList = new List<Vector2Int> { new Vector2Int(x, y) };
                if (Mathf.Clamp(x + 1, 0, 32) == x + 1)
                {
                    for (int i = x + 1; i < 33; i++)
                    {
                        if (!wallGrid[i, y].wallData[0]) break;
                        newHorizontalList.Add(new Vector2Int(i, y));
                        x = i;
                    }
                }
                horizontalLists.Add(newHorizontalList);
            }
        }

        for (int x = 0; x < wallGrid.GetLength(0); x++)
        {
            for (int y = 0; y < wallGrid.GetLength(1); y++)
            {
                if (!wallGrid[x, y].wallData[0]) continue;
                List<Vector2Int> newVerticalList = new List<Vector2Int> { new Vector2Int(x, y) };
                if (Mathf.Clamp(y + 1, 0, 16) == y + 1)
                {
                    for (int i = y + 1; i < 17; i++)
                    {
                        if (!wallGrid[x, i].wallData[0]) break;
                        newVerticalList.Add(new Vector2Int(x, i));
                        y = i;
                    }
                }
                verticalLists.Add(newVerticalList);
            }
        }

        Debug.Log("Lol");
    }

    
}
