using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GridSaveLoader : MonoBehaviour
{
    static string savePath = "/Scripts/Grid/GridTypes/";
    static CustomNavmesh customNavmesh;

    private void Start()
    {
        customNavmesh = FindFirstObjectByType<CustomNavmesh>();
    }
    public static void SaveToFile(string _name, bool[,] _data, bool _isBaked)
    {
        SaveData newSave = new SaveData();
        newSave.xSize = _data.GetLength(0);
        newSave.ySize = _data.GetLength(1);
        newSave.values = new bool[newSave.xSize * newSave.ySize];
        newSave.isBaked = _isBaked;
        if (_isBaked)
        {
            newSave.navMeshNodes = customNavmesh.nodes;
            newSave.navMeshTriangles = customNavmesh.triangles;
        }
        for (int x = 0; x < _data.GetLength(0); x++)
        {
            for (int y = 0;  y < _data.GetLength(1); y++) newSave.values[x * newSave.ySize + y] = _data[x, y];
        }
        string output  = JsonUtility.ToJson(newSave);
        File.WriteAllText(Path.Combine(Application.dataPath + savePath + _name + ".txt"), output);
    }
    public static bool[,] LoadFromFile(string _name)
    {
        if (File.Exists(Path.Combine(Application.dataPath + savePath + _name + ".txt")))
        {
            Node.DestroyAllNodes();
            string input = File.ReadAllText(Path.Combine(Application.dataPath + savePath + _name + ".txt"));
            SaveData foundData = JsonUtility.FromJson<SaveData>(input);
            bool[,] returnValue = new bool[foundData.xSize, foundData.ySize];
            for (int x = 0; x < foundData.xSize; x++)
            {
                for (int y = 0; y < foundData.ySize; y++)
                {
                    returnValue[x, y] = foundData.values[x * foundData.ySize + y];
                }
            }
            if (foundData.isBaked) customNavmesh.LoadNavmesh(foundData.navMeshNodes, foundData.navMeshTriangles);
            return returnValue;
        }
        return null;
    }

}

[Serializable]
struct SaveData
{
    public int xSize;
    public int ySize;
    public bool[] values;
    public bool isBaked;
    public List<Node> navMeshNodes;
    public List<IndexTriangle> navMeshTriangles;
}