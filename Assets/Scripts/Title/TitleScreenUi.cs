using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenUi : MonoBehaviour
{
    private static readonly Vector2 TitleAnchoredPosition = new(0f, -88f);
    private static readonly Vector2 StartButtonPosition = new(0f, 180f);
    private static readonly Vector2 TutorialButtonPosition = new(-132f, 96f);
    private static readonly Vector2 ExitButtonPosition = new(132f, 96f);
    private static readonly Vector2 HintAnchoredPosition = new(0f, 28f);

    private static readonly Color PrimaryButtonColor = new(0.1f, 0.45f, 0.3f, 1f);
    private static readonly Color SecondaryButtonColor = new(0.12f, 0.34f, 0.58f, 1f);
    private static readonly Color TertiaryButtonColor = new(0.28f, 0.3f, 0.36f, 1f);

    [SerializeField] private string title = "Tower Gambit";
    [SerializeField] private string hintText = "Enter 키로 시작  ·  T 튜토리얼  ·  Esc 종료";

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI hintLabel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button exitButton;

    public Button StartButton => startButton;
    public Button TutorialButton => tutorialButton;
    public Button ExitButton => exitButton;

    private CanvasGroup _canvasGroup;
    private Image _topFade;
    private Image _bottomBar;
    private Tween _fadeTween;
    private Sequence _entranceSequence;

    private void Awake()
    {
        EnsureUi();
        EnsureCanvasGroup();
        UiFonts.ApplyNexonToAllUiText();
        SetVisible(true, immediate: true);
    }

    private void OnDestroy()
    {
        _fadeTween?.Kill();
        _entranceSequence?.Kill();
    }

    public Tween FadeIn(float duration)
    {
        Tween fade = FadeTo(1f, duration, interactable: true);
        PlayEntranceAnimation(duration);
        return fade;
    }

    public Tween FadeOut(float duration)
    {
        _entranceSequence?.Kill();
        return FadeTo(0f, duration, interactable: false);
    }

    public void SetVisible(bool visible, bool immediate = false)
    {
        EnsureCanvasGroup();
        if (_canvasGroup == null)
            return;

        _fadeTween?.Kill();
        _entranceSequence?.Kill();

        float alpha = visible ? 1f : 0f;
        _canvasGroup.alpha = immediate ? alpha : _canvasGroup.alpha;
        _canvasGroup.interactable = visible;
        _canvasGroup.blocksRaycasts = visible;

        if (visible && immediate)
            SnapEntranceVisible();
    }

    public void PlayEntranceAnimation(float duration = 0.55f)
    {
        _entranceSequence?.Kill();
        SnapEntranceHidden();

        _entranceSequence = DOTween.Sequence();
        _entranceSequence.AppendInterval(0.08f);

        if (titleText != null)
            _entranceSequence.Join(titleText.DOFade(1f, duration * 0.7f).SetEase(Ease.OutQuad));

        if (startButton != null)
            _entranceSequence.Join(AnimateButtonEntrance(startButton, duration, 0.12f));

        if (tutorialButton != null)
            _entranceSequence.Join(AnimateButtonEntrance(tutorialButton, duration * 0.85f, 0.2f));

        if (exitButton != null)
            _entranceSequence.Join(AnimateButtonEntrance(exitButton, duration * 0.85f, 0.24f));

        if (hintLabel != null)
            _entranceSequence.Join(hintLabel.DOFade(0.55f, duration * 0.6f).SetDelay(0.28f).SetEase(Ease.OutQuad));
    }

    private Tween FadeTo(float alpha, float duration, bool interactable)
    {
        EnsureCanvasGroup();
        if (_canvasGroup == null)
            return null;

        _fadeTween?.Kill();
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        _fadeTween = _canvasGroup
            .DOFade(alpha, duration)
            .SetEase(alpha > _canvasGroup.alpha ? Ease.OutQuad : Ease.InQuad)
            .OnComplete(() =>
            {
                _canvasGroup.interactable = interactable;
                _canvasGroup.blocksRaycasts = interactable;
            });

        return _fadeTween;
    }

    private void EnsureCanvasGroup()
    {
        if (_canvasGroup != null)
            return;

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void EnsureUi()
    {
        RectTransform canvasRect = transform as RectTransform;
        if (canvasRect == null)
            return;

        EnsureBackdrop(canvasRect);
        RemoveLegacySubtitle(canvasRect);

        titleText ??= CreateLabel(
            canvasRect,
            "TitleText",
            title,
            54,
            FontStyles.Bold,
            Color.white);

        hintLabel ??= CreateLabel(
            canvasRect,
            "HintText",
            hintText,
            20,
            FontStyles.Normal,
            new Color(0.88f, 0.92f, 0.9f, 0.55f));

        startButton ??= CreateMenuButton(canvasRect, "StartButton", "게임 시작");
        tutorialButton ??= CreateMenuButton(canvasRect, "TutorialButton", "튜토리얼");
        exitButton ??= CreateMenuButton(canvasRect, "ExitButton", "게임 종료");

        titleText.text = title;
        hintLabel.text = hintText;

        LayoutTitle(titleText);
        LayoutHint(hintLabel);

        LayoutMenuButton(
            startButton,
            "게임 시작",
            StartButtonPosition,
            new Vector2(360f, 72f),
            32,
            PrimaryButtonColor);

        LayoutMenuButton(
            tutorialButton,
            "튜토리얼",
            TutorialButtonPosition,
            new Vector2(200f, 52f),
            24,
            SecondaryButtonColor);

        LayoutMenuButton(
            exitButton,
            "게임 종료",
            ExitButtonPosition,
            new Vector2(200f, 52f),
            24,
            TertiaryButtonColor);

        ApplyFont(titleText);
        ApplyFont(hintLabel);
        ApplyFont(startButton.GetComponentInChildren<TextMeshProUGUI>(true));
        ApplyFont(tutorialButton.GetComponentInChildren<TextMeshProUGUI>(true));
        ApplyFont(exitButton.GetComponentInChildren<TextMeshProUGUI>(true));

        EnsureSiblingOrder();
        SnapEntranceVisible();
    }

    private static void RemoveLegacySubtitle(RectTransform canvasRect)
    {
        Transform legacySubtitle = canvasRect.Find("SubtitleText");
        if (legacySubtitle != null)
            Destroy(legacySubtitle.gameObject);
    }

    private void EnsureBackdrop(RectTransform canvasRect)
    {
        _topFade = EnsureBackdropImage(canvasRect, "TopFade", new Vector2(0f, 0.55f), Vector2.one, new Color(0f, 0f, 0f, 0.18f));
        _bottomBar = EnsureBackdropImage(canvasRect, "BottomBar", Vector2.zero, new Vector2(1f, 0.28f), new Color(0.04f, 0.08f, 0.07f, 0.72f));
    }

    private static Image EnsureBackdropImage(
        RectTransform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color)
    {
        Transform existing = parent.Find(name);
        GameObject backdropGo;
        if (existing != null)
        {
            backdropGo = existing.gameObject;
        }
        else
        {
            backdropGo = new GameObject(name, typeof(RectTransform), typeof(Image));
            backdropGo.transform.SetParent(parent, false);
        }

        RectTransform rect = backdropGo.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;

        Image image = backdropGo.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private void EnsureSiblingOrder()
    {
        if (_topFade != null)
            _topFade.transform.SetAsFirstSibling();

        if (_bottomBar != null)
            _bottomBar.transform.SetSiblingIndex(1);

        if (titleText != null)
            titleText.transform.SetSiblingIndex(2);

        if (hintLabel != null)
            hintLabel.transform.SetAsLastSibling();
    }

    private static void LayoutTitle(TextMeshProUGUI label)
    {
        if (label == null)
            return;

        RectTransform rect = label.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = TitleAnchoredPosition;
        rect.sizeDelta = new Vector2(820f, 88f);
        label.raycastTarget = false;
    }

    private static void LayoutHint(TextMeshProUGUI label)
    {
        if (label == null)
            return;

        RectTransform rect = label.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = HintAnchoredPosition;
        rect.sizeDelta = new Vector2(720f, 28f);
        label.raycastTarget = false;
    }

    private static void LayoutMenuButton(
        Button button,
        string label,
        Vector2 anchoredPosition,
        Vector2 size,
        float fontSize,
        Color backgroundColor)
    {
        if (button == null)
            return;

        RectTransform rect = button.transform as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        if (button.TryGetComponent(out Image image))
            image.color = backgroundColor;

        TextMeshProUGUI buttonLabel = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (buttonLabel != null)
        {
            buttonLabel.text = label;
            buttonLabel.fontSize = fontSize;
        }
    }

    private static Button CreateMenuButton(RectTransform parent, string name, string label)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
            return existing.GetComponent<Button>();

        GameObject buttonGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(parent, false);

        Button button = buttonGo.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.92f, 1f, 0.96f, 1f);
        colors.pressedColor = new Color(0.78f, 0.9f, 0.84f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        TextMeshProUGUI buttonLabel = CreateLabel(buttonGo.transform, "Label", label, 28, FontStyles.Bold, Color.white);
        buttonLabel.raycastTarget = false;

        return button;
    }

    private static TextMeshProUGUI CreateLabel(
        Transform parent,
        string name,
        string text,
        float fontSize,
        FontStyles fontStyle,
        Color color)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
            return existing.GetComponent<TextMeshProUGUI>();

        GameObject labelGo = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(parent, false);

        RectTransform rect = labelGo.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelGo.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
        return label;
    }

    private static void ApplyFont(TextMeshProUGUI label)
    {
        if (label == null)
            return;

        TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (TMP_FontAsset font in fonts)
        {
            if (font != null && font.name.Contains("NEXON Football Gothic"))
            {
                label.font = font;
                break;
            }
        }
    }

    private void SnapEntranceVisible()
    {
        SetLabelAlpha(titleText, 1f);
        SetLabelAlpha(hintLabel, 0.55f);
        SnapButtonVisible(startButton);
        SnapButtonVisible(tutorialButton);
        SnapButtonVisible(exitButton);
    }

    private void SnapEntranceHidden()
    {
        SetLabelAlpha(titleText, 0f);
        SetLabelAlpha(hintLabel, 0f);
        SnapButtonHidden(startButton);
        SnapButtonHidden(tutorialButton);
        SnapButtonHidden(exitButton);
    }

    private static void SetLabelAlpha(TextMeshProUGUI label, float alpha)
    {
        if (label == null)
            return;

        Color color = label.color;
        color.a = alpha;
        label.color = color;
    }

    private static void SnapButtonVisible(Button button)
    {
        if (button == null)
            return;

        if (button.TryGetComponent(out CanvasGroup group))
        {
            group.alpha = 1f;
            group.transform.localScale = Vector3.one;
        }
    }

    private static void SnapButtonHidden(Button button)
    {
        if (button == null)
            return;

        CanvasGroup group = EnsureButtonCanvasGroup(button);
        group.alpha = 0f;
        button.transform.localScale = new Vector3(0.96f, 0.96f, 1f);
    }

    private static Tween AnimateButtonEntrance(Button button, float duration, float delay)
    {
        CanvasGroup group = EnsureButtonCanvasGroup(button);
        group.alpha = 0f;
        button.transform.localScale = new Vector3(0.96f, 0.96f, 1f);

        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(delay);
        sequence.Join(group.DOFade(1f, duration).SetEase(Ease.OutQuad));
        sequence.Join(button.transform.DOScale(1f, duration).SetEase(Ease.OutBack, 1.05f));
        return sequence;
    }

    private static CanvasGroup EnsureButtonCanvasGroup(Button button)
    {
        if (!button.TryGetComponent(out CanvasGroup group))
            group = button.gameObject.AddComponent<CanvasGroup>();

        return group;
    }
}
