using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;

    private void Awake()
    {
        resumeButton.onClick.AddListener(TogglePause);
        restartButton.onClick.AddListener(() => GameManager.Instance.Restart());
    }

    private void Start()
    {
        panel.SetActive(false);
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();
    }

    private void TogglePause()
    {
        bool newState = !panel.activeSelf;
        panel.SetActive(newState);
        GameManager.Instance.SetPaused(newState);
    }
}
