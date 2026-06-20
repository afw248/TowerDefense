using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarningMessageUi : MonoBehaviour
{
    public static WarningMessageUi Instance { get; private set; }

    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float displayDuration = 2f;

    private Coroutine _hideRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (panelRoot == null)
            panelRoot = gameObject;

        messageText ??= GetComponentInChildren<TextMeshProUGUI>(true);
        panelRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Initialize(GameObject root, TextMeshProUGUI text)
    {
        panelRoot = root;
        messageText = text;
        panelRoot.SetActive(false);
    }

    public void Show(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        messageText ??= GetComponentInChildren<TextMeshProUGUI>(true);

        if (panelRoot == null || messageText == null)
            return;

        messageText.text = message;
        panelRoot.SetActive(true);

        if (_hideRoutine != null)
            StopCoroutine(_hideRoutine);

        _hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        if (panelRoot != null)
            panelRoot.SetActive(false);
        _hideRoutine = null;
    }

    public static void EnsureExists(Transform hudRoot)
    {
        if (Instance != null || hudRoot == null)
            return;

        Transform existing = hudRoot.Find("WarningMessage");
        if (existing != null)
        {
            WarningMessageUi existingUi = existing.GetComponent<WarningMessageUi>();
            if (existingUi == null)
                existing.gameObject.AddComponent<WarningMessageUi>();
            return;
        }

        GameObject panel = new GameObject("WarningMessage", typeof(RectTransform), typeof(Image), typeof(WarningMessageUi));
        panel.transform.SetParent(hudRoot, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.12f);
        rect.anchorMax = new Vector2(0.5f, 0.12f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(460f, 56f);

        Image bg = panel.GetComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);
        bg.type = Image.Type.Sliced;
        bg.raycastTarget = false;

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.95f, 0.35f, 0.2f, 0.85f);
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject textGo = new GameObject("WarningText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(panel.transform, false);
        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16f, 8f);
        textRect.offsetMax = new Vector2(-16f, -8f);

        TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = string.Empty;
        GameHudTheme.StyleLabel(tmp, 22f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = GameHudTheme.GoldText;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;

        panel.GetComponent<WarningMessageUi>().Initialize(panel, tmp);
    }
}
