using TMPro;
using Tower;
using UnityEngine;
using UnityEngine.UI;

public class ArchetypeUpgradePanelUi : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private ArchetypeUpgradeRowUi bowRow;
    [SerializeField] private ArchetypeUpgradeRowUi culverinRow;
    [SerializeField] private ArchetypeUpgradeRowUi missileRow;
    [SerializeField] private ArchetypeUpgradeRowUi mergeNormalRow;
    [SerializeField] private ArchetypeUpgradeRowUi mergeRareRow;
    [SerializeField] private ArchetypeUpgradeRowUi mergeEpicRow;
    [SerializeField] private ArchetypeUpgradeRowUi summonUpgradeRow;
    [SerializeField] private ArchetypeUpgradeRowUi mergeUnlockRareRow;
    [SerializeField] private ArchetypeUpgradeRowUi mergeUnlockEpicRow;

    private ArchetypeUpgradeManager _manager;
    private CanvasGroup _canvasGroup;
    private bool _initialized;

    public bool IsVisible => EnsureInitialized() && _canvasGroup.alpha > 0.01f;

    private void Awake()
    {
        EnsureInitialized();
        Hide();
    }

    public void PrepareHidden()
    {
        EnsureInitialized();
        Hide();
    }

    private bool EnsureInitialized()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        if (!panelRoot.activeSelf)
            panelRoot.SetActive(true);

        if (_canvasGroup == null)
        {
            _canvasGroup = panelRoot.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = panelRoot.AddComponent<CanvasGroup>();
        }

        EnsureRowReferences();
        WireRowButtons();
        UpdatePanelTitle();

        if (!_initialized)
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            _initialized = true;
        }

        return _canvasGroup != null;
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Hide);

        UnbindManager();
    }

    private void OnEnable() => BindManager();

    private void OnDisable() => UnbindManager();

    public void Toggle()
    {
        EnsureInitialized();
        if (IsVisible)
            Hide();
        else
            Show();
    }

    public void Show()
    {
        EnsureInitialized();
        EnsureRowReferences();
        BindManager();

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        SetBackgroundRaycastTarget(true);
        GameHudLayoutBootstrap.RepairUpgradePanelLayout(this);
        WireRowButtons();
        UpdatePanelTitle();
        Refresh();
    }

    public void Hide()
    {
        if (_canvasGroup == null)
            return;

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        SetBackgroundRaycastTarget(false);
    }

    private void SetBackgroundRaycastTarget(bool enabled)
    {
        if (panelRoot == null)
            return;

        Image background = panelRoot.GetComponent<Image>();
        if (background != null)
            background.raycastTarget = enabled;
    }

    public void Refresh()
    {
        BindManager();
        if (_manager == null)
            return;

        bowRow?.RefreshArchetype(TowerArchetype.Bow, _manager);
        culverinRow?.RefreshArchetype(TowerArchetype.Culverin, _manager);
        missileRow?.RefreshArchetype(TowerArchetype.Missile, _manager);
        mergeNormalRow?.RefreshMerge(TowerGrade.Normal, _manager);
        mergeRareRow?.RefreshMerge(TowerGrade.Rare, _manager);
        mergeEpicRow?.RefreshMerge(TowerGrade.Epic, _manager);
        summonUpgradeRow?.RefreshSummon(_manager);
        HideMergeUnlockRows();
    }

    private void WireRowButtons()
    {
        WireArchetypeRow(bowRow, "BowRow", TowerArchetype.Bow);
        WireArchetypeRow(culverinRow, "CulverinRow", TowerArchetype.Culverin);
        WireArchetypeRow(missileRow, "MissileRow", TowerArchetype.Missile);
        WireMergeRow(mergeNormalRow, "MergeNormalRow", TowerGrade.Normal);
        WireMergeRow(mergeRareRow, "MergeRareRow", TowerGrade.Rare);
        WireMergeRow(mergeEpicRow, "MergeEpicRow", TowerGrade.Epic);
        WireSummonUpgradeRow();
        HideMergeUnlockRows();
    }

    private void WireArchetypeRow(ArchetypeUpgradeRowUi row, string rowName, TowerArchetype archetype)
    {
        row ??= EnsureRowComponent(rowName);
        if (row == null)
            return;

        row.SetupArchetype(archetype, GetArchetypeLabel(archetype));
        row.BindUpgradeClick(() => HandleArchetypeUpgrade(archetype));
    }

    private void WireMergeRow(ArchetypeUpgradeRowUi row, string rowName, TowerGrade fromGrade)
    {
        row ??= EnsureRowComponent(rowName);
        if (row == null)
            return;

        row.SetupMerge(fromGrade, GetMergeLabel(fromGrade));
        row.BindUpgradeClick(() => HandleMergeUpgrade(fromGrade));
    }

    private static string GetArchetypeLabel(TowerArchetype archetype) => archetype switch
    {
        TowerArchetype.Culverin => "대포",
        TowerArchetype.Missile => "미사일",
        _ => "석궁"
    };

    private static string GetMergeLabel(TowerGrade fromGrade) =>
        TowerGradeLabels.GetMergeChanceUpgradeLabel(fromGrade);

    private void HandleSummonUpgrade()
    {
        BindManager();
        if (_manager == null)
        {
            WarningMessageUi.Instance?.Show("소환 확률 강화를 사용할 수 없습니다.");
            return;
        }

        if (_manager.TryUpgradeSummon())
        {
            Refresh();
            return;
        }

        if (!_manager.CanUpgradeSummon())
            WarningMessageUi.Instance?.Show("최대 레벨입니다.");
        else
            WarningMessageUi.Instance?.Show("코인이 부족합니다!");
    }

    private void WireSummonUpgradeRow()
    {
        summonUpgradeRow ??= EnsureRowComponent("SummonUpgradeRow") ?? CloneRow("BowRow", "SummonUpgradeRow");
        if (summonUpgradeRow == null)
            return;

        summonUpgradeRow.SetupSummon("소환 확률");
        summonUpgradeRow.BindUpgradeClick(HandleSummonUpgrade);
    }

    private void HideMergeUnlockRows()
    {
        if (mergeUnlockRareRow != null)
            mergeUnlockRareRow.gameObject.SetActive(false);

        if (mergeUnlockEpicRow != null)
            mergeUnlockEpicRow.gameObject.SetActive(false);

        Transform rareUnlock = transform.Find("MergeUnlockRareRow");
        if (rareUnlock != null)
            rareUnlock.gameObject.SetActive(false);

        Transform epicUnlock = transform.Find("MergeUnlockEpicRow");
        if (epicUnlock != null)
            epicUnlock.gameObject.SetActive(false);
    }

    private void HandleArchetypeUpgrade(TowerArchetype archetype)
    {
        BindManager();
        if (_manager == null)
        {
            WarningMessageUi.Instance?.Show("강화 시스템을 사용할 수 없습니다.");
            return;
        }

        if (_manager.TryUpgradeArchetype(archetype))
        {
            Refresh();
            return;
        }

        if (!_manager.CanUpgradeArchetype(archetype))
            WarningMessageUi.Instance?.Show("최대 레벨입니다.");
        else
            WarningMessageUi.Instance?.Show("코인이 부족합니다!");
    }

    private void HandleMergeUpgrade(TowerGrade fromGrade)
    {
        BindManager();
        if (_manager == null)
        {
            WarningMessageUi.Instance?.Show("합성 확률 강화를 사용할 수 없습니다.");
            return;
        }

        if (_manager.TryUpgradeMerge(fromGrade))
        {
            Refresh();
            return;
        }

        if (!_manager.CanUpgradeMerge(fromGrade))
            WarningMessageUi.Instance?.Show("최대 레벨입니다.");
        else
            WarningMessageUi.Instance?.Show("코인이 부족합니다!");
    }

    private void EnsureRowReferences()
    {
        bowRow ??= EnsureRowComponent("BowRow");
        culverinRow ??= EnsureRowComponent("CulverinRow");
        missileRow ??= EnsureRowComponent("MissileRow");
        mergeNormalRow ??= EnsureRowComponent("MergeNormalRow") ?? CloneRow("BowRow", "MergeNormalRow");
        mergeRareRow ??= EnsureRowComponent("MergeRareRow") ?? CloneRow("BowRow", "MergeRareRow");
        mergeEpicRow ??= EnsureRowComponent("MergeEpicRow") ?? CloneRow("BowRow", "MergeEpicRow");
        summonUpgradeRow ??= EnsureRowComponent("SummonUpgradeRow") ?? CloneRow("BowRow", "SummonUpgradeRow");
        mergeUnlockRareRow ??= EnsureRowComponent("MergeUnlockRareRow");
        mergeUnlockEpicRow ??= EnsureRowComponent("MergeUnlockEpicRow");
        HideMergeUnlockRows();
    }

    private ArchetypeUpgradeRowUi CloneRow(string templateName, string newName)
    {
        Transform existing = transform.Find(newName);
        if (existing != null)
            return existing.GetComponent<ArchetypeUpgradeRowUi>();

        Transform template = transform.Find(templateName);
        if (template == null)
            return null;

        GameObject clone = Instantiate(template.gameObject, transform);
        clone.name = newName;
        clone.SetActive(true);
        return clone.GetComponent<ArchetypeUpgradeRowUi>();
    }

    private ArchetypeUpgradeRowUi EnsureRowComponent(string rowName)
    {
        Transform rowTransform = transform.Find(rowName);
        if (rowTransform == null)
            return null;

        ArchetypeUpgradeRowUi row = rowTransform.GetComponent<ArchetypeUpgradeRowUi>();
        if (row == null)
            row = rowTransform.gameObject.AddComponent<ArchetypeUpgradeRowUi>();

        rowTransform.gameObject.SetActive(true);
        return row;
    }

    private void UpdatePanelTitle()
    {
        Transform title = transform.Find("ArchetypeUpgradeTitle");
        if (title != null && title.TryGetComponent(out TextMeshProUGUI tmp))
            tmp.text = "강화";
    }

    private void BindManager()
    {
        ArchetypeUpgradeManager instance = ArchetypeUpgradeManager.Instance;
        if (instance == null)
            return;

        if (_manager == instance)
            return;

        UnbindManager();
        _manager = instance;
        _manager.Changed += Refresh;
    }

    private void UnbindManager()
    {
        if (_manager != null)
            _manager.Changed -= Refresh;

        _manager = null;
    }
}

public class ArchetypeUpgradeRowUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI bonusText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonLabel;

    private bool _clickBound;

    private static readonly Color CostColor = new(1f, 0.84f, 0.2f, 1f);

    private void Awake() => EnsureUiReferences();

    private void EnsureUiReferences()
    {
        titleText ??= transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
        levelText ??= transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
        bonusText ??= transform.Find("BonusText")?.GetComponent<TextMeshProUGUI>();
        costText ??= transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
        upgradeButton ??= transform.Find("UpgradeButton")?.GetComponent<Button>();
        upgradeButtonLabel ??= upgradeButton?.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void BindUpgradeClick(UnityEngine.Events.UnityAction onClick)
    {
        EnsureUiReferences();
        if (upgradeButton == null || onClick == null)
            return;

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(onClick);
        _clickBound = true;
    }

    private void OnDestroy()
    {
        if (upgradeButton != null && _clickBound)
            upgradeButton.onClick.RemoveAllListeners();
    }

    public void SetupArchetype(TowerArchetype archetype, string label)
    {
        if (titleText != null)
            titleText.text = label;
    }

    public void SetupMerge(TowerGrade fromGrade, string label)
    {
        if (titleText != null)
            titleText.text = label;
    }

    public void SetupSummon(string label)
    {
        if (titleText != null)
            titleText.text = label;
    }

    public void RefreshArchetype(TowerArchetype archetype, ArchetypeUpgradeManager manager)
    {
        EnsureUiReferences();
        if (manager == null)
            return;

        int level = manager.GetArchetypeLevel(archetype);
        int cost = manager.GetArchetypeUpgradeCost(archetype);
        bool canUpgrade = manager.CanUpgradeArchetype(archetype);
        bool canAfford = EconomyManager.Instance != null && EconomyManager.Instance.CanAfford(cost);

        if (levelText != null)
            levelText.text = $"Lv.{level}";

        if (bonusText != null)
            bonusText.text = $"공격 +{manager.GetBonusAttack(archetype):0.#}";

        ApplyCostAndButton(cost, canUpgrade, canAfford);
    }

    public void RefreshMerge(TowerGrade fromGrade, ArchetypeUpgradeManager manager)
    {
        EnsureUiReferences();
        if (manager == null)
            return;

        int level = manager.GetMergeUpgradeLevel(fromGrade);
        int chance = manager.GetEffectiveMergeChancePercent(fromGrade);
        int cost = manager.GetMergeUpgradeCost(fromGrade);
        bool canUpgrade = manager.CanUpgradeMerge(fromGrade);
        bool canAfford = EconomyManager.Instance != null && EconomyManager.Instance.CanAfford(cost);

        if (levelText != null)
            levelText.text = $"Lv.{level}";

        if (bonusText != null)
            bonusText.text = $"확률 {chance}%";

        ApplyCostAndButton(cost, canUpgrade, canAfford);
    }

    public void RefreshSummon(ArchetypeUpgradeManager manager)
    {
        EnsureUiReferences();
        if (manager == null)
            return;

        int level = manager.GetSummonUpgradeLevel();
        int cost = manager.GetSummonUpgradeCost();
        bool canUpgrade = manager.CanUpgradeSummon();
        bool canAfford = EconomyManager.Instance != null && EconomyManager.Instance.CanAfford(cost);

        if (levelText != null)
            levelText.text = $"Lv.{level}";

        if (bonusText != null)
            bonusText.text = "고급 등급 ↑";

        ApplyCostAndButton(cost, canUpgrade, canAfford);
    }

    private void ApplyCostAndButton(int cost, bool canUpgrade, bool canAfford)
    {
        if (costText != null)
        {
            costText.gameObject.SetActive(true);
            costText.color = CostColor;
            costText.text = canUpgrade ? $"{cost}" : "MAX";
        }

        if (upgradeButtonLabel != null)
            upgradeButtonLabel.text = canUpgrade ? "강화" : "최대";

        if (upgradeButton != null)
            upgradeButton.interactable = canUpgrade && canAfford;
    }
}
