using Tower;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerGradeRevealUi : MonoBehaviour
{
    public static TowerGradeRevealUi Instance { get; private set; }

    private PopUpTowerGrade _popup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _popup = GetComponentInChildren<PopUpTowerGrade>(true);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Show(TowerGrade grade, TowerArchetype archetype)
    {
        if (grade < TowerGrade.Epic)
            return;

        _popup ??= GetComponentInChildren<PopUpTowerGrade>(true);
        if (_popup == null)
            return;

        _popup.Popup(grade, archetype);
        GameAudioManager.Instance?.PlayGradeReveal(grade);
    }

    public static void EnsureExists(Transform hudRoot)
    {
        if (Instance != null || hudRoot == null)
            return;

        Transform existing = hudRoot.Find("TowerGradeReveal");
        if (existing != null)
        {
            if (existing.GetComponent<TowerGradeRevealUi>() == null)
                existing.gameObject.AddComponent<TowerGradeRevealUi>();
            return;
        }

        GameObject panel = new GameObject("TowerGradeReveal", typeof(RectTransform), typeof(TowerGradeRevealUi));
        panel.transform.SetParent(hudRoot, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 72f);
        rect.sizeDelta = new Vector2(520f, 108f);

        GameObject popupGo = new GameObject("PopupPanel", typeof(RectTransform), typeof(Image), typeof(PopUpTowerGrade));
        popupGo.transform.SetParent(panel.transform, false);
        RectTransform popupRect = popupGo.GetComponent<RectTransform>();
        popupRect.anchorMin = Vector2.zero;
        popupRect.anchorMax = Vector2.one;
        popupRect.offsetMin = Vector2.zero;
        popupRect.offsetMax = Vector2.zero;

        Image bg = popupGo.GetComponent<Image>();
        bg.color = new Color(0.18f, 0.1f, 0.34f, 0.98f);
        bg.raycastTarget = false;

        Outline outline = popupGo.AddComponent<Outline>();
        outline.effectColor = new Color(0.82f, 0.55f, 1f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject accentBarGo = new GameObject("AccentBar", typeof(RectTransform), typeof(Image));
        accentBarGo.transform.SetParent(popupGo.transform, false);
        RectTransform accentRect = accentBarGo.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 1f);
        accentRect.anchorMax = new Vector2(1f, 1f);
        accentRect.pivot = new Vector2(0.5f, 1f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(0f, 6f);
        Image accentBar = accentBarGo.GetComponent<Image>();
        accentBar.color = new Color(0.82f, 0.55f, 1f, 1f);
        accentBar.raycastTarget = false;

        TextMeshProUGUI title = CreateLabel(popupGo.transform, "TitleText", "에픽 등장!", 38,
            new Vector2(0f, 0.52f), new Vector2(1f, 1f), new Vector2(20f, 10f), new Vector2(-20f, -4f));
        TextMeshProUGUI subtitle = CreateLabel(popupGo.transform, "SubtitleText", "석궁 타워", 24,
            new Vector2(0f, 0f), new Vector2(1f, 0.52f), new Vector2(20f, 8f), new Vector2(-20f, -8f),
            FontStyles.Normal, new Color(0.94f, 0.9f, 1f, 1f));

        popupGo.GetComponent<PopUpTowerGrade>().Bind(bg, accentBar, title, subtitle);
        popupGo.SetActive(false);
    }

    private static TextMeshProUGUI CreateLabel(
        Transform parent,
        string name,
        string text,
        int fontSize,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax,
        FontStyles fontStyle = FontStyles.Bold,
        Color color = default)
    {
        if (color == default)
            color = Color.white;

        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform labelRect = go.GetComponent<RectTransform>();
        labelRect.anchorMin = anchorMin;
        labelRect.anchorMax = anchorMax;
        labelRect.offsetMin = offsetMin;
        labelRect.offsetMax = offsetMax;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        UiFonts.ApplyNexon(tmp);
        tmp.fontSize = fontSize;
        tmp.fontStyle = fontStyle;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        return tmp;
    }
}
