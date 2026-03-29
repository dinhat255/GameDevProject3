using UnityEngine;
using System;

public class PlayerCombat : MonoBehaviour
{
    private const string ExperienceTag = "Experience";

    [Header("EXP & Level")]
    public int currentLevel = 1;
    public int expToNextLevel = 100;

    private int currentEXP = 0;
    public int totalEXP = 0;

    [Header("Player Stat Scaling")]
    public float healPerLevel = 10f;
    public float damageMultiplierPerLevel = 1.03f;
    public float speedMultiplierPerLevel = 1.02f;

    [Header("Base Combat Stats")]
    public float baseDamage = 10f;

    public float CurrentDamage { get; private set; }
    public int CurrentEXP => currentEXP;
    public int ExpToNextLevel => expToNextLevel;

    private PlayerHealth playerHealth;
    private MainPlayerController playerController;

    public event Action StatsChanged;

    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerController = GetComponent<MainPlayerController>();

        CurrentDamage = baseDamage;
        StatsChanged?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(ExperienceTag)) return;

        ExpGemPickup pickup = other.GetComponent<ExpGemPickup>();
        int amount = pickup != null ? pickup.ExpValue : 1;

        GainExp(amount);

        if (pickup != null)
        {
            pickup.Collect();
        }
        else
        {
            Destroy(other.gameObject);
        }
    }

    private void GainExp(int amount)
    {
        currentEXP += amount;
        totalEXP += amount;

        while (currentEXP >= expToNextLevel)
        {
            currentEXP -= expToNextLevel;
            LevelUp();
        }
        StatsChanged?.Invoke();
    }

    private void LevelUp()
    {
        currentLevel++;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLevelUpSfx();
        }

        if (playerHealth != null)
        {
            playerHealth.Heal(healPerLevel);
        }

        CurrentDamage *= damageMultiplierPerLevel;

        if (playerController != null)
        {
            playerController.AddMoveSpeedMultiplier(speedMultiplierPerLevel);
        }

        StatsChanged?.Invoke();
    }

}
