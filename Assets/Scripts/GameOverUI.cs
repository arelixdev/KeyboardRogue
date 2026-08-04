using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button restartButton;

    private void Awake()
    {
        restartButton.onClick.AddListener(RestartLevel);
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

    public void RestartLevel()
    {
        GameManager.Instance.Restart();
    }
}
