using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*
 * Most of the code in this class has been adapted from (Nordeus, 2019/2023)
 * See LICENSES.cs for license and reference
 */

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
        HalfEdge savedThis = this;
        HalfEdge savedNext = next;
        HalfEdge savedPrevious = previous;
        HalfEdge savedOpposite = opposite;
        HalfEdge oppositeNext = opposite.next;
        HalfEdge oppositePrevious = opposite.previous;
        Vertex savedVertex = vertex;
        Vertex nextVertex = next.vertex;
        Vertex previousVertex = previous.vertex;
        Vertex oppositeNextVertex = opposite.next.vertex;

        savedVertex.halfEdge = next;
        previousVertex.halfEdge = opposite.next;

        next = savedPrevious;
        previous = oppositeNext;
        savedNext.next = savedOpposite;
        savedNext.previous = oppositePrevious;
        savedPrevious.next = oppositeNext;
        savedPrevious.previous = savedThis;
        savedOpposite.next = oppositePrevious;
        savedOpposite.previous = savedNext;
        oppositeNext.next = savedThis;
        oppositeNext.previous = savedPrevious;
        oppositePrevious.next = savedNext;
        oppositePrevious.previous = savedOpposite;

        vertex = nextVertex;
        savedNext.vertex = nextVertex;
        savedPrevious.vertex = previousVertex;
        savedOpposite.vertex = oppositeNextVertex;
        oppositeNext.vertex = oppositeNextVertex;
        oppositePrevious.vertex = savedVertex;

        Triangle thisTriangle = savedThis.triangle;
        Triangle oppositeTriangle = savedOpposite.triangle;

        triangle = thisTriangle;
        savedPrevious.triangle = thisTriangle;
        oppositeNext.triangle = thisTriangle;

        savedNext.triangle = oppositeTriangle;
        savedOpposite.triangle = oppositeTriangle;
        oppositePrevious.triangle = oppositeTriangle;

        thisTriangle.point1 = nextVertex;
        thisTriangle.point2 = previousVertex;
        thisTriangle.point3 = oppositeNextVertex;

        oppositeTriangle.point1 = nextVertex;
        oppositeTriangle.point2 = oppositeNextVertex;
        oppositeTriangle.point3 = savedVertex;

        thisTriangle.startingHalfEdge = savedPrevious;
        oppositeTriangle.startingHalfEdge = savedOpposite;
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
    public static bool IsPointInTriangle(Vector2 _point1, Vector2 _point2, Vector2 _point3, Vector2 _point4)
    {
        float denominator = ((_point2.y - _point3.y) * (_point1.x - _point3.x) + (_point3.x - _point2.x) * (_point1.y - _point3.y));

        float a = ((_point2.y - _point3.y) * (_point4.x - _point3.x) + (_point3.x - _point2.x) * (_point4.y - _point3.y)) / denominator;
        float b = ((_point3.y - _point1.y) * (_point4.x - _point3.x) + (_point1.x - _point3.x) * (_point4.y - _point3.y)) / denominator;
        float c = 1 - a - b;

        if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f) return true;

        return false;
    }

    public static bool AreEdgesIntersecting(Edge edge1, Edge edge2)
    {
        Vector2 line1A = edge1.point1.position;
        Vector2 line1B = edge1.point2.position;
        Vector2 line2A = edge2.point1.position;
        Vector2 line2B = edge2.point2.position;

        return AreLinesIntersecting(line1A, line1B, line2A, line2B, true);
    }

    public static bool AreLinesIntersecting(Vector2 _line1A, Vector2 _line1B, Vector2 _line2A, Vector2 _line2B, bool includeEndPoints)
    {
        float denominator = (_line2B.y - _line2A.y) * (_line1B.x - _line1A.x) - (_line2B.x - _line2A.x) * (_line1B.y - _line1A.y);

        if (denominator != 0f)
        {
            float determinantA = ((_line2B.x - _line2A.x) * (_line1A.y - _line2A.y) - (_line2B.y - _line2A.y) * (_line1A.x - _line2A.x)) / denominator;
            float determinantB = ((_line1B.x - _line1A.x) * (_line1A.y - _line2A.y) - (_line1B.y - _line1A.y) * (_line1A.x - _line2A.x)) / denominator;

            if (includeEndPoints)
            {
                if (determinantA >= 0f && determinantA <= 1f && determinantB >= 0f && determinantB <= 1f) return true;
            }
            else
            {
                if (determinantA > 0f && determinantA < 1f && determinantB > 0f && determinantB < 1f) return true;
            }

        }
        return false;
    }

    public static List<HalfEdge> TrianglesToHalfEdges(List<Triangle> _triangleList)
    {
        List<HalfEdge> rv = new();
        foreach (Triangle triangle in _triangleList) triangle.OrientClockwiseTriangles();

        for (int i = 0; i < _triangleList.Count; i++)
        {
            Triangle currentTriangle = _triangleList[i];

            HalfEdge halfEdge1 = new HalfEdge(currentTriangle.point1);
            HalfEdge halfEdge2 = new HalfEdge(currentTriangle.point2);
            HalfEdge halfEdge3 = new HalfEdge(currentTriangle.point3);

            halfEdge1.next = halfEdge2;
            halfEdge2.next = halfEdge3;
            halfEdge3.next = halfEdge1;
            halfEdge1.previous = halfEdge3;
            halfEdge2.previous = halfEdge1;
            halfEdge3.previous = halfEdge2;
            halfEdge1.vertex.halfEdge = halfEdge2;
            halfEdge2.vertex.halfEdge = halfEdge3;
            halfEdge3.vertex.halfEdge = halfEdge1;

            currentTriangle.startingHalfEdge = halfEdge1;

            halfEdge1.triangle = currentTriangle;
            halfEdge2.triangle = currentTriangle;
            halfEdge3.triangle = currentTriangle;

            rv.Add(halfEdge1);
            rv.Add(halfEdge2);
            rv.Add(halfEdge3);
        }

        for (int i = 0; i < rv.Count; i++)
        {
            HalfEdge currentHalfEdge = rv[i];

            Vertex thisVertex = currentHalfEdge.vertex;
            Vertex previousVertex = currentHalfEdge.previous.vertex;

            for (int j = 0; j < rv.Count; j++)
            {
                if (i == j) continue;

                HalfEdge oppositeVertex = rv[j];

                if (previousVertex.position == oppositeVertex.vertex.position && thisVertex.position == oppositeVertex.previous.vertex.position)
                {
                    currentHalfEdge.opposite = oppositeVertex;
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
        bool abc = IsTriangleClockwise(a, b, c);
        bool abd = IsTriangleClockwise(a, b, d);
        bool bcd = IsTriangleClockwise(b, c, d);
        bool cad = IsTriangleClockwise(c, a, d);

        int addedBool = abd ? 1 : 0;
        addedBool += bcd ? 1 : 0;
        addedBool += cad ? 1 : 0;

        if (addedBool == 1 & !abc || addedBool == 2 & abc ) return true;

        return false;
    }

    public static bool IsTriangleClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;
        return determinant <= 0.0f;
    }

    public static bool IsEdgePartOfTriangulation(List<Triangle> triangulation, Vector2 p1, Vector2 p2)
    {
        for (int i = 0; i < triangulation.Count; i++)
        {
            Vector2 t_p1 = triangulation[i].point1.position;
            Vector2 t_p2 = triangulation[i].point2.position;
            Vector2 t_p3 = triangulation[i].point3.position;

            if ((t_p1 == p1 && t_p2 == p2) || (t_p1 == p2 && t_p2 == p1)) return true;
            if ((t_p2 == p1 && t_p3 == p2) || (t_p2 == p2 && t_p3 == p1)) return true;
            if ((t_p3 == p1 && t_p1 == p2) || (t_p3 == p2 && t_p1 == p1)) return true;
        }

        return false;
    }

    public static List<HalfEdge> FindIntersectingEdges(List<Triangle> triangulation, Vector2 p1, Vector2 p2)
    {
        List<HalfEdge> intersectingEdges = new();

        for (int i = 0; i < triangulation.Count; i++)
        {
            HalfEdge e1 = triangulation[i].startingHalfEdge;
            HalfEdge e2 = e1.next;
            HalfEdge e3 = e2.next;

            TryAddEdgeToIntersectingEdges(e1, p1, p2, intersectingEdges);
            TryAddEdgeToIntersectingEdges(e2, p1, p2, intersectingEdges);
            TryAddEdgeToIntersectingEdges(e3, p1, p2, intersectingEdges);
        }

        return intersectingEdges;
    }

    private static void TryAddEdgeToIntersectingEdges(HalfEdge e, Vector3 p1, Vector3 p2, List<HalfEdge> intersectingEdges)
    {
        Vector3 e_p1 = e.vertex.position;
        Vector3 e_p2 = e.previous.vertex.position;

        if (IsEdgeCrossingEdge(e_p1, e_p2, p1, p2))
        {
            for (int i = 0; i < intersectingEdges.Count; i++)
            {
                if (intersectingEdges[i] == e || intersectingEdges[i].opposite == e) return;
            }

            intersectingEdges.Add(e);
        }
    }

    public static List<HalfEdge> RemoveIntersectingEdges(Vector3 v_i, Vector3 v_j, List<HalfEdge> intersectingEdges)
    {
        List<HalfEdge> newEdges = new();
        int safety = 0;

        while (intersectingEdges.Count > 0)
        {
            safety += 1;

            if (safety > 10000)
            {
                Debug.Log("Stuck in infinite loop when fixing constrained edges");

                break;
            }

            HalfEdge e = intersectingEdges[0];
            intersectingEdges.RemoveAt(0);

            Vector3 v_k = e.vertex.position;
            Vector3 v_l = e.previous.vertex.position;
            Vector3 v_third_pos = e.next.vertex.position;
            Vector3 v_opposite_pos = e.opposite.next.vertex.position;

            if (!IsQuadrilateralConvex(v_k, v_l, v_third_pos, v_opposite_pos))
            {
                intersectingEdges.Add(e);

                continue;
            }
            else
            {
                e.Flip();
                Vector3 v_m = e.vertex.position;
                Vector3 v_n = e.previous.vertex.position;

                if (IsEdgeCrossingEdge(v_i, v_j, v_m, v_n)) intersectingEdges.Add(e);
                else newEdges.Add(e);
            }
        }

        return newEdges;
    }
    static bool IsEdgeCrossingEdge(Vector2 e1_p1, Vector2 e1_p2, Vector2 e2_p1, Vector2 e2_p2)
    {
        if (e1_p1 == e2_p1 || e1_p1 == e2_p2 || e1_p2 == e2_p1 || e1_p2 == e2_p2) return false;
        if (!AreLinesIntersecting(e1_p1, e1_p2, e2_p1, e2_p2, false)) return false;
        return true;
    }
}
