using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnWave
{
    public GameObject enemyPrefab;
    public float timeToStartSpawning = 0f;
}

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Unlock Waves")]
    public EnemySpawnWave[] spawnWaves;

    [Header("Spawn Settings")]
    public float initialSpawnInterval = 2.0f;
    public float minSpawnInterval = 0.5f;
    public int maxEnemies = 50;
    public float minSpawnRadius = 10f;
    public float maxSpawnRadius = 15f;

    [Header("Difficulty Scaling")]
    public float waveInterval = 30f;
    public float healthMultiplierPerWave = 1.15f;
    public float damageMultiplierPerWave = 1.10f;
    public float speedMultiplierPerWave = 1.05f;

    public int currentWave = 1;
    private float spawnTimer = 0f;
    private float gameTimer = 0f;

    private Transform player;
    private List<GameObject> availableEnemies = new List<GameObject>();

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("EnemyManager: Không tìm thấy Player!");
            enabled = false;
            return;
        }

        UpdateAvailableEnemies();
    }

    private void Update()
    {
        if (player == null) return;

        gameTimer += Time.deltaTime;
        spawnTimer -= Time.deltaTime;

        HandleWaveProgression();
        UpdateAvailableEnemies();

        if (spawnTimer <= 0f)
        {
            float spawnInterval = Mathf.Lerp(
                initialSpawnInterval,
                minSpawnInterval,
                gameTimer / 300f
            );

            spawnTimer = spawnInterval;

            int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (enemyCount < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    private void HandleWaveProgression()
    {
        if (gameTimer >= currentWave * waveInterval)
        {
            currentWave++;
            Debug.Log($"WAVE UP → Wave {currentWave}");
        }
    }

    private void UpdateAvailableEnemies()
    {
        foreach (var wave in spawnWaves)
        {
            if (gameTimer >= wave.timeToStartSpawning &&
                !availableEnemies.Contains(wave.enemyPrefab))
            {
                availableEnemies.Add(wave.enemyPrefab);
                Debug.Log("Unlocked Enemy: " + wave.enemyPrefab.name);
            }
        }
    }

    private void SpawnEnemy()
    {
        if (availableEnemies.Count == 0) return;

        GameObject prefab = availableEnemies[Random.Range(0, availableEnemies.Count)];

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector3 spawnPos = player.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;

        GameObject enemyObj = Instantiate(prefab, spawnPos, Quaternion.identity);

        EnemyAI ai = enemyObj.GetComponent<EnemyAI>();
        if (ai != null)
        {
            float h = Mathf.Pow(healthMultiplierPerWave, currentWave - 1);
            float d = Mathf.Pow(damageMultiplierPerWave, currentWave - 1);
            float s = Mathf.Pow(speedMultiplierPerWave, currentWave - 1);

            ai.ScaleStats(h, d, s);
        }
    }
}
