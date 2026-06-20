using CoreSystem.EffectSystem;
using Player;
using Tower;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(-10)]
public class TowerMergeController : MonoBehaviour
{
    private static readonly Color DragOutlineColor = new(0.2f, 1f, 1f, 1f);
    private static readonly Color ValidOutlineColor = new(0.15f, 1f, 0.45f, 1f);
    private static readonly Color InvalidOutlineColor = new(1f, 0.15f, 0.15f, 1f);

    [SerializeField] private TileManager tileManager;
    [SerializeField] private InputSO inputSO;
    [SerializeField] private AllPlayerListSO allTowerList;
    [SerializeField] private TowerMergeConfigSO mergeConfig;
    [SerializeField] private float holdLiftHeight = 1.2f;
    [SerializeField] private float mergeOverlapBoundsScale = 0.82f;

    private AbstractPlayer _draggedTower;
    private Tile _sourceTile;
    private AbstractPlayer _hoverTarget;
    private Tile _hoverEmptyTile;
    private bool _isDragging;

    public bool IsDragging => _isDragging;

    private void Awake()
    {
        tileManager ??= GetComponent<TileManager>();
        tileManager ??= FindFirstObjectByType<TileManager>();
        inputSO ??= Resources.FindObjectsOfTypeAll<InputSO>()[0];
        mergeConfig ??= Resources.Load<TowerMergeConfigSO>("TowerMergeConfig");
    }

    private void OnEnable()
    {
        if (tileManager != null)
            tileManager.SetMergeDragActive(false);
    }

    private void Update()
    {
        if (TitlePreviewMode.Active)
            return;

        if (inputSO == null || Mouse.current == null)
            return;

        inputSO.ScreenMousePosition = Mouse.current.position.ReadValue();

        bool pointerOverUi = IsPointerOverUi();

        if (!_isDragging && !pointerOverUi && Mouse.current.leftButton.wasPressedThisFrame)
            TryBeginDrag();

        if (_isDragging)
        {
            if (!pointerOverUi)
            {
                UpdateDragPosition();
                UpdateHoverTarget();
            }
        }

        if (_isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
            EndDrag();
    }

    private void TryBeginDrag()
    {
        if (tileManager == null || !tileManager.TryGetTowerUnderPointer(out AbstractPlayer tower, out Tile tile))
            return;

        tileManager.ClearInspectionExternal();
        _draggedTower = tower;
        _sourceTile = tile;
        _isDragging = true;
        tileManager.SetMergeDragActive(true);

        tower.BeginHoldForRelocation();
        tower.SetHoldFeedbackColor(DragOutlineColor);
    }

    private void UpdateDragPosition()
    {
        if (_draggedTower == null)
            return;

        Vector3 pointerPosition = GetPointerWorldPosition(_draggedTower.PlacementGroundY + holdLiftHeight);
        _draggedTower.UpdateDragPosition(pointerPosition);
    }

    private void UpdateHoverTarget()
    {
        AbstractPlayer previousTarget = _hoverTarget;
        _hoverTarget = FindOverlappingMergeCandidate();
        _hoverEmptyTile = ResolveHoverEmptyTile();

        AbstractPlayer mergeTarget = ResolvePreferredMergeTarget();

        if (mergeTarget != null && CanMerge(_draggedTower, mergeTarget))
        {
            int chance = GetEffectiveMergeChance(_draggedTower.Grade);

            TowerMergeChanceUi.Instance.Show(mergeTarget.transform, _draggedTower.transform, _draggedTower.Grade, chance);
            _draggedTower.SetHoldFeedbackColor(ValidOutlineColor);
            mergeTarget.SetHoldFeedbackColor(ValidOutlineColor);
        }
        else if (mergeTarget != null)
        {
            TowerMergeChanceUi.Instance.Hide();
            _draggedTower.SetHoldFeedbackColor(InvalidOutlineColor);
            mergeTarget.SetHoldFeedbackColor(InvalidOutlineColor);
        }
        else if (_hoverEmptyTile != null)
        {
            TowerMergeChanceUi.Instance.Hide();
            _draggedTower.SetHoldFeedbackColor(ValidOutlineColor);

            if (previousTarget != null)
                previousTarget.ResetHoldFeedbackColor();
        }
        else
        {
            TowerMergeChanceUi.Instance.Hide();
            _draggedTower.SetHoldFeedbackColor(DragOutlineColor);

            if (previousTarget != null)
                previousTarget.ResetHoldFeedbackColor();
        }

        if (previousTarget != null && previousTarget != mergeTarget)
            previousTarget.ResetHoldFeedbackColor();
    }

    private void EndDrag()
    {
        TowerMergeChanceUi.Instance.Hide();

        if (_draggedTower == null || _sourceTile == null)
        {
            ResetDragState();
            return;
        }

        AbstractPlayer mergeTarget = ResolvePreferredMergeTarget();
        bool canMerge = mergeTarget != null && CanMerge(_draggedTower, mergeTarget);

        if (canMerge)
        {
            AttemptMerge(_draggedTower, mergeTarget, _sourceTile, tileManager.FindTileOfTower(mergeTarget));
        }
        else if (ResolveDropEmptyTile(out Tile dropTile))
        {
            tileManager.TryMoveTowerToTile(_draggedTower, _sourceTile, dropTile);
        }
        else if (_hoverTarget != null)
        {
            if (IsMergePairBlockedByUnlock(_draggedTower, _hoverTarget))
                WarningMessageUi.Instance?.Show("합성 해금이 필요합니다! 강화 탭에서 해금하세요.");
            ReturnDraggedTower();
        }
        else
        {
            ReturnDraggedTower();
        }

        if (_hoverTarget != null)
            _hoverTarget.ResetHoldFeedbackColor();

        ResetDragState();
    }

    private void AttemptMerge(
        AbstractPlayer source,
        AbstractPlayer target,
        Tile sourceTile,
        Tile targetTile)
    {
        if (source == null || target == null || sourceTile == null || targetTile == null || tileManager == null)
        {
            ReturnDraggedTower();
            return;
        }

        TowerGrade grade = source.Grade;
        int chancePercent = GetEffectiveMergeChance(grade);
        bool success = Random.Range(0, 100) < chancePercent;
        Vector3 effectPosition = target.transform.position;

        if (success)
        {
            if (!TryGetUpgradePrefab(source, out AbstractPlayer upgradedPrefab))
            {
                ReturnDraggedTower();
                return;
            }

            PlayMergeVfx(grade, success: true, effectPosition);
            RequestMergeSuccessShake(grade);
            tileManager.RemoveTowerImmediate(source);
            tileManager.RemoveTowerImmediate(target);
            tileManager.SpawnTowerOnTile(targetTile, upgradedPrefab);
            return;
        }

        PlayMergeVfx(grade, success: false, effectPosition);
        tileManager.RemoveTowerImmediate(source);
        target.ResetHoldFeedbackColor();
        WarningMessageUi.Instance?.Show("합성 실패! 드래그한 타워만 사라집니다.");
    }

    private void ReturnDraggedTower()
    {
        if (_draggedTower == null || _sourceTile == null)
            return;

        Vector3 tileCenter = _sourceTile.transform.position;
        _draggedTower.ReturnDragToTile(tileCenter);
    }

    private void ResetDragState()
    {
        if (_draggedTower != null)
            _draggedTower.ResetHoldFeedbackColor();

        _draggedTower = null;
        _sourceTile = null;
        _hoverTarget = null;
        _hoverEmptyTile = null;
        _isDragging = false;
        tileManager?.SetMergeDragActive(false);
    }

    private Tile ResolveHoverEmptyTile()
    {
        return ResolveDropEmptyTile(out Tile tile) ? tile : null;
    }

    private bool ResolveDropEmptyTile(out Tile tile)
    {
        tile = null;

        if (_draggedTower == null || _sourceTile == null || tileManager == null)
            return false;

        if (!tileManager.TryGetDropTileAtWorldPosition(_draggedTower.transform.position, out Tile candidate))
            return false;

        if (candidate == null || !candidate.IsEmpty || candidate == _sourceTile)
            return false;

        tile = candidate;
        return true;
    }

    private AbstractPlayer ResolvePreferredMergeTarget()
    {
        if (_hoverTarget == null || _draggedTower == null)
            return null;

        if (_hoverEmptyTile == null)
            return _hoverTarget;

        float dropDistanceSq = HorizontalDistanceSq(_draggedTower.transform.position, _hoverEmptyTile.transform.position);
        float mergeDistanceSq = HorizontalDistanceSq(_draggedTower.transform.position, _hoverTarget.transform.position);
        return mergeDistanceSq < dropDistanceSq ? _hoverTarget : null;
    }

    private static float HorizontalDistanceSq(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return (a - b).sqrMagnitude;
    }

    private AbstractPlayer FindOverlappingMergeCandidate()
    {
        if (_draggedTower == null || tileManager == null)
            return null;

        AbstractPlayer best = null;
        float bestDistanceSq = float.MaxValue;

        foreach (AbstractPlayer candidate in tileManager.GetOccupiedTowers())
        {
            if (candidate == null || candidate == _draggedTower)
                continue;

            if (!AreTowersOverlapping(_draggedTower, candidate))
                continue;

            float distanceSq = (candidate.transform.position - _draggedTower.transform.position).sqrMagnitude;
            if (distanceSq >= bestDistanceSq)
                continue;

            bestDistanceSq = distanceSq;
            best = candidate;
        }

        return best;
    }

    private bool CanMerge(AbstractPlayer source, AbstractPlayer target)
    {
        if (source == null || target == null || source == target)
            return false;

        if (source.Grade >= TowerGrade.Legendary || target.Grade >= TowerGrade.Legendary)
            return false;

        return source.Grade == target.Grade && source.Archetype == target.Archetype;
    }

    private static bool IsMergePairBlockedByUnlock(AbstractPlayer source, AbstractPlayer target)
    {
        return false;
    }

    private static void RequestMergeSuccessShake(TowerGrade fromGrade)
    {
        float intensity = fromGrade switch
        {
            TowerGrade.Rare => 0.2f,
            TowerGrade.Epic => 0.3f,
            _ => 0.15f,
        };

        GameplayCameraShake.RequestShake(intensity);
    }

    private bool TryGetUpgradePrefab(AbstractPlayer tower, out AbstractPlayer prefab)
    {
        prefab = null;

        if (tower == null || allTowerList == null || allTowerList.towerList == null)
            return false;

        if (tower.Grade >= TowerGrade.Legendary)
            return false;

        TowerGrade nextGrade = tower.Grade + 1;
        TowerArchetype archetype = tower.Archetype;

        foreach (GradeList gradeList in allTowerList.towerList)
        {
            if (gradeList?.tower == null)
                continue;

            foreach (AbstractPlayer candidate in gradeList.tower)
            {
                if (candidate == null)
                    continue;

                if (candidate.Grade == nextGrade && candidate.Archetype == archetype)
                {
                    prefab = candidate;
                    return true;
                }
            }
        }

        return false;
    }

    private void PlayMergeVfx(TowerGrade fromGrade, bool success, Vector3 position)
    {
        if (mergeConfig == null || !mergeConfig.TryGetTier(fromGrade, out TowerMergeConfigSO.MergeTierSettings tier))
            return;

        HitEffectDataSO vfx = success ? tier.successVfx : tier.failureVfx;
        Vector3 vfxPosition = position + Vector3.up * (success ? 0.6f : 0.2f);
        HitVfxUtility.Play(vfx, vfxPosition, Quaternion.identity);
        GameAudioManager.Instance?.PlaySfx(success ? GameAudioId.MergeSuccess : GameAudioId.MergeFail);
    }

    private Vector3 GetPointerWorldPosition(float height)
    {
        Camera camera = inputSO != null ? inputSO.MainCam : Camera.main;
        if (camera == null)
            return Vector3.zero;

        Ray ray = camera.ScreenPointToRay(inputSO.ScreenMousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, height, 0f));

        if (!groundPlane.Raycast(ray, out float enter))
            return _draggedTower != null ? _draggedTower.transform.position : Vector3.zero;

        Vector3 point = ray.GetPoint(enter);
        point.y = height;
        return point;
    }

    private bool AreTowersOverlapping(AbstractPlayer a, AbstractPlayer b)
    {
        BoxCollider colA = a != null ? a.GetComponent<BoxCollider>() : null;
        BoxCollider colB = b != null ? b.GetComponent<BoxCollider>() : null;

        if (colA == null || colB == null)
            return false;

        Bounds boundsA = ShrinkBoundsXZ(colA.bounds, mergeOverlapBoundsScale);
        Bounds boundsB = ShrinkBoundsXZ(colB.bounds, mergeOverlapBoundsScale);

        bool overlapX = boundsA.min.x <= boundsB.max.x && boundsA.max.x >= boundsB.min.x;
        bool overlapZ = boundsA.min.z <= boundsB.max.z && boundsA.max.z >= boundsB.min.z;
        return overlapX && overlapZ;
    }

    private static Bounds ShrinkBoundsXZ(Bounds bounds, float scale)
    {
        Vector3 size = bounds.size;
        size.x *= scale;
        size.z *= scale;
        return new Bounds(bounds.center, size);
    }

    
    private int GetEffectiveMergeChance(TowerGrade grade)
    {
        if (ArchetypeUpgradeManager.Instance != null)
            return ArchetypeUpgradeManager.Instance.GetEffectiveMergeChancePercent(grade);

        return mergeConfig != null ? mergeConfig.GetSuccessChancePercent(grade) : 0;
    }

    private static bool IsPointerOverUi()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
