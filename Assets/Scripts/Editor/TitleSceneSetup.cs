#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class TitleSceneSetup
{
    private const string TitleScenePath = "Assets/Scenes/TitleScene.unity";
    private const string GameScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("TowerDefense/Setup Title Scene")]
    public static void SetupFromMenu()
    {
        SetupAll();
        Debug.Log("타이틀 씬 설정 완료.");
    }

    public static void SetupAll()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject root = new GameObject("TitleScreen");
        TitleScreenController controller = root.AddComponent<TitleScreenController>();
        root.AddComponent<TitleGameFlow>();

        GameObject cameraGo = new GameObject("TitleCamera");
        cameraGo.tag = "MainCamera";
        Camera camera = cameraGo.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = GameplayViewSettings.OrthographicSize;
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.backgroundColor = new Color(0.72f, 0.86f, 0.98f, 1f);
        cameraGo.AddComponent<AudioListener>();
        TitleCameraRig cameraRig = cameraGo.AddComponent<TitleCameraRig>();

        GameObject canvasGo = new GameObject("TitleCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        TitleScreenUi screenUi = canvasGo.AddComponent<TitleScreenUi>();
        screenUi.EnsureUi();

        SettingsPanelUi.EnsureOnCanvas(canvasGo.transform, titlePanel: true);
        root.AddComponent<SettingsInputHandler>();

        GameObject eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        if (screenUi.StartButton != null)
        {
            SerializedObject eventSystemSo = new SerializedObject(eventSystemGo.GetComponent<EventSystem>());
            eventSystemSo.FindProperty("m_FirstSelected").objectReferenceValue = screenUi.StartButton.gameObject;
            eventSystemSo.ApplyModifiedPropertiesWithoutUndo();
        }

        SerializedObject controllerSo = new SerializedObject(controller);
        controllerSo.FindProperty("gameFlow").objectReferenceValue = root.GetComponent<TitleGameFlow>();
        controllerSo.ApplyModifiedPropertiesWithoutUndo();

        TitleGameFlow gameFlow = root.GetComponent<TitleGameFlow>();
        SerializedObject flowSo = new SerializedObject(gameFlow);
        flowSo.FindProperty("cameraRig").objectReferenceValue = cameraRig;
        flowSo.FindProperty("screenUi").objectReferenceValue = screenUi;
        flowSo.ApplyModifiedPropertiesWithoutUndo();

        ApplyNexonFont(canvasGo.transform);
        ApplyTitleEnvironment();

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, TitleScenePath);
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
    }

    private static void UpdateBuildSettings()
    {
        EditorBuildSettingsScene[] scenes =
        {
            new EditorBuildSettingsScene(TitleScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true),
        };

        EditorBuildSettings.scenes = scenes;
    }

    private static void ApplyNexonFont(Transform root)
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/NEXON Football Gothic L SDF.asset");
        if (font == null)
            return;

        foreach (TextMeshProUGUI label in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            label.font = font;
    }

    private static void ApplyTitleEnvironment()
    {
        Material skybox = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/3D/Next_Log/Fantasy_Map/Map/Texture/Materials/Sky_Box_01.mat");
        if (skybox != null)
        {
            RenderSettings.skybox = skybox;
            if (skybox.HasProperty("_Exposure"))
                skybox.SetFloat("_Exposure", 1.15f);
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.72f, 0.84f, 0.98f);
        RenderSettings.ambientEquatorColor = new Color(0.62f, 0.78f, 0.55f);
        RenderSettings.ambientGroundColor = new Color(0.45f, 0.38f, 0.32f);
    }
}
#endif
