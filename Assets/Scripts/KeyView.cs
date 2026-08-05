using TMPro;
using UnityEngine;

public enum KeyModifierType
{
    None,
    // Temporaires, remis a zero a chaque combat (particularite Elite, voir EliteEncounterConfig).
    Disabled,
    Sticky,
    Frozen,
    // Permanents pour le reste de la run, choisis par le joueur (voir BonusDefinition/Phase 4).
    Heal,
    Broken,
}

public class KeyView : MonoBehaviour
{
    private static readonly Color DisabledColor = new Color(0.25f, 0.25f, 0.25f);
    private static readonly Color StickyColor = new Color(0.35f, 0.75f, 0.25f);
    private static readonly Color FrozenColor = new Color(0.4f, 0.8f, 1f);
    private static readonly Color HealColor = new Color(0.95f, 0.45f, 0.75f);
    private static readonly Color BrokenColor = new Color(0.15f, 0.1f, 0.1f);

    public char Character { get; private set; }
    public int Row { get; private set; }
    public int Col { get; private set; }
    public KeyModifierType Modifier { get; private set; } = KeyModifierType.None;

    private TMP_Text label;
    private Renderer bodyRenderer;
    private Color baseColor;
    private int stickyHitsRemaining;

    public void SetCharacter(char character, int row, int col)
    {
        Character = character;
        Row = row;
        Col = col;

        if (label == null)
            label = GetComponentInChildren<TMP_Text>();

        label.text = character.ToString();
        name = $"Key_{character}";

        ResetModifier();
    }

    public void SetModifier(KeyModifierType modifier, int stickyHitsRequired = 3)
    {
        Modifier = modifier;
        stickyHitsRemaining = stickyHitsRequired;
        ApplyVisual();
    }

    public void ResetModifier()
    {
        Modifier = KeyModifierType.None;
        stickyHitsRemaining = 0;
        ApplyVisual();
    }

    // Appele quand le joueur presse une touche gluante sur laquelle il est englue. Renvoie vrai
    // une fois que suffisamment de pressions ont ete enregistrees et que la touche se libere.
    public bool RegisterStickyHit()
    {
        stickyHitsRemaining = Mathf.Max(0, stickyHitsRemaining - 1);
        if (stickyHitsRemaining > 0)
            return false;

        ResetModifier();
        return true;
    }

    private void ApplyVisual()
    {
        if (bodyRenderer == null)
            bodyRenderer = GetComponentInChildren<Renderer>();
        if (bodyRenderer == null)
            return;

        if (baseColor == default)
            baseColor = bodyRenderer.material.color;

        bodyRenderer.material.color = Modifier switch
        {
            KeyModifierType.Disabled => DisabledColor,
            KeyModifierType.Sticky => StickyColor,
            KeyModifierType.Frozen => FrozenColor,
            KeyModifierType.Heal => HealColor,
            KeyModifierType.Broken => BrokenColor,
            _ => baseColor,
        };
    }
}
