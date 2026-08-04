using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // Tous les ennemis du niveau ont ete generes et vaincus. Un futur systeme d'evenement s'en servira pour enchainer la suite.
    public event Action AllEnemiesDefeated;

    [SerializeField] private EnemyBasic enemyPrefab;
    [SerializeField] private int maxEnemiesPerLevel = 12;
    [SerializeField] private int maxConcurrentEnemies = 3;
    [SerializeField] private float spawnInterval = 4f;

    private readonly List<EnemyBasic> activeEnemies = new List<EnemyBasic>();
    private int spawnedCount;
    private float spawnTimer;
    private bool zoneCleared;

    public int SpawnedCount => spawnedCount;
    public int ActiveCount => activeEnemies.Count;

    private void Update()
    {
        if (spawnedCount >= maxEnemiesPerLevel)
            return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer < spawnInterval)
            return;

        spawnTimer = 0f;

        if (activeEnemies.Count >= maxConcurrentEnemies)
            return;

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        EnemyBasic enemy = Instantiate(enemyPrefab);
        enemy.Removed += OnEnemyRemoved;

        activeEnemies.Add(enemy);
        spawnedCount++;
    }

    private void OnEnemyRemoved(EnemyBasic enemy)
    {
        enemy.Removed -= OnEnemyRemoved;
        activeEnemies.Remove(enemy);

        if (!zoneCleared && spawnedCount >= maxEnemiesPerLevel && activeEnemies.Count == 0)
        {
            zoneCleared = true;
            AllEnemiesDefeated?.Invoke();
        }
    }
}
