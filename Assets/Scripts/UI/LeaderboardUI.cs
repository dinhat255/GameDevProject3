using UnityEngine;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject leaderboardPanel;

    [Header("Texts")]
    public TextMeshProUGUI[] scoreLines;

    public void ToggleLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            bool willBeActive = !leaderboardPanel.activeSelf;
            leaderboardPanel.SetActive(willBeActive);

            if (willBeActive)
            {
                RefreshUI();
            }
        }
    }

    public void ShowLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
            RefreshUI();
        }
    }

    public void HideLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }

    private void RefreshUI()
    {
        if (SaveManager.Instance == null || scoreLines == null || scoreLines.Length == 0)
        {
            return;
        }
        var scores = SaveManager.Instance.GetHighScores();
        for (int i = 0; i < scoreLines.Length; i++)
        {
            if (i < scores.Count)
            {
                scoreLines[i].text = $"{i + 1}. {scores[i].score} pts   {scores[i].time}";
            }
            else
            {
                scoreLines[i].text = $"{i + 1}. ---";
            }
        }
    }
}
