using TMPro;
using UnityEngine;

public class FPSCounterUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI fpsText;

    [Header("Display")]
    [SerializeField] private string prefix = "FPS: ";
    [SerializeField] private float refreshInterval = 0.5f;

    private float elapsed;
    private int frameCount;

    private void Awake()
    {
        if (fpsText == null)
        {
            fpsText = GetComponent<TextMeshProUGUI>();
        }

        refreshInterval = Mathf.Max(0.05f, refreshInterval);
    }

    private void Update()
    {
        float delta = Time.unscaledDeltaTime;
        elapsed += delta;
        frameCount++;

        if (elapsed < refreshInterval)
        {
            return;
        }

        float avgDelta = elapsed / frameCount;
        int fps = avgDelta > 0f ? Mathf.RoundToInt(1f / avgDelta) : 0;

        if (fpsText != null)
        {
            fpsText.text = prefix + fps;
        }

        elapsed = 0f;
        frameCount = 0;
    }
}
