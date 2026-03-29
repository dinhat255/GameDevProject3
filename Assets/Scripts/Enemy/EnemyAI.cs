using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    private const string MoveXParam = "MoveX";
    private const string MoveYParam = "MoveY";
    private const string AttackTrigger = "Attack";
    private const string TakeDamageTrigger = "TakeDamage";
    private const string DieTrigger = "Die";
    private const string IsDeadParam = "isDead";
    private const float FlipThreshold = 0.01f;
    private const float DropScatterRadius = 0.3f;
    private const float DestroyDelay = 1.5f;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float attackRange = 1.5f;
    public bool useSpriteFlipping = false;

    [Header("Performance")]
    public float aiTickInterval = 0.08f;
    public float farAiTickInterval = 0.18f;
    public float farDistance = 12f;
    public bool logSpawnScaling = false;

    [Header("Stats")]
    public float baseHealth = 30f;
    public float attackDamage = 10f;
    public float attackCooldown = 2.0f;

    [Header("Drops")]
    public GameObject expGem;
    public int expDropCount = 1;

    [Header("Combat")]
    public GameObject attackHitbox;

    [Header("VFX")]
    public ParticleSystem hitParticlePrefab;
    public Vector3 hitParticleOffset = Vector3.zero;
    public float hitParticleLife = 1f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animatorRef;
    private float currentHealth;
    private bool isDead;
    private float attackTimer;
    private Vector3 baseScale;

    private float defaultBaseHealth;
    private float defaultAttackDamage;
    private float defaultMoveSpeed;

    private float thinkTimer;
    private float attackRangeSqr;
    private float farDistanceSqr;
    private Vector2 desiredDirection;
    private float lastAnimX;
    private float lastAnimY;

    private Action<EnemyAI> returnToPoolAction;
    private Action<GameObject, Vector3> spawnExpDropAction;

    private static readonly Dictionary<int, Queue<ParticleSystem>> HitParticlePools = new Dictionary<int, Queue<ParticleSystem>>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animatorRef = GetComponent<Animator>();
        baseScale = transform.localScale;

        defaultBaseHealth = baseHealth;
        defaultAttackDamage = attackDamage;
        defaultMoveSpeed = moveSpeed;

        currentHealth = baseHealth;
        attackTimer = 0f;
        RecalculateCachedThresholds();
    }

    private void OnEnable()
    {
        DisableAttackHitbox();
        desiredDirection = Vector2.zero;
        thinkTimer = UnityEngine.Random.Range(0f, Mathf.Max(0.01f, aiTickInterval));
    }

    private void OnValidate()
    {
        RecalculateCachedThresholds();
    }

    public void ConfigureForSpawn(Transform playerTarget, float healthMul, float damageMul, float speedMul, Action<EnemyAI> returnToPool)
    {
        player = playerTarget;
        returnToPoolAction = returnToPool;

        isDead = false;
        attackTimer = 0f;
        desiredDirection = Vector2.zero;
        transform.localScale = baseScale;

        baseHealth = defaultBaseHealth * healthMul;
        attackDamage = defaultAttackDamage * damageMul;
        moveSpeed = defaultMoveSpeed * speedMul;
        currentHealth = baseHealth;

        if (animatorRef != null)
        {
            animatorRef.Rebind();
            if (gameObject.activeInHierarchy)
            {
                animatorRef.Update(0f);
            }
            animatorRef.SetBool(IsDeadParam, false);
            lastAnimX = 0f;
            lastAnimY = 0f;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        EnableAllColliders();
        DisableAttackHitbox();

    }

    public void SetExpDropSpawner(Action<GameObject, Vector3> expDropSpawner)
    {
        spawnExpDropAction = expDropSpawner;
    }

    private void FixedUpdate()
    {
        if (isDead || player == null)
        {
            return;
        }

        if (attackTimer > 0f)
        {
            attackTimer -= Time.fixedDeltaTime;
        }

        thinkTimer -= Time.fixedDeltaTime;
        if (thinkTimer <= 0f)
        {
            Vector2 toPlayer = player.position - transform.position;
            float sqrDistanceToPlayer = toPlayer.sqrMagnitude;

            desiredDirection = sqrDistanceToPlayer > attackRangeSqr ? toPlayer.normalized : Vector2.zero;

            float nextTick = sqrDistanceToPlayer > farDistanceSqr ? farAiTickInterval : aiTickInterval;
            thinkTimer = Mathf.Max(0.01f, nextTick);
        }

        if (desiredDirection == Vector2.zero)
        {
            TryAttack();
        }
        else
        {
            MoveTowardsPlayer(desiredDirection);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damageAmount;

        SpawnHitParticle();

        if (currentHealth <= 0f)
        {
            HandleDeath();
        }
        else if (animatorRef != null)
        {
            animatorRef.SetTrigger(TakeDamageTrigger);
        }
    }

    private void SpawnHitParticle()
    {
        if (hitParticlePrefab == null)
        {
            return;
        }

        ParticleSystem vfx = RentHitParticle(out int poolKey);
        if (vfx == null)
        {
            return;
        }

        vfx.transform.SetPositionAndRotation(transform.position + hitParticleOffset, Quaternion.identity);
        vfx.gameObject.SetActive(true);
        vfx.Play(true);

        StartCoroutine(ReturnHitParticleAfter(vfx, poolKey, Mathf.Max(0.1f, hitParticleLife)));
    }

    private ParticleSystem RentHitParticle(out int poolKey)
    {
        poolKey = hitParticlePrefab.GetInstanceID();

        if (!HitParticlePools.TryGetValue(poolKey, out Queue<ParticleSystem> pool))
        {
            pool = new Queue<ParticleSystem>();
            HitParticlePools[poolKey] = pool;
        }

        while (pool.Count > 0)
        {
            ParticleSystem pooled = pool.Dequeue();
            if (pooled != null)
            {
                return pooled;
            }
        }

        ParticleSystem created = Instantiate(hitParticlePrefab);
        created.gameObject.SetActive(false);
        return created;
    }

    private System.Collections.IEnumerator ReturnHitParticleAfter(ParticleSystem particle, int poolKey, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (particle == null)
        {
            yield break;
        }

        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particle.gameObject.SetActive(false);

        if (!HitParticlePools.TryGetValue(poolKey, out Queue<ParticleSystem> pool))
        {
            pool = new Queue<ParticleSystem>();
            HitParticlePools[poolKey] = pool;
        }

        pool.Enqueue(particle);
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void EnableAttackHitbox()
    {
        if (attackHitbox == null)
        {
            return;
        }

        EnemyHitbox hitboxScript = attackHitbox.GetComponent<EnemyHitbox>();
        if (hitboxScript != null)
        {
            hitboxScript.damage = attackDamage;
        }

        attackHitbox.SetActive(true);
    }

    public void DisableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    private void MoveTowardsPlayer(Vector2 direction)
    {
        if (rb != null)
        {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
        }

        if (useSpriteFlipping)
        {
            UpdateSpriteFlip(direction.x);
            return;
        }

        UpdateMoveAnimation(direction.x, direction.y);
    }

    private void UpdateMoveAnimation(float x, float y)
    {
        if (animatorRef == null)
        {
            return;
        }

        if (Mathf.Abs(lastAnimX - x) > 0.01f)
        {
            animatorRef.SetFloat(MoveXParam, x);
            lastAnimX = x;
        }

        if (Mathf.Abs(lastAnimY - y) > 0.01f)
        {
            animatorRef.SetFloat(MoveYParam, y);
            lastAnimY = y;
        }
    }

    private void UpdateSpriteFlip(float directionX)
    {
        float absScaleX = Mathf.Abs(baseScale.x);
        float scaleY = baseScale.y;
        float scaleZ = baseScale.z;

        if (directionX > FlipThreshold)
        {
            transform.localScale = new Vector3(-absScaleX, scaleY, scaleZ);
        }
        else if (directionX < -FlipThreshold)
        {
            transform.localScale = new Vector3(absScaleX, scaleY, scaleZ);
        }
    }

    private void TryAttack()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (attackTimer > 0f)
        {
            return;
        }

        if (animatorRef != null)
        {
            animatorRef.SetTrigger(AttackTrigger);
        }

        attackTimer = attackCooldown;
    }

    private void HandleDeath()
    {
        isDead = true;

        if (animatorRef != null)
        {
            animatorRef.SetBool(IsDeadParam, true);
            animatorRef.SetTrigger(DieTrigger);
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        DisableAttackHitbox();
        DisableAllColliders();
        SpawnExpDrops();

        StartCoroutine(ReturnToPoolAfterDelay());
    }

    private System.Collections.IEnumerator ReturnToPoolAfterDelay()
    {
        yield return new WaitForSeconds(DestroyDelay);

        if (returnToPoolAction != null)
        {
            returnToPoolAction.Invoke(this);
            yield break;
        }

        Destroy(gameObject);
    }

    private void DisableAllColliders()
    {
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }
    }

    private void EnableAllColliders()
    {
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = true;
        }
    }

    private void SpawnExpDrops()
    {
        if (expGem == null)
        {
            return;
        }

        for (int i = 0; i < expDropCount; i++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * DropScatterRadius;
            Vector3 dropPos = (Vector2)transform.position + offset;

            if (spawnExpDropAction != null)
            {
                spawnExpDropAction.Invoke(expGem, dropPos);
            }
            else
            {
                Instantiate(expGem, dropPos, Quaternion.identity);
            }
        }
    }

    private void RecalculateCachedThresholds()
    {
        attackRangeSqr = attackRange * attackRange;
        farDistanceSqr = farDistance * farDistance;
    }
}
