using UnityEngine;
using UnityEngine.UI;

public class ArchetypeUpgradeButtonUi : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private ArchetypeUpgradePanelUi upgradePanel;

    private void Awake()
    {
        button ??= GetComponent<Button>();
        upgradePanel ??= FindFirstObjectByType<ArchetypeUpgradePanelUi>(FindObjectsInactive.Include);

        if (button != null)
            button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }

    private void HandleClick()
    {
        upgradePanel ??= FindFirstObjectByType<ArchetypeUpgradePanelUi>(FindObjectsInactive.Include);
        if (upgradePanel == null)
            return;

        upgradePanel.Toggle();
    }
}
