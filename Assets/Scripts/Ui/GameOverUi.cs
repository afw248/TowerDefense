using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUi : MonoBehaviour
{
    public static GameOverUi Instance { get; private set; }

    public enum EndScreenMode
    {
        Defeat,
        Victory,
        TutorialComplete,
    }

    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button titleButton;

    private EndScreenMode _currentMode = EndScreenMode.Defeat;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        panelRoot ??= gameObject;
        ResolveReferences();
        EnsureActionButtons();

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void ResolveReferences()
    {
        if (titleText == null)
        {
            Transform titleTransform = transform.Find("GameOverTitle");
            if (titleTransform != null)
                titleText = titleTransform.GetComponent<TextMeshProUGUI>();
        }

        ResolveMessageText();
    }

    private void ResolveMessageText()
    {
        if (messageText != null)
            return;

        Transform messageTransform = transform.Find("GameOverMessage");
        if (messageTransform != null)
            messageText = messageTransform.GetComponent<TextMeshProUGUI>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Initialize(
        GameObject root,
        TextMeshProUGUI title,
        TextMeshProUGUI message,
        Button restart,
        Button titleReturnButton = null)
    {
        panelRoot = root;
        titleText = title;
        messageText = message;
        restartButton = restart;
        titleButton = titleReturnButton;
        EnsureActionButtons();

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Show(string reason)
    {
        ShowDefeat(reason);
    }

    public void ShowDefeat(string reason)
    {
        _currentMode = EndScreenMode.Defeat;
        ResolveReferences();
        EnsureActionButtons();

        if (titleText != null)
        {
            titleText.text = "패배";
            titleText.color = Color.white;
        }

        if (messageText != null)
            messageText.text = reason;

        ConfigureButtons();
        ShowPanel();
        GameAudioManager.Instance?.PlaySfx(GameAudioId.Defeat);
    }

    public void ShowVictory(string message)
    {
        _currentMode = EndScreenMode.Victory;
        ResolveReferences();
        EnsureActionButtons();

        if (titleText != null)
        {
            titleText.text = "클리어";
            titleText.color = new Color(1f, 0.88f, 0.2f, 1f);
        }

        if (messageText != null)
            messageText.text = message;

        ConfigureButtons();
        ShowPanel();
        GameAudioManager.Instance?.PlaySfx(GameAudioId.Victory);
    }

    public void ShowTutorialComplete(string message)
    {
        _currentMode = EndScreenMode.TutorialComplete;
        ResolveReferences();
        EnsureActionButtons();

        if (titleText != null)
        {
            titleText.text = "튜토리얼 완료";
            titleText.color = new Color(0.45f, 0.95f, 1f, 1f);
        }

        if (messageText != null)
            messageText.text = message;

        ConfigureButtons();
        ShowPanel();
        GameAudioManager.Instance?.PlaySfx(GameAudioId.Victory);
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void ConfigureButtons()
    {
        bool showRestart = _currentMode != EndScreenMode.TutorialComplete;
        bool showTitle = true;

        if (restartButton != null)
            restartButton.gameObject.SetActive(showRestart);

        if (titleButton != null)
            titleButton.gameObject.SetActive(showTitle);

        if (showRestart && showTitle)
        {
            SetButtonPosition(restartButton, new Vector2(-120f, -90f));
            SetButtonPosition(titleButton, new Vector2(120f, -90f));
        }
        else if (showRestart)
        {
            SetButtonPosition(restartButton, new Vector2(0f, -90f));
        }
        else if (showTitle)
        {
            SetButtonPosition(titleButton, new Vector2(0f, -90f));
        }
    }

    private static void SetButtonPosition(Button button, Vector2 anchoredPosition)
    {
        if (button == null)
            return;

        RectTransform rect = button.transform as RectTransform;
        if (rect != null)
            rect.anchoredPosition = anchoredPosition;
    }

    private void ShowPanel()
    {
        if (panelRoot != null)
        {
            panelRoot.transform.SetAsLastSibling();
            panelRoot.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    private void EnsureActionButtons()
    {
        if (restartButton == null)
        {
            Transform buttonTransform = transform.Find("RestartButton");
            if (buttonTransform != null)
                restartButton = buttonTransform.GetComponent<Button>();
        }

        if (titleButton == null)
        {
            Transform buttonTransform = transform.Find("TitleButton");
            if (buttonTransform != null)
                titleButton = buttonTransform.GetComponent<Button>();
        }

        if (restartButton == null)
            restartButton = CreateActionButton(transform, "RestartButton", "재시작", new Vector2(-120f, -90f));

        if (titleButton == null)
            titleButton = CreateActionButton(transform, "TitleButton", "타이틀로", new Vector2(120f, -90f));

        restartButton.onClick.RemoveListener(HandleRestartClicked);
        restartButton.onClick.AddListener(HandleRestartClicked);

        titleButton.onClick.RemoveListener(HandleTitleClicked);
        titleButton.onClick.AddListener(HandleTitleClicked);
    }

    private static void HandleRestartClicked()
    {
        GameSessionRestarter.RestartActiveScene();
    }

    private static void HandleTitleClicked()
    {
        GameSessionReturnToTitle.Return();
    }

    public static GameOverUi EnsureExists(Transform hudRoot)
    {
        if (Instance != null)
            return Instance;

        if (hudRoot == null)
            return null;

        Transform existing = hudRoot.Find("GameOverPanel");
        if (existing != null)
        {
            GameOverUi existingUi = existing.GetComponent<GameOverUi>();
            if (existingUi == null)
                existingUi = existing.gameObject.AddComponent<GameOverUi>();

            return existingUi;
        }

        GameObject panel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(Image), typeof(GameOverUi));
        panel.transform.SetParent(hudRoot, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image bg = panel.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.82f);
        bg.raycastTarget = true;

        TextMeshProUGUI title = CreateCenteredLabel(
            panel.transform,
            "GameOverTitle",
            "패배",
            48,
            FontStyles.Bold,
            new Vector2(0f, 40f),
            new Vector2(320f, 60f),
            Color.white);

        TextMeshProUGUI message = CreateCenteredLabel(
            panel.transform,
            "GameOverMessage",
            string.Empty,
            28,
            FontStyles.Normal,
            new Vector2(0f, -20f),
            new Vector2(600f, 40f),
            Color.white);

        Button restart = CreateActionButton(panel.transform, "RestartButton", "재시작", new Vector2(-120f, -90f));
        Button titleBtn = CreateActionButton(panel.transform, "TitleButton", "타이틀로", new Vector2(120f, -90f));

        GameOverUi ui = panel.GetComponent<GameOverUi>();
        ui.Initialize(panel, title, message, restart, titleBtn);
        panel.SetActive(false);
        return ui;
    }

    private static TextMeshProUGUI CreateCenteredLabel(
        Transform parent,
        string name,
        string text,
        float fontSize,
        FontStyles fontStyle,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Color color)
    {
        GameObject labelGo = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(parent, false);

        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = anchoredPosition;
        labelRect.sizeDelta = sizeDelta;

        TextMeshProUGUI label = labelGo.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;

        return label;
    }

    private static Button CreateActionButton(Transform parent, string name, string label, Vector2 anchoredPosition)
    {
        GameObject buttonGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonGo.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = anchoredPosition;
        buttonRect.sizeDelta = new Vector2(220f, 52f);

        Image buttonImage = buttonGo.GetComponent<Image>();
        buttonImage.color = name == "TitleButton"
            ? new Color(0.28f, 0.3f, 0.36f, 1f)
            : new Color(0.18f, 0.45f, 0.32f, 1f);

        Button button = buttonGo.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.92f, 1f, 0.96f, 1f);
        colors.pressedColor = new Color(0.78f, 0.9f, 0.84f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(buttonGo.transform, false);

        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI labelText = labelGo.GetComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 26;
        labelText.fontStyle = FontStyles.Bold;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = Color.white;
        labelText.raycastTarget = false;

        return button;
    }
}
