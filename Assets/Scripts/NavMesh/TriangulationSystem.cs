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

        Triangle t1 = savedThis.triangle;
        Triangle t2 = savedOpposite.triangle;

        triangle = t1;
        savedPrevious.triangle = t1;
        oppositeNext.triangle = t1;

        savedNext.triangle = t2;
        savedOpposite.triangle = t2;
        oppositePrevious.triangle = t2;

        t1.point1 = nextVertex;
        t1.point2 = previousVertex;
        t1.point3 = oppositeNextVertex;

        t2.point1 = nextVertex;
        t2.point2 = oppositeNextVertex;
        t2.point3 = savedVertex;

        t1.startingHalfEdge = savedPrevious;
        t2.startingHalfEdge = savedOpposite;

        //HalfEdge savedNext = next;
        //HalfEdge savedPrevious = previous;
        //HalfEdge savedOpposite = opposite;
        //HalfEdge oppositeNext = opposite.next;
        //HalfEdge oppositePrevious = opposite.previous;
        //Vertex savedVertex = vertex;
        //Vertex nextVertex = next.vertex;
        //Vertex previousVertex = previous.vertex;
        //Vertex oppositeNextVertex = opposite.next.vertex;

        //vertex.halfEdge = next;
        //previousVertex.halfEdge = opposite.next;

        //next = savedPrevious;
        //previous = oppositeNext;

        //next.next = savedOpposite;
        //next.previous = oppositePrevious;

        //previous.next = oppositeNext;
        //previous.previous = this;

        //opposite.next = oppositePrevious;
        //opposite.previous = savedNext;

        //oppositeNext.next = this;
        //oppositeNext.previous = savedPrevious;

        //oppositePrevious.next = savedNext;
        //oppositePrevious.previous = savedOpposite;

        //vertex = nextVertex;
        //next.vertex = nextVertex;
        //previous.vertex = previousVertex;
        //opposite.vertex = oppositeNextVertex;
        //oppositeNext.vertex = oppositeNextVertex;
        //oppositePrevious.vertex = savedVertex;

        //Triangle triangle1 = this.triangle;
        //Triangle triangle2 = savedOpposite.triangle ;

        //previous.triangle = triangle1;
        //oppositeNext.triangle = triangle1;

        //next.triangle = triangle2;
        //opposite.triangle = triangle2;
        //oppositePrevious.triangle = triangle2;


        //triangle1.point1 = nextVertex;
        //triangle1.point2 = previousVertex;
        //triangle1.point3 = oppositeNextVertex;

        //triangle2.point1 = nextVertex;
        //triangle2.point2 = oppositeNextVertex;
        //triangle2.point3 = savedVertex;

        //triangle1.startingHalfEdge = savedPrevious;
        //triangle2.startingHalfEdge = savedOpposite;
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
    public static float IsAPointLeftOfVectorOrOnTheLine(Vector2 a, Vector2 b, Vector2 p)
    {
        float determinant = (a.x - p.x) * (b.y - p.y) - (a.y - p.y) * (b.x - p.x);

        return determinant;
    }

    public static bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
    {
        bool isWithinTriangle = false;

        float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

        float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
        float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
        float c = 1 - a - b;

        if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
        {
            isWithinTriangle = true;
        }

        return isWithinTriangle;
    }

    public static List<Triangle> TriangulateConvexPolygon(List<Vertex> convexHullpoints)
    {
        List<Triangle> triangles = new List<Triangle>();

        for (int i = 2; i < convexHullpoints.Count; i++)
        {
            Vertex a = convexHullpoints[0];
            Vertex b = convexHullpoints[i - 1];
            Vertex c = convexHullpoints[i];

            triangles.Add(new Triangle(a, b, c));
        }

        return triangles;
    }

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

    public static bool IsEdgePartOfTriangulation(List<Triangle> triangulation, Vector2 p1, Vector2 p2)
    {
        for (int i = 0; i < triangulation.Count; i++)
        {
            Vector2 t_p1 = triangulation[i].point1.position;
            Vector2 t_p2 = triangulation[i].point2.position;
            Vector2 t_p3 = triangulation[i].point3.position;

            if ((t_p1 == p1 && t_p2 == p2) || (t_p1 == p2 && t_p2 == p1))
            {
                return true;
            }
            if ((t_p2 == p1 && t_p3 == p2) || (t_p2 == p2 && t_p3 == p1))
            {
                return true;
            }
            if ((t_p3 == p1 && t_p1 == p2) || (t_p3 == p2 && t_p1 == p1))
            {
                return true;
            }
        }

        return false;
    }

    public static List<HalfEdge> FindIntersectingEdges(List<Triangle> triangulation, Vector2 p1, Vector2 p2)
    {
        List<HalfEdge> intersectingEdges = new List<HalfEdge>();


        for (int i = 0; i < triangulation.Count; i++)
        {
            //The edges the triangle consists of
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
                if (intersectingEdges[i] == e || intersectingEdges[i].opposite == e)
                {
                    return;
                }
            }

            intersectingEdges.Add(e);
        }
    }

    public static List<HalfEdge> RemoveIntersectingEdges(Vector3 v_i, Vector3 v_j, List<HalfEdge> intersectingEdges)
    {
        List<HalfEdge> newEdges = new List<HalfEdge>();

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

                if (IsEdgeCrossingEdge(v_i, v_j, v_m, v_n))
                {
                    intersectingEdges.Add(e);
                }
                else
                {
                    newEdges.Add(e);
                }
            }
        }

        return newEdges;
    }
    static bool IsEdgeCrossingEdge(Vector2 e1_p1, Vector2 e1_p2, Vector2 e2_p1, Vector2 e2_p2)
    {

        if (e1_p1 == e2_p1 || e1_p1 == e2_p2 || e1_p2 == e2_p1 || e1_p2 == e2_p2)
        {
            return false;
        }

        if (!AreLinesIntersecting(e1_p1, e1_p2, e2_p1, e2_p2, false))
        {
            return false;
        }

        return true;
    }
}
