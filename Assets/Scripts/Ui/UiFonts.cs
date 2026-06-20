using TMPro;
using UnityEngine;

public static class UiFonts
{
    private const string NexonResourcePath = "NEXON Football Gothic L SDF";

    private static TMP_FontAsset _nexon;

    public static TMP_FontAsset Nexon
    {
        get
        {
            if (_nexon != null)
                return _nexon;

            _nexon = Resources.Load<TMP_FontAsset>(NexonResourcePath);
            if (_nexon != null)
                return _nexon;

            TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (TMP_FontAsset font in fonts)
            {
                if (font != null && font.name.StartsWith("NEXON"))
                {
                    _nexon = font;
                    break;
                }
            }

            return _nexon;
        }
    }

    public static void ApplyNexon(TextMeshProUGUI text)
    {
        if (text == null)
            return;

        TMP_FontAsset font = Nexon;
        if (font != null)
            text.font = font;
    }

    public static void ApplyNexonToAllUiText(bool includeInactive = true)
    {
        TMP_FontAsset font = Nexon;
        if (font == null)
            return;

        TextMeshProUGUI[] labels = Object.FindObjectsByType<TextMeshProUGUI>(
            includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        foreach (TextMeshProUGUI label in labels)
        {
            if (label == null)
                continue;

            label.font = font;
        }
    }
}
