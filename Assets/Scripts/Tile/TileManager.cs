using FSM;
using GGMLib.AnimatorSystem;
using Player;
using System;
using System.Collections;
using System.Collections.Generic;
using Tower;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class TileRow
{
    public Tile[] row;
}

public class TileManager : MonoBehaviour
{
    [SerializeField] private Transform gridParent;
    [field: SerializeField] public TileRow[] TowerTile { get; private set; }
    [SerializeField] private AllPlayerListSO AlltowerSO;
    public AllPlayerListSO AllTowers => AlltowerSO;
    [SerializeField] private InputSO inputSO;
    [SerializeField] private LayerMask tileLayer;
    [SerializeField] private float tileSelectionRadius = 1.1f;
    [SerializeField] private float dropTileRadius = 2.2f;

    [SerializeField] private EconomyConfigSO economyConfig;

    private readonly List<Tile> occupiedTowerTile = new();
    private AbstractPlayer _inspectedTower;
    private bool _isMergeDragActive;

    public event Action<AbstractPlayer> TowerSelected;
    public event Action TowerDeselected;
    public event Action<int> OccupiedCountChanged;

    public int OccupiedCount => occupiedTowerTile.Count;

    public bool HasEmptyTile
    {
        get
        {
            if (TowerTile == null)
                return false;

            for (int y = 0; y < TowerTile.Length; y++)
            {
                if (TowerTile[y]?.row == null)
                    continue;

                for (int x = 0; x < TowerTile[y].row.Length; x++)
                {
                    Tile tile = TowerTile[y].row[x];
                    if (tile != null && tile.IsEmpty)
                        return true;
                }
            }

            return false;
        }
    }

    public bool IsAtCapacity
    {
        get
        {
            EconomyConfigSO config = economyConfig ?? EconomyManager.Instance?.Config;
            int max = config != null ? config.maxUnitCapacity : int.MaxValue;
            return OccupiedCount >= max;
        }
    }

    private void OnEnable()
    {
        inputSO.TileRightClick += HandleRightClick;
    }

    private void OnDisable()
    {
        inputSO.TileRightClick -= HandleRightClick;
    }

    private void Awake()
    {
        if (gridParent != null)
        {
            int rowCount = gridParent.childCount;

            TowerTile = new TileRow[rowCount];

            for (int y = 0; y < rowCount; y++)
            {
                Transform rowTransform = gridParent.GetChild(y);

                TowerTile[y] = new TileRow();

                TowerTile[y].row =
                    rowTransform.GetComponentsInChildren<Tile>();
            }
        }
    }

    private void Start()
    {
        RefreshTowerIdentityVisuals();
    }

    private void RefreshTowerIdentityVisuals()
    {
        if (TowerTile == null)
            return;

        for (int y = 0; y < TowerTile.Length; y++)
        {
            if (TowerTile[y]?.row == null)
                continue;

            for (int x = 0; x < TowerTile[y].row.Length; x++)
            {
                Tile tile = TowerTile[y].row[x];
                if (tile == null || tile.IsEmpty)
                    continue;

                tile.CurrentOccupant?.EnableGradeOutline();
            }
        }
    }

    public void SetMergeDragActive(bool active) => _isMergeDragActive = active;

    public void ClearInspectionExternal() => ClearInspection();

    public bool TryGetTowerUnderPointer(out AbstractPlayer tower, out Tile tile)
    {
        tower = null;
        tile = null;

        if (!TryGetTileUnderPointer(out Tile hitTile) || hitTile.IsEmpty)
            return false;

        tower = hitTile.CurrentOccupant;
        tile = hitTile;
        return tower != null;
    }

    public Tile FindTileOfTower(AbstractPlayer tower)
    {
        if (TowerTile == null || tower == null)
            return null;

        for (int y = 0; y < TowerTile.Length; y++)
        {
            if (TowerTile[y]?.row == null)
                continue;

            for (int x = 0; x < TowerTile[y].row.Length; x++)
            {
                Tile tile = TowerTile[y].row[x];
                if (tile != null && tile.CurrentOccupant == tower)
                    return tile;
            }
        }

        return null;
    }

    public IEnumerable<AbstractPlayer> GetOccupiedTowers()
    {
        for (int i = 0; i < occupiedTowerTile.Count; i++)
        {
            Tile tile = occupiedTowerTile[i];
            if (tile != null && !tile.IsEmpty)
                yield return tile.CurrentOccupant;
        }
    }

    public void RemoveTowerImmediate(AbstractPlayer tower, float removeDuration = 0.5f)
    {
        if (tower == null)
            return;

        Tile tile = FindTileOfTower(tower);
        if (tile == null)
            return;

        if (_inspectedTower == tower)
            ClearInspection();

        tower.ChangeState(PlayerState.REMOVE, removeDuration);
        tile.Vacant();
        occupiedTowerTile.Remove(tile);
        NotifyOccupiedCountChanged();
        GameAudioManager.Instance?.PlaySfx(GameAudioId.TowerRemove);
    }

    public void ClearAllTowersImmediate()
    {
        ClearInspection();

        List<AbstractPlayer> towers = new();
        foreach (AbstractPlayer tower in GetOccupiedTowers())
            towers.Add(tower);

        foreach (AbstractPlayer tower in towers)
        {
            if (tower == null)
                continue;

            Tile tile = FindTileOfTower(tower);
            if (tile != null)
            {
                tile.Vacant();
                occupiedTowerTile.Remove(tile);
            }

            UnityEngine.Object.Destroy(tower.gameObject);
        }

        NotifyOccupiedCountChanged();
    }

    public bool SpawnTowerOnTile(Tile tile, AbstractPlayer prefab)
    {
        if (tile == null || prefab == null || !tile.IsEmpty)
            return false;

        Vector3 spawnPosition = new Vector3(
            tile.transform.position.x,
            prefab.transform.position.y,
            tile.transform.position.z);

        AbstractPlayer instance = Instantiate(prefab, spawnPosition, Quaternion.identity);
        tile.Occupy(instance);

        if (!occupiedTowerTile.Contains(tile))
            occupiedTowerTile.Add(tile);

        NotifyOccupiedCountChanged();
        instance.EnableGradeOutline();

        if (instance.Grade >= TowerGrade.Epic)
        {
            TowerGradeRevealUi.Instance?.Show(instance.Grade, instance.Archetype);
        }
        else
            GameAudioManager.Instance?.PlaySfx(GameAudioId.TowerPlace);

        return true;
    }

    public bool TryGetDropTileAtWorldPosition(Vector3 worldPosition, out Tile tile)
    {
        return TryFindNearestTileAtPosition(worldPosition, out tile, occupiedOnly: false, dropTileRadius);
    }

    public bool TryMoveTowerToTile(AbstractPlayer tower, Tile fromTile, Tile toTile)
    {
        if (tower == null || fromTile == null || toTile == null || fromTile == toTile || !toTile.IsEmpty)
            return false;

        fromTile.Vacant();
        occupiedTowerTile.Remove(fromTile);

        Vector3 destination = new Vector3(
            toTile.transform.position.x,
            tower.PlacementGroundY,
            toTile.transform.position.z);

        toTile.Occupy(tower);
        tower.PlaceAfterHold(destination);

        if (!occupiedTowerTile.Contains(toTile))
            occupiedTowerTile.Add(toTile);

        NotifyOccupiedCountChanged();
        GameAudioManager.Instance?.PlaySfx(GameAudioId.TowerPlace);
        return true;
    }

    private void HandleRightClick()
    {
        if (_isMergeDragActive)
            return;

        if (!TryGetTileUnderPointerDirect(out Tile tile))
        {
            ClearInspection();
            return;
        }

        if (tile.IsEmpty)
        {
            ClearInspection();
            return;
        }

        AbstractPlayer tower = tile.CurrentOccupant;
        if (_inspectedTower == tower)
        {
            ClearInspection();
            return;
        }

        _inspectedTower = tower;
        TowerSelected?.Invoke(tower);
    }

    private bool TryGetTileUnderPointer(out Tile tile)
    {
        if (TryResolveTileFromRaycast(out tile))
            return true;

        return TryFindNearestTileUnderPointer(out tile, occupiedOnly: true);
    }

    private bool TryGetTileUnderPointerDirect(out Tile tile)
    {
        return TryResolveTileFromRaycast(out tile);
    }

    private bool TryResolveTileFromRaycast(out Tile tile)
    {
        tile = null;

        if (Camera.main == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(inputSO.ScreenMousePosition);
        int selectionMask = tileLayer.value | (1 << 0);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, selectionMask);
        if (hits.Length == 0)
            return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            AbstractPlayer tower = hit.collider.GetComponentInParent<AbstractPlayer>();
            if (tower != null)
            {
                tile = FindTileOfTower(tower);
                if (tile != null)
                    return true;
            }

            Tile hitTile = hit.collider.GetComponentInParent<Tile>();
            if (hitTile == null)
                hitTile = hit.collider.GetComponent<Tile>();

            if (hitTile != null)
            {
                tile = hitTile;
                return true;
            }
        }

        return false;
    }

    private bool TryFindNearestTileUnderPointer(out Tile tile, bool occupiedOnly)
    {
        tile = null;

        if (Camera.main == null || TowerTile == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(inputSO.ScreenMousePosition);
        float groundY = TowerTile[0]?.row?[0] != null
            ? TowerTile[0].row[0].transform.position.y
            : 0f;
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));

        if (!groundPlane.Raycast(ray, out float enter))
            return false;

        Vector3 pointerOnGround = ray.GetPoint(enter);
        return TryFindNearestTileAtPosition(pointerOnGround, out tile, occupiedOnly, tileSelectionRadius);
    }

    private bool TryFindNearestTileAtPosition(
        Vector3 worldPosition,
        out Tile tile,
        bool occupiedOnly,
        float searchRadius)
    {
        tile = null;

        if (TowerTile == null)
            return false;

        float groundY = TowerTile[0]?.row?[0] != null
            ? TowerTile[0].row[0].transform.position.y
            : worldPosition.y;

        Vector3 sampleOnGround = worldPosition;
        sampleOnGround.y = groundY;

        float bestDistanceSq = searchRadius * searchRadius;
        Tile bestTile = null;

        for (int y = 0; y < TowerTile.Length; y++)
        {
            if (TowerTile[y]?.row == null)
                continue;

            for (int x = 0; x < TowerTile[y].row.Length; x++)
            {
                Tile candidate = TowerTile[y].row[x];
                if (candidate == null)
                    continue;

                if (occupiedOnly && candidate.IsEmpty)
                    continue;

                Vector3 tileCenter = candidate.transform.position;
                tileCenter.y = groundY;

                float distanceSq = (sampleOnGround - tileCenter).sqrMagnitude;
                if (distanceSq > bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                bestTile = candidate;
            }
        }

        if (bestTile == null)
            return false;

        tile = bestTile;
        return true;
    }

    public SummonResult TrySummonTower()
    {
        if (IsGameOver())
            return SummonResult.AtCapacity;

        EconomyManager economy = EconomyManager.Instance;
        EconomyConfigSO config = economyConfig ?? economy?.Config;

        if (IsAtCapacity)
            return SummonResult.AtCapacity;

        if (economy == null || config == null)
        {
            return SpawnPlayerOnRandomTile() ? SummonResult.Success : SummonResult.NoSpace;
        }

        int summonCost = economy.GetSummonCost();

        if (!economy.CanAfford(summonCost))
            return SummonResult.NotEnoughGold;

        if (!economy.TrySpend(summonCost))
            return SummonResult.NotEnoughGold;

        if (!SpawnPlayerOnRandomTile())
        {
            economy.AddGold(summonCost);
            return SummonResult.NoSpace;
        }

        economy.RegisterSummon();
        return SummonResult.Success;
    }

    public bool SpawnPlayerOnRandomTile()
    {
        List<Tile> emptyTiles = CollectEmptyTiles();
        if (emptyTiles.Count == 0)
        {
            if (TowerTile == null || TowerTile.Length == 0)
                Debug.LogError("TowerTile이 비어있습니다.");

            return false;
        }

        int randomIndex = Random.Range(0, emptyTiles.Count);
        return TrySpawnTowerOnTile(emptyTiles[randomIndex]);
    }

    public int SpawnTitlePreviewTowers(int count, Vector3 focusPoint)
    {
        if (count <= 0)
            return 0;

        List<Tile> emptyTiles = CollectEmptyTiles();
        if (emptyTiles.Count == 0)
            return 0;

        emptyTiles.Sort((a, b) =>
        {
            float aDistance = (a.transform.position - focusPoint).sqrMagnitude;
            float bDistance = (b.transform.position - focusPoint).sqrMagnitude;
            return aDistance.CompareTo(bDistance);
        });

        int spawned = 0;
        int step = Mathf.Max(1, emptyTiles.Count / count);

        for (int i = 0; i < emptyTiles.Count && spawned < count; i += step)
        {
            if (!TrySpawnTowerOnTile(emptyTiles[i]))
                continue;

            spawned++;
        }

        return spawned;
    }

    private List<Tile> CollectEmptyTiles()
    {
        List<Tile> emptyTiles = new();

        if (TowerTile == null)
            return emptyTiles;

        for (int y = 0; y < TowerTile.Length; y++)
        {
            if (TowerTile[y]?.row == null)
                continue;

            for (int x = 0; x < TowerTile[y].row.Length; x++)
            {
                Tile tile = TowerTile[y].row[x];
                if (tile != null && tile.IsEmpty)
                    emptyTiles.Add(tile);
            }
        }

        return emptyTiles;
    }

    private bool TrySpawnTowerOnTile(Tile targetTile)
    {
        if (targetTile == null || !targetTile.IsEmpty)
            return false;

        GradeList selectedGrade = GetRandomGrade();
        if (selectedGrade.tower == null || selectedGrade.tower.Count == 0)
            return false;

        AbstractPlayer playerPrefab =
            selectedGrade.tower[Random.Range(0, selectedGrade.tower.Count)];

        return SpawnTowerOnTile(targetTile, playerPrefab);
    }

    public bool TrySellTower(AbstractPlayer tower)
    {
        if (tower == null)
            return false;

        Tile tile = FindTileOfTower(tower);
        if (tile == null)
            return false;

        EconomyManager economy = EconomyManager.Instance;
        EconomyConfigSO config = economyConfig ?? economy?.Config;
        int refund = config != null ? config.GetSellRefund(tower.Grade) : 0;

        if (_inspectedTower == tower)
            ClearInspection();

        tower.ChangeState(PlayerState.REMOVE, 1.5f);
        tile.Vacant();
        occupiedTowerTile.Remove(tile);
        NotifyOccupiedCountChanged();

        economy?.AddGold(refund);
        return true;
    }

    private void ClearInspection()
    {
        if (_inspectedTower == null)
            return;

        _inspectedTower = null;
        TowerDeselected?.Invoke();
    }

    private GradeList GetRandomGrade()
    {
        int currentWave = FindFirstObjectByType<WaveManager>()?.CurrentWave ?? 0;
        EconomyConfigSO config = economyConfig ?? EconomyManager.Instance?.Config;
        int summonUpgradeLevel = ArchetypeUpgradeManager.Instance?.GetSummonUpgradeLevel() ?? 0;

        return SummonGradeOdds.PickRandomGrade(AlltowerSO, currentWave, config, summonUpgradeLevel)
            ?? GetFallbackGradeList(TowerGrade.Normal);
    }

    private GradeList GetFallbackGradeList(TowerGrade grade)
    {
        foreach (GradeList candidate in AlltowerSO.towerList)
        {
            if (TryGetGrade(candidate, out TowerGrade towerGrade) && towerGrade == grade)
                return candidate;
        }

        return AlltowerSO.towerList[0];
    }

    private static bool TryGetGrade(GradeList gradeList, out TowerGrade grade)
    {
        grade = TowerGrade.Normal;
        if (gradeList == null || string.IsNullOrWhiteSpace(gradeList.gradeName))
            return false;

        return Enum.TryParse(gradeList.gradeName, true, out grade);
    }

    public void RemovePlayerFromTile(int x, int y)
    {
        if (y >= TowerTile.Length ||
            x >= TowerTile[y].row.Length)
            return;

        Tile targetTile = TowerTile[y].row[x];

        if (targetTile != null && !targetTile.IsEmpty)
        {
            if (_inspectedTower == targetTile.CurrentOccupant)
                ClearInspection();

            targetTile.CurrentOccupant.ChangeState(PlayerState.REMOVE, 1.5f);
            targetTile.Vacant();
            occupiedTowerTile.Remove(targetTile);
            NotifyOccupiedCountChanged();
        }
    }

    private void NotifyOccupiedCountChanged()
    {
        OccupiedCountChanged?.Invoke(OccupiedCount);
    }

    private static bool IsGameOver()
    {
        return (FieldEnemyTracker.Instance != null && FieldEnemyTracker.Instance.IsGameOver) ||
               (LeakTracker.Instance != null && LeakTracker.Instance.IsGameOver);
    }
}

public enum SummonResult
{
    Success,
    AtCapacity,
    NotEnoughGold,
    NoSpace
}
