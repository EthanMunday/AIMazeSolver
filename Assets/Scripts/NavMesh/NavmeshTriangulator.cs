using System.Collections.Generic;
using System.Linq;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class NavmeshTriangulator : MonoBehaviour
{
    public List<Triangle> ConstrainedDelaunay(List<NavmeshPoints> _pointsList)
    {
        List<List<Vertex>> exlclusionVertexList = new List<List<Vertex>>();
        List<Triangle> delaunayTriangulation = DelaunayTriangulation(_pointsList);
        foreach (NavmeshPoints currentPoints in _pointsList)
        {
            if (!currentPoints.isInside) exlclusionVertexList.Add(currentPoints.points);
        }
        List<Triangle> constrainedDelaunayTriangulation = new List<Triangle>();
        constrainedDelaunayTriangulation = AddConstraints(delaunayTriangulation, exlclusionVertexList);
        DisplayLines(constrainedDelaunayTriangulation, Color.blue, 7f);
        return constrainedDelaunayTriangulation;
    }

    private static List<Triangle> AddConstraints(List<Triangle> _triangles, List<List<Vertex>> _constraints)
    {
        foreach (List<Vertex> currentShape in _constraints)
        {
            for (int i = 0; i < currentShape.Count; i++)
            {
                Vector2 v_i, v_j;
                v_i = currentShape[i].position;
                if (i + 1 == currentShape.Count) v_j = currentShape[0].position;
                else v_j = currentShape[i + 1].position;
                if (TriangulationSystem.IsEdgePartOfTriangulation(_triangles, v_i, v_j))
                {
                    continue;
                }

                List<HalfEdge> intersectingEdges = TriangulationSystem.FindIntersectingEdges(_triangles, v_i, v_j);

                List<HalfEdge> newEdges = TriangulationSystem.RemoveIntersectingEdges(v_i, v_j, intersectingEdges);

                RestoreDelaunayTriangulation(v_i, v_j, newEdges);

            }

            RemoveSuperfluousTriangles(_triangles, currentShape);
        }

        return _triangles;
    }
    public List<Triangle> DelaunayTriangulation(List<NavmeshPoints> _pointsList)
    {
        List<Triangle> triangles = TriangulatePoints(_pointsList);
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

                Vector2 aPos, bPos, cPos, dPos;
                aPos = thisEdge.vertex.position;
                bPos = thisEdge.next.vertex.position;
                cPos = thisEdge.previous.vertex.position;
                dPos = thisEdge.opposite.next.vertex.position;

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
                Debug.Log("Found a delaunay triangulation with " + flippedEdges + " flipped edges");

                break;
            }
        }
        //DisplayLines(triangles, Color.green, 3f);
        return triangles;
    }

    public List<Triangle> TriangulatePoints(List<NavmeshPoints> _pointsList)
    {
        List<Triangle> triangles = new List<Triangle>();
        List<Vertex> combinedPoints = new List<Vertex>();
        foreach (NavmeshPoints currentPoints in _pointsList) combinedPoints.AddRange(currentPoints.points);

        combinedPoints = combinedPoints.OrderBy(n => n.position.x).ThenBy(n => n.position.y).ToList();
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
            Vector2 currentPoint = combinedPoints[i].position;
            List<Edge> newEdges = new List<Edge>();
            for (int j = 0; j < edges.Count; j++)
            {
                Edge currentEdge = edges[j];
                Vector2 midPoint = (currentEdge.point1.position + currentEdge.point2.position) / 2f;
                Edge edgeToMidpoint = new Edge(new Vertex(currentPoint), new Vertex(midPoint));
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
                    Edge edgeToPoint1, edgeToPoint2;
                    edgeToPoint1 = new Edge(currentEdge.point1, new Vertex(currentPoint));
                    edgeToPoint2 = new Edge(currentEdge.point2, new Vertex(currentPoint));
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
        //DisplayLines(triangles, Color.red, 5f);
        return triangles;
    }

    public List<Triangle> TriangulatePointsFromHull(List<NavmeshPoints> _pointsList)
    {
        List<Vertex> pointsOnConvexHull = GenerateHull(_pointsList);

        List<Triangle> triangles = TriangulationSystem.TriangulateConvexPolygon(pointsOnConvexHull);

        foreach (NavmeshPoints currentPoints in _pointsList)
        {
            for (int i = 0; i < currentPoints.points.Count; i++)
            {
                Vertex currentPoint = currentPoints.points[i];

                Vector2 p = currentPoint.position;

                for (int j = 0; j < triangles.Count; j++)
                {
                    Triangle t = triangles[j];

                    Vector2 p1 = t.point1.position;
                    Vector2 p2 = t.point2.position;
                    Vector2 p3 = t.point3.position;

                    if (TriangulationSystem.IsPointInTriangle(p1, p2, p3, p))
                    {
                        //Create 3 new triangles
                        Triangle t1 = new Triangle(t.point1, t.point2, currentPoint);
                        Triangle t2 = new Triangle(t.point2, t.point3, currentPoint);
                        Triangle t3 = new Triangle(t.point3, t.point1, currentPoint);

                        //Remove the old triangle
                        triangles.Remove(t);

                        //Add the new triangles
                        triangles.Add(t1);
                        triangles.Add(t2);
                        triangles.Add(t3);

                        break;
                    }
                }
            }
        }
        //DisplayLines(triangles, Color.red, 5f);
        return triangles;
    }

    public static List<Vertex> GenerateHull(List<NavmeshPoints> _navmeshPoints)
    {
        List<Vertex> vertexPoints = new List<Vertex>();
        foreach(NavmeshPoints currentPoints in _navmeshPoints)
        {
            vertexPoints.AddRange(currentPoints.points);
        }

        if (vertexPoints.Count == 3)
        {
            return vertexPoints;
        }

        if (vertexPoints.Count < 3)
        {
            return null;
        }
        List<Vertex> convexHull = new List<Vertex>();

        Vertex startVertex = vertexPoints[0];

        Vector3 startPos = startVertex.position;

        for (int i = 1; i < vertexPoints.Count; i++)
        {
            Vector3 testPos = vertexPoints[i].position;

            if (testPos.x < startPos.x || (Mathf.Approximately(testPos.x, startPos.x) && testPos.z < startPos.z))
            {
                startVertex = vertexPoints[i];

                startPos = startVertex.position;
            }
        }

        convexHull.Add(startVertex);
        vertexPoints.Remove(startVertex);

        Vertex currentPoint = convexHull[0];
        List<Vertex> colinearPoints = new List<Vertex>();
        int counter = 0;

        while (true)
        {
            if (counter == 2)
            {
                vertexPoints.Add(convexHull[0]);
            }

            Vertex nextPoint = vertexPoints[Random.Range(0, vertexPoints.Count)];

            Vector2 a = currentPoint.position;

            Vector2 b = nextPoint.position;

            for (int i = 0; i < vertexPoints.Count; i++)
            {
                if (vertexPoints[i].Equals(nextPoint))
                {
                    continue;
                }

                Vector2 c = vertexPoints[i].position;

                float relation = TriangulationSystem.IsAPointLeftOfVectorOrOnTheLine(a, b, c);

                float accuracy = 0.00001f;

                if (relation < accuracy && relation > -accuracy)
                {
                    colinearPoints.Add(vertexPoints[i]);
                }

                else if (relation < 0f)
                {
                    nextPoint = vertexPoints[i];

                    b = nextPoint.position;

                    colinearPoints.Clear();
                }
            }

            if (colinearPoints.Count > 0)
            {
                colinearPoints.Add(nextPoint);

                colinearPoints = colinearPoints.OrderBy(n => Vector3.SqrMagnitude(n.position - currentPoint.position)).ToList();

                convexHull.AddRange(colinearPoints);

                currentPoint = colinearPoints[colinearPoints.Count - 1];

                for (int i = 0; i < colinearPoints.Count; i++)
                {
                    vertexPoints.Remove(colinearPoints[i]);
                }

                colinearPoints.Clear();
            }
            else
            {
                convexHull.Add(nextPoint);

                vertexPoints.Remove(nextPoint);

                currentPoint = nextPoint;
            }

            if (currentPoint.Equals(convexHull[0]))
            {
                convexHull.RemoveAt(convexHull.Count - 1);

                break;
            }

            counter += 1;
        }

        return convexHull;
    }


    private static void RestoreDelaunayTriangulation(Vector2 v_i, Vector2 v_j, List<HalfEdge> newEdges)
    {
        int safety = 0;

        int flippedEdges = 0;

        while (true)
        {
            safety += 1;

            if (safety > 100000)
            {
                Debug.Log("Stuck in endless loop when delaunay after fixing constrained edges");

                break;
            }

            bool hasFlippedEdge = false;

            for (int j = 0; j < newEdges.Count; j++)
            {
                HalfEdge currentEdge = newEdges[j];

                Vector2 v_k = currentEdge.vertex.position;
                Vector2 v_l = currentEdge.previous.vertex.position;

                if ((v_k == v_i && v_l == v_j) || (v_l == v_i && v_k == v_j))
                {
                    continue;
                }

                Vector2 v_third_pos = currentEdge.next.vertex.position;
                Vector2 v_opposite_pos = currentEdge.opposite.next.vertex.position;

                float circleTestValue = TriangulationSystem.IsPointInsideOutsideOrOnCircle(v_k, v_l, v_third_pos, v_opposite_pos);

                if (circleTestValue < 0f)
                {
                    if (TriangulationSystem.IsQuadrilateralConvex(v_k, v_l, v_third_pos, v_opposite_pos))
                    {
                        if (TriangulationSystem.IsPointInsideOutsideOrOnCircle(v_opposite_pos, v_l, v_third_pos, v_k) <= circleTestValue)
                        {
                            continue;
                        }

                        hasFlippedEdge = true;

                        currentEdge.Flip();

                        flippedEdges += 1;
                    }
                }
            }

            if (!hasFlippedEdge)
            {
                Debug.Log("Found a constrained delaunay triangulation in " + flippedEdges + " flips");

                break;
            }
        }
    }

    
    private static void RemoveSuperfluousTriangles(List<Triangle> triangulation, List<Vertex> constraints)
    {
        if (constraints.Count < 3)
        {
            return;
        }

        Triangle borderTriangle = null;

        Vector2 constrained_p1 = constraints[0].position;
        Vector2 constrained_p2 = constraints[1].position;

        for (int i = 0; i < triangulation.Count; i++)
        {
            HalfEdge e1 = triangulation[i].startingHalfEdge;
            HalfEdge e2 = e1.next;
            HalfEdge e3 = e2.next;

            if (e1.vertex.position == constrained_p2 && e1.previous.vertex.position == constrained_p1)
            {
                borderTriangle = triangulation[i];
                break;
            }
            if (e2.vertex.position == constrained_p2 && e2.previous.vertex.position == constrained_p1)
            {
                borderTriangle = triangulation[i];
                break;
            }
            if (e3.vertex.position == constrained_p2 && e3.previous.vertex.position == constrained_p1)
            {
                borderTriangle = triangulation[i];
                break;
            }
        }

        if (borderTriangle == null)
        {
            return;
        }


        List<Triangle> trianglesToBeDeleted = new List<Triangle>();

        List<Triangle> neighborsToCheck = new List<Triangle>();

        neighborsToCheck.Add(borderTriangle);

        int safety = 0;

        while (true)
        {
            safety += 1;

            if (safety > 10000)
            {
                Debug.Log("Stuck in infinite loop when deleteing superfluous triangles");

                break;
            }

            if (neighborsToCheck.Count == 0)
            {
                break;
            }

            Triangle t = neighborsToCheck[0];

            neighborsToCheck.RemoveAt(0);

            trianglesToBeDeleted.Add(t);

            HalfEdge e1 = t.startingHalfEdge;
            HalfEdge e2 = e1.next;
            HalfEdge e3 = e2.next;

            if (
                e1.opposite != null &&
                !trianglesToBeDeleted.Contains(e1.opposite.triangle) &&
                !neighborsToCheck.Contains(e1.opposite.triangle) &&
                !IsAnEdgeAConstraint(e1.vertex.position, e1.previous.vertex.position, constraints))
            {
                neighborsToCheck.Add(e1.opposite.triangle);
            }
            if (
                e2.opposite != null &&
                !trianglesToBeDeleted.Contains(e2.opposite.triangle) &&
                !neighborsToCheck.Contains(e2.opposite.triangle) &&
                !IsAnEdgeAConstraint(e2.vertex.position, e2.previous.vertex.position, constraints))
            {
                neighborsToCheck.Add(e2.opposite.triangle);
            }
            if (
                e3.opposite != null &&
                !trianglesToBeDeleted.Contains(e3.opposite.triangle) &&
                !neighborsToCheck.Contains(e3.opposite.triangle) &&
                !IsAnEdgeAConstraint(e3.vertex.position, e3.previous.vertex.position, constraints))
            {
                neighborsToCheck.Add(e3.opposite.triangle);
            }
        }

        Debug.Log("Removed " + trianglesToBeDeleted.Count + " Triangles");

        for (int i = 0; i < trianglesToBeDeleted.Count; i++)
        {
            Triangle t = trianglesToBeDeleted[i];

            triangulation.Remove(t);
            HalfEdge t_e1 = t.startingHalfEdge;
            HalfEdge t_e2 = t_e1.next;
            HalfEdge t_e3 = t_e2.next;

            if (t_e1.opposite != null)
            {
                t_e1.opposite.opposite = null;
            }
            if (t_e2.opposite != null)
            {
                t_e2.opposite.opposite = null;
            }
            if (t_e3.opposite != null)
            {
                t_e3.opposite.opposite = null;
            }
        }
    }

    private static bool IsAnEdgeAConstraint(Vector2 p1, Vector2 p2, List<Vertex> constraints)
    {
        for (int i = 0; i < constraints.Count; i++)
        {
            Vector2 c_p1, c_p2;
            c_p1 = constraints[i].position;
            if (i + 1 == constraints.Count) c_p2 = constraints[0].position;
            else c_p2 = constraints[i + 1].position;

            if ((p1 == c_p1 && p2 == c_p2) || (p2 == c_p1 && p1 == c_p2))
            {
                return true;
            }
        }

        return false;
    }

    void DisplayLines(List<Triangle> _triangles, Color color, float time)
    {
        foreach (Triangle currentTriangle in _triangles)
        {
            Vector2 p1, p2, p3;
            p1 = new Vector2(currentTriangle.point1.position.x, currentTriangle.point1.position.y);
            p2 = new Vector2(currentTriangle.point2.position.x, currentTriangle.point2.position.y);
            p3 = new Vector2(currentTriangle.point3.position.x, currentTriangle.point3.position.y);

            Debug.DrawLine(p1, p2, color, time);
            Debug.DrawLine(p2, p3, color, time);
            Debug.DrawLine(p3, p1, color, time);
        }
    }

    
}
