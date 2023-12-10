using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
    public Vector2 position;

    public Vertex(Vector2 _position)
    {
        position = _position;
    }
}

public class Edge
{
    public Vertex point1, point2;

    public Edge(Vertex _point1, Vertex _point2)
    {
        point1 = _point1;
        point2 = _point2;
    }
}

public class HalfEdge
{ 

}

public class Triangle
{
    public Vertex point1, point2, point3;

    public Triangle(Vertex _point1, Vertex _point2, Vertex _point3)
    {
        point1 = _point1;
        point2 = _point2;
        point3 = _point3;
    }
}

public class TriangulationSystem
{
    
}

