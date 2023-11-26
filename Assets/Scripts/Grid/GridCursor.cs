using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridCursor : MonoBehaviour
{
    public int gridXSize;
    public int gridYSize;
    public string saveLoad;
    Grid globalGrid;
    Camera cameraComponent;

    public GameObject wall;
    public LayerMask wallMask;
    bool[,] wallGrid;
    List<GameObject> wallObjList;
    List<WallData> wallDataList;

    ControlBindings bindings;
    InputAction lmb;
    InputAction rmb;
    Vector2 input;

    void Start()
    {
        globalGrid = GetComponent<Grid>();
        cameraComponent = FindFirstObjectByType<Camera>();
        bindings = ControlManager.inputs;

        wallGrid = new bool[gridXSize+1, gridYSize+1];
        wallDataList = new List<WallData>();
        wallObjList = new List<GameObject>();

        for (int i = 0; i < wallGrid.GetLength(0); i++)
        {
            for (int j = 0; j < wallGrid.GetLength(1); j++)
            {
                wallGrid[i, j] = false;
            }
        }

        AddInput(lmb, bindings.Player.LeftClick);
        AddInput(rmb, bindings.Player.RightClick);
    }

    private void Update()
    {
        if (input.sqrMagnitude == 0) return;
        Vector2 ray = cameraComponent.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, 100f, wallMask);
        if (hit.collider == null) return;
        Vector3Int hitPoint = globalGrid.WorldToCell(hit.point);
        hitPoint.x = Mathf.Clamp(hitPoint.x, 0, gridXSize);
        hitPoint.y = Mathf.Clamp(hitPoint.y, 0, gridYSize);
        if (input.x == 1.0f && !wallGrid[hitPoint.x, hitPoint.y])
        {
            if (hitPoint.x < gridXSize + 1 && hitPoint.y < gridYSize + 1)
            {
                wallGrid[hitPoint.x, hitPoint.y] = true;
                UpdateGrid2();
            }
        }

        else if (input.y == 1.0f && wallGrid[hitPoint.x, hitPoint.y])
        {
            if (hitPoint.x < gridXSize + 1 && hitPoint.y < gridYSize + 1)
            {
                wallGrid[hitPoint.x, hitPoint.y] = false;
                UpdateGrid2();
            }
        }
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

    void UpdateGrid2()
    {
        wallDataList.Clear();
        foreach (GameObject wall in wallObjList) Destroy(wall);
        wallObjList.Clear();

        List<List<Vector2Int>> horizontalLists = new List<List<Vector2Int>>();
        List<List<Vector2Int>> verticalLists = new List<List<Vector2Int>>();

        for (int y = 0; y < wallGrid.GetLength(1); y++)
        {
            for (int x = 0; x < wallGrid.GetLength(0); x++)
            {
                if (!wallGrid[x, y]) continue;
                List<Vector2Int> newHorizontalList = new List<Vector2Int> { new Vector2Int(x, y) };
                if (Mathf.Clamp(x + 1, 0, gridXSize) == x + 1)
                {
                    for (int i = x + 1; i < gridXSize + 1; i++)
                    {
                        if (!wallGrid[i, y]) break;
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
                if (!wallGrid[x, y]) continue;
                List<Vector2Int> newVerticalList = new List<Vector2Int> { new Vector2Int(x, y) };
                if (Mathf.Clamp(y + 1, 0, gridYSize) == y + 1)
                {
                    for (int i = y + 1; i < gridYSize + 1; i++)
                    {
                        if (!wallGrid[x, i]) break;
                        newVerticalList.Add(new Vector2Int(x, i));
                        y = i;
                    }
                }
                verticalLists.Add(newVerticalList);
            }
        }

        InstantiateLists(0, horizontalLists, verticalLists);
        InstantiateLists(1, verticalLists, horizontalLists);
    }


    void InstantiateLists(int _dimension, List<List<Vector2Int>> _listArray, List<List<Vector2Int>> _alternateArray)
    {
        foreach (List<Vector2Int> currentList in _listArray)
        {

            if (currentList.Count == 1)
            {
                if (IsInListArray(currentList[0], _alternateArray, currentList)) continue;
            }
            Vector3 first = globalGrid.CellToWorld(new Vector3Int(currentList[0].x, currentList[0].y));
            Vector3 last = globalGrid.CellToWorld(new Vector3Int(currentList[currentList.Count - 1].x, currentList[currentList.Count - 1].y));
            Vector3 position = (first + last) / 2;
            Vector3 scale = CalculateScale(currentList.Count, _dimension);
            CreateWall(position, scale);
        }
    }

    bool IsInListArray(Vector2Int _value, List<List<Vector2Int>> _listArray, List<Vector2Int> _currentList)
    {
        foreach (List<Vector2Int> array in _listArray)
        {
            if (array.Count == 1)
            {
                if (_currentList[0] == array[0]) continue;
            }
            if (array.Contains(_value)) return true;
        }
        return false;
    }

    Vector3 CalculateScale(int _count, int _dimension)
    {
        Vector3 rv = Vector3.one;
        if (_dimension == 1) rv.y = (_count - 0.5f) * 2;
        else rv.x = (_count - 0.5f) * 2;
        return rv;
    }


    private void CreateWall(Vector3 _midPoint, Vector3 _scale)
    {
        if (wallDataList.Contains(new WallData { position = _midPoint, scale = _scale })) return;
        GameObject newblock = Instantiate(wall, _midPoint + new Vector3(0.5f, 0.5f), Quaternion.Euler(Vector3.zero));
        newblock.transform.parent = transform;
        newblock.transform.localScale = _scale;
        wallObjList.Add(newblock);
        wallDataList.Add(new WallData { position = _midPoint, scale = _scale });
    }

    public void SaveGame()
    {
        GridSaveLoader.SaveToFile(saveLoad, wallGrid);
    }

    public void LoadGame()
    {
        bool[,] newGrid = GridSaveLoader.LoadFromFile(saveLoad);
        if (newGrid == null) return;
        if (newGrid.GetLength(0) != wallGrid.GetLength(0)) return;
        if (newGrid.GetLength(1) != wallGrid.GetLength(1)) return;
        wallGrid = newGrid;
        UpdateGrid2();
    }

    public struct WallData
    {
        public Vector3 position;
        public Vector3 scale;
    }
}
