using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleGameFlow : MonoBehaviour
{
    public static TitleGameFlow Instance { get; private set; }

    [SerializeField] private TitleCameraRig cameraRig;
    [SerializeField] private TitleScreenUi screenUi;
    [SerializeField] private float cameraTweenDuration = 1.2f;
    [SerializeField] private float titleFadeDuration = 0.45f;
    [SerializeField] private float hudFadeDuration = 0.4f;
    [SerializeField] private float transitionGap = 0.15f;

    private bool _isTransitioning;
    private Coroutine _transitionRoutine;

    public bool IsTransitioning => _isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResolveReferences();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ConfigureLoadedGameScene(Scene scene)
    {
        TitlePreviewEnvironment.Configure(scene);
        TitlePreviewEnvironment.SetGameSystemsActive(false);
        TitleHudTransition.SetVisible(false, immediate: true);
        cameraRig?.ResumeTitleOrbit();
    }

    public void BeginSession(bool tutorial)
    {
        if (_isTransitioning)
            return;

        _transitionRoutine = StartCoroutine(BeginSessionRoutine(tutorial));
    }

    public void ReturnToTitle()
    {
        if (_isTransitioning)
            return;

        _transitionRoutine = StartCoroutine(ReturnToTitleRoutine());
    }

    public void RestartSession()
    {
        if (_isTransitioning)
            return;

        _transitionRoutine = StartCoroutine(RestartSessionRoutine());
    }

    private IEnumerator BeginSessionRoutine(bool tutorial)
    {
        _isTransitioning = true;
        SetTitleButtonsInteractable(false);

        GameSessionMode.IsTutorial = tutorial;
        TitlePreviewMode.Active = false;

        Sequence sequence = DOTween.Sequence();
        if (cameraRig != null)
            sequence.Join(cameraRig.TweenToSnapshotView(cameraTweenDuration));

        if (screenUi != null)
            sequence.Join(screenUi.FadeOut(titleFadeDuration));

        yield return sequence.WaitForCompletion();
        yield return new WaitForSeconds(transitionGap);

        GameSessionResetter.ResetForNewSession();
        TitleFieldFreeze.Release();
        TitlePreviewEnvironment.SetGameSystemsActive(true);
        TitleHudTransition.SetVisible(false, immediate: true);
        GameHudLayoutBootstrap.ApplyFinalPresentation();

        GameAudioManager.EnsureExists();
        GameAudioManager.Instance?.PlayBgm(GameBgmTrack.Gameplay);

        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        waveManager?.BeginSession();

        GameHudController hudController = FindFirstObjectByType<GameHudController>(FindObjectsInactive.Include);
        if (hudController != null)
        {
            hudController.gameObject.SetActive(true);
            hudController.RefreshAll();
        }

        TitleHudTransition.FadeIn(hudFadeDuration);
        yield return new WaitForSecondsRealtime(hudFadeDuration);

        if (!tutorial)
            CombatSkillTutorialPresenter.TryShowIfNeeded();

        _isTransitioning = false;
        SetTitleButtonsInteractable(true);
        _transitionRoutine = null;
    }

    private IEnumerator ReturnToTitleRoutine()
    {
        _isTransitioning = true;
        SetTitleButtonsInteractable(false);

        Time.timeScale = 1f;
        GameSessionMode.IsTutorial = false;
        TitlePreviewMode.Active = true;

        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        waveManager?.StopSpawning();

        GameSessionResetter.ClearEnemiesOnly();
        if (GameOverUi.Instance != null)
            GameOverUi.Instance.Hide();

        Sequence sequence = DOTween.Sequence();
        sequence.Join(TitleHudTransition.FadeOut(titleFadeDuration));
        if (screenUi != null)
            sequence.Join(screenUi.FadeIn(titleFadeDuration));

        yield return sequence.WaitForCompletion();

        TitlePreviewEnvironment.SetGameSystemsActive(false);
        TitleFieldFreeze.Apply();
        GameplayCameraShake.ReleaseForTitle();
        cameraRig?.ResumeTitleOrbit();

        GameAudioManager.Instance?.PlayBgm(GameBgmTrack.Title);

        _isTransitioning = false;
        SetTitleButtonsInteractable(true);
        _transitionRoutine = null;

        TitleScreenController controller = FindFirstObjectByType<TitleScreenController>();
        if (controller != null)
            controller.OnReturnedToTitle();
    }

    private IEnumerator RestartSessionRoutine()
    {
        _isTransitioning = true;
        TitlePreviewMode.Active = false;

        TitleHudTransition.FadeOut(0.2f);
        yield return new WaitForSeconds(0.2f);

        cameraRig?.SnapToGameplayView();
        GameSessionResetter.ResetForNewSession();
        TitleFieldFreeze.Release();
        TitlePreviewEnvironment.SetGameSystemsActive(true);
        TitleHudTransition.SetVisible(false, immediate: true);
        GameHudLayoutBootstrap.ApplyFinalPresentation();

        GameAudioManager.Instance?.PlayBgm(GameBgmTrack.Gameplay);

        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        waveManager?.BeginSession();

        GameHudController hudController = FindFirstObjectByType<GameHudController>(FindObjectsInactive.Include);
        hudController?.RefreshAll();

        TitleHudTransition.FadeIn(hudFadeDuration);
        yield return new WaitForSecondsRealtime(hudFadeDuration);

        _isTransitioning = false;
        _transitionRoutine = null;
    }

    private void SetTitleButtonsInteractable(bool interactable)
    {
        if (screenUi == null)
            return;

        if (screenUi.StartButton != null)
            screenUi.StartButton.interactable = interactable;

        if (screenUi.TutorialButton != null)
            screenUi.TutorialButton.interactable = interactable;

        if (screenUi.ExitButton != null)
            screenUi.ExitButton.interactable = interactable;
    }

    private void ResolveReferences()
    {
        cameraRig ??= FindFirstObjectByType<TitleCameraRig>();
        screenUi ??= FindFirstObjectByType<TitleScreenUi>();
    }
}
