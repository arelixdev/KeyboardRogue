using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

// Ecoute la frappe du joueur en parallele du dash (meme touches, deux systemes independants).
// Suit une sequence en cours parmi tous les sorts de la run; surligne en dore les touches deja
// tapees correctement, quel que soit si le sort a ete decouvert (Phase 5).
public class SpellCaster : MonoBehaviour
{
    [SerializeField] private KeyboardGenerator keyboard;

    private Health playerHealth;
    private readonly List<char> inputBuffer = new List<char>();
    private readonly List<KeyView> highlightedKeys = new List<KeyView>();

    private void Start()
    {
        if (keyboard == null)
            keyboard = GameManager.Instance.Keyboard;

        playerHealth = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput += HandleTextInput;
    }

    private void OnDisable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= HandleTextInput;
    }

    private void HandleTextInput(char character)
    {
        char upper = char.ToUpperInvariant(character);

        // Une touche hors clavier de jeu (retour arriere, entree, etc. peuvent remonter ici via
        // l'input system) est simplement ignoree: elle ne doit pas casser une sequence en cours.
        if (!keyboard.Keys.TryGetValue(upper, out KeyView key))
            return;

        if (IsUnusable(key))
        {
            ResetBuffer();
            return;
        }

        inputBuffer.Add(upper);
        List<SpellDefinition> candidates = GetCandidates(inputBuffer);

        if (candidates.Count == 0)
        {
            // Ne prolonge aucune sequence en cours: on retente en repartant de cette seule touche.
            ResetBuffer();
            inputBuffer.Add(upper);
            candidates = GetCandidates(inputBuffer);

            if (candidates.Count == 0)
            {
                ResetBuffer();
                return;
            }
        }

        SpellDefinition completed = candidates.FirstOrDefault(s => LevelSession.SpellSequences[s].Length == inputBuffer.Count);
        if (completed != null)
        {
            Cast(completed);
            ResetBuffer();
            return;
        }

        UpdateHighlight();
    }

    // Une touche desactivee ou definitivement cassee ne peut ni prolonger ni demarrer une sequence:
    // un sort qui en depend devient injouable pour le reste de la run.
    private static bool IsUnusable(KeyView key)
    {
        return key.Modifier == KeyModifierType.Disabled || key.Modifier == KeyModifierType.Broken;
    }

    private static List<SpellDefinition> GetCandidates(List<char> buffer)
    {
        var result = new List<SpellDefinition>();
        foreach (KeyValuePair<SpellDefinition, char[]> entry in LevelSession.SpellSequences)
        {
            char[] sequence = entry.Value;
            if (buffer.Count > sequence.Length)
                continue;

            bool matches = true;
            for (int i = 0; i < buffer.Count; i++)
            {
                if (sequence[i] != buffer[i])
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
                result.Add(entry.Key);
        }

        return result;
    }

    private void Cast(SpellDefinition spell)
    {
        // Lancer un sort le revele aussi, meme sans etre passe par l'event qui l'annonce.
        LevelSession.DiscoveredSpells.Add(spell);

        switch (spell.effectType)
        {
            case SpellEffectType.DamageAllEnemies:
                int damage = Mathf.RoundToInt(spell.effectValue);
                foreach (EnemyBasic enemy in EnemyBasic.ActiveEnemiesList.ToList())
                    enemy.ApplyDamage(damage);
                break;

            case SpellEffectType.Heal:
                playerHealth.Heal(Mathf.RoundToInt(spell.effectValue));
                break;

            case SpellEffectType.FreezeAllEnemies:
                foreach (EnemyBasic enemy in EnemyBasic.ActiveEnemiesList.ToList())
                    enemy.Stun(spell.effectValue);
                break;

            case SpellEffectType.DestroyRandomEnemy:
                List<EnemyBasic> alive = EnemyBasic.ActiveEnemiesList.ToList();
                if (alive.Count > 0)
                    alive[Random.Range(0, alive.Count)].Kill();
                break;
        }
    }

    private void UpdateHighlight()
    {
        ClearHighlight();
        foreach (char c in inputBuffer)
        {
            if (keyboard.Keys.TryGetValue(c, out KeyView key))
            {
                key.SetSpellHighlighted(true);
                highlightedKeys.Add(key);
            }
        }
    }

    private void ClearHighlight()
    {
        foreach (KeyView key in highlightedKeys)
            key.SetSpellHighlighted(false);
        highlightedKeys.Clear();
    }

    private void ResetBuffer()
    {
        inputBuffer.Clear();
        ClearHighlight();
    }
}
