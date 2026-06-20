using UnityEngine;
using DG;
using DG.Tweening;
using UnityEngine.InputSystem;
using TMPro;
public class PopUpWave : MonoBehaviour
{
    private RectTransform rect;
    private TextMeshProUGUI wavetext;
    private void OnEnable()
    {
        rect = GetComponent<RectTransform>();
        wavetext = GetComponentInChildren<TextMeshProUGUI>();
        rect.localScale = new Vector3(1,0,1);
    }
    public void Popup(int wave, bool isBossWave = false)
    {
        rect ??= GetComponent<RectTransform>();
        wavetext ??= GetComponentInChildren<TextMeshProUGUI>();
        if (rect == null || wavetext == null)
            return;

        rect.transform.DOKill();
        Sequence mySequence = DOTween.Sequence();

        wavetext.text = isBossWave ? $"보스 {wave}" : $"웨이브 {wave}";

        mySequence.Append(rect.transform.DOScale(Vector3.one, 0.2f)).SetEase(Ease.OutQuad);
        mySequence.AppendInterval(0.5f);
        mySequence.Append(rect.transform.DOScale(new Vector3(1,0,1), 0.2f)).SetEase(Ease.OutQuad);

    }
}
