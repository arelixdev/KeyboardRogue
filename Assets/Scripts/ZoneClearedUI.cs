using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ZoneClearedUI : MonoBehaviour
{
    [SerializeField] private GameObject banner;
    [SerializeField] private TMP_Text bonusText;
    [SerializeField] private Button mapButton;

    private KeyboardGenerator keyboard;
    private BonusDefinition chosenBonus;
    private bool waitingForKeyChoice;

    private void Awake()
    {
        mapButton.onClick.AddListener(ReturnToMap);
    }

    private void Start()
    {
        GameManager.Instance.Spawner.AllEnemiesDefeated += ShowMessage;
        banner.SetActive(false);
    }

    // Affiche le message. Ne bloque pas le jeu: le joueur peut continuer a se deplacer jusqu'a
    // avoir place son bonus/malus (le cas echeant) puis clique pour revenir a la carte.
    private void ShowMessage()
    {
        banner.SetActive(true);
        mapButton.gameObject.SetActive(false);

        keyboard = GameManager.Instance.Keyboard;
        chosenBonus = PickBonus();

        if (chosenBonus == null)
        {
            bonusText.text = string.Empty;
            mapButton.gameObject.SetActive(true);
            return;
        }

        if (chosenBonus.scope == BonusScope.Global)
        {
            BonusEffects.ApplyGlobal(chosenBonus);
            bonusText.text = $"{chosenBonus.bonusName}\n{chosenBonus.description}";
            mapButton.gameObject.SetActive(true);
        }
        else
        {
            bonusText.text = $"{chosenBonus.bonusName}\n{chosenBonus.description}\n\nAppuie sur une touche du clavier pour y placer cet effet.";
            waitingForKeyChoice = true;
            if (Keyboard.current != null)
                Keyboard.current.onTextInput += HandleKeyChoice;
        }
    }

    // Tirage pondere (BonusDefinition.weight) dans la liste de l'etage courant.
    private BonusDefinition PickBonus()
    {
        DungeonFloorConfig floor = LevelSession.CurrentFloorConfig;
        if (floor == null || floor.possibleBonuses == null || floor.possibleBonuses.Length == 0)
            return null;

        float totalWeight = 0f;
        foreach (BonusDefinition bonus in floor.possibleBonuses)
            totalWeight += Mathf.Max(0.01f, bonus.weight);

        float roll = Random.value * totalWeight;
        float cursor = 0f;
        foreach (BonusDefinition bonus in floor.possibleBonuses)
        {
            cursor += Mathf.Max(0.01f, bonus.weight);
            if (roll <= cursor)
                return bonus;
        }

        return floor.possibleBonuses[floor.possibleBonuses.Length - 1];
    }

    private void HandleKeyChoice(char character)
    {
        char upper = char.ToUpperInvariant(character);
        if (keyboard == null || !keyboard.Keys.TryGetValue(upper, out KeyView key))
            return;

        BonusEffects.BindKeyEffect(upper, chosenBonus.keyEffectType);
        key.SetModifier(chosenBonus.keyEffectType.ToKeyModifier());

        waitingForKeyChoice = false;
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= HandleKeyChoice;

        mapButton.gameObject.SetActive(true);
    }

    private void ReturnToMap()
    {
        if (waitingForKeyChoice)
            return;

        LevelSession.CompletePendingNode();
        SceneManager.LoadScene("MapScene");
    }

    private void OnDestroy()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= HandleKeyChoice;
    }
}
