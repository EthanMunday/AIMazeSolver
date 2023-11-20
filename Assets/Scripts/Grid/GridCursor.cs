using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GridCursor : MonoBehaviour
{
    Grid globalGrid;
    Camera cameraComponent;

    public GameObject wall;
    public LayerMask wallMask;
    bool[,] wallGrid;
    List<GameObject> wallObjList;
    List<WallData> wallDataList;


    public GameObject floor;
    public List<Color> roomColorList;
    List<GameObject> roomObjList;

    ControlBindings bindings;
    InputAction lmb;
    InputAction rmb;
    Vector2 input;

    void Start()
    {
        globalGrid = GetComponent<Grid>();
        cameraComponent = FindFirstObjectByType<Camera>();
        bindings = ControlManager.inputs;

        wallGrid = new bool[33, 17];
        wallDataList = new List<WallData>();
        wallObjList = new List<GameObject>();

        roomObjList = new List<GameObject>();

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
        hitPoint.x = Mathf.Clamp(hitPoint.x, 0, 32);
        hitPoint.y = Mathf.Clamp(hitPoint.y, 0, 17);
        if (input.x == 1.0f && !wallGrid[hitPoint.x, hitPoint.y])
        {
            if (hitPoint.x < 33 && hitPoint.y < 18)
            {
                wallGrid[hitPoint.x, hitPoint.y] = true;
                UpdateGrid2();
                RoomDetection();
            }
        }

        else if (input.y == 1.0f && wallGrid[hitPoint.x, hitPoint.y])
        {
            if (hitPoint.x < 33 && hitPoint.y < 18)
            {
                wallGrid[hitPoint.x, hitPoint.y] = false;
                UpdateGrid2();
                RoomDetection();    
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
                if (Mathf.Clamp(x + 1, 0, 32) == x + 1)
                {
                    for (int i = x + 1; i < 33; i++)
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
                if (Mathf.Clamp(y + 1, 0, 16) == y + 1)
                {
                    for (int i = y + 1; i < 17; i++)
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
        GameObject newblock = Instantiate(wall, _midPoint + new Vector3(0.5f, 0.5f, -0.5f), Quaternion.Euler(Vector3.zero));
        newblock.transform.parent = transform;
        newblock.transform.localScale = _scale;
        wallObjList.Add(newblock);
        wallDataList.Add(new WallData { position = _midPoint, scale = _scale });
    }

    void CreateFloor(Vector2Int _midPoint, Color _color)
    {
        Vector3Int newPos = new Vector3Int(_midPoint.x,_midPoint.y);
        GameObject newblock = Instantiate(floor, globalGrid.CellToWorld(newPos) + new Vector3(0.5f, 0.5f, -0.5f), Quaternion.identity);
        newblock.GetComponent<SpriteRenderer>().color = _color;
        roomObjList.Add(newblock);
        newblock.transform.parent = transform;
        newblock.transform.localScale *= 2;
    }

    private void RoomDetection()
    {
        // Destroys all Room objects
        foreach (GameObject room in roomObjList) Destroy(room);

        // Makes lists for areas to search and list of finalised rooms
        List<Vector2Int> searchedList = new List<Vector2Int>();
        List<List<Vector2Int>> roomArray = new List<List<Vector2Int>>();

        // Populates the areas to search with tiles without any walls
        // The aditional check here won't be necissary since we're using edge based walls
        // Starts from -1 of the min and +1 of the max so an outer layer of tiles get checked to account for the outside area
        for (int x = -1; x < 33; x++)
        {
            for (int y = -1; y < 17;  y++)
            {
                searchedList.Add(new Vector2Int(x, y));
                if (Mathf.Clamp(x, 0, 32) != x) continue;
                if (Mathf.Clamp(y, 0, 16) != y) continue;
                // wallGrid is a list of bools to determine if it's a wall or not
                if (wallGrid[x,y]) searchedList.Remove(new Vector2Int(x, y));
            }
        }
        
        // Checks the rooms with a custom class
        RoomDetectionAgent agent = new RoomDetectionAgent();
        while (searchedList.Count > 0)
        {
            List<Vector2Int> newRoom = new List<Vector2Int>();
            agent.CheckRoom(searchedList[0], ref searchedList, ref newRoom, ref wallGrid);
            roomArray.Add(newRoom);
        }

        // Creates the visual aspect of each room
        for (int x = 0; x < roomArray.Count; x++)
        {
            if (x >= roomColorList.Count) continue;
            foreach(Vector2Int room in roomArray[x])
            {
                CreateFloor(room, roomColorList[x]);
            }
        }
    }

    public struct WallData
    {
        public Vector3 position;
        public Vector3 scale;
    }
}
