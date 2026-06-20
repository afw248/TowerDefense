using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DefaultExecutionOrder(-10000)]
public class UiInputBootstrap : MonoBehaviour
{
    private static Controls _controls;
    private static InputActionAsset _uiActionsAsset;
    private static bool _lifecycleHooksRegistered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        ShutdownInput();
        _lifecycleHooksRegistered = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapAfterSceneLoad()
    {
        RegisterLifecycleHooks();
        EnsureUiInputModule();
    }

    private void Awake()
    {
        RegisterLifecycleHooks();
        EnsureUiInputModule();
    }

    public static void EnsureUiInputModule()
    {
        RegisterLifecycleHooks();

        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem = go.GetComponent<EventSystem>();
        }

        InputSystemUIInputModule module = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (module == null)
            module = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();

        InputActionAsset asset = ResolveUiActionsAsset();
        if (asset == null)
            return;

        WireInputModule(module, asset);
    }

    private static void RegisterLifecycleHooks()
    {
        if (_lifecycleHooksRegistered)
            return;

        _lifecycleHooksRegistered = true;
        Application.quitting += ShutdownInput;
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
#endif
    }

#if UNITY_EDITOR
    private static void HandlePlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
            ShutdownInput();
    }
#endif

    private static InputActionAsset ResolveUiActionsAsset()
    {
        if (_uiActionsAsset != null)
            return _uiActionsAsset;

        InputActionAsset[] assets = Resources.FindObjectsOfTypeAll<InputActionAsset>();
        foreach (InputActionAsset asset in assets)
        {
            if (asset != null && asset.name == "InputSystem_Actions")
            {
                _uiActionsAsset = asset;
                return _uiActionsAsset;
            }
        }

        _controls ??= new Controls();
        _uiActionsAsset = _controls.asset;
        return _uiActionsAsset;
    }

    private static void WireInputModule(InputSystemUIInputModule module, InputActionAsset asset)
    {
        InputActionMap uiMap = asset.FindActionMap("UI", throwIfNotFound: false);
        if (uiMap != null && !uiMap.enabled)
            uiMap.Enable();

        module.actionsAsset = asset;
        module.point = BindAction(uiMap, "Point");
        module.move = BindAction(uiMap, "Navigate");
        module.submit = BindAction(uiMap, "Submit");
        module.cancel = BindAction(uiMap, "Cancel");
        module.leftClick = BindAction(uiMap, "Click");
        module.rightClick = BindAction(uiMap, "RightClick");
        module.middleClick = BindAction(uiMap, "MiddleClick");
        module.scrollWheel = BindAction(uiMap, "ScrollWheel");
        module.trackedDevicePosition = BindAction(uiMap, "TrackedDevicePosition");
        module.trackedDeviceOrientation = BindAction(uiMap, "TrackedDeviceOrientation");

        module.enabled = false;
        module.enabled = true;
    }

    private static void ShutdownInput()
    {
        if (_controls != null)
        {
            if (_controls.UI.enabled)
                _controls.UI.Disable();

            if (_controls.Player.enabled)
                _controls.Player.Disable();

            _controls.Dispose();
            _controls = null;
        }

        if (_uiActionsAsset != null)
        {
            InputActionMap uiMap = _uiActionsAsset.FindActionMap("UI", throwIfNotFound: false);
            if (uiMap != null && uiMap.enabled)
                uiMap.Disable();

            _uiActionsAsset = null;
        }
    }

    private static InputActionReference BindAction(InputActionMap map, string actionName)
    {
        if (map == null)
            return null;

        InputAction action = map.FindAction(actionName, throwIfNotFound: false);
        return action != null ? InputActionReference.Create(action) : null;
    }
}
