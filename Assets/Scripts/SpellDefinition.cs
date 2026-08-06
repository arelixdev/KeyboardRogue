using UnityEngine;

public enum SpellEffectType
{
    DamageAllEnemies,
    Heal,
    FreezeAllEnemies,
    DestroyRandomEnemy,
}

// Un sort = une sequence de touches distinctes (longueur propre au sort) a taper dans l'ordre.
// Les touches exactes sont tirees au hasard une fois par run (voir SpellSystem), toujours
// differentes d'une run a l'autre mais toujours de la meme longueur ici configuree.
[CreateAssetMenu(fileName = "SpellDefinition", menuName = "KeyboardRogue/Spell")]
public class SpellDefinition : ScriptableObject
{
    public string spellName = "Sort";
    [TextArea] public string description = "Description de l'effet.";

    [Range(2, 6)] public int sequenceLength = 3;

    public SpellEffectType effectType;
    // Degats (DamageAllEnemies), PV (Heal) ou duree en secondes (FreezeAllEnemies). Ignore pour DestroyRandomEnemy.
    public float effectValue = 1f;
}
