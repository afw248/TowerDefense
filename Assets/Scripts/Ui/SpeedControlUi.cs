using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeedControlUi : MonoBehaviour
{
    [SerializeField] private Button normalButton;
    [SerializeField] private Button fastButton;
    [SerializeField] private Button turboButton;
    [SerializeField] private Button ultraButton;
    [SerializeField] private Button hyperButton;
    [SerializeField] private TextMeshProUGUI normalLabel;
    [SerializeField] private TextMeshProUGUI fastLabel;
    [SerializeField] private TextMeshProUGUI turboLabel;
    [SerializeField] private TextMeshProUGUI ultraLabel;
    [SerializeField] private TextMeshProUGUI hyperLabel;

    private static readonly Color ActiveColor = new(1f, 0.88f, 0.35f, 1f);
    private static readonly Color InactiveColor = new(0.62f, 0.66f, 0.72f, 1f);

    private GameSpeedController _controller;

    private void Awake()
    {
        if (normalButton != null)
            normalButton.onClick.AddListener(HandleNormalClick);

        if (fastButton != null)
            fastButton.onClick.AddListener(HandleFastClick);

        if (turboButton != null)
            turboButton.onClick.AddListener(HandleTurboClick);

        if (ultraButton != null)
            ultraButton.onClick.AddListener(HandleUltraClick);

        if (hyperButton != null)
            hyperButton.onClick.AddListener(HandleHyperClick);
    }

    private void OnDestroy()
    {
        if (normalButton != null)
            normalButton.onClick.RemoveListener(HandleNormalClick);

        if (fastButton != null)
            fastButton.onClick.RemoveListener(HandleFastClick);

        if (turboButton != null)
            turboButton.onClick.RemoveListener(HandleTurboClick);

        if (ultraButton != null)
            ultraButton.onClick.RemoveListener(HandleUltraClick);

        if (hyperButton != null)
            hyperButton.onClick.RemoveListener(HandleHyperClick);

        if (_controller != null)
            _controller.OnSpeedChanged -= HandleSpeedChanged;
    }

    public void Bind(GameSpeedController controller)
    {
        if (_controller != null)
            _controller.OnSpeedChanged -= HandleSpeedChanged;

        _controller = controller;

        if (_controller != null)
        {
            _controller.OnSpeedChanged += HandleSpeedChanged;
            HandleSpeedChanged(_controller.CurrentSpeed);
        }
    }

    private void HandleNormalClick() => _controller?.SetNormalSpeed();

    private void HandleFastClick() => _controller?.SetFastSpeed();

    private void HandleTurboClick() => _controller?.SetTurboSpeed();

    private void HandleUltraClick() => _controller?.SetUltraSpeed();

    private void HandleHyperClick() => _controller?.SetHyperSpeed();

    private void HandleSpeedChanged(float speed)
    {
        SetLabelColor(normalLabel, _controller != null && _controller.IsNormal);
        SetLabelColor(fastLabel, _controller != null && _controller.IsFast);
        SetLabelColor(turboLabel, _controller != null && _controller.IsTurbo);
        SetLabelColor(ultraLabel, _controller != null && _controller.IsUltra);
        SetLabelColor(hyperLabel, _controller != null && _controller.IsHyper);
    }

    private static void SetLabelColor(TextMeshProUGUI label, bool active)
    {
        if (label != null)
            label.color = active ? ActiveColor : InactiveColor;
    }
}
