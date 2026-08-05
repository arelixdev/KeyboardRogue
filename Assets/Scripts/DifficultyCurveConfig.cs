using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyCurveConfig", menuName = "KeyboardRogue/Difficulty Curve")]
public class DifficultyCurveConfig : ScriptableObject
{
    [Header("Base (profondeur 0)")]
    public int baseEnemies = 6;
    public int baseConcurrent = 2;
    public float baseInterval = 5f;

    [Header("Croissance par rangee de profondeur")]
    public float enemiesPerRow = 2f;
    public float concurrentPerRow = 0.5f;
    public float intervalReductionPerRow = 0.3f;
    public float minInterval = 1.5f;

    [Header("Multiplicateurs Elite")]
    public float eliteEnemiesMultiplier = 1.5f;
    public int eliteConcurrentBonus = 1;
    public float eliteIntervalMultiplier = 0.8f;

    [Header("Multiplicateurs Boss")]
    public float bossEnemiesMultiplier = 2f;
    public int bossConcurrentBonus = 2;
    public float bossIntervalMultiplier = 0.6f;

    [Header("Limites")]
    public int minConcurrent = 2;
    public int maxConcurrent = 6;
}
