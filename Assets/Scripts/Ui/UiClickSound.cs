using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UiClickSound : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(HandleClick);
    }

    private static void HandleClick()
    {
        GameAudioManager.Instance?.PlayUiClick();
    }
}
