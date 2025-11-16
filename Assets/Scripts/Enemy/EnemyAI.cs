using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float attackRange = 1.5f; // Tầm đánh
    public bool useSpriteFlipping = false; // THÊM MỚI: Tích vào ô này cho con Ma (Ghost)

    [Header("Stats")]
    public float baseHealth = 30f; // Máu cơ bản
    public float attackDamage = 10f; // Sát thương
    public float attackCooldown = 2.0f; // Tấn công mỗi 2 giây

    [Header("Drops")]
    public GameObject expGem; // Prefab của exp

    // Biến nội bộ
    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private float currentHealth;
    private bool isDead = false;
    private float attackTimer; // Biến đếm cooldown

    private Vector3 baseScale;

    // Biến cho Hitbox (nếu bạn dùng)
    public GameObject attackHitbox;

    [Header("Drops")]
    public int expDropCount = 1;
    void Start()
    {
        currentHealth = baseHealth;

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        player = GameObject.FindGameObjectWithTag("Player").transform;

        attackTimer = 0f;

        // Lưu lại kích thước gốc khi game bắt đầu
        baseScale = transform.localScale;
    }

    // XÓA BỎ: Hàm ScaleStatsBasedOnTime() và healthPerMinute đã bị xóa

    void FixedUpdate()
    {
        if (isDead || player == null) return;

        if (attackTimer > 0)
        {
            attackTimer -= Time.fixedDeltaTime;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            // --- TRẠNG THÁI: DI CHUYỂN (FLY/WALK) ---
            Vector2 direction = (player.position - transform.position).normalized;

            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

            // --- LOGIC MỚI: LẬT SPRITE HOẶC DÙNG BLEND TREE ---
            if (useSpriteFlipping)
            {
                // Lấy giá trị X tuyệt đối (luôn dương) từ scale gốc
                float absScaleX = Mathf.Abs(baseScale.x);
                float scaleY = baseScale.y;
                float scaleZ = baseScale.z;

                // Nếu Enemy di chuyển sang phải (hướng X dương)
                if (direction.x > 0.01f)
                {
                    // Sprite sẽ lật sang trái (scale X âm) để "quay mặt" về hướng Player đang đi qua
                    transform.localScale = new Vector3(-absScaleX, scaleY, scaleZ);
                }
                // Nếu Enemy di chuyển sang trái (hướng X âm)
                else if (direction.x < -0.01f)
                {
                    // Sprite sẽ quay về hướng Player (scale X dương)
                    transform.localScale = new Vector3(absScaleX, scaleY, scaleZ);
                }
                // (Nếu sprite của bạn có chiều hướng ngược lại,
                // bạn có thể đổi -absScaleX thành absScaleX và ngược lại)
            }
            else
            {
                // Dùng cho Bat, Warlock, Pumpkin (quái có 8 hướng)
                anim.SetFloat("MoveX", direction.x);
                anim.SetFloat("MoveY", direction.y);
            }
            // --------------------------------------------------
        }
        else
        {
            // --- TRẠNG THÁI: TẤN CÔNG (ATTACK / APPEAR) ---
            rb.linearVelocity = Vector2.zero; // SỬA LỖI: dùng 'velocity'

            // CHỈ TẤN CÔNG NẾU TIMER <= 0
            if (attackTimer <= 0f)
            {
                anim.SetTrigger("Attack");
                attackTimer = attackCooldown; // Đặt lại timer!
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            isDead = true;
            anim.SetBool("isDead", true);
            anim.SetTrigger("Die");
            rb.linearVelocity = Vector2.zero; // SỬA LỖI: dùng 'velocity'
                                              // TẮT TẤT CẢ HITBOX NGAY KHI CHẾT  ⬇⬇⬇
            if (attackHitbox != null)
                attackHitbox.SetActive(false);

            // Tắt collider thân chính
            GetComponent<Collider2D>().enabled = false;

            // Nếu enemy có nhiều collider (trong con) → tắt luôn
            foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
                col.enabled = false;
            GetComponent<Collider2D>().enabled = false;
            Destroy(gameObject, 1.5f); // Tăng thời gian lên 1.5s cho chắc

            if (expGem != null)
            {
                for (int i = 0; i < expDropCount; i++)
                {
                    Vector2 offset = Random.insideUnitCircle * 0.3f;
                    Instantiate(expGem, (Vector2)transform.position + offset, Quaternion.identity);
                }
            }

        }
        else
        {
            anim.SetTrigger("TakeDamage");
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    // --- CÁC HÀM CHO HITBOX (nếu bạn dùng) ---
    public void EnableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            EnemyHitbox hitboxScript = attackHitbox.GetComponent<EnemyHitbox>();
            if (hitboxScript != null)
            {
                hitboxScript.damage = this.attackDamage;
            }
            attackHitbox.SetActive(true);
        }
    }

    public void DisableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    public void ScaleStats(float healthMul, float damageMul, float speedMul)
    {
        baseHealth *= healthMul;
        attackDamage *= damageMul;
        moveSpeed *= speedMul;

        currentHealth = baseHealth; // reset HP đã scale

        Debug.Log($"Scaled → HP:{baseHealth} | DMG:{attackDamage} | SPD:{moveSpeed}");
    }

}