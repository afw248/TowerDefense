using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class WaveSkipButtonUi : MonoBehaviour
{
    private const float PanelHeightWithSkip = 112f;
    private const float TimerTextY = 34f;
    private const float SkipButtonY = 4f;

    [SerializeField] private Button skipButton;
    [SerializeField] private TextMeshProUGUI labelText;

    private WaveManager _waveManager;

    private void Awake()
    {
        EnsureButton();
    }

    private void OnEnable()
    {
        RefreshDisplay();
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        EnsureButton();

        if (skipButton == null)
            return;

        if (!Application.isPlaying)
        {
            skipButton.gameObject.SetActive(false);
            return;
        }

        if (_waveManager == null)
            _waveManager = FindFirstObjectByType<WaveManager>();

        bool canSkip = !GameSessionMode.IsTutorial
            && _waveManager != null
            && _waveManager.CanSkipWave;
        skipButton.gameObject.SetActive(canSkip);
        skipButton.interactable = canSkip;
    }

    private void EnsureButton()
    {
        if (skipButton != null)
            return;

        Transform existing = transform.Find("WaveSkipButton");
        if (existing != null)
        {
            skipButton = existing.GetComponent<Button>();
            labelText = existing.GetComponentInChildren<TextMeshProUGUI>(true);

            Image existingImage = existing.GetComponent<Image>();
            if (existingImage != null)
            {
                existingImage.sprite = TowerInfoUiHelpers.GetUiSprite();
                existingImage.type = Image.Type.Sliced;
            }

            WireButton();
            return;
        }

        skipButton = CreateSkipButton(transform);
        labelText = skipButton.GetComponentInChildren<TextMeshProUGUI>(true);
        WireButton();
    }

    private void WireButton()
    {
        if (skipButton == null)
            return;

        skipButton.onClick.RemoveListener(HandleSkipClicked);
        skipButton.onClick.AddListener(HandleSkipClicked);
    }

    private void HandleSkipClicked()
    {
        if (_waveManager == null)
            _waveManager = FindFirstObjectByType<WaveManager>();

        _waveManager?.TrySkipWave();
        RefreshDisplay();
    }

    public static WaveSkipButtonUi EnsureUnderWaveTimer()
    {
        Transform panel = FindFieldEnemyCountPanel();
        if (panel == null)
            return null;

        ExpandPanelLayout(panel);
        RepositionWaveTimer(panel);

        WaveSkipButtonUi ui = panel.GetComponent<WaveSkipButtonUi>();
        if (ui == null)
            ui = panel.gameObject.AddComponent<WaveSkipButtonUi>();

        ui.EnsureButton();
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

        if (panelRect.sizeDelta.y < PanelHeightWithSkip)
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, PanelHeightWithSkip);
    }

    private static void RepositionWaveTimer(Transform panel)
    {
        Transform timerTransform = panel.Find("WaveTimerText");
        if (timerTransform is not RectTransform timerRect)
            return;

        Vector2 pos = timerRect.anchoredPosition;
        pos.y = TimerTextY;
        timerRect.anchoredPosition = pos;
    }

    private static Button CreateSkipButton(Transform parent)
    {
        GameObject buttonGo = new GameObject("WaveSkipButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonGo.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, SkipButtonY);
        buttonRect.sizeDelta = new Vector2(132f, 24f);

        Image buttonImage = buttonGo.GetComponent<Image>();
        buttonImage.sprite = TowerInfoUiHelpers.GetUiSprite();
        buttonImage.type = Image.Type.Sliced;
        buttonImage.color = new Color(0.2f, 0.38f, 0.58f, 0.95f);

        Button button = buttonGo.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.96f, 1f, 1f);
        colors.pressedColor = new Color(0.75f, 0.86f, 0.98f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(buttonGo.transform, false);

        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelGo.GetComponent<TextMeshProUGUI>();
        label.text = "웨이브 스킵";
        label.fontSize = 14f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;

        buttonGo.SetActive(false);
        return button;
    }
}
