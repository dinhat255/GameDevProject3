using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;

        // Setup slider
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            healthSlider.interactable = false;
        }

        UpdateHealthText();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(float damageAmount)
    {
        if (isInvulnerable) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Player Health: " + currentHealth + "/" + maxHealth);

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        UpdateHealthText();

        if (gameObject.activeInHierarchy)
            StartCoroutine(HitFlashRoutine());

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.LogError("PLAYER ĐÃ CHẾT!");
        gameObject.SetActive(false);
        FindAnyObjectByType<GameManager>().ShowGameOver();
    }

    private System.Collections.IEnumerator HitFlashRoutine()
    {
        isInvulnerable = true;

        float timer = 0f;
        bool flip = false;

        while (timer < invulnerableTime)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = flip ? flashColor : originalColor;

            flip = !flip;
            timer += flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        isInvulnerable = false;
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
