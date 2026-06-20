using TMPro;
using UnityEngine;

public class UnitCapacityUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI capacityText;

    public void SetCount(int current, int max)
    {
        if (capacityText != null)
            capacityText.text = $"{current} / {max}";
    }

    public void SetCountOnly(int current)
    {
        if (capacityText != null)
            capacityText.text = $"/ {current}";
    }
}
