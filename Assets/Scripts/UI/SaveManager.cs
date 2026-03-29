using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

[Serializable]
public class ScoreEntry
{
    public int score;
    public string time;
}

public class ScoreList
{
    public List<ScoreEntry> entries = new List<ScoreEntry>();
}

public class SaveManager : MonoBehaviour
{
    private const string HighScoresKey = "HighScores";
    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        transform.SetParent(null);
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveScore(int score, string time)
    {
        var list = GetScoreList();
        list.entries.Add(new ScoreEntry { score = score, time = time });
        list.entries.Sort((a, b) => b.score.CompareTo(a.score));
        if (list.entries.Count > 5) list.entries.RemoveRange(5, list.entries.Count - 5);
        string json = JsonUtility.ToJson(list);
        PlayerPrefs.SetString(HighScoresKey, json);
        PlayerPrefs.Save();
    }

    public List<ScoreEntry> GetHighScores()
    {
        return GetScoreList().entries;
    }

    private ScoreList GetScoreList()
    {
        string json = PlayerPrefs.GetString(HighScoresKey, "");
        if (!string.IsNullOrEmpty(json))
        {
            return JsonUtility.FromJson<ScoreList>(json);
        }
        return new ScoreList();
    }
}
