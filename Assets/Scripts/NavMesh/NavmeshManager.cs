using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

public class NavmeshManager : MonoBehaviour
{
    public GameObject fakeNodeObject;
    public static GameObject fakeNodeObjectClone;
    static NavmeshTriangulator triangulator;
    static WallGrid grid;
    static Grid globalGrid;
    static List<GameObject> fakeNodes;

    private void Start()
    {
        fakeNodeObjectClone = fakeNodeObject;
        grid = FindFirstObjectByType<GridCursor>().wallGrid;
        triangulator = GetComponent<NavmeshTriangulator>();
        globalGrid = FindFirstObjectByType<Grid>();
        fakeNodes = new List<GameObject>();
    }

    public static void BakeNavmeshes()
    {
        BakeNavmeshes(grid.values);
    }
    public static void BakeNavmeshes(bool[,] gridValues)
    {
        NavmeshAgent agent = new NavmeshAgent(gridValues, globalGrid);
        List<Vector2Int> alreadySearched = new List<Vector2Int>();
        List<List<Vertex>> pointListArray = new List<List<Vertex>>();
        List<Vertex> startingPoints = AddStartingPoints(gridValues);
        pointListArray.Add(startingPoints);
        for (int y = 0; y < gridValues.GetLength(1); y++)
        {
            for (int x = 0; x < gridValues.GetLength(0); x++)
            {
                Vector2Int currentPosition = new Vector2Int(x, y);
                if (alreadySearched.Contains(currentPosition) || !gridValues[x, y]) continue;

                agent.position = currentPosition + new Vector2Int(-1,0);
                if (!alreadySearched.Contains(agent.position) && IsInData(currentPosition, gridValues, false))
                {
                    List<Vertex> newPoints = agent.Search(0, ref alreadySearched);
                    if (newPoints.Count >= 4) pointListArray.Add(newPoints);
                }

                agent.position = currentPosition + new Vector2Int(1, 0);
                if (!alreadySearched.Contains(agent.position) && IsInData(currentPosition, gridValues, false))
                {
                    List<Vertex> newPoints = agent.Search(1, ref alreadySearched);
                    if (newPoints.Count >= 4) pointListArray.Add(newPoints);
                }
            }
        }
        DisplayNavmesh(pointListArray);
        triangulator.TriangulatePoints(pointListArray);
    }

    static List<Vertex> AddStartingPoints(bool[,] _values) 
    {
        List<Vertex> points = new List<Vertex>();
        int xSize = _values.GetLength(0);
        int ySize = _values.GetLength(1);
        for (int x = 0; x < xSize; x += xSize - 1)
        {
            for (int y = 0; y < ySize; y += ySize - 1)
            {
                Vector3 transformedPositiom = globalGrid.CellToWorld(new Vector3Int(x, y));
                points.Add(new Vertex (transformedPositiom + Vector3.one / 2));
            }
        }
        return points;
    }

    static void DisplayNavmesh(List<List<Vertex>> _listArray)
    {
        foreach (GameObject node in fakeNodes) Destroy(node);
        fakeNodes.Clear();

        foreach (List<Vertex> list in _listArray)
        {
            foreach(Vertex point in list)
            {
                GameObject currentObj = Instantiate(fakeNodeObjectClone, point.position, Quaternion.identity);
                fakeNodes.Add(currentObj);
            }
        }
    }

    static bool IsInData(Vector2Int point, bool[,] list, bool calculateWithData)
    {
        if (Mathf.Clamp(point.x, 0, list.GetLength(0) - 1) == point.x)
        {
            if (Mathf.Clamp(point.y, 0, list.GetLength(1) - 1) == point.y)
            {
                if (calculateWithData)
                {
                    return calculateWithData == list[point.x, point.y];
                }

                else
                {
                    return true;
                }
            }
        }
        return false;
    }
}

public class NavmeshAgent
{
    public Vector2Int position;
    Vector2Int direction = new Vector2Int(0,1);
    Grid globalGrid;
    bool[,] data;
    int xSize;
    int ySize;

    public List<Vertex> Search(int _isInside, ref List<Vector2Int> alreadyVisited)
    {
        direction = new Vector2Int(0, 1);
        Vector2Int initialPosition = position;
        List<Vector2Int> visitedList = new List<Vector2Int>();
        List<Vertex> rv = new List<Vertex>();
        bool isSearching = true;
        int searchDepth = 0;

        while (isSearching)
        {
            if (IsInData(position, true)) break;
            if (searchDepth > 10000) break;
            if (initialPosition == position && searchDepth > 0) break;
            visitedList.Add(position);

            int returnedCheck = CheckNext(_isInside);
            if (returnedCheck != 0)
            {
                AddToList(position, ref rv, _isInside);
                direction = RotateVector2Int(direction, 90 * returnedCheck);
            }

            position += direction;
            searchDepth++;
        }
        if (_isInside == 1) AddToList(position, ref rv, _isInside);

        alreadyVisited.AddRange(visitedList);
        return rv;
    }

    bool IsInData(Vector2Int point, bool calculateWithData = false)
    {
        if (Mathf.Clamp(point.x, 0, xSize - 1) == point.x)
        {
            if (Mathf.Clamp(point.y, 0, ySize - 1) == point.y)
            {
                if (calculateWithData)
                {
                    return data[point.x, point.y];
                }

                else
                {
                    return true;
                }
            }
        }
        return false;
    }

    void AddToList(Vector2Int point, ref List<Vertex> list, int _isInside)
    {
        if (Mathf.Clamp(point.x, 0, xSize - 1) == point.x)
        {
            if (Mathf.Clamp(point.y, 0, ySize - 1) == point.y)
            {
                Vector2 transformedPositiom = globalGrid.CellToWorld(new Vector3Int(point.x, point.y));
                list.Add(new Vertex(transformedPositiom + Vector2.one / 2));
            }
        }
    }

    public int CheckNext(int _isInside)
    {
        Vector2Int newUpPosition = position + direction;
        Vector2Int newLeftPosition = position + RotateVector2Int(direction, -90);
        Vector2Int newRightPosition = position + RotateVector2Int(direction, 90);
        if (IsInData(newUpPosition, true))
        {

            if (IsInData(newLeftPosition, true))
            {
                if (IsInData(newRightPosition, true))
                {
                    return 2;
                }
                return 1;
            }

            if (IsInData(newRightPosition, true)) return 3;
        }

        if (IsInData(newLeftPosition, true) & _isInside == 1) return 0;
        if (IsInData(newRightPosition, true) & _isInside == 0) return 0;

        return _isInside == 0 ? 1 : 3;
    }

    public Vector2Int RotateVector2Int(Vector2Int _input, int _amount)
    {
        Vector3 rv = new Vector3(_input.x, _input.y);
        rv = Quaternion.Euler(new Vector3(0, 0, -_amount)) * rv;
        return new Vector2Int(Mathf.RoundToInt(rv.x), Mathf.RoundToInt(rv.y));
    }

    public NavmeshAgent(bool[,] _data, Grid _globalGrid)
    {
        data = _data;
        xSize = data.GetLength(0);
        ySize = data.GetLength(1);
        globalGrid = _globalGrid;
    }
}