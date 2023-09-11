using DelaunatorSharp;
using System.Collections;
using System.Collections.Generic;

public class Node
{
    public enum eNodeType
    {
        eDefault,
        eStart,
        eEnd
    }

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
