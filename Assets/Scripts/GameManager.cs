using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public PlayerCombat playerCombat;
    public EnemyManager enemyManager;

    [Header("UI - HUD")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI pointsText;

    [Header("UI - GameOver")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI finalPointsText;

    private float playTime = 0f;
    private bool isGameOver = false;

    private void Start()
    {
        if (playerCombat == null)
            playerCombat = FindFirstObjectByType<PlayerCombat>();

        if (enemyManager == null)
            enemyManager = FindFirstObjectByType<EnemyManager>();
    }

    private void Update()
    {
        if (isGameOver) return;

        UpdateTimerUI();
        UpdateWaveUI();
        UpdatePointsUI();
    }

    private void UpdateTimerUI()
    {
        playTime += Time.deltaTime;

        int minutes = (int)(playTime / 60);
        int seconds = (int)(playTime % 60);
        int fraction = (int)((playTime - (int)playTime) * 100);

        timerText.text = $"{minutes:00}:{seconds:00}:{fraction:00}";
    }

    private void UpdateWaveUI()
    {
        if (enemyManager != null)
            waveText.text = "WAVE " + enemyManager.currentWave;
    }

    private void UpdatePointsUI()
    {
        if (playerCombat == null) return;

        pointsText.text = "Points: " + playerCombat.totalEXP;
    }

    public void ShowGameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        finalPointsText.text = "Points: " + playerCombat.totalEXP;

        int minutes = (int)(playTime / 60);
        int seconds = (int)(playTime % 60);
        int fraction = (int)((playTime - (int)playTime) * 100);

        finalTimeText.text = $"{minutes:00}:{seconds:00}:{fraction:00}";

        gameOverPanel.SetActive(true);
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");
    }
}
