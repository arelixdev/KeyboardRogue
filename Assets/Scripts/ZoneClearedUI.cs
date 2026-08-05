using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ZoneClearedUI : MonoBehaviour
{
    [SerializeField] private GameObject banner;
    [SerializeField] private Button mapButton;

    private void Awake()
    {
        mapButton.onClick.AddListener(ReturnToMap);
    }

    private void Start()
    {
        GameManager.Instance.Spawner.AllEnemiesDefeated += ShowMessage;
        banner.SetActive(false);
    }

    // Affiche le message. Ne bloque pas le jeu: le joueur peut continuer a se deplacer jusqu'a cliquer pour revenir a la carte.
    private void ShowMessage()
    {
        banner.SetActive(true);
    }

    private void ReturnToMap()
    {
        LevelSession.CompletePendingNode();
        SceneManager.LoadScene("MapScene");
    }
}
