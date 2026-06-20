using TMPro;
using UnityEngine;

[ExecuteAlways]
public class WaveTimerUi : MonoBehaviour
{
    private const string DefaultPreviewText = "다음 웨이브 --:--";

    [SerializeField] private TextMeshProUGUI timerText;

    private WaveManager _waveManager;

    private void Awake()
    {
        timerText ??= transform.Find("WaveTimerText")?.GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        RefreshDisplay();
    }

    private void Update()
    {
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        timerText ??= transform.Find("WaveTimerText")?.GetComponent<TextMeshProUGUI>();
        if (timerText == null)
            return;

        if (!Application.isPlaying)
        {
            timerText.text = DefaultPreviewText;
            return;
        }

        if (_waveManager == null)
            _waveManager = FindFirstObjectByType<WaveManager>();

        if (_waveManager == null)
        {
            timerText.text = DefaultPreviewText;
            return;
        }

        timerText.text = FormatRemainingTime(
            _waveManager.RemainingWaveTime,
            _waveManager.IsBossWaveActive,
            _waveManager.IsPreFirstWaveDelay);
    }

    public static WaveTimerUi EnsureUnderFieldEnemyCount()
    {
        Transform panel = FindFieldEnemyCountPanel();
        if (panel == null)
            return null;

        ExpandPanelLayout(panel);

        WaveTimerUi ui = panel.GetComponent<WaveTimerUi>();
        if (ui == null)
            ui = panel.gameObject.AddComponent<WaveTimerUi>();

        TextMeshProUGUI label = panel.Find("WaveTimerText")?.GetComponent<TextMeshProUGUI>();
        if (label == null)
            label = CreateTimerLabel(panel);

        ui.timerText = label;
        ui.RefreshDisplay();
        return ui;
    }

    private static Transform FindFieldEnemyCountPanel()
    {
        FieldEnemyCountUi fieldEnemyCount = FindFirstObjectByType<FieldEnemyCountUi>(FindObjectsInactive.Include);
        if (fieldEnemyCount != null)
            return fieldEnemyCount.transform;

        GameObject waveCanvas = GameObject.Find("WaveUiCanvas");
        return waveCanvas != null ? waveCanvas.transform.Find("FieldEnemyCount") : null;
    }

    private static void ExpandPanelLayout(Transform panel)
    {
        if (panel is not RectTransform panelRect)
            return;

        panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, 112f);

        ShiftChildRect(panel.Find("MobIcon"), 10f);
        ShiftChildRect(panel.Find("MobBarBg"), 10f);
        ShiftChildRect(panel.Find("MobCountText"), 10f);
    }

    private static void ShiftChildRect(Transform child, float anchoredY)
    {
        if (child is not RectTransform rect)
            return;

        Vector2 pos = rect.anchoredPosition;
        pos.y = anchoredY;
        rect.anchoredPosition = pos;
    }

    private static TextMeshProUGUI CreateTimerLabel(Transform panel)
    {
        GameObject labelGo = new GameObject("WaveTimerText", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(panel, false);

        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.anchoredPosition = new Vector2(0f, 34f);
        labelRect.sizeDelta = new Vector2(-24f, 24f);

        TextMeshProUGUI label = labelGo.GetComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 20f;
        label.fontStyle = FontStyles.Bold;
        label.color = new Color(0.82f, 0.86f, 0.92f, 1f);
        label.text = DefaultPreviewText;
        return label;
    }

    public static string FormatRemainingTime(float remainingSeconds, bool isBossWave = false, bool isPreFirstWaveDelay = false)
    {
        int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(remainingSeconds));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        string prefix = isPreFirstWaveDelay
            ? "첫 웨이브"
            : isBossWave ? "보스 웨이브" : "다음 웨이브";
        return $"{prefix} {minutes:00}:{seconds:00}";
    }
}
