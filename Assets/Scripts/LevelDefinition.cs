using UnityEngine;

[CreateAssetMenu(fileName = "LevelDefinition", menuName = "KeyboardRogue/Level Definition")]
public class LevelDefinition : ScriptableObject
{
    public string levelName = "Niveau";
    public KeyboardLayoutType keyboardLayout = KeyboardLayoutType.AZERTY;
    public int maxEnemiesPerLevel = 12;
    public int maxConcurrentEnemies = 3;
    public float spawnInterval = 4f;
}
