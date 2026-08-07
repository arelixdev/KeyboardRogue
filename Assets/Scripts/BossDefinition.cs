using UnityEngine;

// Un boss selectionnable par etage (DungeonFloorConfig.possibleBosses). Le prefab doit porter un
// composant derivant de BossController + un Health.
[CreateAssetMenu(fileName = "BossDefinition", menuName = "KeyboardRogue/Boss")]
public class BossDefinition : ScriptableObject
{
    public string bossName = "Boss";
    [TextArea] public string description;

    public GameObject bossPrefab;
    [Min(1)] public int bossHp = 20;

    // Par defaut un combat de boss n'a pas de mobs. "Condition particuliere": certains boss
    // peuvent en faire apparaitre en plus (reutilise les memes champs qu'un LevelDefinition normal).
    public bool spawnsMobs = false;
    public int maxEnemiesPerLevel = 6;
    public int maxConcurrentEnemies = 2;
    public float spawnInterval = 5f;

    [Header("Poing (FistBossController)")]
    [Tooltip("Duree (s) pendant laquelle un poing reste pose au sol/vulnerable avant de remonter.")]
    public float fistVulnerableDuration = 4f;
    [Tooltip("Portee de la zone d'ombre autour de la case ciblee, en cases, dans chacune des 4 directions (croix).")]
    [Min(0)] public int fistZoneRadius = 2;
    [Tooltip("Echelle visuelle des spheres representant les poings.")]
    public float fistVisualScale = 0.85f;
    [Tooltip("Duree (s) sur laquelle l'onde de choc au sol grandit apres l'impact.")]
    public float shockwaveGrowDuration = 1f;
    [Tooltip("Rayon max (unites monde) atteint par l'onde de choc.")]
    public float shockwaveMaxRadius = 1.4f;
    [Tooltip("Degats infliges au joueur si l'onde de choc le touche.")]
    public int shockwaveDamage = 1;
}
