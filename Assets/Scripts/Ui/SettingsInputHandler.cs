using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsInputHandler : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (TitleGameFlow.Instance != null && TitleGameFlow.Instance.IsTransitioning)
            return;

        SettingsPanelUi panel = SettingsPanelUi.GetActivePanel();
        panel?.Toggle();
    }
}
