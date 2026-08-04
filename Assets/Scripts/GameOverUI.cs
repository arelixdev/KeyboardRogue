using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Health playerHealth;

    private void Awake()
    {
        restartButton.onClick.AddListener(RestartLevel);
    }

    private void Start()
    {
        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerKeyboardMover>().GetComponent<Health>();

        playerHealth.Died += ShowGameOver;
        panel.SetActive(false);
    }

    private void ShowGameOver()
    {
        panel.SetActive(true);
        Time.timeScale = 0f;

        PlayerKeyboardMover player = FindAnyObjectByType<PlayerKeyboardMover>();
        if (player != null)
            player.enabled = false;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
