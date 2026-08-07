using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerKeyboardMover Player { get; private set; }
    public KeyboardGenerator Keyboard { get; private set; }
    public EnemySpawner Spawner { get; private set; }
    // Non-null uniquement si le niveau courant est un combat de boss (voir BossDefinition).
    public BossController Boss { get; private set; }

    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        Instance = this;

        Player = FindAnyObjectByType<PlayerKeyboardMover>();
        Keyboard = FindAnyObjectByType<KeyboardGenerator>();
        Spawner = FindAnyObjectByType<EnemySpawner>();

        BossDefinition bossDefinition = LevelSession.Current != null ? LevelSession.Current.chosenBoss : null;
        if (bossDefinition != null && bossDefinition.bossPrefab != null)
        {
            GameObject bossInstance = Instantiate(bossDefinition.bossPrefab);
            Boss = bossInstance.GetComponent<BossController>();
        }
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
