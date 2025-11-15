using UnityEngine;
using System.Collections.Generic; // BẮT BUỘC CÓ dòng này để dùng List

// [System.Serializable] để nó hiện ra trong Inspector
[System.Serializable]
public class EnemySpawnWave
{
    public GameObject enemyPrefab;    // Kéo Prefab (Bat, Ghost...) vào đây
    public float timeToStartSpawning; // Thời gian "mở khóa" quái này (tính bằng giây)
}

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Tiers (Sắp xếp từ yếu đến mạnh)")]
    // THAY THẾ: Dùng một danh sách (Array) các đợt quái
    public EnemySpawnWave[] spawnWaves;

    [Header("Spawning Settings")]
    public float initialSpawnInterval = 2.0f;
    public float minSpawnInterval = 0.5f;
    public int maxEnemies = 50;
    public float minSpawnRadius = 10f;
    public float maxSpawnRadius = 15f;

    [Header("Difficulty Scaling")]
    public float timeToMaxDifficulty = 300f; // 5 phút

    private float spawnTimer;
    private Transform playerTransform;
    private float gameTimer = 0f;

    // Biến này để lưu danh sách quái có thể "đẻ"
    private List<GameObject> availableEnemies = new List<GameObject>();

    void Start()
    {
        spawnTimer = 0f;
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerTransform == null)
        {
            Debug.LogError("Không tìm thấy Player!");
        }

        // Cập nhật danh sách quái có thể spawn ngay lúc bắt đầu
        UpdateAvailableEnemies();
    }

    void Update()
    {
        if (playerTransform == null) return;

        gameTimer += Time.deltaTime;
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            // Tính toán thời gian spawn (logic này vẫn giữ nguyên)
            float currentSpawnInterval = Mathf.Lerp(
                initialSpawnInterval,
                minSpawnInterval,
                gameTimer / timeToMaxDifficulty
            );
            spawnTimer = currentSpawnInterval;

            // Kiểm tra xem có nên "mở khóa" quái mới không
            UpdateAvailableEnemies();

            // Spawn quái
            int currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy(); // Gọi hàm "đẻ" quái (đã được nâng cấp)
            }
        }
    }

    // Kiểm tra xem thời gian đã đủ để thêm quái mới vào danh sách chưa
    void UpdateAvailableEnemies()
    {
        // Duyệt qua tất cả các đợt quái bạn cài đặt
        foreach (var wave in spawnWaves)
        {
            // Nếu thời gian game >= thời gian "mở khóa"
            if (gameTimer >= wave.timeToStartSpawning)
            {
                // Và nếu quái này CHƯA có trong danh sách
                if (!availableEnemies.Contains(wave.enemyPrefab))
                {
                    // Thêm nó vào danh sách "có thể đẻ"
                    availableEnemies.Add(wave.enemyPrefab);
                    Debug.Log("Đã mở khóa quái mới: " + wave.enemyPrefab.name);
                }
            }
        }
    }

    void SpawnEnemy()
    {
        // Nếu không có quái nào (game vừa bắt đầu, chưa kịp load)
        if (availableEnemies.Count == 0) return;

        // --- SỬA LỖI ---
        // 1. Chọn 1 Prefab NGẪU NHIÊN từ danh sách "đã mở khóa"
        GameObject prefabToSpawn = availableEnemies[Random.Range(0, availableEnemies.Count)];

        // 2. Code tính vị trí spawn (giữ nguyên)
        float randomAngle = Random.Range(0f, 360f);
        float randomRadius = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector2 spawnDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        Vector3 spawnPosition = playerTransform.position + (Vector3)(spawnDirection * randomRadius);

        // 3. "Đẻ" con quái đã chọn ngẫu nhiên
        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        // 4. (XÓA BỎ) Không gọi hàm ScaleStatsBasedOnTime nữa
        // vì sức mạnh quái là cố định
    }
}