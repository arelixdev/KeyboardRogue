using UnityEngine;

public class ZoneClearedUI : MonoBehaviour
{
    [SerializeField] private GameObject banner;

    private void Start()
    {
        GameManager.Instance.Spawner.AllEnemiesDefeated += ShowMessage;
        banner.SetActive(false);
    }

    // Affiche le message. Ne bloque pas le jeu: un futur evenement prendra le relais pour enchainer sur la suite.
    private void ShowMessage()
    {
        banner.SetActive(true);
    }
}
