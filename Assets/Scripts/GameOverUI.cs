using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button restartButton;

    private void Awake()
    {
        restartButton.onClick.AddListener(RestartRun);
    }

    private void Start()
    {
        GameManager.Instance.Player.GetComponent<Health>().Died += ShowGameOver;
        panel.SetActive(false);
    }

    private void ShowGameOver()
    {
        panel.SetActive(true);
        GameManager.Instance.SetGameOver();
        GameManager.Instance.Player.enabled = false;
    }

    // Mort = fin de la run entiere (convention roguelike): on repart d'une carte neuve.
    public void RestartRun()
    {
        LevelSession.StartNewRun();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MapScene");
    }
}
