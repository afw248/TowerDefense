using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelUi : MonoBehaviour
{
    private static SettingsPanelUi _titlePanel;
    private static SettingsPanelUi _gamePanel;

    [SerializeField] private bool isTitlePanel;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private SettingsVolumeUi volumeUi;
    [SerializeField] private SettingsDisplayUi displayUi;

    private CanvasGroup _canvasGroup;
    private bool _listenersRegistered;

    public bool IsTitlePanel => isTitlePanel;
    public bool IsOpen => _canvasGroup != null && _canvasGroup.alpha > 0.01f;

    private void Awake()
    {
        CachePanelReferences();
        EnsureInitialized();
        Hide();
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(HandleCloseClicked);

        if (isTitlePanel && _titlePanel == this)
            _titlePanel = null;
        else if (!isTitlePanel && _gamePanel == this)
            _gamePanel = null;
    }

    public void MarkAsTitlePanel()
    {
        isTitlePanel = true;
        RegisterPanelCache();
    }

    public void Configure(GameObject root, Button close, SettingsVolumeUi volume, SettingsDisplayUi display = null)
    {
        panelRoot = root;
        closeButton = close;
        volumeUi = volume;
        displayUi = display ?? displayUi;
        _listenersRegistered = false;
        EnsureInitialized();
    }

    public void NotifyLayoutBuilt()
    {
        CachePanelReferences();
    }

    public void PrepareHidden()
    {
        EnsureInitialized();
        Hide();
    }

    public static SettingsPanelUi GetActivePanel()
    {
        if (TitlePreviewMode.Active)
            return _titlePanel != null ? _titlePanel : _gamePanel;

        return _gamePanel != null ? _gamePanel : _titlePanel;
    }

    public static bool HasBuiltLayout(Transform panelTransform)
    {
        return panelTransform != null
               && panelTransform.Find("SettingsCard") != null
               && panelTransform.Find("SettingsCard/ResolutionRow") != null
               && panelTransform.Find("SettingsCard/FullScreenRow") != null;
    }

    public static void RefreshVolumeLabels(Transform panelTransform)
    {
        if (panelTransform == null)
            return;

        Transform label = panelTransform.Find("SettingsCard/SfxVolumeRow/SfxVolumeLabel");
        if (label != null && label.TryGetComponent(out TMPro.TextMeshProUGUI tmp))
            tmp.text = "VFX";
    }

    public static void HideAllPanels()
    {
        if (_titlePanel != null)
            _titlePanel.PrepareHidden();

        if (_gamePanel != null && _gamePanel != _titlePanel)
            _gamePanel.PrepareHidden();
    }

    public static SettingsPanelUi EnsureOnCanvas(Transform canvasRoot, bool titlePanel)
    {
        if (canvasRoot == null)
            return null;

        Transform existing = canvasRoot.Find("SettingsPanel");
        GameObject panelGo = existing != null
            ? existing.gameObject
            : new GameObject("SettingsPanel", typeof(RectTransform), typeof(SettingsPanelUi));

        if (existing == null)
            panelGo.transform.SetParent(canvasRoot, false);

        SettingsPanelUi panel = panelGo.GetComponent<SettingsPanelUi>();
        if (panel == null)
            panel = panelGo.AddComponent<SettingsPanelUi>();

        if (titlePanel)
            panel.MarkAsTitlePanel();

        if (!HasBuiltLayout(panel.transform))
            SettingsPanelLayoutBuilder.Rebuild(panel, showReturnToTitle: !titlePanel);
        else
        {
            panel.ApplyFooterVisibility(!titlePanel);
            RefreshVolumeLabels(panel.transform);
        }

        panel.RegisterPanelCache();
        panel.PrepareHidden();
        return panel;
    }

    public void ApplyFooterVisibility(bool showReturnToTitle)
    {
        Transform returnButton = transform.Find("SettingsCard/Footer/ReturnToTitleButton");
        if (returnButton != null)
            returnButton.gameObject.SetActive(showReturnToTitle);
    }

    private void RegisterPanelCache()
    {
        if (isTitlePanel)
            _titlePanel = this;
        else
            _gamePanel = this;
    }

    private void CachePanelReferences()
    {
        if (!HasBuiltLayout(transform))
            return;

        Transform card = transform.Find("SettingsCard");
        if (card == null)
            return;

        Transform footer = card.Find("Footer");
        if (footer != null)
            closeButton ??= footer.Find("SettingsCloseButton")?.GetComponent<Button>();

        volumeUi ??= GetComponent<SettingsVolumeUi>();
        if (volumeUi == null)
            return;

        Button returnButton = footer != null
            ? footer.Find("ReturnToTitleButton")?.GetComponent<Button>()
            : null;

        volumeUi.Bind(
            card.Find("MasterVolumeRow/MasterVolumeSlider")?.GetComponent<Slider>(),
            card.Find("BgmVolumeRow/BgmVolumeSlider")?.GetComponent<Slider>(),
            card.Find("SfxVolumeRow/SfxVolumeSlider")?.GetComponent<Slider>(),
            returnButton);

        displayUi ??= GetComponent<SettingsDisplayUi>();
        if (displayUi != null)
        {
            displayUi.Bind(
                card.Find("ResolutionRow/ResolutionControls/ResolutionValueLabel")?.GetComponent<TMPro.TextMeshProUGUI>(),
                card.Find("ResolutionRow/ResolutionControls/ResolutionPreviousButton")?.GetComponent<Button>(),
                card.Find("ResolutionRow/ResolutionControls/ResolutionNextButton")?.GetComponent<Button>(),
                card.Find("FullScreenRow/FullScreenControls/FullScreenValueLabel")?.GetComponent<TMPro.TextMeshProUGUI>(),
                card.Find("FullScreenRow/FullScreenControls/FullScreenToggle")?.GetComponent<Toggle>());
        }
    }

    private bool EnsureInitialized()
    {
        panelRoot ??= gameObject;

        if (!panelRoot.activeSelf)
            panelRoot.SetActive(true);

        if (!HasBuiltLayout(transform))
            SettingsPanelLayoutBuilder.Rebuild(this, showReturnToTitle: !isTitlePanel);
        else
            CachePanelReferences();

        _canvasGroup ??= panelRoot.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = panelRoot.AddComponent<CanvasGroup>();

        if (closeButton != null && !_listenersRegistered)
        {
            closeButton.onClick.AddListener(HandleCloseClicked);
            _listenersRegistered = true;
        }

        volumeUi ??= GetComponent<SettingsVolumeUi>();
        displayUi ??= GetComponent<SettingsDisplayUi>();
        RegisterPanelCache();

        return _canvasGroup != null;
    }

    private void HandleCloseClicked()
    {
        GameAudioManager.Instance?.PlayUiClick();
        Hide();
    }

    public void Show()
    {
        EnsureInitialized();

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        SetBackgroundRaycastTarget(true);
        volumeUi?.SyncFromSettings();
        displayUi?.SyncFromSettings();
        GameAudioManager.Instance?.PlaySfx(GameAudioId.UiOpen);
    }

    public void Hide()
    {
        if (_canvasGroup == null)
            return;

        if (IsOpen)
            GameAudioManager.Instance?.PlaySfx(GameAudioId.UiClose);

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        SetBackgroundRaycastTarget(false);
    }

    private void SetBackgroundRaycastTarget(bool enabled)
    {
        if (panelRoot == null)
            return;

        Image background = panelRoot.GetComponent<Image>();
        if (background != null)
            background.raycastTarget = enabled;
    }

    public void Toggle()
    {
        EnsureInitialized();

        if (IsOpen)
            Hide();
        else
            Show();
    }
}
