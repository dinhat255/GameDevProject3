using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const string MenuSceneName = "MenuScene";
    private const int TargetFps = 60;

    [Header("References")]
    public PlayerCombat playerCombat;
    public PlayerHealth playerHealth;
    public EnemyManager enemyManager;

    [Header("UI - HUD")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI pointsText;

    [Header("UI - Wave Transition")]
    public float waveMoveOutDuration = 0.25f;
    public float waveCountDuration = 0.25f;
    public float waveMoveBackDuration = 0.25f;
    public float waveScaleMultiplier = 1.8f;

    [Header("UI - GameOver")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI finalPointsText;

    private float playTime = 0f;
    private bool isGameOver = false;
    private bool eventsBound = false;
    private RectTransform waveRect;
    private RectTransform waveParentRect;
    private Vector2 waveStartAnchoredPos;
    private Vector3 waveStartScale = Vector3.one;
    private Coroutine waveTransitionRoutine;
    private int displayedWave = 1;

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFps;

        ResolveReferences();
        BindEvents();

        if (waveText != null)
        {
            waveRect = waveText.rectTransform;
            waveParentRect = waveRect.parent as RectTransform;
            waveStartAnchoredPos = waveRect.anchoredPosition;
            waveStartScale = waveRect.localScale;
        }

        displayedWave = enemyManager != null ? enemyManager.currentWave : 1;

        RefreshWaveHUD();
        RefreshPointsHUD();
    }

    private void Update()
    {
        if (isGameOver)
        {
            return;
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        playTime += Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = FormatTime(playTime);
        }
    }

    private void RefreshWaveHUD()
    {
        if (waveText != null)
        {
            waveText.text = "WAVE " + displayedWave;
        }
    }

    private void RefreshWaveHUD(int wave)
    {
        if (waveText == null)
        {
            displayedWave = wave;
            return;
        }

        if (waveTransitionRoutine != null)
        {
            StopCoroutine(waveTransitionRoutine);
            waveRect.anchoredPosition = waveStartAnchoredPos;
            waveRect.localScale = waveStartScale;
        }

        waveTransitionRoutine = StartCoroutine(WaveTransitionRoutine(displayedWave, wave));
    }

    private void RefreshPointsHUD()
    {
        if (playerCombat == null || pointsText == null)
        {
            return;
        }

        pointsText.text = "Points: " + playerCombat.totalEXP;
    }

    private void RefreshPointsHUDFromEvent()
    {
        RefreshPointsHUD();
    }

    public void ShowGameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        int finalScore = playerCombat != null ? playerCombat.totalEXP : 0;
        string formattedTime = FormatTime(playTime);

        if (finalPointsText != null)
        {
            finalPointsText.text = "Points: " + finalScore;
        }

        if (finalTimeText != null)
        {
            finalTimeText.text = formattedTime;
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveScore(finalScore, formattedTime);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(MenuSceneName);
    }

    private void OnDestroy()
    {
        UnbindEvents();
    }

    private void ResolveReferences()
    {
        if (playerCombat == null)
        {
            playerCombat = FindFirstObjectByType<PlayerCombat>();
        }

        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (enemyManager == null)
        {
            enemyManager = FindFirstObjectByType<EnemyManager>();
        }
    }

    private void BindEvents()
    {
        if (eventsBound)
        {
            return;
        }

        if (playerCombat != null)
        {
            playerCombat.StatsChanged += RefreshPointsHUDFromEvent;
        }

        if (playerHealth != null)
        {
            playerHealth.PlayerDied += ShowGameOver;
        }

        if (enemyManager != null)
        {
            enemyManager.WaveChanged += RefreshWaveHUD;
        }

        eventsBound = true;
    }

    private void UnbindEvents()
    {
        if (!eventsBound)
        {
            return;
        }

        if (playerCombat != null)
        {
            playerCombat.StatsChanged -= RefreshPointsHUDFromEvent;
        }

        if (playerHealth != null)
        {
            playerHealth.PlayerDied -= ShowGameOver;
        }

        if (enemyManager != null)
        {
            enemyManager.WaveChanged -= RefreshWaveHUD;
        }

        eventsBound = false;
    }

    private System.Collections.IEnumerator WaveTransitionRoutine(int fromWave, int toWave)
    {
        if (enemyManager != null)
        {
            enemyManager.SetSpawnPaused(true);
        }

        Vector2 centerAnchoredPos = Vector2.zero;
        if (waveParentRect != null)
        {
            Camera uiCam = null;
            Canvas canvas = waveText.canvas;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCam = canvas.worldCamera;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                waveParentRect,
                new Vector2(Screen.width * 0.5f, Screen.height * 0.5f),
                uiCam,
                out centerAnchoredPos);
        }

        float outDuration = Mathf.Max(0.01f, waveMoveOutDuration);
        float t = 0f;
        Vector3 targetScale = waveStartScale * Mathf.Max(1f, waveScaleMultiplier);

        while (t < outDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / outDuration);
            waveRect.anchoredPosition = Vector2.Lerp(waveStartAnchoredPos, centerAnchoredPos, k);
            waveRect.localScale = Vector3.Lerp(waveStartScale, targetScale, k);
            waveText.text = "WAVE " + fromWave;
            yield return null;
        }

        float countDuration = Mathf.Max(0.01f, waveCountDuration);
        t = 0f;
        while (t < countDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / countDuration);
            int current = Mathf.RoundToInt(Mathf.Lerp(fromWave, toWave, k));
            waveText.text = "WAVE " + current;
            waveRect.anchoredPosition = centerAnchoredPos;
            waveRect.localScale = targetScale;
            yield return null;
        }

        float backDuration = Mathf.Max(0.01f, waveMoveBackDuration);
        t = 0f;
        while (t < backDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / backDuration);
            waveRect.anchoredPosition = Vector2.Lerp(centerAnchoredPos, waveStartAnchoredPos, k);
            waveRect.localScale = Vector3.Lerp(targetScale, waveStartScale, k);
            waveText.text = "WAVE " + toWave;
            yield return null;
        }

        displayedWave = toWave;
        waveRect.anchoredPosition = waveStartAnchoredPos;
        waveRect.localScale = waveStartScale;
        waveText.text = "WAVE " + displayedWave;
        waveTransitionRoutine = null;

        if (enemyManager != null)
        {
            enemyManager.SetSpawnPaused(false);
        }
    }

    private static string FormatTime(float timeValue)
    {
        int minutes = (int)(timeValue / 60);
        int seconds = (int)(timeValue % 60);
        int fraction = (int)((timeValue - (int)timeValue) * 100);

        return $"{minutes:00}:{seconds:00}:{fraction:00}";
    }
}
