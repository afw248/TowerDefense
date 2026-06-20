using TMPro;

using UnityEngine;



public class WaveUi : MonoBehaviour

{

    [SerializeField] private TextMeshProUGUI waveText;

    [SerializeField] private TextMeshProUGUI waveTimeText;

    [field: SerializeField] public PopUpWave PopupWave { get; set; }



    private void Awake()

    {

        RepairWaveTextLayout();

    }



    public void SetWaves(int wave, bool isBossWave = false)

    {

        RepairWaveTextLayout();



        if (waveText == null)

            return;



        waveText.text = isBossWave ? $"보스 {wave}" : $"웨이브 {wave}";

    }



    public void SetTimer(float remainingSeconds, bool isBossWave = false, bool isPreFirstWaveDelay = false)

    {

        if (waveTimeText == null)

            return;



        waveTimeText.text = WaveTimerUi.FormatRemainingTime(

            remainingSeconds,

            isBossWave,

            isPreFirstWaveDelay);

    }



    public void RepairWaveTextLayout()
    {
        waveText ??= transform.Find("WavesText")?.GetComponent<TextMeshProUGUI>();
        waveTimeText ??= transform.Find("WaveTimeText")?.GetComponent<TextMeshProUGUI>();

        if (waveTimeText != null)
            waveTimeText.gameObject.SetActive(false);

        if (waveText == null)
            return;

        waveText.textWrappingMode = TextWrappingModes.NoWrap;
        waveText.overflowMode = TextOverflowModes.Overflow;
        waveText.enableAutoSizing = false;
        waveText.fontSize = 30f;
        waveText.fontStyle = FontStyles.Bold;
        waveText.alignment = TextAlignmentOptions.MidlineLeft;

        RectTransform rect = waveText.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(16f, -12f);
        rect.sizeDelta = new Vector2(260f, 40f);

        UiFonts.ApplyNexon(waveText);
    }

}


