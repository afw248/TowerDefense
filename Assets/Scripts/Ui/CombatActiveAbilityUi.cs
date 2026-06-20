using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatActiveAbilityUi : MonoBehaviour
{
    [SerializeField] private Button freezeButton;
    [SerializeField] private Button globalDamageButton;
    [SerializeField] private TextMeshProUGUI freezeLabel;
    [SerializeField] private TextMeshProUGUI damageLabel;
    [SerializeField] private TextMeshProUGUI freezeCooldownText;
    [SerializeField] private TextMeshProUGUI damageCooldownText;
    [SerializeField] private Image freezeCooldownFill;
    [SerializeField] private Image damageCooldownFill;

    private CombatActiveAbilityController _controller;
    private bool _listenersBound;

    private void OnEnable()
    {
        EnsureWired();
    }

    private void OnDisable()
    {
        if (_controller != null)
            _controller.Changed -= Refresh;
    }

    private void OnDestroy()
    {
        if (freezeButton != null)
            freezeButton.onClick.RemoveListener(HandleFreezeClicked);

        if (globalDamageButton != null)
            globalDamageButton.onClick.RemoveListener(HandleDamageClicked);
    }

    private void Update()
    {
        if (_controller == null)
            _controller = ResolveController();

        if (_controller == null)
            return;

        RefreshCooldownVisuals();
    }

    public void EnsureWired()
    {
        BindReferences();
        EnsureListenersBound();

        _controller = ResolveController();
        if (_controller != null)
        {
            _controller.Changed -= Refresh;
            _controller.Changed += Refresh;
        }

        Refresh();
    }

    private void EnsureListenersBound()
    {
        BindReferences();

        if (_listenersBound || freezeButton == null || globalDamageButton == null)
            return;

        freezeButton.onClick.AddListener(HandleFreezeClicked);
        globalDamageButton.onClick.AddListener(HandleDamageClicked);
        _listenersBound = true;
    }

    private static CombatActiveAbilityController ResolveController()
    {
        return CombatActiveAbilityController.Instance
            ?? Object.FindFirstObjectByType<CombatActiveAbilityController>(FindObjectsInactive.Include);
    }

    private void BindReferences()
    {
        Transform freezeRoot = transform.Find("FreezeButton");
        Transform damageRoot = transform.Find("GlobalDamageButton");

        freezeButton ??= freezeRoot?.GetComponent<Button>();
        globalDamageButton ??= damageRoot?.GetComponent<Button>();

        freezeLabel ??= freezeRoot?.Find("Label")?.GetComponent<TextMeshProUGUI>();
        damageLabel ??= damageRoot?.Find("Label")?.GetComponent<TextMeshProUGUI>();
        freezeCooldownText ??= freezeRoot?.Find("CooldownText")?.GetComponent<TextMeshProUGUI>();
        damageCooldownText ??= damageRoot?.Find("CooldownText")?.GetComponent<TextMeshProUGUI>();
        freezeCooldownFill ??= freezeRoot?.Find("CooldownFill")?.GetComponent<Image>();
        damageCooldownFill ??= damageRoot?.Find("CooldownFill")?.GetComponent<Image>();
    }

    private void HandleFreezeClicked()
    {
        GameAudioManager.Instance?.PlayUiClick();
        _controller ??= ResolveController();

        if (_controller == null)
        {
            WarningMessageUi.Instance?.Show("스킬 시스템을 찾을 수 없습니다");
            return;
        }

        if (!_controller.TryUseFreeze(out string reason) && !string.IsNullOrEmpty(reason))
            WarningMessageUi.Instance?.Show(reason);
    }

    private void HandleDamageClicked()
    {
        GameAudioManager.Instance?.PlayUiClick();
        _controller ??= ResolveController();

        if (_controller == null)
        {
            WarningMessageUi.Instance?.Show("스킬 시스템을 찾을 수 없습니다");
            return;
        }

        if (!_controller.TryUseGlobalDamage(out string reason) && !string.IsNullOrEmpty(reason))
            WarningMessageUi.Instance?.Show(reason);
    }

    public void Refresh()
    {
        EnsureListenersBound();
        _controller ??= ResolveController();

        if (freezeLabel != null)
        {
            freezeLabel.text = "시간 정지";
            UiFonts.ApplyNexon(freezeLabel);
        }

        if (damageLabel != null)
        {
            damageLabel.text = "전체 피해";
            UiFonts.ApplyNexon(damageLabel);
        }

        RefreshCooldownVisuals();
        RefreshButtonState(
            freezeButton,
            _controller != null && _controller.CanUseFreeze(out _),
            _controller != null && _controller.CanAttemptUse(out _));
        RefreshButtonState(
            globalDamageButton,
            _controller != null && _controller.CanUseGlobalDamage(out _),
            _controller != null && _controller.CanAttemptUse(out _));
    }

    private void RefreshCooldownVisuals()
    {
        if (_controller == null)
            return;

        ApplyCooldown(
            freezeCooldownFill,
            freezeCooldownText,
            _controller.FreezeCooldownRemaining,
            _controller.FreezeCooldownDuration,
            freezeButton,
            _controller.CanUseFreeze(out _),
            _controller.CanAttemptUse(out _));

        ApplyCooldown(
            damageCooldownFill,
            damageCooldownText,
            _controller.GlobalDamageCooldownRemaining,
            _controller.GlobalDamageCooldownDuration,
            globalDamageButton,
            _controller.CanUseGlobalDamage(out _),
            _controller.CanAttemptUse(out _));
    }

    private static void ApplyCooldown(
        Image fill,
        TextMeshProUGUI cooldownText,
        float remaining,
        float duration,
        Button button,
        bool canUse,
        bool canClick)
    {
        float ratio = duration > 0f ? Mathf.Clamp01(remaining / duration) : 0f;

        if (fill != null)
        {
            fill.gameObject.SetActive(remaining > 0f);
            fill.fillAmount = ratio;
        }

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(remaining > 0f);
            cooldownText.text = remaining >= 10f
                ? $"{Mathf.CeilToInt(remaining)}s"
                : $"{remaining:0.#}s";
        }

        RefreshButtonState(button, canUse, canClick);
    }

    private static void RefreshButtonState(Button button, bool canUse, bool canClick)
    {
        if (button == null)
            return;

        button.interactable = canClick;

        Image bg = button.GetComponent<Image>();
        if (bg == null)
            return;

        bg.color = canUse
            ? GameHudTheme.PanelBackground
            : new Color(0.18f, 0.20f, 0.26f, 0.92f);
    }
}
