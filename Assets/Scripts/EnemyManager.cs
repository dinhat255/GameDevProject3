using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // Kéo Prefab "Enemy" của bạn vào đây trong Inspector
    public GameObject enemyPrefab; 
    
    // Thời gian giữa mỗi lần "đẻ" quái (giây)
    public float initialSpawnInterval = 10.0f; // ĐỔI TÊN: Thời gian spawn ban đầu
    public float minSpawnInterval = 0.5f;     // THÊM MỚI: Thời gian spawn ít nhất (nhanh nhất)
    
    // Khoảng cách tối thiểu/tối đa so với Player
    public float minSpawnRadius = 10f; 
    public float maxSpawnRadius = 15f;

    public int maxEnemies = 50;

    private bool isStart = true;

    [Header("Difficulty Scaling")]
    public float timeToMaxDifficulty = 300f; // THÊM MỚI: Thời gian để đạt độ khó tối đa (300s = 5 phút)

    private float spawnTimer;
    private Transform playerTransform;

    private float gameTimer = 0f; // THÊM MỚI: Đồng hồ bấm giờ toàn cục

    void Start()
    {
        // Bắt đầu đếm ngược để "đẻ" ngay
        spawnTimer = initialSpawnInterval; // Dùng thời gian ban đầu
        
        // Tìm Player
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (isStart)
        {
            isStart = false;
            Instantiate(enemyPrefab, playerTransform.position + new Vector3(3f, 3f, 0f), Quaternion.identity);
            print("Spawned initial enemy");
        }
        
        if (playerTransform == null)
        {
            Debug.LogError("Không tìm thấy Player! Hãy đảm bảo Player có Tag là 'Player'");
        }
    }

    void Update()
    {
        if (playerTransform == null) return; 

        // 1. Đồng hồ toàn cục luôn đếm
        gameTimer += Time.deltaTime;

        spawnTimer -= Time.deltaTime;

        

        if (spawnTimer <= 0f)
        {
            // 2. Tính toán thời gian spawn MỚI dựa trên gameTimer
            // Dùng Lerp để giảm dần từ initial xuống min over time
            float currentSpawnInterval = Mathf.Lerp(
                initialSpawnInterval,  // Giảm từ...
                minSpawnInterval,      // ...xuống đến
                gameTimer / timeToMaxDifficulty // % quãng đường đã đi (từ 0.0 đến 1.0)
            );

            // 3. Đặt lại đồng hồ spawn với giá trị MỚI
            spawnTimer = currentSpawnInterval; 

            // Logic kiểm tra số lượng Enemy cũ
            int currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy(); // Gọi hàm "đẻ" quái
            }
            
        }
    }

    void SpawnEnemy()
    {
        // 1. Tính một góc ngẫu nhiên (0 đến 360 độ)
        float randomAngle = Random.Range(0f, 360f);
        float randomRadius = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector2 spawnDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        Vector3 spawnPosition = playerTransform.position + (Vector3)(spawnDirection * randomRadius);

        // --- NÂNG CẤP HÀM SPAWN ---
        // 4. "Đẻ" quái và lưu nó vào một biến
        GameObject newEnemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // 5. Lấy script EnemyAI từ con quái vừa "đẻ"
        EnemyAI enemyScript = newEnemyObject.GetComponent<EnemyAI>();

        // 6. "Bơm" máu cho nó
        if (enemyScript != null)
        {
            // Gọi một hàm mới (chúng ta sẽ tạo ở Bước 2)
            // và truyền "gameTimer" (thời gian hiện tại) vào
            enemyScript.ScaleStatsBasedOnTime(gameTimer);
        }
    }
}