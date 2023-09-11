using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunatorSharp;
using System.Linq;
using DelaunatorSharp.Unity.Extensions;
using System;
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

    public enum eNodeType
    {
        eDefault,
        eStart,
        eEnd
    }

    public struct Node
    {
        public IPoint point;
        public eNodeType nodeType;

        public Node(IPoint p, eNodeType t)
        {
            point = p;
            nodeType = t;
        }

        public void SetStart()
        {
            nodeType = eNodeType.eStart;
        }

        public void SetEnd()
        {
            nodeType = eNodeType.eEnd;
        }
    }

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

            var pointGameObject = Instantiate(trianglePointPrefab, PointsContainer);
            pointGameObject.transform.SetPositionAndRotation(edge.P.ToVector3(), Quaternion.identity);
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

        var startPointGameObject = Instantiate(startPointPrefab, PointsContainer);
        startPointGameObject.transform.SetPositionAndRotation(startPoint.ToVector3(), Quaternion.identity);

        var endPointGameObject = Instantiate(startPointPrefab, PointsContainer);
        endPointGameObject.transform.SetPositionAndRotation(endPoint.ToVector3(), Quaternion.identity);
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
        Clear();

        var sampler = UniformPoissonDiskSampler.SampleCircle(Vector2.zero, generationSize, generationMinDistance);
        points = sampler.Select(point => new Vector2(point.x, point.y)).ToPoints().ToList();

        for (int i = 0; i < points.Count; i++)
        {
            nodes.Add(new Node(points[i], eNodeType.eDefault));
        }

        Debug.Log($"Generated Points Count {points.Count}");
        Create();
        return;
    }
}
