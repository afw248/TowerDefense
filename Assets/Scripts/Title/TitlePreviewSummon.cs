using Agents;
using CombatSystem;
using FSM;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public static class TitlePreviewSummon
{
    private const int PreviewTowerMin = 3;
    private const int PreviewTowerMax = 5;
    private static readonly Vector3 PreviewFocus = new(6f, 0f, -3f);

    public static void Apply(Scene gameScene) => Execute(gameScene);

    public static void Execute(Scene gameScene)
    {
        if (!gameScene.IsValid())
            return;

        TileManager tileManager = FindTileManager(gameScene);
        if (tileManager == null)
            return;

        int towerCount = Random.Range(PreviewTowerMin, PreviewTowerMax + 1);
        tileManager.SpawnTitlePreviewTowers(towerCount, PreviewFocus);
        FreezePreviewTowers(gameScene);
    }

    private static TileManager FindTileManager(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            TileManager tileManager = root.GetComponentInChildren<TileManager>(true);
            if (tileManager != null)
                return tileManager;
        }

        return null;
    }

    private static void FreezePreviewTowers(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Tile tile in root.GetComponentsInChildren<Tile>(true))
            {
                if (tile == null || tile.IsEmpty)
                    continue;

                AbstractPlayer tower = tile.CurrentOccupant;
                tower.ChangeState(PlayerState.IDLE, 0f);
                DisableTowerActions(tower.gameObject);
            }
        }
    }

    private static void DisableTowerActions(GameObject root)
    {
        foreach (CharacterController controller in root.GetComponentsInChildren<CharacterController>(true))
            controller.enabled = false;

        foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour is AbstractPlayer or AgentRenderer or AgentTrigger)
                continue;

            if (behaviour is ISkillModule or ISensor or CombatSystem.HealthModule)
                behaviour.enabled = false;
        }
    }
}
