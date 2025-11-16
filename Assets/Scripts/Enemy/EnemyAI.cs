using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float attackRange = 1.5f;
    public bool useSpriteFlipping = false;

    [Header("Stats")]
    public float baseHealth = 30f;
    public float attackDamage = 10f;
    public float attackCooldown = 2.0f;

    [Header("Drops")]
    public GameObject expGem;

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private float currentHealth;
    private bool isDead = false;
    private float attackTimer;

    private Vector3 baseScale;

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

        baseScale = transform.localScale;
    }

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
            Vector2 direction = (player.position - transform.position).normalized;

            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

            if (useSpriteFlipping)
            {
                float absScaleX = Mathf.Abs(baseScale.x);
                float scaleY = baseScale.y;
                float scaleZ = baseScale.z;

                if (direction.x > 0.01f)
                {
                    transform.localScale = new Vector3(-absScaleX, scaleY, scaleZ);
                }
                else if (direction.x < -0.01f)
                {
                    transform.localScale = new Vector3(absScaleX, scaleY, scaleZ);
                }
            }
            else
            {
                anim.SetFloat("MoveX", direction.x);
                anim.SetFloat("MoveY", direction.y);
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;

            if (attackTimer <= 0f)
            {
                anim.SetTrigger("Attack");
                attackTimer = attackCooldown;
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
            rb.linearVelocity = Vector2.zero;

            if (attackHitbox != null)
                attackHitbox.SetActive(false);

            GetComponent<Collider2D>().enabled = false;

            foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
                col.enabled = false;
            GetComponent<Collider2D>().enabled = false;
            Destroy(gameObject, 1.5f);

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

        currentHealth = baseHealth;

        Debug.Log($"Scaled → HP:{baseHealth} | DMG:{attackDamage} | SPD:{moveSpeed}");
    }

}