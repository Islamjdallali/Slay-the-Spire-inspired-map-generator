using DelaunatorSharp;

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

    public float g { get; private set; }
    public float h { get; private set; }
    public float f => g = h;

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
