using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    public float damage = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyAI enemy = other.GetComponent<EnemyAI>();
        if (enemy != null && enemy.IsDead() == false)
        {
            enemy.TakeDamage(damage);
        }
    }
}
