using UnityEngine;
using UnityEngine.UI;

public class SettingsButtonUi : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private SettingsPanelUi settingsPanel;

    private void Awake()
    {
        button ??= GetComponent<Button>();
        settingsPanel ??= SettingsPanelUi.GetActivePanel();

        if (button != null)
            button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }

    private void HandleClick()
    {
        GameAudioManager.Instance?.PlayUiClick();
        settingsPanel ??= SettingsPanelUi.GetActivePanel();
        settingsPanel?.Toggle();
    }
}
