using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class Node
{
    [SerializeField] public int id;
    [SerializeField] public Vector3 position;
    [SerializeField] public List<int> adjacencyList = new List<int>();
    public static List<Node> nodeList = new List<Node>();
    public int GetNewID()
    {
        nodeList.Add(this);
        return nodeList.Count - 1;
    }

    public static int PositionExists(Vector3 position)
    {
        foreach(Node node in nodeList)
        {
            if (node.position == position) return node.id;
        }
        return -1;
    }

    void RefreshIDs()
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            nodeList[i].id = i;
        }
    }

    public static void DestroyAllNodes()
    {
        nodeList.Clear();
    }

    public void DestroyNode()
    {
        nodeList.RemoveAt(id);
        RefreshIDs();
    }

    public Node(Vector3 _position)
    {
        position = _position;
        id = GetNewID();
        adjacencyList = new List<int>();
    }

    ~Node()
    {
        DestroyNode();
    }
}

public class CustomNavmesh : MonoBehaviour
{
    public GameObject fakeNodeObject;
    public LayerMask wallLayerMask;
    public List<Node> nodes;
    WallGrid grid;
    List<GameObject> fakeNodes;

    void Start()
    {

    }

    public void LoadNavmesh(List<Node> _nodes)
    {
        nodes.Clear();
        nodes.AddRange(_nodes);
        //DisplayNavmesh();
    }

    public void BakeNavmesh()
    {
        GridCursor.isBaked = true;
        //DisplayNavmesh();
    }

    void DisplayNavmesh()
    {
        foreach (GameObject node in fakeNodes) Destroy(node);
        fakeNodes.Clear();

        for (int i = 0; i < nodes.Count; i++)
        {
            GameObject currentObj = Instantiate(fakeNodeObject, nodes[i].position, Quaternion.identity);
            fakeNodes.Add(currentObj);
            currentObj.transform.parent = transform;
        }
    }

    ~CustomNavmesh()
    {
        nodes.Clear();
    }

}
