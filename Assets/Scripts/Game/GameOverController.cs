using UnityEngine;

[DefaultExecutionOrder(-50)]
public class GameOverController : MonoBehaviour
{
    [SerializeField] private GameOverUi gameOverUi;

    private void Awake()
    {
        ResolveGameOverUi();
    }

    private void OnEnable()
    {
        Subscribe();
        ApplyExistingGameOverState();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void BindGameOverUi(GameOverUi ui)
    {
        if (ui == null)
            return;

        gameOverUi = ui;
    }

    private void ResolveGameOverUi()
    {
        gameOverUi ??= GameOverUi.Instance;
        gameOverUi ??= FindFirstObjectByType<GameOverUi>(FindObjectsInactive.Include);

        if (gameOverUi != null)
            return;

        GameObject canvas = GameObject.Find("GameHudCanvas");
        if (canvas != null)
            gameOverUi = GameOverUi.EnsureExists(canvas.transform);
    }

    private void Subscribe()
    {
        if (FieldEnemyTracker.Instance != null)
        {
            FieldEnemyTracker.Instance.OnGameOver -= HandleFieldOverflow;
            FieldEnemyTracker.Instance.OnGameOver += HandleFieldOverflow;
        }

        if (LeakTracker.Instance != null)
        {
            LeakTracker.Instance.OnGameOver -= HandleLeakOverflow;
            LeakTracker.Instance.OnGameOver += HandleLeakOverflow;
        }
    }

    private void Unsubscribe()
    {
        if (FieldEnemyTracker.Instance != null)
            FieldEnemyTracker.Instance.OnGameOver -= HandleFieldOverflow;

        if (LeakTracker.Instance != null)
            LeakTracker.Instance.OnGameOver -= HandleLeakOverflow;
    }

    private void ApplyExistingGameOverState()
    {
        if (FieldEnemyTracker.Instance != null && FieldEnemyTracker.Instance.IsGameOver)
            HandleFieldOverflow();
        else if (LeakTracker.Instance != null && LeakTracker.Instance.IsGameOver)
            HandleLeakOverflow();
    }

    private void HandleFieldOverflow()
    {
        FieldEnemyTracker tracker = FieldEnemyTracker.Instance;
        int maxCount = tracker != null ? tracker.MaxCount : 60;
        GameOverPresenter.ShowFieldOverflow(maxCount);
    }

    private void HandleLeakOverflow()
    {
        GameOverPresenter.ShowLeakOverflow();
    }
}
