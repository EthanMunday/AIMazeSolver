using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem.Editor;
using UnityEngine.UI;
using UnityEngine.UIElements;

[Serializable]
public class Node
{
    [SerializeField] public Index _id = new Index(-1);
    [SerializeField] public Vector2 position;
    [SerializeField] public List<NodeEdge> adjacencyList = new List<NodeEdge>();
    public static List<Node> nodeList = new List<Node>();
    public int id
    {
        get
        {
            return _id.value;
        }
        set
        {
            _id.value = value;
        }
    }

    public int GetNewID()
    {
        nodeList.Add(this);
        return nodeList.Count - 1;
    }


    public static int PositionExists(Vector2 position)
    {
        foreach(Node node in nodeList)
        {
            if (node.position == position) return node.id;
        }
        return -1;
    }

    public static Index PositionIndex(Vector2 position)
    {
        foreach (Node node in nodeList)
        {
            if (node.position == position) return node._id;
        }
        return new Index(-1);
    }

    public void AddAdjacency(Node _adjacentNode)
    {
        if (_adjacentNode == this) return;
        foreach(NodeEdge currentEdge in adjacencyList)
        {
            if (currentEdge.Contains(_adjacentNode.id)) return;
        }
        NodeEdge newEdge = new NodeEdge(this, _adjacentNode);
        adjacencyList.Add(newEdge);
    }


    public void Destroy()
    {
        foreach (NodeEdge currentEdge in adjacencyList)
        {
            if (currentEdge.to.value >= nodeList.Count) continue;
            RemoveAdjancency(nodeList[currentEdge.to.value].adjacencyList);
        }
        if (nodeList.Count > id) nodeList.RemoveAt(id);
        RefreshIDs();
    }

    void RemoveAdjancency(List<NodeEdge> _list)
    {
        for (int i = 0; i < _list.Count; i++)
        {
            if (_list[i].to == _id)
            {
                _list.RemoveAt(i);
                return;
            }
        }
    }

    public static void DestroyAllNodes()
    {
        nodeList.Clear();
    }

    void RefreshIDs()
    {
        for (int i = 0;  i < nodeList.Count; i++)
        {
            nodeList[i].id = i;
        }
    }

    public Node(Vector2 _position)
    {
        position = _position;
        id = GetNewID();
        adjacencyList = new List<NodeEdge>();
    }
}

[Serializable]
public class Index
{
    public int value;
    public Index(int _value) { this.value = _value; }
}

[Serializable]
public class NodeEdge
{
    [SerializeField] public Index from;
    [SerializeField] public Index to;
    [SerializeField] public float cost;
    
    public bool Contains(int _value)
    {
        if (_value == to.value) return true;
        if (_value == from.value) return true;
        return false;
    }

    float GetDistance(Node _node1, Node _node2)
    {
        Vector2 vector1 = _node1.position;
        Vector2 vector2 = _node2.position;
        return (vector1 - vector2).magnitude;
    }

    public NodeEdge(Node _from, Node _to)
    {
        from = _from._id;
        to = _to._id;
        cost = GetDistance(_from, _to);
    }
}

public class CustomNavmesh : MonoBehaviour
{
    public static CustomNavmesh _ref;
    public List<Node> nodes = new List<Node>();
    public List<IndexTriangle> triangles = new List<IndexTriangle>();
    public List<GameObject> currentAgents = new List<GameObject>();
    public float spawningRate;
    bool toggleSpawning = false;

    private void Start()
    {
        if (_ref == null) _ref = this;
    }

    private void Update()
    {
        if (!GridCursor.isBaked && toggleSpawning) CancelSpawning();
    }

    public void ToggleSpawning()
    {
        if (!GridCursor.isBaked) CancelSpawning();

        if (toggleSpawning) CancelSpawning();

        else
        {
            toggleSpawning = true;
            InvokeRepeating("SpawnAgent", 0f, spawningRate);
        }
    }

    void CancelSpawning()
    {
        toggleSpawning = false;
        CancelInvoke();
        foreach (GameObject agent in currentAgents)
        {
            Destroy(agent);
        }
        currentAgents.Clear();
    }

    public void SpawnAgent()
    {
        if (!GridCursor.isBaked) return;

        int safety = 0;
        while (true)
        {
            if (safety == 1000) break;

            float x = GridCursor.gridXSize - 1;
            float y = GridCursor.gridYSize - 1;
            Vector2 pawn = new Vector2(UnityEngine.Random.Range(1f, x), UnityEngine.Random.Range(1f, y));
            Vector2 target = new Vector2(UnityEngine.Random.Range(1f, x), UnityEngine.Random.Range(1f, y));
            List<Vector2> path = FindPathWithAStar(pawn, target);

            if (path.Count != 0)
            {
                UnityEngine.Object agent = Instantiate(Resources.Load("AIAgent"), pawn, Quaternion.identity);
                currentAgents.Add(agent.GameObject());
                AIMovementController controller = agent.GetComponent<AIMovementController>();
                controller.SetTarget(path);
                controller.removeList = currentAgents;
                return;
            }

            safety++;
        }
    }

    public void ShowNavmesh(Vector2 _start, Vector2 _target)
    {
        if (!GridCursor.isBaked) return;
        List<Vector2> path = FindPathWithAStar(_start, _target);
        for (int i = 0;  i < path.Count - 1; i++)
        {
            Debug.DrawLine(path[i], path[i + 1], Color.magenta, 5f);
        }
    }

    public void LoadNavmesh(List<Node> _nodes, List<IndexTriangle> _triangles)
    {
        foreach (Node currentNode in nodes) currentNode.Destroy();
        nodes = _nodes;
        Node.nodeList.AddRange(nodes);
        triangles = _triangles;
        if (nodes != null && triangles != null) GridCursor.isBaked = true;
    }

    public void BakeNavmesh(List<Triangle> _triangles)
    {
        GridCursor.isBaked = true;
        foreach (Node currentNode in nodes) currentNode.Destroy();
        nodes.Clear();
        triangles.Clear();

        foreach (Triangle currentTriangle in _triangles)
        {
            if (Node.PositionExists(currentTriangle.point1.position) == -1) CreateNode(currentTriangle.point1);
            if (Node.PositionExists(currentTriangle.point2.position) == -1) CreateNode(currentTriangle.point2);
            if (Node.PositionExists(currentTriangle.point3.position) == -1) CreateNode(currentTriangle.point3);
        }
        foreach (Triangle currentTriangle in _triangles)
        {
            AddAdjacencies(currentTriangle);
        }
    }

    Node CreateNode(Vertex _currentVertex)
    {
        Node createdNode = new Node(_currentVertex.position);
        nodes.Add(createdNode);
        return createdNode;
    }

    void AddAdjacencies(Triangle _currentTriangle)
    {
        int index1 = Node.PositionExists(_currentTriangle.point1.position);
        int index2 = Node.PositionExists(_currentTriangle.point2.position);
        int index3 = Node.PositionExists(_currentTriangle.point3.position);

        Node.nodeList[index1].AddAdjacency(Node.nodeList[index2]);
        Node.nodeList[index1].AddAdjacency(Node.nodeList[index3]);

        Node.nodeList[index2].AddAdjacency(Node.nodeList[index3]);
        Node.nodeList[index2].AddAdjacency(Node.nodeList[index1]);

        Node.nodeList[index3].AddAdjacency(Node.nodeList[index1]);
        Node.nodeList[index3].AddAdjacency(Node.nodeList[index2]);

        triangles.Add(
            new IndexTriangle(
                Node.nodeList[index1]._id, 
                Node.nodeList[index2]._id, 
                Node.nodeList[index3]._id));
    }

    public List<Vector2> FindPathWithAStar(Vector2 _position, Vector2 _target)
    {
        List<Vector2> rv = new List<Vector2>();
        Node startNode = CreateNode(new Vertex(_position));
        Node endNode = CreateNode(new Vertex(_target));

        bool startLocated = false;
        bool endLocated = false;
        foreach (IndexTriangle currentTriangle in triangles)
        {
            Node node1 = Node.nodeList[currentTriangle.point1.value];
            Node node2 = Node.nodeList[currentTriangle.point2.value];
            Node node3 = Node.nodeList[currentTriangle.point3.value];
            if (TriangulationSystem.IsPointInTriangle(node1.position,node2.position,node3.position,startNode.position))
            {
                if (!startLocated)
                {
                    startLocated = true;
                    startNode.AddAdjacency(node1);
                    startNode.AddAdjacency(node2);
                    startNode.AddAdjacency(node3);
                }
            }

            if (TriangulationSystem.IsPointInTriangle(node1.position, node2.position, node3.position, endNode.position))
            {
                if (!endLocated)
                {
                    endLocated = true;
                    endNode.AddAdjacency(node1);
                    endNode.AddAdjacency(node2);
                    endNode.AddAdjacency(node3);
                    node1.AddAdjacency(endNode);
                    node2.AddAdjacency(endNode);
                    node3.AddAdjacency(endNode);
                }
            }
        }

        if (!startLocated | !endLocated)
        {
            nodes.Remove(startNode);
            nodes.Remove(endNode);
            startNode.Destroy();
            endNode.Destroy();
            return new List<Vector2>();
        }

        List<int> path = AStar(startNode, endNode);
        foreach (int index in path)
        {
            rv.Add(Node.nodeList[index].position);
        }
        nodes.Remove(startNode);
        nodes.Remove(endNode);
        startNode.Destroy();
        endNode.Destroy();

        return rv;
    }

    List<int> AStar(Node _start, Node _end)
    {
        List<int> rv = new List<int>();
        int[] path = new int[Node.nodeList.Count];
        float[] cost = new float[Node.nodeList.Count];
        List<NodeEdge> alreadySearched = new List<NodeEdge>();
        MinPriorityQueue<NodeEdge> queue = new MinPriorityQueue<NodeEdge>();
        for (int i = 0; i < path.Length; i++)
        {
            path[i] = -1;
            cost[i] = float.MaxValue;
        }

        cost[_start.id] = HCost(_start, _end);
        foreach (NodeEdge currentEdge in _start.adjacencyList)
        {
            queue.Add(currentEdge, cost[_start.id] + currentEdge.cost + HCost(Node.nodeList[currentEdge.to.value], _end));
        }

        bool pathFound = false;
        while (queue.count > 0)
        {
            NodeEdge currentEdge = queue.Pop();
            alreadySearched.Add(currentEdge);
            float newCost = cost[currentEdge.from.value] + currentEdge.cost; 
            
            if (cost[currentEdge.to.value] <= newCost) continue;

            cost[currentEdge.to.value] = newCost;
            path[currentEdge.to.value] = currentEdge.from.value;

            Node newNode = Node.nodeList[currentEdge.to.value];
            if (newNode.id == _end.id)
            {
                pathFound = true;
                continue;
            }

            foreach (NodeEdge adjacentEdge in newNode.adjacencyList)
            {
                if (!alreadySearched.Contains(adjacentEdge) && !queue.Contains(adjacentEdge))
                {
                    queue.Add(adjacentEdge, newCost + adjacentEdge.cost + HCost(Node.nodeList[adjacentEdge.to.value], _end));
                }
            }
        }
        
        if (pathFound == false)
        {
            Debug.Log("Error: No path found");
            return rv;
        }

        int pathBack = _end.id;
        rv.Add(pathBack);
        while (pathBack != _start.id)
        {
            pathBack = path[pathBack];
            rv.Insert(0, pathBack);
        }

        return rv;
    }

    float HCost(Node _start, Node _target)
    {
        Vector2 vector1 = _start.position;
        Vector2 vector2 = _target.position;
        return (vector1 - vector2).magnitude;
    }
}

[Serializable]
public class IndexTriangle
{
    public Index point1;
    public Index point2;
    public Index point3;

    public IndexTriangle(Index _point1,  Index _point2, Index _point3)
    {
        point1 = _point1;
        point2 = _point2;
        point3 = _point3;
    }
}

class MinPriorityQueue<T>
{
    class QueueItem
    {
        public T item;
        public float priority;
        public QueueItem(T _item, float _priority)
        {
            item = _item;
            priority = _priority;
        }
    }

    List<QueueItem> queue = new List<QueueItem>();
    public int count
    {
        get
        {
            return queue.Count;
        }
    }

    public void Add(T _item, float _priority)
    {
        queue.Add(new QueueItem(_item, _priority));
        queue = queue.OrderBy(n => n.priority).ToList();
    }

    public T Peek()
    {
        if (queue.Count == 0) return default(T);
        return queue[0].item;
    }

    public T Pop()
    {
        if (queue.Count == 0) return default(T);
        T rv = queue[0].item;
        queue.RemoveAt(0);
        return rv;
    }

    public bool Contains(T _item)
    {
        foreach (QueueItem currentItem in queue)
        {
            if (_item.Equals(currentItem.item)) return true;
        }
        return false;
    }
}
