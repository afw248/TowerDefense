using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsDisplayUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resolutionLabel;
    [SerializeField] private TextMeshProUGUI fullScreenLabel;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Toggle fullScreenToggle;

    private bool _listenersRegistered;

    public void Bind(
        TextMeshProUGUI label,
        Button previous,
        Button next,
        TextMeshProUGUI fullScreenValue,
        Toggle fullScreen)
    {
        UnregisterListeners();

        resolutionLabel = label;
        previousButton = previous;
        nextButton = next;
        fullScreenLabel = fullScreenValue;
        fullScreenToggle = fullScreen;

        RegisterListeners();
        SyncFromSettings();
    }

    private void Awake()
    {
        RegisterListeners();
        SyncFromSettings();
    }

    private void OnEnable()
    {
        SyncFromSettings();
    }

    private void OnDestroy()
    {
        UnregisterListeners();
        GameDisplaySettings.Changed -= SyncFromSettings;
    }

    private void RegisterListeners()
    {
        if (_listenersRegistered)
            return;

        if (previousButton != null)
            previousButton.onClick.AddListener(HandlePreviousClicked);

        if (nextButton != null)
            nextButton.onClick.AddListener(HandleNextClicked);

        if (fullScreenToggle != null)
            fullScreenToggle.onValueChanged.AddListener(HandleFullScreenChanged);

        GameDisplaySettings.Changed += SyncFromSettings;
        _listenersRegistered = true;
    }

    private void UnregisterListeners()
    {
        if (!_listenersRegistered)
            return;

        if (previousButton != null)
            previousButton.onClick.RemoveListener(HandlePreviousClicked);

        if (nextButton != null)
            nextButton.onClick.RemoveListener(HandleNextClicked);

        if (fullScreenToggle != null)
            fullScreenToggle.onValueChanged.RemoveListener(HandleFullScreenChanged);

        GameDisplaySettings.Changed -= SyncFromSettings;
        _listenersRegistered = false;
    }

    public void SyncFromSettings()
    {
        if (resolutionLabel != null)
            resolutionLabel.text = GameDisplaySettings.CurrentPreset.Label;

        if (fullScreenToggle != null)
            fullScreenToggle.SetIsOnWithoutNotify(GameDisplaySettings.IsFullScreen);

        if (fullScreenLabel != null)
            fullScreenLabel.text = GameDisplaySettings.IsFullScreen ? "켜짐" : "꺼짐";
    }

    private void HandlePreviousClicked()
    {
        GameAudioManager.Instance?.PlayUiClick();
        GameDisplaySettings.PresetIndex--;
    }

    private void HandleNextClicked()
    {
        GameAudioManager.Instance?.PlayUiClick();
        GameDisplaySettings.PresetIndex++;
    }

    private void HandleFullScreenChanged(bool isOn)
    {
        GameAudioManager.Instance?.PlayUiClick();
        GameDisplaySettings.IsFullScreen = isOn;
    }
}
