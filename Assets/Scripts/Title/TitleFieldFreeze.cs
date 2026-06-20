using Agents;
using CombatSystem;
using FSM;
using Player;
using UnityEngine;

public static class TitleFieldFreeze
{
    public static void Apply()
    {
        TileManager tileManager = Object.FindFirstObjectByType<TileManager>();
        if (tileManager == null)
            return;

        foreach (AbstractPlayer tower in tileManager.GetOccupiedTowers())
        {
            if (tower == null)
                continue;

            tower.ChangeState(PlayerState.IDLE, 0f);
            DisableTowerActions(tower.gameObject);
        }
    }

    public static void Release()
    {
        TileManager tileManager = Object.FindFirstObjectByType<TileManager>();
        if (tileManager == null)
            return;

        foreach (AbstractPlayer tower in tileManager.GetOccupiedTowers())
        {
            if (tower == null)
                continue;

            EnableTowerActions(tower.gameObject);
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

            if (behaviour is ISkillModule or ISensor or HealthModule)
                behaviour.enabled = false;
        }
    }

    private static void EnableTowerActions(GameObject root)
    {
        foreach (CharacterController controller in root.GetComponentsInChildren<CharacterController>(true))
            controller.enabled = true;

        foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour is AbstractPlayer or AgentRenderer or AgentTrigger)
                continue;

            if (behaviour is ISkillModule or ISensor or HealthModule)
                behaviour.enabled = true;
        }
    }
}
