using System.Linq;
using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text enemiesText;
    [SerializeField] private TMP_Text spellsText;

    private Health playerHealth;

    private void Start()
    {
        playerHealth = GameManager.Instance.Player.GetComponent<Health>();
    }

    private void Update()
    {
        // Peut devenir nul le temps d'une frame pendant le dechargement de la scene (retour a la carte).
        if (playerHealth == null || GameManager.Instance == null)
            return;

        healthText.text = $"PV: {playerHealth.Current}/{playerHealth.Max}";

        if (GameManager.Instance.Boss != null)
            enemiesText.text = $"Boss: {GameManager.Instance.Boss.Health.Current}/{GameManager.Instance.Boss.Health.Max}";
        else
            enemiesText.text = $"Ennemis restants: {GameManager.Instance.Spawner.RemainingCount}";

        if (spellsText != null)
            spellsText.text = BuildSpellsText();
    }

    private string BuildSpellsText()
    {
        if (LevelSession.DiscoveredSpells.Count == 0)
            return "Sorts: aucun decouvert";

        var lines = LevelSession.DiscoveredSpells
            .Where(spell => LevelSession.SpellSequences.ContainsKey(spell))
            .Select(spell => $"{spell.spellName}: {string.Join("-", LevelSession.SpellSequences[spell])}");

        return "Sorts:\n" + string.Join("\n", lines);
    }
}
