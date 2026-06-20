using UnityEngine;
using UnityEngine.UI;

public class SettingsVolumeUi : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button returnToTitleButton;

    private bool _listenersRegistered;

    public void Bind(
        Slider master,
        Slider bgm,
        Slider sfx,
        Button titleButton = null)
    {
        UnregisterListeners();

        masterSlider = master;
        bgmSlider = bgm;
        sfxSlider = sfx;
        returnToTitleButton = titleButton;

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
    }

    private void RegisterListeners()
    {
        if (_listenersRegistered)
            return;

        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(HandleMasterChanged);

        if (bgmSlider != null)
            bgmSlider.onValueChanged.AddListener(HandleBgmChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(HandleSfxChanged);

        if (returnToTitleButton != null)
            returnToTitleButton.onClick.AddListener(HandleReturnToTitleClicked);

        _listenersRegistered = true;
    }

    private void UnregisterListeners()
    {
        if (!_listenersRegistered)
            return;

        if (masterSlider != null)
            masterSlider.onValueChanged.RemoveListener(HandleMasterChanged);

        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(HandleBgmChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(HandleSfxChanged);

        if (returnToTitleButton != null)
            returnToTitleButton.onClick.RemoveListener(HandleReturnToTitleClicked);

        _listenersRegistered = false;
    }

    public void SyncFromSettings()
    {
        SetSliderValue(masterSlider, GameAudioSettings.MasterVolume);
        SetSliderValue(bgmSlider, GameAudioSettings.BgmVolume);
        SetSliderValue(sfxSlider, GameAudioSettings.VfxVolume);
    }

    private static void SetSliderValue(Slider slider, float value)
    {
        if (slider == null)
            return;

        slider.SetValueWithoutNotify(Mathf.Clamp01(value));
    }

    private void HandleMasterChanged(float value) => GameAudioSettings.MasterVolume = value;
    private void HandleBgmChanged(float value) => GameAudioSettings.BgmVolume = value;
    private void HandleSfxChanged(float value) => GameAudioSettings.VfxVolume = value;

    private void HandleReturnToTitleClicked()
    {
        GameAudioManager.Instance?.PlayUiClick();

        SettingsPanelUi panel = GetComponent<SettingsPanelUi>();
        panel?.Hide();

        if (TitlePreviewMode.Active)
            return;

        GameSessionReturnToTitle.Return();
    }
}
