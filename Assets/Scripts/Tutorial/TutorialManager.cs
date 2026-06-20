using System.Collections;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [SerializeField] private TutorialWaveConfigSO config;
    [SerializeField] private TutorialPopupUi popupUi;

    public TutorialWaveConfigSO Config => config;

    private void Awake()
    {
        if (!GameSessionMode.IsTutorial)
        {
            enabled = false;
            return;
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        config ??= TutorialWaveConfigSO.GetActive(FindSpawnList());
        EnsurePopupUi();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public IEnumerator ShowWaveIntroAndWait(int wave)
    {
        if (config == null || !config.TryGetStep(wave, out TutorialWaveStep step))
            yield break;

        EnsurePopupUi();
        if (popupUi == null)
            yield break;

        bool acknowledged = false;
        popupUi.Show(step.title, step.message, () => acknowledged = true);

        while (!acknowledged)
            yield return null;
    }

    private void EnsurePopupUi()
    {
        if (popupUi != null)
            return;

        GameObject canvas = GameObject.Find("GameHudCanvas");
        if (canvas == null)
            return;

        popupUi = TutorialPopupUi.EnsureExists(canvas.transform);
    }

    public static TutorialManager EnsureExists()
    {
        if (!GameSessionMode.IsTutorial)
            return null;

        if (Instance != null)
            return Instance;

        TutorialManager existing = FindFirstObjectByType<TutorialManager>();
        if (existing != null)
        {
            Instance = existing;
            return existing;
        }

        GameObject host = new GameObject(nameof(TutorialManager));
        return host.AddComponent<TutorialManager>();
    }

    private static SpawnEnemyListSO FindSpawnList()
    {
        SpawnManager spawner = FindFirstObjectByType<SpawnManager>();
        return spawner != null ? spawner.spawn : null;
    }
}
