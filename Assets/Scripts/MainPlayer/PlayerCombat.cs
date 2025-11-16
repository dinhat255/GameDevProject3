using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCombat : MonoBehaviour
{
    [Header("EXP & Level")]
    public int currentLevel = 1;
    public int expToNextLevel = 100;

    private int currentEXP = 0;
    public int totalEXP = 0;
    [Header("UI")]
    public Slider expSlider;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText;

    [Header("Player Stat Scaling")]
    public float healPerLevel = 10f;
    public float damageMultiplierPerLevel = 1.03f;
    public float speedMultiplierPerLevel = 1.02f;

    [Header("Base Combat Stats")]
    public float baseDamage = 10f;

    public float CurrentDamage { get; private set; }

    private PlayerHealth playerHealth;
    private MainPlayerController playerController;

    [Header("Combat UI")]
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI speedText;

    private void Start()
    {
        if (expSlider != null)
        {
            expSlider.minValue = 0;
            expSlider.maxValue = expToNextLevel;
            expSlider.value = currentEXP;
        }

        playerHealth = GetComponent<PlayerHealth>();
        playerController = GetComponent<MainPlayerController>();

        CurrentDamage = baseDamage;

        UpdateLevelText();
        UpdateExpText();
        UpdateCombatUI();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Experience"))
        {
            GainExp(1);
            Destroy(other.gameObject);
        }
    }

    void GainExp(int amount)
    {
        currentEXP += amount;
        totalEXP += amount;
        while (currentEXP >= expToNextLevel)
        {
            currentEXP -= expToNextLevel;
            LevelUp();
        }

        UpdateExpUI();
        UpdateExpText();

        Debug.Log($"EXP: {currentEXP}/{expToNextLevel} | Level: {currentLevel}");
    }

    void LevelUp()
    {
        currentLevel++;
        UpdateLevelText();

        if (playerHealth != null)
        {
            playerHealth.Heal(healPerLevel);
        }

        CurrentDamage *= damageMultiplierPerLevel;

        if (playerController != null)
        {
            playerController.AddMoveSpeedMultiplier(speedMultiplierPerLevel);
        }
        UpdateCombatUI();

        Debug.Log("LEVEL UP! Level hiện tại: " + currentLevel + " | Damage: " + CurrentDamage);
    }

    void UpdateExpUI()
    {
        if (expSlider != null)
        {
            expSlider.value = currentEXP;
        }
    }

    void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = "Lv. " + currentLevel;
        }
    }

    void UpdateExpText()
    {
        if (expText != null)
        {
            expText.text = currentEXP + " / " + expToNextLevel;
        }
    }

    void UpdateCombatUI()
    {
        if (damageText != null)
            damageText.text = "Damage: " + CurrentDamage.ToString("0.0");

        if (speedText != null && playerController != null)
            speedText.text = "Speed: " + playerController.CurrentMoveSpeed.ToString("0.0");
    }

}
