using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class WallGrid
{
    public Grid globalGrid;
    public bool[,] values;
    public static Dictionary<Vector3, bool[]> vertexPoints;
    List<GameObject> wallObjList;
    List<Vector2Int> intersectionsList;
    int gridXSize;
    int gridYSize;
    GameObject wall;
    GameObject gridObject;
    List<WallData> wallDataList;

    public void SetGrid(bool[,] _values)
    {
        // Updates the whole grid, doesn't flag navmesh as false
        if (_values == null) return;
        if (_values.GetLength(0) != values.GetLength(0)) return;
        if (_values.GetLength(1) != values.GetLength(1)) return;
        values = _values;
        RefreshGeometry();
    }

    public void UpdateGrid(Vector3Int _position, bool _value)
    {
        // Updates the positions value
        values[_position.x, _position.y] = _value;
        GridCursor.isBaked = false;
        RefreshGeometry();
    }

    void RefreshGeometry()
    {
        // Clears all wall instances
        wallDataList.Clear();
        vertexPoints.Clear();
        intersectionsList.Clear();
        foreach (GameObject wall in wallObjList) Object.Destroy(wall);
        wallObjList.Clear();

        List<List<Vector2Int>> horizontalLists = new List<List<Vector2Int>>();
        List<List<Vector2Int>> verticalLists = new List<List<Vector2Int>>();

        // Creates walls horizontally
        for (int y = 0; y < values.GetLength(1); y++)
        {
            for (int x = 0; x < values.GetLength(0); x++)
            {
                if (!values[x, y]) continue;
                List<Vector2Int> newHorizontalList = new List<Vector2Int> { new Vector2Int(x, y) };
                if (Mathf.Clamp(x + 1, 0, gridXSize) == x + 1)
                {
                    for (int i = x + 1; i < gridXSize + 1; i++)
                    {
                        if (!values[i, y]) break;
                        newHorizontalList.Add(new Vector2Int(i, y));
                        x = i;
                    }
                }
                horizontalLists.Add(newHorizontalList);
            }
        }

        // Creates walls vertically
        for (int x = 0; x < values.GetLength(0); x++)
        {
            for (int y = 0; y < values.GetLength(1); y++)
            {
                if (!values[x, y]) continue;
                List<Vector2Int> newVerticalList = new List<Vector2Int> { new Vector2Int(x, y) };
                if (Mathf.Clamp(y + 1, 0, gridYSize) == y + 1)
                {
                    for (int i = y + 1; i < gridYSize + 1; i++)
                    {
                        if (!values[x, i]) break;
                        newVerticalList.Add(new Vector2Int(x, i));
                        y = i;
                    }
                }
                verticalLists.Add(newVerticalList);
            }
        }

        // Spawns in the objects taken from the previous creations
        InstantiateLists(0, horizontalLists, verticalLists);
        InstantiateLists(1, verticalLists, horizontalLists);
        
    }

    void InstantiateLists(int _dimension, List<List<Vector2Int>> _listArray, List<List<Vector2Int>> _alternateArray)
    {
        foreach (List<Vector2Int> currentList in _listArray)
        {
            // Checks to not create duplicate walls
            if (currentList.Count == 1)
            {
                if (IsInListArray(currentList[0], _alternateArray, currentList))
                {
                    continue;
                }
            }
            Vector3 first = globalGrid.CellToWorld(new Vector3Int(currentList[0].x, currentList[0].y));
            Vector3 last = globalGrid.CellToWorld(new Vector3Int(currentList[currentList.Count - 1].x, currentList[currentList.Count - 1].y));
            Vector3 position = (first + last) / 2;
            Vector3 scale = CalculateScale(currentList.Count, _dimension);
            CreateWall(position, scale);
        }
    }

    void CreateWall(Vector3 _midPoint, Vector3 _scale)
    {
        if (wallDataList.Contains(new WallData { position = _midPoint, scale = _scale })) return;
        GameObject newblock = Object.Instantiate(wall, _midPoint + new Vector3(0.5f, 0.5f), Quaternion.Euler(Vector3.zero));
        newblock.transform.parent = gridObject.transform;
        newblock.transform.localScale = _scale;
        wallObjList.Add(newblock);
        wallDataList.Add(new WallData { position = _midPoint, scale = _scale });
    }

    bool IsInListArray(Vector2Int _value, List<List<Vector2Int>> _listArray, List<Vector2Int> _currentList)
    {
        foreach (List<Vector2Int> array in _listArray)
        {
            if (array.Count == 1)
            {
                if (_currentList[0] == array[0])
                {
                    continue;
                }
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


    public WallGrid(int _xSize, int _ySize, Grid _gridReference, GameObject _wallObject, GameObject _gridObject)
    {
        values = new bool[_xSize + 1, _ySize + 1];
        for (int i = 0; i < values.GetLength(0); i++)
        {
            for (int j = 0; j < values.GetLength(1); j++)
            {
                values[i, j] = false;
            }
        }
        gridXSize = _xSize;
        gridYSize = _ySize;
        globalGrid = _gridReference;
        wall = _wallObject;
        gridObject = _gridObject;
        wallObjList = new List<GameObject>();
        wallDataList = new List<WallData>();
        vertexPoints = new Dictionary<Vector3, bool[]>();
        intersectionsList = new List<Vector2Int>();
    }
}

public struct WallData
{
    public Vector3 position;
    public Vector3 scale;
}

