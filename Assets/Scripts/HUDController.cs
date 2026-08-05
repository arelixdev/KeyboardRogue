using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text enemiesText;

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
        enemiesText.text = $"Ennemis restants: {GameManager.Instance.Spawner.RemainingCount}";
    }
}
