using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
    public static List<List<MapNode>> Generate(DungeonFloorConfig config)
    {
        int regularRowCount = Mathf.Max(1, config.regularRowCount);
        int columns = Mathf.Max(1, config.columns);

        var rows = new List<List<MapNode>>();

        for (int r = 0; r < regularRowCount; r++)
            rows.Add(GenerateRegularRow(r, columns));

        rows.Add(GenerateRestRow(regularRowCount, columns));
        rows.Add(GenerateBossRow(regularRowCount + 1));

        for (int r = 0; r < rows.Count - 1; r++)
            ConnectRows(rows[r], rows[r + 1]);

        return rows;
    }

    private static List<MapNode> GenerateRegularRow(int row, int columns)
    {
        var nodes = new List<MapNode>();
        for (int c = 0; c < columns; c++)
        {
            MapNodeType type = row == 0 ? MapNodeType.Basic : RollType();
            nodes.Add(new MapNode { Row = row, Column = c, Type = type });
        }
        return nodes;
    }

    private static MapNodeType RollType()
    {
        float roll = Random.value;
        if (roll < 0.55f) return MapNodeType.Basic;
        if (roll < 0.75f) return MapNodeType.Elite;
        if (roll < 0.95f) return MapNodeType.Event;
        return MapNodeType.Rest;
    }

    private static List<MapNode> GenerateRestRow(int row, int columns)
    {
        var nodes = new List<MapNode>();
        for (int c = 0; c < columns; c++)
            nodes.Add(new MapNode { Row = row, Column = c, Type = MapNodeType.Rest });
        return nodes;
    }

    private static List<MapNode> GenerateBossRow(int row)
    {
        return new List<MapNode> { new MapNode { Row = row, Column = 0, Type = MapNodeType.Boss } };
    }

    private static void ConnectRows(List<MapNode> from, List<MapNode> to)
    {
        foreach (MapNode node in from)
        {
            if (to.Count == 1)
            {
                node.Connections.Add(0);
                continue;
            }

            int connectionCount = Random.Range(1, 3);
            var candidates = new List<int>();
            for (int dc = -1; dc <= 1; dc++)
            {
                int col = node.Column + dc;
                if (col >= 0 && col < to.Count)
                    candidates.Add(col);
            }

            Shuffle(candidates);
            for (int i = 0; i < Mathf.Min(connectionCount, candidates.Count); i++)
                node.Connections.Add(candidates[i]);
        }

        // Garantit qu'aucun noeud de la rangee suivante n'est totalement inaccessible, en restant
        // dans le voisinage proche (meme regle +-1 colonne que les connexions normales) pour ne
        // jamais creer de trait qui traverse toute la carte.
        for (int col = 0; col < to.Count; col++)
        {
            bool hasIncoming = from.Exists(n => n.Connections.Contains(col));
            if (hasIncoming)
                continue;

            List<MapNode> nearbySources = from.FindAll(n => Mathf.Abs(n.Column - col) <= 1);
            MapNode source = nearbySources.Count > 0
                ? nearbySources[Random.Range(0, nearbySources.Count)]
                : from[Mathf.Clamp(col, 0, from.Count - 1)];

            source.Connections.Add(col);
        }
    }

    private static void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
