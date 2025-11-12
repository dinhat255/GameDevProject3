using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // Các biến cũ
    public float moveSpeed = 3f;
    private Transform player;
    private Rigidbody2D rb;

    // Các biến mới
    private Animator anim; // Biến để điều khiển Animator
    public float health = 30f; // Máu của Enemy
    public float attackRange = 1.5f; // Tầm đánh
    private bool isDead = false; // Cờ đánh dấu đã chết

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // Lấy component Animator
        
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Không tìm thấy GameObject với tag 'Player'!");
        }
    }

    void FixedUpdate()
    {
        if (isDead || player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            // --- TRẠNG THÁI: DI CHUYỂN ---
            Vector2 direction = (player.position - transform.position).normalized;
            
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

            // Cập nhật Animator để chọn hoạt ảnh đi bộ đúng hướng
            anim.SetFloat("MoveX", direction.x);
            anim.SetFloat("MoveY", direction.y);
        }
        else
        {
            // --- TRẠNG THÁI: TẤN CÔNG ---
            rb.linearVelocity = Vector2.zero;
            // anim.SetFloat("MoveX", 0); // Đảm bảo hoạt ảnh dừng hướng
            // anim.SetFloat("MoveY", 0); // khi không di chuyển
            anim.SetTrigger("Attack");
        }
    }

    // Đây là hàm để các vũ khí của Player gọi
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return; // Nếu chết rồi thì không nhận sát thương nữa

        health -= damageAmount;

        if (health <= 0)
        {
            // --- TRẠNG THÁI: CHẾT ---
            isDead = true;
            anim.SetBool("isDead", true);
            anim.SetTrigger("Die"); // Kích hoạt Trigger "Die"
            rb.linearVelocity = Vector2.zero; // Ngừng di chuyển
            GetComponent<Collider2D>().enabled = false; // Tắt va chạm
            // Tùy chọn: Hủy đối tượng này sau vài giây
            // Destroy(gameObject, 3f); // Hủy sau 3 giây
            Destroy(gameObject, 1.0f);
        }
        else
        {
            // --- TRẠNG THÁI: BỊ ĐÁNH ---
            anim.SetTrigger("TakeDamage"); // Kích hoạt Trigger "TakeDamage"
        }
    }

    public bool IsDead()
    {
        return isDead;
    }
}