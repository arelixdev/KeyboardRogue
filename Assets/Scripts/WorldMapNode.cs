using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapNode : MonoBehaviour
{
    [SerializeField] private LevelDefinition level;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;

    public LevelDefinition Level => level;
    public Button Button => button;

    public void SetState(bool unlocked, bool completed)
    {
        // Un niveau deja termine reste visible mais ne se relance plus depuis la carte.
        button.interactable = unlocked && !completed;

        if (completed)
            label.text = level.levelName + " (Termine)";
        else if (unlocked)
            label.text = level.levelName;
        else
            label.text = "Verrouille";

        label.color = completed ? new Color(0.4f, 1f, 0.4f) : Color.white;
    }
}
