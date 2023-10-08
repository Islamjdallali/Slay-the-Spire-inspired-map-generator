using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunatorSharp;
using System.Linq;
using DelaunatorSharp.Unity.Extensions;
using DelaunatorSharp.Unity;
using Unity.VisualScripting;
using static Node;

public class PointGeneration : MonoBehaviour
{
    [SerializeField] GameObject trianglePointPrefab;
    [SerializeField] GameObject startPointPrefab;

    [SerializeField] private IPoint startPoint;
    [SerializeField] private IPoint endPoint;

    int startPointIndex;
    int endPointIndex;

    private List<IPoint> points = new List<IPoint>();
    private List<Node> nodes = new List<Node>();
    private Delaunator delaunator;
    private Transform PointsContainer;
    private Transform TrianglesContainer;
    private Transform PathsContainer;

    [SerializeField] float triangleEdgeWidth = .01f;

    [SerializeField] Color triangleEdgeColor = Color.black;
    [SerializeField] Color pathColor = Color.green;
    [SerializeField] Material lineMaterial;

    [SerializeField] float generationSize = 3;
    [SerializeField] float generationMinDistance = .2f;

    [SerializeField] List<Node> path = new List<Node>();

    [SerializeField] int numberOfPaths;
    [SerializeField] int numberOfChangedNodes;

    private void Start()
    {
        Clear();
    }

    public void GeneratePath()
    {

        for (int j = 0; j < numberOfPaths; j++)
        {
            path = FindPath(nodes[startPointIndex], nodes[endPointIndex]);

            for (int i = 0; i < path.Count; i++)
            {
                if (!path[i].isFlagged)
                {
                    if (path[i].nodeType == Node.eNodeType.eDefault)
                    {
                        var pointGameObject = Instantiate(trianglePointPrefab);
                        pointGameObject.transform.SetPositionAndRotation(path[i].point.ToVector3(), Quaternion.identity);
                    }
                    else
                    {
                        var pointGameObject = Instantiate(startPointPrefab);
                        pointGameObject.transform.SetPositionAndRotation(path[i].point.ToVector3(), Quaternion.identity);
                    }

                    path[i].SetFlagged(true);
                }

                if (i + 1 != path.Count)
                {
                    CreateLine(PathsContainer, $"Path - {path[i]}", new Vector3[] { path[i].point.ToVector3(), path[i + 1].point.ToVector3() }, pathColor, triangleEdgeWidth, 0);
                }
            }
        }

        Destroy(TrianglesContainer.gameObject);
        Destroy(PointsContainer.gameObject);
    }

    private void Create()
    {
        if (points.Count < 3) return;

        Clear();

        delaunator = new Delaunator(points.ToArray());

        CreateTriangle();
    }

    private void Clear()
    {
        CreateNewContainers();

        delaunator = null;
    }

    private void CreateTriangle()
    {
        if (delaunator == null) return;

        delaunator.ForEachTriangleEdge(edge =>
        {
            CreateLine(TrianglesContainer, $"TriangleEdge - {edge.Index}", new Vector3[] { edge.P.ToVector3(), edge.Q.ToVector3() }, triangleEdgeColor, triangleEdgeWidth, 0);
        });

        double y = 0;


        for (int i = 0; i < points.Count; i++)
        {
            if (y > points[i].Y)
            {
                y = points[i].Y;
                startPoint = points[i];
                startPointIndex = i;
            }
        }

        nodes[startPointIndex].SetStart();

        for (int i = 0; i < points.Count; i++)
        {
            if (y < points[i].Y)
            {
                y = points[i].Y;
                endPoint = points[i];
                endPointIndex = i;
            }
        }

        nodes[endPointIndex].SetEnd();

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].nodeType == Node.eNodeType.eDefault)
            {
                var pointGameObject = Instantiate(trianglePointPrefab, PointsContainer);
                pointGameObject.transform.SetPositionAndRotation(nodes[i].point.ToVector3(), Quaternion.identity);
            }
            else
            {
                var pointGameObject = Instantiate(startPointPrefab, PointsContainer);
                pointGameObject.transform.SetPositionAndRotation(nodes[i].point.ToVector3(), Quaternion.identity);
            }
        }
    }

    float GetDistance(IPoint self,IPoint other)
    {
        return Vector2.Distance(self.ToVector2(), other.ToVector2());
    }

    List<Node> GetNeighbouringNodes(int index)
    {
        var neighbouringNodes = new List<Node>();
        var neighbouringPoints = new List<IPoint>();

        delaunator.ForEachTriangleEdge(edge =>
        {
            if (nodes[index].point == edge.P)
            {
                neighbouringPoints.Add(edge.Q);
            }

            if (edge.Q == nodes[index].point)
            {
                neighbouringPoints.Add(edge.P);
            }
        });

        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < neighbouringPoints.Count; j++)
            {
                if (nodes[i].point == neighbouringPoints[j])
                {
                    neighbouringNodes.Add(nodes[i]);
                }
            }
        }

        return neighbouringNodes;
    }

    public List<Node> FindPath(Node startNode, Node targetNode)
    {
        var toSearch = new List<Node>() { startNode};
        var processed = new List<Node>();
        var nodeForPaths = new List<Node>(nodes);
        var randNodeToDelete = new List<Node>();

        int randIndex = 0;

        if (path.Count != 0)
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (path[i].nodeType == eNodeType.eDefault)
                {
                    randNodeToDelete.Add(path[i]);
                }
            }


            for (int j = 0; j < numberOfChangedNodes; j++)
            {
                randIndex = Random.Range(0, randNodeToDelete.Count);

                for (int i = 0; i < nodeForPaths.Count; i++)
                {
                    if (nodeForPaths[i].point == randNodeToDelete[randIndex].point)
                    {
                        nodeForPaths[i].AddG(100);
                    }
                }
            }
        }

        while (toSearch.Any())
        {
            //Debug.Log("Searching");

            var current = toSearch[0];
            var currentIndex = 0;
            foreach(var t in toSearch)
            {
                if (t.f < current.f || t.f == current.f && t.h < current.h)
                {
                    //Debug.Log("Value is : " + t.point);
                    current = t;
                }
            }

            processed.Add(current);
            toSearch.Remove(current);

            if (current == targetNode) 
            {
                //Debug.Log("Target Reached");
                
                var currentPathNode = targetNode;
                var path = new List<Node>();
                while (currentPathNode != startNode)
                {
                    path.Add(currentPathNode);
                    currentPathNode = currentPathNode.connection;
                }

                if (currentPathNode == startNode)
                {
                    path.Add(startNode);
                }

                return path;
            }

            for (int i = 0; i < nodeForPaths.Count; i++)
            {
                if (nodeForPaths[i].point == current.point)
                {
                    currentIndex = i;
                }
            }

            foreach (var neighbour in GetNeighbouringNodes(currentIndex).Where(t => !processed.Contains(t)))
            {
                var inSearch = toSearch.Contains(neighbour);

                var costToNeighbour = current.g + GetDistance(current.point, neighbour.point);

                if (!inSearch || costToNeighbour < neighbour.g)
                {
                    neighbour.SetG(costToNeighbour);
                    neighbour.SetConnection(current);

                    //Debug.Log("Neighbour : " + neighbour.point + " Set connection to : " + current.point);

                    if (!inSearch)
                    {
                        neighbour.SetH(GetDistance(neighbour.point, targetNode.point));
                        toSearch.Add(neighbour);
                    }
                }
            }

        }

        return null;
    }

    private void CreateLine(Transform container, string name, Vector3[] points, Color color, float width, int order = 1)
    {
        var lineGameObject = new GameObject(name);
        lineGameObject.transform.parent = container;
        var lineRenderer = lineGameObject.AddComponent<LineRenderer>();

        lineRenderer.SetPositions(points);

        lineRenderer.material = lineMaterial ?? new Material(Shader.Find("Standard"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.sortingOrder = order;
    }

    private void CreateNewContainers()
    {
        CreateNewPointsContainer();
        CreateNewTrianglesContainer();
        CreateNewPathsContainer();
    }

    private void CreateNewPointsContainer()
    {
        if (PointsContainer != null)
        {
            Destroy(PointsContainer.gameObject);
        }

        PointsContainer = new GameObject(nameof(PointsContainer)).transform;
    }

    private void CreateNewTrianglesContainer()
    {
        if (TrianglesContainer != null)
        {
            Destroy(TrianglesContainer.gameObject);
        }

        TrianglesContainer = new GameObject(nameof(TrianglesContainer)).transform;
    }

    private void CreateNewPathsContainer()
    {
        if (PathsContainer != null)
        {
            Destroy(PathsContainer.gameObject);
        }

        PathsContainer = new GameObject(nameof(PathsContainer)).transform;
    }

    public void Generate()
    {
        nodes.Clear();
        Clear();

        var sampler = UniformPoissonDiskSampler.SampleCircle(Vector2.zero, generationSize, generationMinDistance);
        points = sampler.Select(point => new Vector2(point.x, point.y)).ToPoints().ToList();

        for (int i = 0; i < points.Count; i++)
        {
            nodes.Add(new Node(points[i], Node.eNodeType.eDefault));
        }

        Debug.Log($"Generated Points Count {points.Count}");
        Create();
        return;
    }
}
