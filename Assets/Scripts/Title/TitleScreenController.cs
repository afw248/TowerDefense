using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-200)]
public class TitleScreenController : MonoBehaviour
{
    [SerializeField] private TitleGameFlow gameFlow;

    private TitleScreenUi _screenUi;

    private void Awake()
    {
        TitlePreviewMode.Active = true;
        Time.timeScale = 1f;

        ResolveReferences();
        EnsureEventSystem();
        EnsureSettingsSupport();
        GameAudioManager.EnsureExists();
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        _screenUi = FindFirstObjectByType<TitleScreenUi>();
        _screenUi?.EnsureUi();
        BindButtons(_screenUi);
        FocusPrimaryButton(_screenUi);
        _screenUi?.PlayEntranceAnimation();

        Scene gameScene = SceneManager.GetSceneByName(GameSceneNames.Game);
        if (gameScene.IsValid() && gameScene.isLoaded)
            gameFlow?.ConfigureLoadedGameScene(gameScene);
        else
            SceneManager.LoadScene(GameSceneNames.Game, LoadSceneMode.Additive);
    }

    private void Update()
    {
        if (TitleGameFlow.Instance != null && TitleGameFlow.Instance.IsTransitioning)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            StartGame();
        else if (keyboard.tKey.wasPressedThisFrame)
            StartTutorial();
    }

    private void BindButtons(TitleScreenUi screenUi)
    {
        if (screenUi == null)
            return;

        if (screenUi.StartButton != null)
            screenUi.StartButton.onClick.AddListener(StartGame);

        if (screenUi.TutorialButton != null)
            screenUi.TutorialButton.onClick.AddListener(StartTutorial);

        if (screenUi.ExitButton != null)
            screenUi.ExitButton.onClick.AddListener(QuitGame);
    }

    private static void FocusPrimaryButton(TitleScreenUi screenUi)
    {
        if (screenUi?.StartButton == null)
            return;

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null)
            eventSystem.SetSelectedGameObject(screenUi.StartButton.gameObject);
    }

    public void OnReturnedToTitle()
    {
        _screenUi ??= FindFirstObjectByType<TitleScreenUi>();
        FocusPrimaryButton(_screenUi);
        _screenUi?.PlayEntranceAnimation();
    }

    public void QuitGame()
    {
        GameSessionQuit.Quit();
    }

    public void StartGame()
    {
        gameFlow?.BeginSession(tutorial: false);
    }

    public void StartTutorial()
    {
        gameFlow?.BeginSession(tutorial: true);
    }

    private void ResolveReferences()
    {
        gameFlow ??= GetComponent<TitleGameFlow>();
        if (gameFlow == null)
            gameFlow = gameObject.AddComponent<TitleGameFlow>();
    }

    private static void EnsureSettingsSupport()
    {
        TitleScreenController controller = FindFirstObjectByType<TitleScreenController>();
        if (controller != null && controller.GetComponent<SettingsInputHandler>() == null)
            controller.gameObject.AddComponent<SettingsInputHandler>();

        TitleScreenUi screenUi = FindFirstObjectByType<TitleScreenUi>();
        if (screenUi != null)
            SettingsPanelUi.EnsureOnCanvas(screenUi.transform, titlePanel: true);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Additive || scene.name != GameSceneNames.Game)
            return;

        Camera previewCamera = FindMainCameraInScene(scene);
        TitleCameraRig cameraRig = FindFirstObjectByType<TitleCameraRig>();
        if (previewCamera != null && cameraRig != null)
            cameraRig.ApplyGameCameraSettings(previewCamera);

        gameFlow?.ConfigureLoadedGameScene(scene);
    }

    private static Camera FindMainCameraInScene(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Camera camera in root.GetComponentsInChildren<Camera>(true))
            {
                if (camera.CompareTag("MainCamera"))
                    return camera;
            }
        }

        return null;
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        InputSystemUIInputModule module = eventSystemGo.GetComponent<InputSystemUIInputModule>();
        module.enabled = true;
    }
}
