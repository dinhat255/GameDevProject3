using UnityEngine;
using System.Collections.Generic;

public class WeaponHitbox : MonoBehaviour
{
    public float damage = 1f;

    private HashSet<EnemyAI> alreadyHitEnemies = new HashSet<EnemyAI>();
    private PlayerCombat playerCombat;

    private void Awake()
    {
        playerCombat = GetComponentInParent<PlayerCombat>();
    }

    private void OnEnable()
    {
        alreadyHitEnemies.Clear();

        if (playerCombat != null)
        {
            damage = playerCombat.CurrentDamage;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyAI enemy = other.GetComponentInParent<EnemyAI>();
        if (enemy != null && enemy.IsDead() == false)
        {
            if (alreadyHitEnemies.Contains(enemy)) return;

            alreadyHitEnemies.Add(enemy);
            enemy.TakeDamage(damage);
        }
    }
}
