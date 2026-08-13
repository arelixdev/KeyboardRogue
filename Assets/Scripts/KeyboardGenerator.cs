using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum KeyboardLayoutType
{
    AZERTY,
    QWERTY,
}

public class KeyboardGenerator : MonoBehaviour
{
    // Rangees physiques (3 x 10 touches) par layout.
    private static readonly Dictionary<KeyboardLayoutType, string[]> LayoutRows = new Dictionary<KeyboardLayoutType, string[]>
    {
        [KeyboardLayoutType.AZERTY] = new[]
        {
            "AZERTYUIOP",
            "QSDFGHJKLM",
            "WXCVBN,;:!",
        },
        [KeyboardLayoutType.QWERTY] = new[]
        {
            "QWERTYUIOP",
            "ASDFGHJKL;",
            "ZXCVBNM,./",
        },
    };

    [SerializeField] private KeyboardLayoutType layout = KeyboardLayoutType.AZERTY;
    [SerializeField] private KeyView keyPrefab;
    [SerializeField] private float spacingX = 1.1f;
    [SerializeField] private float spacingZ = 1.1f;
    [SerializeField] private float rowStagger = 0.3f;

    private readonly Dictionary<char, KeyView> keys = new Dictionary<char, KeyView>();
    private KeyboardLayoutType generatedLayout;

    public KeyboardLayoutType Layout => layout;
    public IReadOnlyDictionary<char, KeyView> Keys => keys;

    // Toutes les touches disponibles pour un layout donne (utilise par le systeme de sorts pour
    // tirer des sequences de touches valides sans dependre d'une instance generee).
    public static IEnumerable<char> GetAllCharacters(KeyboardLayoutType forLayout) => LayoutRows[forLayout].SelectMany(row => row);

    private void Awake()
    {
        // Le niveau choisi sur la carte du monde peut imposer son propre layout.
        if (LevelSession.Current != null)
            layout = LevelSession.Current.keyboardLayout;

        // Awake plutot que Start: garantit que 'Keys' est deja rempli quand
        // d'autres scripts (ex: le joueur) le lisent dans leur propre Start().
        Generate();

        // Les liaisons permanentes (Phase 4, choisies par le joueur) s'appliquent d'abord; les
        // modificateurs Elite temporaires (Phase 3) ne piochent que parmi les touches restantes,
        // pour ne jamais ecraser un effet permanent.
        ApplyPermanentKeyEffects();
        ApplyEliteEncounter(LevelSession.Current?.eliteEncounter);
    }

    private void ApplyPermanentKeyEffects()
    {
        foreach (KeyValuePair<char, PermanentKeyEffectType> binding in LevelSession.PermanentKeyEffects)
        {
            if (keys.TryGetValue(binding.Key, out KeyView key))
                key.SetModifier(binding.Value.ToKeyModifier());
        }
    }

    // Applique les modificateurs de touches (desactivee/gluante/gelee) d'une particularite Elite.
    private void ApplyEliteEncounter(EliteEncounterConfig encounter)
    {
        if (encounter == null)
            return;

        List<KeyView> pool = keys.Values.Where(k => k.Modifier == KeyModifierType.None).ToList();

        if (encounter.singleModifierTypePerFight)
        {
            (KeyModifierType modifier, int count) = PickSingleModifierType(encounter);
            ApplyModifierToRandomKeys(pool, modifier, count, encounter.stickyHitsRequired);
            return;
        }

        ApplyModifierToRandomKeys(pool, KeyModifierType.Disabled, encounter.disabledKeyCount);
        ApplyModifierToRandomKeys(pool, KeyModifierType.Sticky, encounter.stickyKeyCount, encounter.stickyHitsRequired);
        ApplyModifierToRandomKeys(pool, KeyModifierType.Frozen, encounter.frozenKeyCount);
    }

    // Tire un seul type de modificateur (Casse/Englue/Gele), au poids de son compte configure,
    // pour que toutes les cases impactees d'un meme combat Elite restent thematiquement coherentes.
    private static (KeyModifierType modifier, int count) PickSingleModifierType(EliteEncounterConfig encounter)
    {
        var candidates = new List<(KeyModifierType modifier, int count)>();
        if (encounter.disabledKeyCount > 0) candidates.Add((KeyModifierType.Disabled, encounter.disabledKeyCount));
        if (encounter.stickyKeyCount > 0) candidates.Add((KeyModifierType.Sticky, encounter.stickyKeyCount));
        if (encounter.frozenKeyCount > 0) candidates.Add((KeyModifierType.Frozen, encounter.frozenKeyCount));

        if (candidates.Count == 0)
            return (KeyModifierType.None, 0);

        int totalCount = candidates.Sum(c => c.count);
        int roll = Random.Range(0, totalCount);
        int cursor = 0;
        foreach ((KeyModifierType modifier, int count) in candidates)
        {
            cursor += count;
            if (roll < cursor)
                return (modifier, totalCount);
        }

        return (candidates[candidates.Count - 1].modifier, totalCount);
    }

    private static void ApplyModifierToRandomKeys(List<KeyView> pool, KeyModifierType modifier, int count, int stickyHitsRequired = 3)
    {
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            pool[index].SetModifier(modifier, stickyHitsRequired);
            pool.RemoveAt(index);
        }
    }

    public void SetLayout(KeyboardLayoutType newLayout)
    {
        layout = newLayout;
        Generate();
    }

    [ContextMenu("Generate Keyboard")]
    public void Generate()
    {
        ClearChildren();
        keys.Clear();

        string[] rows = LayoutRows[layout];

        for (int row = 0; row < rows.Length; row++)
        {
            string chars = rows[row];
            for (int col = 0; col < chars.Length; col++)
            {
                char c = chars[col];
                Vector3 localPosition = new Vector3(col * spacingX + row * rowStagger, 0f, -row * spacingZ);

                KeyView key = Instantiate(keyPrefab, transform);
                key.transform.localPosition = localPosition;
                key.PickRandomVariant();
                key.SetCharacter(c, row, col);

                keys[c] = key;
            }
        }

        generatedLayout = layout;
    }

    [ContextMenu("Switch To AZERTY")]
    private void SwitchToAzerty() => SetLayout(KeyboardLayoutType.AZERTY);

    [ContextMenu("Switch To QWERTY")]
    private void SwitchToQwerty() => SetLayout(KeyboardLayoutType.QWERTY);

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (layout == generatedLayout || keyPrefab == null)
            return;

        // Deferred: DestroyImmediate/Instantiate ne sont pas surs a appeler directement depuis OnValidate.
        EditorApplication.delayCall += () =>
        {
            if (this == null)
                return;

            Generate();
        };
    }
#endif
}
