using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopupUi : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button continueButton;

    private Action _onContinue;

    public bool IsVisible => panelRoot != null && panelRoot.activeSelf;

    private void Awake()
    {
        panelRoot ??= gameObject;
        ResolveReferences();
        WireContinueButton();
        HideImmediate();
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(HandleContinueClicked);
    }

    public void Show(string title, string message, Action onContinue)
    {
        ResolveReferences();
        WireContinueButton();
        ApplyTutorialFonts();
        _onContinue = onContinue;

        if (titleText != null)
            titleText.text = title;

        if (messageText != null)
            messageText.text = message;

        if (panelRoot != null)
        {
            panelRoot.transform.SetAsLastSibling();
            panelRoot.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void HideImmediate()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void HandleContinueClicked()
    {
        Action callback = _onContinue;
        _onContinue = null;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (GameSpeedController.Instance != null)
            GameSpeedController.Instance.RestoreCurrentSpeed();
        else
            Time.timeScale = 1f;

        callback?.Invoke();
    }

    private void WireContinueButton()
    {
        if (continueButton == null)
            return;

        continueButton.onClick.RemoveListener(HandleContinueClicked);
        continueButton.onClick.AddListener(HandleContinueClicked);
    }

    private void ResolveReferences()
    {
        if (titleText == null)
            titleText = FindChildText("TutorialTitle");

        if (messageText == null)
            messageText = FindChildText("TutorialMessage");

        if (continueButton == null)
            continueButton = FindChildButton("TutorialContinueButton");

        ApplyTutorialFonts();
    }

    private void ApplyTutorialFonts()
    {
        UiFonts.ApplyNexon(titleText);
        UiFonts.ApplyNexon(messageText);

        if (continueButton == null)
            return;

        TextMeshProUGUI buttonLabel = continueButton.GetComponentInChildren<TextMeshProUGUI>(true);
        UiFonts.ApplyNexon(buttonLabel);
    }

    private TextMeshProUGUI FindChildText(string childName)
    {
        Transform child = FindDeepChild(transform, childName);
        return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
    }

    private Button FindChildButton(string childName)
    {
        Transform child = FindDeepChild(transform, childName);
        return child != null ? child.GetComponent<Button>() : null;
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent == null)
            return null;

        if (parent.name == childName)
            return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeepChild(parent.GetChild(i), childName);
            if (found != null)
                return found;
        }

        return null;
    }

    public static TutorialPopupUi EnsureExists(Transform hudRoot)
    {
        if (hudRoot == null)
            return null;

        Transform existing = hudRoot.Find("TutorialPopupPanel");
        if (existing != null)
        {
            TutorialPopupUi existingUi = existing.GetComponent<TutorialPopupUi>();
            if (existingUi == null)
                existingUi = existing.gameObject.AddComponent<TutorialPopupUi>();

            existingUi.ResolveReferences();
            existingUi.WireContinueButton();
            return existingUi;
        }

        GameObject panel = new GameObject("TutorialPopupPanel", typeof(RectTransform), typeof(Image), typeof(TutorialPopupUi));
        panel.transform.SetParent(hudRoot, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image dim = panel.GetComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.72f);
        dim.raycastTarget = true;

        GameObject card = new GameObject("TutorialCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(panel.transform, false);

        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.sizeDelta = new Vector2(760f, 420f);

        Image cardBg = card.GetComponent<Image>();
        cardBg.color = new Color(0.1f, 0.14f, 0.2f, 0.96f);

        TextMeshProUGUI title = CreateLabel(
            card.transform,
            "TutorialTitle",
            "튜토리얼",
            36,
            FontStyles.Bold,
            new Vector2(0f, 120f),
            new Vector2(680f, 56f),
            new Color(1f, 0.9f, 0.35f, 1f));

        TextMeshProUGUI message = CreateLabel(
            card.transform,
            "TutorialMessage",
            string.Empty,
            24,
            FontStyles.Normal,
            new Vector2(0f, 10f),
            new Vector2(680f, 180f),
            Color.white);
        message.alignment = TextAlignmentOptions.TopLeft;
        message.textWrappingMode = TextWrappingModes.Normal;
        message.overflowMode = TextOverflowModes.Overflow;

        Button continueButton = CreateContinueButton(card.transform);

        TutorialPopupUi ui = panel.GetComponent<TutorialPopupUi>();
        ui.panelRoot = panel;
        ui.titleText = title;
        ui.messageText = message;
        ui.continueButton = continueButton;
        ui.WireContinueButton();
        panel.SetActive(false);
        return ui;
    }

    private static TextMeshProUGUI CreateLabel(
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
        label.textWrappingMode = TextWrappingModes.Normal;
        label.overflowMode = TextOverflowModes.Overflow;
        UiFonts.ApplyNexon(label);
        return label;
    }

    private static Button CreateContinueButton(Transform parent)
    {
        GameObject buttonGo = new GameObject("TutorialContinueButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonGo.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, -150f);
        buttonRect.sizeDelta = new Vector2(220f, 52f);

        Image buttonImage = buttonGo.GetComponent<Image>();
        buttonImage.color = new Color(0.18f, 0.45f, 0.32f, 1f);

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

        TextMeshProUGUI label = labelGo.GetComponent<TextMeshProUGUI>();
        label.text = "시작";
        label.fontSize = 26;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        UiFonts.ApplyNexon(label);

        return button;
    }
}
