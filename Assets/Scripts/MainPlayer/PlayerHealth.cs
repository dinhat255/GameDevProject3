using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    [Header("Hit Effect")]
    public float invulnerableTime = 0.5f;
    public float flashInterval = 0.1f;
    public Color flashColor = Color.red;

    [Header("Death Effect")]
    public float deathFlashDuration = 0.2f;
    public float deathFadeDuration = 0.35f;
    public Color deathFlashColor = new Color(1f, 0.3f, 0.3f, 1f);
    public ParticleSystem deathParticlePrefab;
    public Vector3 deathParticleOffset = Vector3.zero;
    public float deathParticleLife = 2f;

    private bool isInvulnerable = false;
    private bool isDying = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public event Action PlayerDied;

    private void Start()
    {
        currentHealth = maxHealth;
        SetupHealthUI();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        UpdateHealthText();
    }

    public void TakeDamage(float damageAmount)
    {
        if (isInvulnerable || isDying)
        {
            return;
        }

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        UpdateHealthText();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerHurtSfx();
        }

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(HitFlashRoutine());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDying)
        {
            return;
        }

        isDying = true;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetPlayerMoveSfxActive(false);
            AudioManager.Instance.PlayGameOverSfx();
        }

        StartCoroutine(DeathRoutine());
    }

    private System.Collections.IEnumerator DeathRoutine()
    {
        float deathStartTime = Time.unscaledTime;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        SpawnDeathParticle();

        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = false;
        }

        float elapsedSinceDeathStart = Time.unscaledTime - deathStartTime;
        float remainingDelay = deathParticleLife - elapsedSinceDeathStart;
        if (remainingDelay > 0f)
        {
            yield return WaitUnscaled(remainingDelay);
        }

        PlayerDied?.Invoke();
        gameObject.SetActive(false);
    }

    private void SpawnDeathParticle()
    {
        if (deathParticlePrefab == null)
        {
            return;
        }

        ParticleSystem vfx = Instantiate(
            deathParticlePrefab,
            transform.position + deathParticleOffset,
            Quaternion.identity);

        Destroy(vfx.gameObject, Mathf.Max(0.1f, deathParticleLife));
    }

    private static System.Collections.IEnumerator WaitUnscaled(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private System.Collections.IEnumerator HitFlashRoutine()
    {
        isInvulnerable = true;

        float timer = 0f;
        bool flip = false;

        while (timer < invulnerableTime)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = flip ? flashColor : originalColor;
            }

            flip = !flip;
            timer += flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        isInvulnerable = false;
    }

    private void SetupHealthUI()
    {
        if (healthSlider == null)
        {
            return;
        }

        healthSlider.minValue = 0f;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
        healthSlider.interactable = false;
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            int cur = Mathf.RoundToInt(currentHealth);
            int max = Mathf.RoundToInt(maxHealth);
            healthText.text = cur + " / " + max;
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        UpdateHealthText();
    }

}
