using UnityEngine;

// Calcule les parametres de niveau (ennemis/difficulte) a partir d'une courbe configurable (SO).
public static class DifficultyScaling
{
    public static LevelDefinition Build(MapNodeType type, int depth, DifficultyCurveConfig curve, EliteEncounterConfig eliteEncounter = null, BossDefinition chosenBoss = null)
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

        // Particularite Elite: modificateurs additionnels par-dessus le bonus Elite normal.
        if (type == MapNodeType.Elite && eliteEncounter != null)
        {
            enemies = Mathf.RoundToInt(enemies * eliteEncounter.extraEnemiesMultiplier);
            concurrent += eliteEncounter.extraConcurrentBonus;
            interval *= eliteEncounter.extraIntervalMultiplier;
        }

        concurrent = Mathf.Clamp(concurrent, curve.minConcurrent, curve.maxConcurrent);

        // Un boss choisi remplace les vagues d'ennemis classiques: par defaut aucun mob (le combat
        // est le boss lui-meme), sauf si le boss demande explicitement des mobs en plus.
        if (type == MapNodeType.Boss && chosenBoss != null)
        {
            if (chosenBoss.spawnsMobs)
            {
                enemies = chosenBoss.maxEnemiesPerLevel;
                concurrent = chosenBoss.maxConcurrentEnemies;
                interval = chosenBoss.spawnInterval;
            }
            else
            {
                enemies = 0;
                concurrent = 0;
            }
        }

        LevelDefinition level = ScriptableObject.CreateInstance<LevelDefinition>();
        level.levelName = type.ToString();
        level.keyboardLayout = KeyboardPreference.Layout; // choisi une fois par le joueur, pas par la carte.
        level.maxEnemiesPerLevel = enemies;
        level.maxConcurrentEnemies = concurrent;
        level.spawnInterval = interval;
        level.eliteEncounter = type == MapNodeType.Elite ? eliteEncounter : null;
        level.chosenBoss = type == MapNodeType.Boss ? chosenBoss : null;
        return level;
    }
}
