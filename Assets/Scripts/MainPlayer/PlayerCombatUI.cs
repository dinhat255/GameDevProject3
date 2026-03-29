using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCombatUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private MainPlayerController playerController;

    [Header("UI")]
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI speedText;

    private void Awake()
    {
        if (playerCombat == null)
        {
            playerCombat = GetComponentInParent<PlayerCombat>();
        }

        if (playerController == null)
        {
            playerController = GetComponentInParent<MainPlayerController>();
        }
    }

    private void OnEnable()
    {
        if (playerCombat != null)
        {
            playerCombat.StatsChanged += RefreshUI;
        }
    }

    private void Start()
    {
        InitializeExpUI();
        RefreshUI();
    }

    private void OnDisable()
    {
        if (playerCombat != null)
        {
            playerCombat.StatsChanged -= RefreshUI;
        }
    }

    private void InitializeExpUI()
    {
        if (expSlider == null || playerCombat == null)
        {
            return;
        }

        expSlider.minValue = 0;
        expSlider.maxValue = playerCombat.ExpToNextLevel;
    }

    private void RefreshUI()
    {
        if (playerCombat == null)
        {
            return;
        }

        if (expSlider != null)
        {
            expSlider.value = playerCombat.CurrentEXP;
        }

        if (levelText != null)
        {
            levelText.text = "Lv. " + playerCombat.currentLevel;
        }

        if (expText != null)
        {
            expText.text = playerCombat.CurrentEXP + " / " + playerCombat.ExpToNextLevel;
        }

        if (damageText != null)
        {
            damageText.text = "Damage: " + playerCombat.CurrentDamage.ToString("0.0");
        }

        if (speedText != null && playerController != null)
        {
            speedText.text = "Speed: " + playerController.CurrentMoveSpeed.ToString("0.0");
        }
    }
}
