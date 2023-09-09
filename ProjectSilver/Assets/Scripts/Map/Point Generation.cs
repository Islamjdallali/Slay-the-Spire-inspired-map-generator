using System;
using System.Collections.Generic;
using UnityEngine;

public class PointGeneration : MonoBehaviour
{
    [SerializeField] private int planeLen = 30;
    [SerializeField] private int nodeCount = 0;
    [SerializeField] private int pathCount = 12;

    [SerializeField] private List<Vector2> points;


    // Start is called before the first frame update
    void Start()
    {
        nodeCount = planeLen * planeLen / 12;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Generate()
    {
        points.Add(new Vector2(0, planeLen / 2));
        points.Add(new Vector2(planeLen, planeLen / 2));

        Vector2 center = new Vector2(planeLen / 2, planeLen / 2);

        for (int i = 0; i < nodeCount; i++)
        {
            while(true)
            {
                Vector2 point = new Vector2(UnityEngine.Random.Range(0, planeLen), UnityEngine.Random.Range(0, planeLen));

                var disFromCenter = (point - center).magnitude;

                bool inCircle = disFromCenter <= planeLen * planeLen / 4;

                if (points[i] != point && inCircle)
                {
                    points.Add(point);
                    break;
                }
            }
        }
    }
}
