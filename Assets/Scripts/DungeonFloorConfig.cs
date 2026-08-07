using UnityEngine;

[CreateAssetMenu(fileName = "DungeonFloorConfig", menuName = "KeyboardRogue/Dungeon Floor")]
public class DungeonFloorConfig : ScriptableObject
{
    public string floorName = "Etage 1";
    [Min(1)] public int regularRowCount = 4;
    [Min(1)] public int columns = 4;

    [Header("Contenu")]
    public EventDefinition[] possibleEvents;

    [Range(0f, 1f)] public float eliteEncounterChance = 0.5f;
    public EliteEncounterConfig[] eliteEncounters;

    // Tire a la fin de chaque combat (Phase 4).
    public BonusDefinition[] possibleBonuses;

    // Sorts pouvant etre reveles/lances sur cet etage (Phase 5). Regroupes entre tous les etages
    // de la run pour l'attribution des sequences (voir SpellSystem/LevelSession).
    public SpellDefinition[] possibleSpells;

    // Boss pouvant apparaitre sur la rangee finale de cet etage (Phase 6). Tire au hasard; si vide,
    // le noeud Boss reste un combat classique a vagues d'ennemis (comportement historique).
    public BossDefinition[] possibleBosses;
}
