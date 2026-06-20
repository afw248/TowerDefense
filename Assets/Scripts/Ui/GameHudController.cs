using Player;

using UnityEngine;



public class GameHudController : MonoBehaviour

{

    [SerializeField] private EconomyManager economy;

    [SerializeField] private TileManager tileManager;

    [SerializeField] private FieldEnemyTracker fieldEnemyTracker;

    [SerializeField] private GameSpeedController speedController;

    [SerializeField] private ArchetypeUpgradeManager archetypeUpgradeManager;



    [SerializeField] private MoneyDisplayUi moneyDisplay;

    [SerializeField] private SummonButtonUi summonButton;

    [SerializeField] private UnitCapacityUi unitCapacity;

    [SerializeField] private FieldEnemyCountUi fieldEnemyCount;

    [SerializeField] private SpeedControlUi speedControl;

    [SerializeField] private TowerInfoPanelUi towerInfoPanel;

    [SerializeField] private ArchetypeUpgradePanelUi archetypeUpgradePanel;



    private void Awake()

    {

        economy ??= EconomyManager.Instance;

        fieldEnemyTracker ??= FieldEnemyTracker.Instance;

        speedController ??= GameSpeedController.Instance;

        archetypeUpgradeManager ??= ArchetypeUpgradeManager.Instance;

        tileManager ??= FindFirstObjectByType<TileManager>();



        if (GetComponent<GameHudLayoutBootstrap>() == null)

            gameObject.AddComponent<GameHudLayoutBootstrap>();

    }



    private void OnEnable()

    {

        if (economy != null)

            economy.OnGoldChanged += HandleGoldChanged;



        if (tileManager != null)

        {

            tileManager.TowerSelected += HandleTowerSelected;

            tileManager.TowerDeselected += HandleTowerDeselected;

            tileManager.OccupiedCountChanged += HandleOccupiedCountChanged;

        }



        if (fieldEnemyTracker != null)

            fieldEnemyTracker.OnCountChanged += HandleFieldEnemyChanged;



        if (archetypeUpgradeManager != null)

            archetypeUpgradeManager.Changed += HandleArchetypeUpgradeChanged;



        if (speedController != null)

            speedControl?.Bind(speedController);

        if (!TitlePreviewMode.Active)
        {
            GetComponent<GameHudLayoutBootstrap>()?.EnsureReady();
            GameHudLayoutBootstrap.ApplyFinalPresentation();
        }

    }



    private void OnDisable()

    {

        if (economy != null)

            economy.OnGoldChanged -= HandleGoldChanged;



        if (tileManager != null)

        {

            tileManager.TowerSelected -= HandleTowerSelected;

            tileManager.TowerDeselected -= HandleTowerDeselected;

            tileManager.OccupiedCountChanged -= HandleOccupiedCountChanged;

        }



        if (fieldEnemyTracker != null)

            fieldEnemyTracker.OnCountChanged -= HandleFieldEnemyChanged;



        if (archetypeUpgradeManager != null)

            archetypeUpgradeManager.Changed -= HandleArchetypeUpgradeChanged;

    }



    private void Start()

    {

        RefreshAll();

    }



    public void RefreshAll()

    {

        HandleGoldChanged(economy != null ? economy.Gold : 0);

        HandleOccupiedCountChanged(tileManager != null ? tileManager.OccupiedCount : 0);

        HandleFieldEnemyChanged(

            fieldEnemyTracker != null ? fieldEnemyTracker.AliveCount : 0,

            fieldEnemyTracker != null ? fieldEnemyTracker.MaxCount : 80);



        summonButton?.Refresh();

        towerInfoPanel?.Hide();

        TowerInspectRangeIndicator.Instance.Hide();

        archetypeUpgradePanel?.Refresh();

    }



    private void HandleGoldChanged(int gold)

    {

        moneyDisplay?.SetGold(gold);

        summonButton?.Refresh();

        archetypeUpgradePanel?.Refresh();

        towerInfoPanel?.Refresh();

    }



    private void HandleOccupiedCountChanged(int count)

    {

        EconomyConfigSO config = economy != null ? economy.Config : null;

        int max = config != null ? config.maxUnitCapacity : 28;

        unitCapacity?.SetCount(count, max);

        summonButton?.Refresh();

    }



    private void HandleFieldEnemyChanged(int current, int max)

    {

        fieldEnemyCount?.SetCount(current, max);

    }



    private void HandleArchetypeUpgradeChanged()

    {

        archetypeUpgradePanel?.Refresh();

        towerInfoPanel?.Refresh();

        summonButton?.Refresh();

    }



    private void HandleTowerSelected(AbstractPlayer tower)

    {

        towerInfoPanel?.Show(tower);

        TowerInspectRangeIndicator.Instance.Show(tower);

    }



    private void HandleTowerDeselected()

    {

        towerInfoPanel?.Hide();

        TowerInspectRangeIndicator.Instance.Hide();

    }

}

