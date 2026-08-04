using System.Collections.Generic;
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

    private void Awake()
    {
        // Awake plutot que Start: garantit que 'Keys' est deja rempli quand
        // d'autres scripts (ex: le joueur) le lisent dans leur propre Start().
        Generate();
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
