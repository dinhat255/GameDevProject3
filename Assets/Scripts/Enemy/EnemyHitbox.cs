using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    private const string PlayerTag = "Player";

    public float damage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(PlayerTag))
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }
}
