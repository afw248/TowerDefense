using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputSO", menuName = "Scriptable Objects/InputSO")]
public class InputSO : ScriptableObject, Controls.IPlayerActions
{
    public Vector3 WorldMousePosition;
    public Vector2 ScreenMousePosition;
    public LayerMask whatIsPlayer;
    private Camera _mainCam;
    private Controls _controls;
    public Action TileClick;
    public Action TileRightClick;

    public void InvokeTileRightClick()
    {
        TileRightClick?.Invoke();
    }
    public Camera MainCam
    {
        get
        {
            if (_mainCam == null)
                _mainCam = Camera.main;
            return _mainCam;
        }
    }
    private void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        if (_controls == null)
        {
            _controls = new Controls();
            _controls.Player.SetCallbacks(this);
        }

        _controls.Player.Enable();
    }

    private void OnDisable()
    {
        ReleaseControls();
    }

    private void ReleaseControls()
    {
        if (_controls == null)
            return;

        if (_controls.Player.enabled)
            _controls.Player.Disable();

        if (_controls.UI.enabled)
            _controls.UI.Disable();

        if (!Application.isPlaying)
        {
            _controls = null;
            return;
        }

        _controls.Dispose();
        _controls = null;
    }
    public Vector3 GetWorldMousePosition()
    {
        if (MainCam == null)
            return WorldMousePosition;
        Ray camRay = MainCam.ScreenPointToRay(ScreenMousePosition);
        if (Physics.Raycast(camRay, out RaycastHit hit, MainCam.farClipPlane, whatIsPlayer))
        {
            WorldMousePosition = hit.point;
        }
        return WorldMousePosition;
    }
    public void OnPointer(InputAction.CallbackContext context)
    {
        ScreenMousePosition = context.ReadValue<Vector2>();
    }

    public void OnOnClick(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        TileClick?.Invoke();
    }
}
