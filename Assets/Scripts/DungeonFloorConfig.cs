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
}
