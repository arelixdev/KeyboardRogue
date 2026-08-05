using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapNodeView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text label;

    public MapNode Node { get; private set; }
    public Button Button => button;

    private static readonly Color BasicColor = new Color(0.75f, 0.75f, 0.75f);
    private static readonly Color EliteColor = new Color(0.85f, 0.35f, 0.2f);
    private static readonly Color EventColor = new Color(0.55f, 0.4f, 0.85f);
    private static readonly Color RestColor = new Color(0.35f, 0.75f, 0.4f);
    private static readonly Color BossColor = new Color(0.6f, 0.1f, 0.1f);
    private static readonly Color VisitedTint = new Color(0.3f, 0.85f, 0.3f);

    // Le type de chaque case reste toujours visible (pour planifier son chemin), seule la teinte
    // change selon l'etat: disponible (couleur pleine), deja visitee (verdatre), pas encore accessible (assombrie).
    public void Setup(MapNode node, bool available)
    {
        Node = node;

        bool clickable = available && !node.Visited;
        button.interactable = clickable;
        label.text = TypeLabel(node.Type);

        Color baseColor = TypeColor(node.Type);
        if (node.Visited)
            background.color = Color.Lerp(baseColor, VisitedTint, 0.6f);
        else if (available)
            background.color = baseColor;
        else
            background.color = Dim(baseColor);
    }

    private static Color Dim(Color color)
    {
        return new Color(color.r * 0.4f, color.g * 0.4f, color.b * 0.4f, 1f);
    }

    private static string TypeLabel(MapNodeType type)
    {
        switch (type)
        {
            case MapNodeType.Basic: return "Combat";
            case MapNodeType.Elite: return "Elite";
            case MapNodeType.Event: return "Event";
            case MapNodeType.Rest: return "Repos";
            case MapNodeType.Boss: return "BOSS";
            default: return "?";
        }
    }

    private static Color TypeColor(MapNodeType type)
    {
        switch (type)
        {
            case MapNodeType.Basic: return BasicColor;
            case MapNodeType.Elite: return EliteColor;
            case MapNodeType.Event: return EventColor;
            case MapNodeType.Rest: return RestColor;
            case MapNodeType.Boss: return BossColor;
            default: return BasicColor;
        }
    }
}
