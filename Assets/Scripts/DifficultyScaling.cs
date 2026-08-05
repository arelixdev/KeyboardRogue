using UnityEngine;

// Calcule les parametres de niveau (ennemis/difficulte) a partir d'une courbe configurable (SO).
public static class DifficultyScaling
{
    public static LevelDefinition Build(MapNodeType type, int depth, DifficultyCurveConfig curve)
    {
        int enemies = curve.baseEnemies + Mathf.RoundToInt(depth * curve.enemiesPerRow);
        int concurrent = curve.baseConcurrent + Mathf.RoundToInt(depth * curve.concurrentPerRow);
        float interval = Mathf.Max(curve.minInterval, curve.baseInterval - depth * curve.intervalReductionPerRow);

        switch (type)
        {
            case MapNodeType.Elite:
                enemies = Mathf.RoundToInt(enemies * curve.eliteEnemiesMultiplier);
                concurrent += curve.eliteConcurrentBonus;
                interval *= curve.eliteIntervalMultiplier;
                break;
            case MapNodeType.Boss:
                enemies = Mathf.RoundToInt(enemies * curve.bossEnemiesMultiplier);
                concurrent += curve.bossConcurrentBonus;
                interval *= curve.bossIntervalMultiplier;
                break;
        }

        concurrent = Mathf.Clamp(concurrent, curve.minConcurrent, curve.maxConcurrent);

        LevelDefinition level = ScriptableObject.CreateInstance<LevelDefinition>();
        level.levelName = type.ToString();
        level.keyboardLayout = KeyboardPreference.Layout; // choisi une fois par le joueur, pas par la carte.
        level.maxEnemiesPerLevel = enemies;
        level.maxConcurrentEnemies = concurrent;
        level.spawnInterval = interval;
        return level;
    }
}
