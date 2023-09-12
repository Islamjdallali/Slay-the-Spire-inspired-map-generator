using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunatorSharp;
using System.Linq;
using DelaunatorSharp.Unity.Extensions;
using DelaunatorSharp.Unity;

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

    [SerializeField] float triangleEdgeWidth = .01f;

    [SerializeField] Color triangleEdgeColor = Color.black;
    [SerializeField] Material lineMaterial;

    [SerializeField] float generationSize = 3;
    [SerializeField] float generationMinDistance = .2f;

    private void Start()
    {
        Clear();
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

        var n = GetNeighbouringNodes(startPointIndex);

        for (int i = 0; i < n.Count; i++)
        {
            Debug.Log(n[i].point);
        }

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

        while (toSearch.Any())
        {
            var current = toSearch[0];
            foreach(var t in toSearch)
            {
                if (t.f < current.f || t.f == current.f && t.h < current.h)
                {
                    current = t;
                }
            }
            processed.Add(current);
            toSearch.Remove(current);

            foreach (var t in delaunator.GetTrianglePoints(0))
            {

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
