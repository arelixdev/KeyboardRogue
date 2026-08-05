using UnityEngine;

[CreateAssetMenu(fileName = "DungeonFloorConfig", menuName = "KeyboardRogue/Dungeon Floor")]
public class DungeonFloorConfig : ScriptableObject
{
    public string floorName = "Etage 1";
    [Min(1)] public int regularRowCount = 4;
    [Min(1)] public int columns = 4;
}
