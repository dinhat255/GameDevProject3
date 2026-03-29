using UnityEngine;
using UnityEngine.UI;

public class PauseToggleButton : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite playSprite;
    public Sprite pauseSprite;

    [Header("UI Panels")]
    public GameObject pausePanel;

    private Image buttonImage;
    private bool isPaused;

    private void Awake()
    {
        buttonImage = GetComponent<Image>();
    }

    private void Start()
    {
        SyncStateFromTimeScale();
    }

    public void TogglePause()
    {
        SetPausedState(!isPaused);
    }

    private void SetPausedState(bool paused)
    {
        isPaused = paused;
        Time.timeScale = isPaused ? 0f : 1f;

        UpdateButtonSprite();
        SetAnimators(!isPaused);

        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }
    }

    private void SyncStateFromTimeScale()
    {
        isPaused = Time.timeScale == 0f;
        UpdateButtonSprite();
    }

    private void UpdateButtonSprite()
    {
        if (buttonImage == null)
        {
            return;
        }

        buttonImage.sprite = isPaused ? playSprite : pauseSprite;
    }

    private void SetAnimators(bool state)
    {
        Animator[] animators = FindObjectsByType<Animator>(FindObjectsSortMode.None);

        foreach (Animator anim in animators)
        {
            anim.enabled = state;
        }
    }
}
