using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public struct Vertex
{
    public Vector2 position;
    public HalfEdge halfEdge;

    public Vertex(Vector2 _position)
    {
        position = _position;
        halfEdge = null;
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
    public Vertex vertex;
    public Triangle triangle;

    public HalfEdge next;
    public HalfEdge previous;
    public HalfEdge opposite;

    public HalfEdge(Vertex _vertex)
    {
        vertex = _vertex;
    }
    public void Flip()
    {
        HalfEdge oppositeNext = opposite.next;
        HalfEdge oppositePrevious = opposite.previous;
        Vertex nextVertex = next.vertex;
        Vertex previousVertex = previous.vertex;
        Vertex oppositeNextVertex = opposite.next.vertex;

        vertex.halfEdge = next;
        previousVertex.halfEdge = opposite.next;

        next = previous;
        previous = oppositeNext;

        next.next = opposite;
        next.previous = oppositePrevious;

        previous.next = oppositeNext;
        previous.previous = this;

        opposite.next = oppositePrevious;
        opposite.previous = next;

        oppositeNext.next = this;
        oppositeNext.previous = previous;

        oppositePrevious.next = next;
        oppositePrevious.previous = opposite;

        vertex = nextVertex;
        next.vertex = nextVertex;
        previous.vertex = previousVertex;
        opposite.vertex = oppositeNextVertex;
        oppositeNext.vertex = oppositeNextVertex;
        oppositePrevious.vertex = vertex;

        Triangle triangle1 = triangle;
        Triangle triangle2 = opposite.triangle ;

        previous.triangle = triangle1;
        oppositeNext.triangle = triangle1;

        next.triangle = triangle2;
        opposite.triangle = triangle2;
        oppositePrevious.triangle = triangle2;


        triangle1.point1 = nextVertex;
        triangle1.point2 = previousVertex;
        triangle1.point3 = oppositeNextVertex;

        triangle2.point1 = nextVertex;
        triangle2.point2 = oppositeNextVertex;
        triangle2.point3 = vertex;

        triangle1.startingHalfEdge = previous;
        triangle2.startingHalfEdge = opposite;
    }
}

public class Triangle
{
    public Vertex point1, point2, point3;
    public HalfEdge startingHalfEdge;

    public Triangle(Vertex _point1, Vertex _point2, Vertex _point3)
    {
        point1 = _point1;
        point2 = _point2;
        point3 = _point3;
    }

    public void OrientClockwiseTriangles()
    {
        Vector2 p1 = point1.position;
        Vector2 p2 = point2.position;
        Vector2 p3 = point3.position;

        float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

        if (determinant > 0f)
        {
            point1.position = p2;
            point2.position = p1;
        }
    }
}

public class TriangulationSystem
{
    public static bool AreEdgesIntersecting(Edge edge1, Edge edge2)
    {
        Vector2 l1_p1 = new Vector2(edge1.point1.position.x, edge1.point1.position.y);
        Vector2 l1_p2 = new Vector2(edge1.point2.position.x, edge1.point2.position.y);

        Vector2 l2_p1 = new Vector2(edge2.point1.position.x, edge2.point1.position.y);
        Vector2 l2_p2 = new Vector2(edge2.point2.position.x, edge2.point2.position.y);

        bool isIntersecting = AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, true);

        return isIntersecting;
    }

    public static bool AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2, bool shouldIncludeEndPoints)
    {
        bool isIntersecting = false;
        float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

        if (denominator != 0f)
        {
            float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
            float u_b = ((l1_p2.x - l1_p1.x) * (l1_p1.y - l2_p1.y) - (l1_p2.y - l1_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

            if (shouldIncludeEndPoints)
            {
                if (u_a >= 0f && u_a <= 1f && u_b >= 0f && u_b <= 1f)
                {
                    isIntersecting = true;
                }
            }
            else
            {
                if (u_a > 0f && u_a < 1f && u_b > 0f && u_b < 1f)
                {
                    isIntersecting = true;
                }
            }

        }
        return isIntersecting;
    }

    public static List<HalfEdge> TrianglesToHalfEdges(List<Triangle> _triangleList)
    {
        List<HalfEdge> rv = new List<HalfEdge>();
        foreach (Triangle triangle in _triangleList) triangle.OrientClockwiseTriangles();

        for (int i = 0; i < _triangleList.Count; i++)
        {
            Triangle t = _triangleList[i];

            HalfEdge he1 = new HalfEdge(t.point1);
            HalfEdge he2 = new HalfEdge(t.point2);
            HalfEdge he3 = new HalfEdge(t.point3);

            he1.next = he2;
            he2.next = he3;
            he3.next = he1;

            he1.previous = he3;
            he2.previous = he1;
            he3.previous = he2;

            he1.vertex.halfEdge = he2;
            he2.vertex.halfEdge = he3;
            he3.vertex.halfEdge = he1;

            t.startingHalfEdge = he1;

            he1.triangle = t;
            he2.triangle = t;
            he3.triangle = t;

            rv.Add(he1);
            rv.Add(he2);
            rv.Add(he3);
        }

        for (int i = 0; i < rv.Count; i++)
        {
            HalfEdge he = rv[i];

            Vertex goingToVertex = he.vertex;
            Vertex goingFromVertex = he.previous.vertex;

            for (int j = 0; j < rv.Count; j++)
            {
                //Dont compare with itself
                if (i == j)
                {
                    continue;
                }

                HalfEdge heOpposite = rv[j];

                //Is this edge going between the vertices in the opposite direction
                if (goingFromVertex.position == heOpposite.vertex.position && goingToVertex.position == heOpposite.previous.vertex.position)
                {
                    he.opposite = heOpposite;

                    break;
                }
            }
        }

        return rv;
    }

    public static float IsPointInsideOutsideOrOnCircle(Vector2 aVec, Vector2 bVec, Vector2 cVec, Vector2 dVec)
    {
        float a = aVec.x - dVec.x;
        float d = bVec.x - dVec.x;
        float g = cVec.x - dVec.x;

        float b = aVec.y - dVec.y;
        float e = bVec.y - dVec.y;
        float h = cVec.y - dVec.y;

        float c = a * a + b * b;
        float f = d * d + e * e;
        float i = g * g + h * h;

        float determinant = (a * e * i) + (b * f * g) + (c * d * h) - (g * e * c) - (h * f * a) - (i * d * b);

        return determinant;
    }

    public static bool IsQuadrilateralConvex(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        bool isConvex = false;

        bool abc = IsTriangleClockwise(a, b, c);
        bool abd = IsTriangleClockwise(a, b, d);
        bool bcd = IsTriangleClockwise(b, c, d);
        bool cad = IsTriangleClockwise(c, a, d);

        if (abc && abd && bcd & !cad)
        {
            isConvex = true;
        }
        else if (abc && abd && !bcd & cad)
        {
            isConvex = true;
        }
        else if (abc && !abd && bcd & cad)
        {
            isConvex = true;
        }
        else if (!abc && !abd && !bcd & cad)
        {
            isConvex = true;
        }
        else if (!abc && !abd && bcd & !cad)
        {
            isConvex = true;
        }
        else if (!abc && abd && !bcd & !cad)
        {
            isConvex = true;
        }


        return isConvex;
    }

    public static bool IsTriangleClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;
        return determinant <= 0.0f;
    }
}
