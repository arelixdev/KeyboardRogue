using UnityEngine;

public class ZoneClearedUI : MonoBehaviour
{
    [SerializeField] private GameObject banner;
    [SerializeField] private EnemySpawner spawner;

    private void Start()
    {
        if (spawner == null)
            spawner = FindAnyObjectByType<EnemySpawner>();

        spawner.AllEnemiesDefeated += ShowMessage;
        banner.SetActive(false);
    }

    // Affiche le message. Ne bloque pas le jeu: un futur evenement prendra le relais pour enchainer sur la suite.
    private void ShowMessage()
    {
        banner.SetActive(true);
    }
}
