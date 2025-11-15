using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f; // Tốc độ di chuyển của Player

    private Rigidbody2D rb;
    private Vector2 movement;

    [Header("Attack")]
    public float attackDamage = 10f;    // Sát thương mỗi đòn đánh
    public float attackRange = 5f;     // Tầm đánh (vòng tròn xung quanh Player)
    public float attackCooldown = 1f;  // Tấn công mỗi 1.0 giây
    private float attackTimer;

    private int currentEXP = 0; // Biến đếm EXP

    // Biến để lưu Enemy gần nhất
    private Transform nearestEnemy;

    void Start()
    {
        // Lấy component Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        
        // Đặt timer = 0 để tấn công ngay lập tức khi bắt đầu
        attackTimer = 0f;
    }

    void Update()
    {
        // --- 1. XỬ LÝ DI CHUYỂN ---
        // Lấy input từ bàn phím (A/D/Left/Right)
        movement.x = Input.GetAxisRaw("Horizontal");
        // Lấy input từ bàn phím (W/S/Up/Down)
        movement.y = Input.GetAxisRaw("Vertical");

        // Chuẩn hóa vector để đi chéo không nhanh hơn đi thẳng
        movement.Normalize();

        // --- 2. XỬ LÝ TẤN CÔNG TỰ ĐỘNG ---
        // Đếm ngược timer
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
        else
        {
            // Khi timer = 0, tấn công và reset timer
            Attack();
            attackTimer = attackCooldown;
        }
    }

    void FixedUpdate()
    {
        // Di chuyển Player bằng Rigidbody trong FixedUpdate cho mượt
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    void Attack()
    {
        // --- Tìm Enemy gần nhất ---
        FindNearestEnemy();

        // Nếu tìm thấy Enemy và Enemy trong tầm đánh
        if (nearestEnemy != null && Vector2.Distance(transform.position, nearestEnemy.position) <= attackRange)
        {
            // Lấy script EnemyAI từ Enemy
            EnemyAI enemy = nearestEnemy.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                // Gọi hàm TakeDamage của Enemy
                enemy.TakeDamage(attackDamage);
                
                // (Tùy chọn) Bạn có thể thêm hiệu ứng/âm thanh đánh ở đây
            }
        }
    }

    void FindNearestEnemy()
    {
        float minDistance = Mathf.Infinity; // Khoảng cách nhỏ nhất, ban đầu là vô cực
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); // Tìm tất cả Enemy

        if (enemies.Length == 0)
        {
            nearestEnemy = null;
            return;
        }
        
        // Duyệt qua tất cả các Enemy
        foreach (GameObject enemyObject in enemies)
        {
            // Kiểm tra xem enemy còn "sống" không (tránh tấn công cái xác)
            if (enemyObject.GetComponent<EnemyAI>() != null && enemyObject.GetComponent<EnemyAI>().IsDead() == false)
            {
                float distance = Vector2.Distance(transform.position, enemyObject.transform.position);
            
                // Nếu enemy này gần hơn, lưu nó lại
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = enemyObject.transform;
                }
            }
        }
    }

    // Hàm này tự động được Unity gọi khi Player (có Rigidbody)
    // va chạm với một Collider khác (đã set "Is Trigger = true")
    void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem vật va chạm có Tag là "Experience" không
        if (other.CompareTag("Experience"))
        {
            // Nếu đúng:

            // 1. "Nhặt" (Hủy GameObject Gem đi)
            Destroy(other.gameObject);

            // 2. Tăng EXP
            currentEXP += 1; // (Bạn có thể tăng 5, 10... tùy ý)

            // 3. In ra Console để test
            Debug.Log("EXP: " + currentEXP);

            }
    }
}