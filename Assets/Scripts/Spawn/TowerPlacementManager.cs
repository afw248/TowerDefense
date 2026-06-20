using UnityEngine;
using UnityEngine.InputSystem;

public class TowerPlacementManager : MonoBehaviour
{
    [Header("Input SO Reference")]
    [SerializeField] private InputSO inputSO;

    [Header("Targeting Settings")]
    [SerializeField] private LayerMask towerLayer;

    private GameObject _currentPreviewTower;
    private bool _isPlacing = false;
    private Vector3 _originalPosition;

    private void Update()
    {
        if (!_isPlacing)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                TrySelectTower();
            }
            return;
        }

        if (_currentPreviewTower == null) return;

        Vector3 mouseWorldPos = inputSO.GetWorldMousePosition();
        mouseWorldPos.y = _currentPreviewTower.transform.position.y;
        _currentPreviewTower.transform.position = mouseWorldPos;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CompletePlacement();
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelPlacement();
        }
    }

    private void TrySelectTower()
    {
        if (inputSO.MainCam == null) return;

        Ray ray = inputSO.MainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, towerLayer))
        {
            _currentPreviewTower = hit.collider.gameObject;
            _originalPosition = _currentPreviewTower.transform.position;
            _isPlacing = true;

            DisableTowerComponents(_currentPreviewTower);
            Debug.Log($"[PlacementManager] 타워 선택됨: {_currentPreviewTower.name}");
        }
    }

    private void CompletePlacement()
    {
        EnableTowerComponents(_currentPreviewTower);
        _currentPreviewTower = null;
        _isPlacing = false;
        Debug.Log("[PlacementManager] 타워 재배치 완료!");
    }

    private void CancelPlacement()
    {
        if (_currentPreviewTower != null)
        {
            _currentPreviewTower.transform.position = _originalPosition;
            EnableTowerComponents(_currentPreviewTower);
        }
        _currentPreviewTower = null;
        _isPlacing = false;
        Debug.Log("[PlacementManager] 타워 재배치 취소");
    }

    private void DisableTowerComponents(GameObject target)
    {
        if (target.TryGetComponent(out Collider col)) col.enabled = false;
        if (target.TryGetComponent(out CharacterController cc)) cc.enabled = false;
        if (target.TryGetComponent(out Agents.Agent agent)) agent.enabled = false;
    }

    private void EnableTowerComponents(GameObject target)
    {
        if (target.TryGetComponent(out Collider col)) col.enabled = true;
        if (target.TryGetComponent(out CharacterController cc)) cc.enabled = true;
        if (target.TryGetComponent(out Agents.Agent agent)) agent.enabled = true;
    }
}