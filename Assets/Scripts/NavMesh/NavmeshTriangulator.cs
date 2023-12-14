using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngineInternal;

public class NavmeshTriangulator : MonoBehaviour
{
    public List<Triangle> DelaunayTriangulation(List<List<Vertex>> pointListArray)
    {
        List<Triangle> triangles = TriangulatePoints(pointListArray);
        List<HalfEdge> edges = TriangulationSystem.TrianglesToHalfEdges(triangles);
        int safety = 0;

        int flippedEdges = 0;

        while (true)
        {
            safety += 1;

            if (safety > 100000)
            {
                Debug.Log("Stuck in endless loop");

                break;
            }

            bool hasFlippedEdge = false;

            for (int i = 0; i < edges.Count; i++)
            {
                HalfEdge thisEdge = edges[i];

                if (thisEdge.opposite == null)
                {
                    continue;
                }

                Vector2 aPos = thisEdge.vertex.position;
                Vector2 bPos = thisEdge.next.vertex.position;
                Vector2 cPos = thisEdge.previous.vertex.position;
                Vector2 dPos = thisEdge.opposite.next.vertex.position;

                if (TriangulationSystem.IsPointInsideOutsideOrOnCircle(aPos, bPos, cPos, dPos) < 0f)
                {
                    if (TriangulationSystem.IsQuadrilateralConvex(aPos, bPos, cPos, dPos))
                    {
                        if (TriangulationSystem.IsPointInsideOutsideOrOnCircle(bPos, cPos, dPos, aPos) < 0f)
                        {
                            continue;
                        }

                        flippedEdges += 1;
                        hasFlippedEdge = true;
                        thisEdge.Flip();
                    }
                }
            }

            if (!hasFlippedEdge)
            {
                Debug.Log("Found a delaunay triangulation");
                Debug.Log(flippedEdges);

                break;
            }
        }

        DisplayLines(triangles, Color.red, 6f);
        return triangles;
    }

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

                    if (TriangulationSystem.AreEdgesIntersecting(edgeToMidpoint, edges[k]))
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

        DisplayLines(triangles, Color.green, 12f);
        return triangles;
    }


    void DisplayLines(List<Triangle> _triangles, Color color, float time)
    {
        foreach (Triangle currentTriangle in _triangles)
        {
            Vector3 p1 = new Vector3(currentTriangle.point1.position.x, currentTriangle.point1.position.y);
            Vector3 p2 = new Vector3(currentTriangle.point2.position.x, currentTriangle.point2.position.y);
            Vector3 p3 = new Vector3(currentTriangle.point3.position.x, currentTriangle.point3.position.y);

            Debug.DrawLine(p1, p2, color, time);
            Debug.DrawLine(p2, p3, color, time);
            Debug.DrawLine(p3, p1, color, time);
        }
    }

    
}
