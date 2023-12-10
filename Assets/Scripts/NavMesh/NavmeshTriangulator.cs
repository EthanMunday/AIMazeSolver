using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavmeshTriangulator : MonoBehaviour
{
    public List<Triangle> TriangulatePoints(List<List<Vertex>> pointListAray)
    {
        List<Triangle> triangles = new List<Triangle>();
        List<Vertex> combinedPoints = new List<Vertex>();
        foreach (List<Vertex> pointList in pointListAray) combinedPoints.AddRange(pointList);

        combinedPoints = combinedPoints.OrderBy(n => n.position.x).ToList();
        Triangle newTriangle = new Triangle(combinedPoints[0], combinedPoints[1], combinedPoints[2]);

        triangles.Add(newTriangle);

        List<Edge> edges = new List<Edge>
        {
            new Edge(newTriangle.point1, newTriangle.point2),
            new Edge(newTriangle.point2, newTriangle.point3),
            new Edge(newTriangle.point3, newTriangle.point1)
        };

        for (int i = 3; i < combinedPoints.Count; i++)
        {
            Vector3 currentPoint = combinedPoints[i].position;
            List<Edge> newEdges = new List<Edge>();
            for (int j = 0; j < edges.Count; j++)
            {
                Edge currentEdge = edges[j];
                Vector2 midPoint = (currentEdge.point1.position + currentEdge.point2.position) / 2f;
                Edge edgeToMidpoint = new Edge(new Vertex(currentPoint), new Vertex (midPoint));
                bool canSeeEdge = true;

                for (int k = 0; k < edges.Count; k++)
                {
                    if (k == j)
                    {
                        continue;
                    }

                    if (AreEdgesIntersecting(edgeToMidpoint, edges[k]))
                    {
                        canSeeEdge = false;

                        break;
                    }
                }

                if (canSeeEdge)
                {
                    Edge edgeToPoint1 = new Edge(currentEdge.point1, new Vertex(currentPoint));
                    Edge edgeToPoint2 = new Edge(currentEdge.point2, new Vertex(currentPoint));
                    newEdges.Add(edgeToPoint1);
                    newEdges.Add(edgeToPoint2);
                    Triangle newTri = new Triangle(edgeToPoint1.point1, edgeToPoint1.point2, edgeToPoint2.point1);
                    triangles.Add(newTri);
                }
            }
            for (int j = 0; j < newEdges.Count; j++)
            {
                edges.Add(newEdges[j]);
            }
        }
        DisplayLines(triangles);
        return triangles;
    }



    private static bool AreEdgesIntersecting(Edge edge1, Edge edge2)
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

    void DisplayLines(List<Triangle> _triangles)
    {
        foreach (Triangle currentTriangle in _triangles)
        {
            Vector3 p1 = new Vector3(currentTriangle.point1.position.x, currentTriangle.point1.position.y);
            Vector3 p2 = new Vector3(currentTriangle.point2.position.x, currentTriangle.point2.position.y);
            Vector3 p3 = new Vector3(currentTriangle.point3.position.x, currentTriangle.point3.position.y);

            Debug.DrawLine(p1, p2, Color.green, 8f);
            Debug.DrawLine(p2, p3, Color.green, 8f);
            Debug.DrawLine(p3, p1, Color.green, 8f);
        }
    }
}
