using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameInputRouter : MonoBehaviour
{
    [SerializeField] private InputSO inputSO;
    [SerializeField] private TileManager tileManager;

    private void Awake()
    {
        if (inputSO == null)
            inputSO = Resources.FindObjectsOfTypeAll<InputSO>()[0];

        tileManager ??= FindFirstObjectByType<TileManager>();
    }

    private void Update()
    {
        if (inputSO == null || Mouse.current == null)
            return;

        if (!Mouse.current.rightButton.wasPressedThisFrame)
            return;

        if (IsPointerOverUi())
        {
            tileManager?.ClearInspectionExternal();
            return;
        }

        inputSO.InvokeTileRightClick();
    }

    private static bool IsPointerOverUi()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
