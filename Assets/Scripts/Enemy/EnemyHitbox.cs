using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    // Script này sẽ được EnemyAI "ra lệnh"
    public float damage;

    // Hàm này tự chạy khi Player đi vào vùng Trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Tìm script PlayerHealth trên Player và gây sát thương
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}