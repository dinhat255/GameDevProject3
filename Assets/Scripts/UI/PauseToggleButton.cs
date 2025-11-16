using UnityEngine;
using UnityEngine.UI;

public class PauseToggleButton : MonoBehaviour
{
    public Sprite playSprite;
    public Sprite pauseSprite;

    private Image buttonImage;
    private bool isPaused = false;

    private void Start()
    {
        buttonImage = GetComponent<Image>();
        buttonImage.sprite = pauseSprite;
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            buttonImage.sprite = pauseSprite;
            SetAnimators(true);
            isPaused = false;
        }
        else
        {
            Time.timeScale = 0f;
            buttonImage.sprite = playSprite;
            SetAnimators(false);
            isPaused = true;
        }
    }

    void SetAnimators(bool state)
    {
        Animator[] animators = FindObjectsByType<Animator>(FindObjectsSortMode.None);

        foreach (Animator anim in animators)
        {
            anim.enabled = state;
        }
    }
}
