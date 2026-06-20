using TMPro;
using UnityEngine;

public class MoneyDisplayUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;

    public void SetGold(int gold)
    {
        if (goldText != null)
            goldText.text = gold.ToString("N0");
    }
}
