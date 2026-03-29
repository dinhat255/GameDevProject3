using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class EnemySpawnWave
{
    public GameObject enemyPrefab;
    public float timeToStartSpawning = 0f;
}

public class EnemyManager : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const string EnemyTag = "Enemy";
    private const float SpawnRampDuration = 90f;

    [Header("Enemy Unlock Waves")]
    public EnemySpawnWave[] spawnWaves;

    [Header("Spawn Settings")]
    public float initialSpawnInterval = 2.0f;
    public float minSpawnInterval = 0.5f;
    public int maxEnemies = 50;
    public float minSpawnRadius = 10f;
    public float maxSpawnRadius = 15f;
    public int poolPrewarmPerEnemyType = 8;
    public int expPoolPrewarmPerType = 24;

    [Header("Wave Flow")]
    public int baseEnemiesPerWave = 12;
    public int enemiesPerWaveIncrement = 4;
    public float nextWaveDelay = 1.0f;

    [Header("Difficulty Scaling")]
    public float healthMultiplierPerWave = 1.15f;
    public float damageMultiplierPerWave = 1.10f;
    public float speedMultiplierPerWave = 1.05f;

    public int currentWave = 1;
    private float spawnTimer = 0f;
    private float gameTimer = 0f;
    private int activeEnemyCount = 0;
    private bool spawnPaused = false;
    private bool waveActive = false;
    private int spawnedInWave = 0;
    private int targetEnemiesInWave = 0;
    private float nextWaveTimer = 0f;

    private Transform player;
    private readonly List<GameObject> availableEnemies = new List<GameObject>();
    private EnemyPool enemyPool;
    private ExpGemPool expGemPool;

    public event Action<int> WaveChanged;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag(PlayerTag)?.transform;
        enemyPool = new EnemyPool();
        expGemPool = new ExpGemPool();

        if (player == null)
        {
            enabled = false;
            return;
        }

        activeEnemyCount = GameObject.FindGameObjectsWithTag(EnemyTag).Length;
        InitializePools();
        UpdateAvailableEnemies();
        BeginWave(currentWave);
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        gameTimer += Time.deltaTime;
        spawnTimer -= Time.deltaTime;

        UpdateAvailableEnemies();

        if (!waveActive)
        {
            HandleNextWaveCountdown();
            return;
        }

        if (IsCurrentWaveCompleted())
        {
            EndCurrentWave();
            return;
        }

        if (spawnTimer > 0f)
        {
            return;
        }

        if (spawnPaused)
        {
            return;
        }

        if (spawnedInWave >= targetEnemiesInWave)
        {
            return;
        }

        spawnTimer = CalculateSpawnInterval();

        if (CanSpawnMoreEnemies())
        {
            SpawnEnemy();
            spawnedInWave++;
        }
    }

    private void HandleNextWaveCountdown()
    {
        if (nextWaveTimer > 0f)
        {
            nextWaveTimer -= Time.deltaTime;
            return;
        }

        BeginWave(currentWave);
    }

    private void UpdateAvailableEnemies()
    {
        foreach (var wave in spawnWaves)
        {
            if (wave == null || wave.enemyPrefab == null)
            {
                continue;
            }

            if (gameTimer >= wave.timeToStartSpawning &&
                !availableEnemies.Contains(wave.enemyPrefab))
            {
                availableEnemies.Add(wave.enemyPrefab);
            }
        }
    }

    private void SpawnEnemy()
    {
        if (availableEnemies.Count == 0)
        {
            return;
        }

        GameObject prefab = availableEnemies[UnityEngine.Random.Range(0, availableEnemies.Count)];
        EnemyAI ai = GetEnemyFromPool(prefab);
        if (ai == null)
        {
            return;
        }

        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = UnityEngine.Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector3 spawnPos = player.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;

        ai.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);

        float h = Mathf.Pow(healthMultiplierPerWave, currentWave - 1);
        float d = Mathf.Pow(damageMultiplierPerWave, currentWave - 1);
        float s = Mathf.Pow(speedMultiplierPerWave, currentWave - 1);

        ai.ConfigureForSpawn(player, h, d, s, ReturnEnemyToPool);
        ai.SetExpDropSpawner(expGemPool.Spawn);
        activeEnemyCount++;
    }

    private float CalculateSpawnInterval()
    {
        return Mathf.Lerp(initialSpawnInterval, minSpawnInterval, gameTimer / SpawnRampDuration);
    }

    private bool CanSpawnMoreEnemies()
    {
        return activeEnemyCount < maxEnemies;
    }

    private bool IsCurrentWaveCompleted()
    {
        return spawnedInWave >= targetEnemiesInWave && activeEnemyCount <= 0;
    }

    private void EndCurrentWave()
    {
        waveActive = false;
        nextWaveTimer = Mathf.Max(0f, nextWaveDelay);
        currentWave++;
    }

    private void BeginWave(int waveNumber)
    {
        waveActive = true;
        spawnedInWave = 0;
        targetEnemiesInWave = Mathf.Max(1, baseEnemiesPerWave + (waveNumber - 1) * enemiesPerWaveIncrement);
        spawnTimer = 0f;
        WaveChanged?.Invoke(waveNumber);
    }

    public void SetSpawnPaused(bool paused)
    {
        spawnPaused = paused;
    }

    private void InitializePools()
    {
        if (spawnWaves == null)
        {
            return;
        }

        HashSet<GameObject> uniquePrefabs = new HashSet<GameObject>();
        foreach (EnemySpawnWave wave in spawnWaves)
        {
            if (wave != null && wave.enemyPrefab != null)
            {
                uniquePrefabs.Add(wave.enemyPrefab);
            }
        }

        foreach (GameObject prefab in uniquePrefabs)
        {
            enemyPool.Prewarm(prefab, poolPrewarmPerEnemyType);
        }

        expGemPool.PrewarmFromEnemies(uniquePrefabs, expPoolPrewarmPerType);
    }

    private EnemyAI GetEnemyFromPool(GameObject prefab)
    {
        return enemyPool.Rent(prefab);
    }

    private void ReturnEnemyToPool(EnemyAI ai)
    {
        if (ai == null)
        {
            return;
        }

        enemyPool.Return(ai);
        activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);
    }
}
