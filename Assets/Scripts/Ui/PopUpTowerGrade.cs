using DG.Tweening;
using TMPro;
using Tower;
using UnityEngine;
using UnityEngine.UI;

public class PopUpTowerGrade : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image accentBar;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;

    private RectTransform _rect;
    private Sequence _sequence;

    public void Bind(Image bg, Image accent, TextMeshProUGUI title, TextMeshProUGUI subtitle)
    {
        background = bg;
        accentBar = accent;
        titleText = title;
        subtitleText = subtitle;
    }

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        background ??= GetComponent<Image>();
        titleText ??= transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        subtitleText ??= transform.Find("SubtitleText")?.GetComponent<TextMeshProUGUI>();
        accentBar ??= transform.Find("AccentBar")?.GetComponent<Image>();
    }

    public void Popup(TowerGrade grade, TowerArchetype archetype)
    {
        if (grade < TowerGrade.Epic)
            return;

        Awake();

        if (_rect == null || titleText == null)
            return;

        TowerInfoUiPalette palette = TowerInfoUiThemes.Get(grade);
        string gradeLabel = TowerGradeLabels.GetGradeLabel(grade);
        string archetypeLabel = TowerGradeLabels.GetArchetypeLabel(archetype);

        titleText.text = $"{gradeLabel} 등장!";
        titleText.color = palette.titleTextColor;

        if (subtitleText != null)
        {
            subtitleText.text = $"{archetypeLabel} 타워";
            subtitleText.color = palette.bodyTextColor;
        }

        if (background != null)
            background.color = palette.panelColor;

        if (accentBar != null)
            accentBar.color = palette.accentTextColor;

        _sequence?.Kill();
        _rect.localScale = new Vector3(1f, 0f, 1f);
        gameObject.SetActive(true);

        _sequence = DOTween.Sequence();
        _sequence.Append(_rect.DOScale(Vector3.one, 0.24f).SetEase(Ease.OutBack));
        _sequence.AppendInterval(grade == TowerGrade.Legendary ? 1.1f : 0.85f);
        _sequence.Append(_rect.DOScale(new Vector3(1f, 0f, 1f), 0.18f).SetEase(Ease.InQuad));
        _sequence.OnComplete(() => gameObject.SetActive(false));
    }

    private void OnDisable()
    {
        _sequence?.Kill();
    }
}
