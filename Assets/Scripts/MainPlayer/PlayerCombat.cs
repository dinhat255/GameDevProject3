using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackDamage = 10f;       // Mỗi đòn đánh gây bao nhiêu damage
    public float attackRange = 5f;         // Tầm đánh (player → enemy)
    public float attackCooldown = 1f;      // Thời gian hồi chiêu giữa 2 đòn
    private float attackTimer = 0f;

    private Transform nearestEnemy;

    [Header("EXP")]
    private int currentEXP = 0;

    void Update()
    {
        HandleAttack();
    }

    // ===============================
    //      ATTACK LOGIC
    // ===============================
    void HandleAttack()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            return;
        }

        // Timer đã về 0 → cho phép tấn công
        Attack();
        attackTimer = attackCooldown;
    }

    void Attack()
    {
        FindNearestEnemy();

        if (nearestEnemy == null)
            return;

        // Kiểm tra khoảng cách
        if (Vector2.Distance(transform.position, nearestEnemy.position) <= attackRange)
        {
            EnemyAI enemy = nearestEnemy.GetComponent<EnemyAI>();

            if (enemy != null && enemy.IsDead() == false)
            {
                enemy.TakeDamage(attackDamage);

                // 👉 Nếu bạn muốn thêm animation / sound / effect thì thêm ở đây
                // myAnimator.SetTrigger("attack");
            }
        }
    }

    // ===============================
    //      FIND NEAREST ENEMY
    // ===============================
    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float minDistance = Mathf.Infinity;
        nearestEnemy = null;

        foreach (GameObject enemyObj in enemies)
        {
            EnemyAI enemy = enemyObj.GetComponent<EnemyAI>();

            if (enemy == null || enemy.IsDead())
                continue;

            float dis = Vector2.Distance(transform.position, enemyObj.transform.position);

            if (dis < minDistance)
            {
                minDistance = dis;
                nearestEnemy = enemyObj.transform;
            }
        }
    }

    // ===============================
    //      EXP PICKUP
    // ===============================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Experience"))
        {
            currentEXP++;
            Destroy(other.gameObject);

            Debug.Log("EXP: " + currentEXP);
        }
    }
}
