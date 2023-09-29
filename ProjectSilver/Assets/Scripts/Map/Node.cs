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
    public float f => g + h;

    public float a = 0;

    public Node connection;

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

    public void AddG(float add)
    {
        a = add;
    }

    public void SetG(float G)
    {
        g = G + a;
    }

    public void SetH(float H)
    {
        h = H;
    }

    public void SetConnection(Node node)
    {
        connection = node;
    }
}
