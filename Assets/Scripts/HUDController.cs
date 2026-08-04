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
        healthText.text = $"PV: {playerHealth.Current}/{playerHealth.Max}";
        enemiesText.text = $"Ennemis restants: {GameManager.Instance.Spawner.RemainingCount}";
    }
}
