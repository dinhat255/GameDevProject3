using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject tutorialPanel;

    public void ToggleTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(!tutorialPanel.activeSelf);
        }
    }

    public void ShowTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
    }

    public void HideTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }
}
