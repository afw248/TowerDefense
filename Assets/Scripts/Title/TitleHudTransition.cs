using DG.Tweening;
using UnityEngine;

public static class TitleHudTransition
{
    private static readonly string[] HudCanvasNames =
    {
        "GameHudCanvas",
        "PlayerUiCanvas",
        "WaveUiCanvas",
    };

    public static void SetVisible(bool visible, bool immediate = false)
    {
        GameHudCanvasHelper.EnsureCanvasScales();

        foreach (string canvasName in HudCanvasNames)
        {
            CanvasGroup group = GameHudCanvasHelper.EnsureCanvasGroup(canvasName);
            if (group == null)
                continue;

            group.DOKill();
            float targetAlpha = visible ? 1f : 0f;
            group.alpha = immediate ? targetAlpha : group.alpha;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }

    public static Tween FadeIn(float duration)
    {
        GameHudCanvasHelper.EnsureCanvasScales();
        Sequence sequence = DOTween.Sequence();

        foreach (string canvasName in HudCanvasNames)
        {
            CanvasGroup group = GameHudCanvasHelper.EnsureCanvasGroup(canvasName);
            if (group == null)
                continue;

            group.DOKill();
            group.interactable = false;
            group.blocksRaycasts = false;
            sequence.Join(group.DOFade(1f, duration).SetEase(Ease.OutQuad));
        }

        return sequence.OnComplete(() =>
        {
            foreach (string canvasName in HudCanvasNames)
            {
                CanvasGroup group = GameHudCanvasHelper.EnsureCanvasGroup(canvasName);
                if (group == null)
                    continue;

                group.interactable = true;
                group.blocksRaycasts = true;
            }
        });
    }

    public static Tween FadeOut(float duration)
    {
        Sequence sequence = DOTween.Sequence();

        foreach (string canvasName in HudCanvasNames)
        {
            CanvasGroup group = GameHudCanvasHelper.EnsureCanvasGroup(canvasName);
            if (group == null)
                continue;

            group.DOKill();
            group.interactable = false;
            group.blocksRaycasts = false;
            sequence.Join(group.DOFade(0f, duration).SetEase(Ease.InQuad));
        }

        return sequence;
    }
}
