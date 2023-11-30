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
    GridCursor cursor;
    List<GameObject> fakeNodes;

    void Start()
    {
        cursor = FindFirstObjectByType<GridCursor>();
        nodes = new List<Node>();
        fakeNodes = new List<GameObject>();
    }

    public void LoadNavmesh(List<Node> _nodes)
    {
        nodes.Clear();
        nodes.AddRange(_nodes);
        DisplayNavmesh();
    }

    public void BakeNavmesh()
    {
        for (int i = 0; i < fakeNodes.Count; i++)
        {
            Destroy(fakeNodes[i]);
            nodes.Clear();
            Node.DestroyAllNodes();
        }

        foreach(Vector3 position in WallGrid.vertexPoints.Keys)
        {
            List<Vector3> verts = GetVerticiesOfCollider(position, WallGrid.vertexPoints[position]);
            for (int x = 0; x < verts.Count; x++)
            {
                if (Node.PositionExists(verts[x]) != -1) continue;
                nodes.Add(new Node(verts[x]));
            }

            foreach (Node currentNode in nodes)
            {
                for (int x = 0; x < nodes.Count; x++)
                {
                    if (currentNode == nodes[x]) continue;
                    if (currentNode.adjacencyList.Contains(nodes[x].id)) continue;
                    Vector3 distance = nodes[x].position - currentNode.position;
                    RaycastHit2D hit = Physics2D.Raycast(currentNode.position, distance.normalized, distance.magnitude, wallLayerMask);
                    if (hit.collider != null) continue;
                    currentNode.adjacencyList.Add(nodes[x].id);
                    nodes[x].adjacencyList.Add(nodes[x].id);
                }
            }
        }

        GridCursor.isBaked = true;
        DisplayNavmesh();
    }

    void DisplayNavmesh()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            GameObject currentObj = Instantiate(fakeNodeObject, nodes[i].position, Quaternion.identity);
            fakeNodes.Add(currentObj);
            currentObj.transform.parent = transform;
            foreach (Node currentNode in nodes)
            {
                for (int x = 0; x < currentNode.adjacencyList.Count; x++)
                {
                    Debug.DrawLine(currentNode.position, nodes[currentNode.adjacencyList[x]].position, Color.red, 20f);
                }
            }
        }
    }

    List<Vector3> GetVerticiesOfCollider(Vector3 position, bool[] wallData)
    {
        List<Vector3> rv = new List<Vector3>();
        int list = -1;
        for (int x = -1; x < 2; x += 2)
        {
            for (int y = -1; y < 2; y += 2)
            {
                list++;
                if (wallData[list]) continue;
                Vector3 newVert = position;
                newVert += new Vector3(x / 2f, y / 2f);
                rv.Add(newVert);
            }
        }
        return rv;
    }

    ~CustomNavmesh()
    {
        nodes.Clear();
    }

}
