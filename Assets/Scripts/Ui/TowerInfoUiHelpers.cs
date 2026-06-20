using UnityEngine;
using UnityEngine.UI;

public static class TowerInfoUiHelpers
{
    private static Sprite _uiSprite;

    public static Sprite GetUiSprite()
    {
        if (_uiSprite != null)
            return _uiSprite;

        Texture2D texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[64];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;

        texture.SetPixels(pixels);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        _uiSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 8f, 8f),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(2f, 2f, 2f, 2f));

        return _uiSprite;
    }

    public static void ApplyRangeRing(Image rangeRingImage, float detectRadius, Color ringColor)
    {
        if (rangeRingImage == null)
            return;

        float normalized = Mathf.Clamp01(detectRadius / TowerCombatStats.MaxDetectRadiusReference);
        float scale = Mathf.Lerp(0.38f, 1f, normalized);

        RectTransform ringRect = rangeRingImage.rectTransform;
        ringRect.localScale = new Vector3(scale, scale, 1f);
        rangeRingImage.color = new Color(ringColor.r, ringColor.g, ringColor.b, 0.05f);

        Outline ringOutline = rangeRingImage.GetComponent<Outline>();
        if (ringOutline != null)
            ringOutline.effectColor = ringColor;

        rangeRingImage.enabled = detectRadius > 0f;
    }
}
