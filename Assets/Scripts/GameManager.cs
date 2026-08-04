using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerKeyboardMover Player { get; private set; }
    public KeyboardGenerator Keyboard { get; private set; }
    public EnemySpawner Spawner { get; private set; }

    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        Instance = this;

        Player = FindAnyObjectByType<PlayerKeyboardMover>();
        Keyboard = FindAnyObjectByType<KeyboardGenerator>();
        Spawner = FindAnyObjectByType<EnemySpawner>();
    }

    public void SetPaused(bool paused)
    {
        if (IsGameOver)
            return;

        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
    }

    public void SetGameOver()
    {
        IsGameOver = true;
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
