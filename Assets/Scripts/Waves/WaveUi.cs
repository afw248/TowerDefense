using TMPro;
using UnityEngine;

public class WaveUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI waveText; 
    public void SetWaves(int wave)
    {
        waveText.text ="Wave: "+wave.ToString();
    }
}
