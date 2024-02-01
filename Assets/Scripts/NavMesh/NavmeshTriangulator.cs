using System.Collections.Generic;
using System.Linq;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

/*
 * Most of the code in this class has been adapted from (Nordeus, 2019/2023)
 * See LICENSES.cs for license and reference
 */

public static class NavmeshTriangulator
{
    // The full navmesh generation
    public static List<Triangle> ConstrainedDelaunay(List<NavmeshPoints> _pointsList, bool _displayNavmesh = false, float _displayTime = 0f)
    {
        List<List<Vertex>> exlclusionVertexList = new();
        List<Triangle> delaunayTriangulation = DelaunayTriangulation(_pointsList);
        foreach (NavmeshPoints currentPoints in _pointsList)
        {
            if (!currentPoints.isInside) exlclusionVertexList.Add(currentPoints.points);
        }
        List<Triangle> constrainedDelaunayTriangulation = AddConstraints(delaunayTriangulation, exlclusionVertexList);
        if (_displayNavmesh)DisplayLines(constrainedDelaunayTriangulation, Color.blue, _displayTime);
        return constrainedDelaunayTriangulation;
    }

    // Removing shapes from the triangulation
    private static List<Triangle> AddConstraints(List<Triangle> _triangles, List<List<Vertex>> _constraints)
    {
        foreach (List<Vertex> currentShape in _constraints)
        {
            // Do this for each shape
            for (int i = 0; i < currentShape.Count; i++)
            {
                // Get edge of current shape
                Vector2 v_i, v_j;
                v_i = currentShape[i].position;
                if (i + 1 == currentShape.Count) v_j = currentShape[0].position;
                else v_j = currentShape[i + 1].position;
                // Ignore below if the edge is already fine
                if (TriangulationSystem.IsEdgePartOfTriangulation(_triangles, v_i, v_j)) continue;

                // Finds what edges need to be changed
                List<HalfEdge> intersectingEdges = TriangulationSystem.FindIntersectingEdges(_triangles, v_i, v_j);

                // Change those edges
                List<HalfEdge> newEdges = TriangulationSystem.RemoveIntersectingEdges(v_i, v_j, intersectingEdges);

                // Make the edges more efficient
                RestoreDelaunayTriangulation(v_i, v_j, newEdges);

            }

            // Remove triangles inside of the shape
            RemoveSuperfluousTriangles(_triangles, currentShape);
        }

        return _triangles;
    }
    
    // Making a neat triangulation of points
    public static List<Triangle> DelaunayTriangulation(List<NavmeshPoints> _pointsList)
    {
        List<Triangle> triangles = TriangulatePoints(_pointsList);
        // Turn the triangles into half edges
        List<HalfEdge> edges = TriangulationSystem.TrianglesToHalfEdges(triangles);
        int safety = 0;

        int flippedEdges = 0;

        while (true)
        {
            safety += 1;

            if (safety > 100000)
            {
                Debug.Log("Error: Cannot Delaunay Triangulate");

                break;
            }

            bool hasFlippedEdge = false;

            for (int i = 0; i < edges.Count; i++)
            {
                HalfEdge thisEdge = edges[i];

                if (thisEdge.opposite == null) continue;

                Vector2 aPos, bPos, cPos, dPos;
                aPos = thisEdge.vertex.position;
                bPos = thisEdge.next.vertex.position;
                cPos = thisEdge.previous.vertex.position;
                dPos = thisEdge.opposite.next.vertex.position;

                // If the current edge is not efficient
                if (TriangulationSystem.IsPointInsideOutsideOrOnCircle(aPos, bPos, cPos, dPos) < 0f)
                {
                    if (TriangulationSystem.IsQuadrilateralConvex(aPos, bPos, cPos, dPos))
                    {
                        if (TriangulationSystem.IsPointInsideOutsideOrOnCircle(bPos, cPos, dPos, aPos) < 0f) continue;

                        // Flip the edge
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
        //DisplayLines(triangles, Color.magenta, 3f);
        return triangles;
    }

    // Make a random triangulation of points
    public static List<Triangle> TriangulatePoints(List<NavmeshPoints> _pointsList)
    {
        List<Triangle> triangles = new();
        List<Vertex> combinedPoints = new();
        // Combine all the points
        foreach (NavmeshPoints currentPoints in _pointsList) combinedPoints.AddRange(currentPoints.points);
        // Order the points by x, then by y
        combinedPoints = combinedPoints.OrderBy(n => n.position.x).ThenBy(n => n.position.y).ToList();
        // Create the initial triangle
        Triangle newTriangle = new Triangle(combinedPoints[0], combinedPoints[1], combinedPoints[2]);

        triangles.Add(newTriangle);

        List<Edge> edges = new List<Edge>()
        {
            new Edge(newTriangle.point1, newTriangle.point2),
            new Edge(newTriangle.point2, newTriangle.point3),
            new Edge(newTriangle.point3, newTriangle.point1)
        };

        for (int i = 3; i < combinedPoints.Count; i++)
        {
            // Gets a vertex
            Vector2 currentPoint = combinedPoints[i].position;
            List<Edge> newEdges = new();
            for (int j = 0; j < edges.Count; j++)
            {
                // Tries to connect vertex to an edge
                Edge currentEdge = edges[j];
                Vector2 midPoint = (currentEdge.point1.position + currentEdge.point2.position) / 2f;
                Edge edgeToMidpoint = new Edge(new Vertex(currentPoint), new Vertex(midPoint));
                bool canSeeEdge = true;

                for (int k = 0; k < edges.Count; k++)
                {
                    if (k == j) continue;

                    if (TriangulationSystem.AreEdgesIntersecting(edgeToMidpoint, edges[k]))
                    {
                        canSeeEdge = false;
                        break;
                    }
                }

                // If it can connect to the edge
                if (canSeeEdge)
                {
                    // Create a new triangle
                    Edge edgeToPoint1 = new Edge(currentEdge.point1, new Vertex(currentPoint));
                    Edge edgeToPoint2 = new Edge(currentEdge.point2, new Vertex(currentPoint));
                    Triangle newTri = new Triangle(edgeToPoint1.point1, edgeToPoint1.point2, edgeToPoint2.point1);
                    newEdges.Add(edgeToPoint1);
                    newEdges.Add(edgeToPoint2);
                    triangles.Add(newTri);
                }
            }

            // Adds the new edges
            for (int j = 0; j < newEdges.Count; j++) edges.Add(newEdges[j]);
        }
        //DisplayLines(triangles, Color.red, 5f);
        return triangles;
    }

    // Same delaunay triangulation but prevents the new edge from being interfered with
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

                if ((v_k == v_i && v_l == v_j) || (v_l == v_i && v_k == v_j)) continue;
                Vector2 v_third_pos = currentEdge.next.vertex.position;
                Vector2 v_opposite_pos = currentEdge.opposite.next.vertex.position;

                float circleTestValue = TriangulationSystem.IsPointInsideOutsideOrOnCircle(v_k, v_l, v_third_pos, v_opposite_pos);

                if (circleTestValue < 0f)
                {
                    if (TriangulationSystem.IsQuadrilateralConvex(v_k, v_l, v_third_pos, v_opposite_pos))
                    {
                        if (TriangulationSystem.IsPointInsideOutsideOrOnCircle(v_opposite_pos, v_l, v_third_pos, v_k) <= circleTestValue) continue;

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

    // Removes Triangles in the middle of a constraint
    private static void RemoveSuperfluousTriangles(List<Triangle> triangulation, List<Vertex> constraints)
    {
        if (constraints.Count < 3) return;

        Triangle borderTriangle = null;
        Vector2 constrained_p1 = constraints[0].position;
        Vector2 constrained_p2 = constraints[1].position;

        // Finds the starting triangle
        for (int i = 0; i < triangulation.Count; i++)
        {
            HalfEdge edge1 = triangulation[i].startingHalfEdge;
            HalfEdge edge2 = edge1.next;
            HalfEdge edge3 = edge2.next;

            if (edge1.vertex.position == constrained_p2 && edge1.previous.vertex.position == constrained_p1)
            {
                borderTriangle = triangulation[i];
                break;
            }
            if (edge2.vertex.position == constrained_p2 && edge2.previous.vertex.position == constrained_p1)
            {
                borderTriangle = triangulation[i];
                break;
            }
            if (edge3.vertex.position == constrained_p2 && edge3.previous.vertex.position == constrained_p1)
            {
                borderTriangle = triangulation[i];
                break;
            }
        }

        if (borderTriangle == null) return;

        // Flood fill algorithm on the inner triangles
        List<Triangle> trianglesToBeDeleted = new();
        List<Triangle> neighborsToCheck = new()
        {
            borderTriangle
        };

        int safety = 0;

        while (true)
        {
            safety += 1;

            if (safety > 10000)
            {
                Debug.Log("Error: Cannot Delete Triangles");

                break;
            }

            if (neighborsToCheck.Count == 0) break;

            Triangle currentTriangle = neighborsToCheck[0];

            neighborsToCheck.RemoveAt(0);

            trianglesToBeDeleted.Add(currentTriangle);

            HalfEdge edge1 = currentTriangle.startingHalfEdge;
            HalfEdge edge2 = edge1.next;
            HalfEdge edge3 = edge2.next;

            if (edge1.opposite != null &&
                !trianglesToBeDeleted.Contains(edge1.opposite.triangle) &&
                !neighborsToCheck.Contains(edge1.opposite.triangle) &&
                !IsAnEdgeAConstraint(edge1.vertex.position, edge1.previous.vertex.position, constraints))
            {
                neighborsToCheck.Add(edge1.opposite.triangle);
            }
            if (edge2.opposite != null &&
                !trianglesToBeDeleted.Contains(edge2.opposite.triangle) &&
                !neighborsToCheck.Contains(edge2.opposite.triangle) &&
                !IsAnEdgeAConstraint(edge2.vertex.position, edge2.previous.vertex.position, constraints))
            {
                neighborsToCheck.Add(edge2.opposite.triangle);
            }
            if (edge3.opposite != null &&
                !trianglesToBeDeleted.Contains(edge3.opposite.triangle) &&
                !neighborsToCheck.Contains(edge3.opposite.triangle) &&
                !IsAnEdgeAConstraint(edge3.vertex.position, edge3.previous.vertex.position, constraints))
            {
                neighborsToCheck.Add(edge3.opposite.triangle);
            }
        }

        Debug.Log("Removed " + trianglesToBeDeleted.Count + " Triangles");

        for (int i = 0; i < trianglesToBeDeleted.Count; i++)
        {
            Triangle currentTriangle = trianglesToBeDeleted[i];

            triangulation.Remove(currentTriangle);
            HalfEdge edge1 = currentTriangle.startingHalfEdge;
            HalfEdge edge2 = edge1.next;
            HalfEdge edge3 = edge2.next;

            if (edge1.opposite != null) edge1.opposite.opposite = null;
            if (edge2.opposite != null) edge2.opposite.opposite = null;
            if (edge3.opposite != null) edge3.opposite.opposite = null;
        }
    }


    static bool IsAnEdgeAConstraint(Vector2 point1, Vector2 point2, List<Vertex> constraints)
    {
        for (int i = 0; i < constraints.Count; i++)
        {
            Vector2 constraint1, constraint2;
            constraint1 = constraints[i].position;
            if (i + 1 == constraints.Count) constraint2 = constraints[0].position;
            else constraint2 = constraints[i + 1].position;

            if ((point1 == constraint1 && point2 == constraint2) || (point2 == constraint1 && point1 == constraint2)) return true;
        
        }

        return false;
    }


    // Displays a grid
    static void DisplayLines(List<Triangle> _triangles, Color color, float time)
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
