using System.Collections.Generic;

public enum MapNodeType
{
    Basic,
    Elite,
    Event,
    Rest,
    Boss,
}

public class MapNode
{
    public int Row;
    public int Column;
    public MapNodeType Type;
    public readonly List<int> Connections = new List<int>();
    public bool Visited;
}
