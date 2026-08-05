using UnityEngine;

// Particularite optionnelle tiree pour un noeud Elite: modificateurs de combat en plus du bonus
// Elite normal, et/ou modificateurs de touches sur le clavier de la zone de combat.
[CreateAssetMenu(fileName = "EliteEncounterConfig", menuName = "KeyboardRogue/Elite Encounter")]
public class EliteEncounterConfig : ScriptableObject
{
    public string encounterName = "Elite";
    [TextArea] public string description;

    [Header("Modificateurs de combat (en plus du bonus Elite normal)")]
    public float extraEnemiesMultiplier = 1f;
    public int extraConcurrentBonus = 0;
    public float extraIntervalMultiplier = 1f;

    [Header("Touches desactivees (non ciblables)")]
    public int disabledKeyCount = 0;

    [Header("Touches gluantes (spam la meme touche pour se degluer)")]
    public int stickyKeyCount = 0;
    public int stickyHitsRequired = 3;

    [Header("Touches gelees (degats a l'arrivee et au passage)")]
    public int frozenKeyCount = 0;
}
