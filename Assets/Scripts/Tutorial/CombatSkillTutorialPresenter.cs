using UnityEngine;

public static class CombatSkillTutorialPresenter
{
    private const string PrefKey = "td_combat_skill_tutorial_shown";

    public static void TryShowIfNeeded()
    {
        if (TitlePreviewMode.Active || GameSessionMode.IsTutorial)
            return;

        if (PlayerPrefs.GetInt(PrefKey, 0) == 1)
            return;

        GameObject canvas = GameObject.Find("GameHudCanvas");
        if (canvas == null)
            return;

        TutorialPopupUi popup = TutorialPopupUi.EnsureExists(canvas.transform);
        if (popup == null || popup.IsVisible)
            return;

        popup.Show("전투 스킬", BuildMessage(), MarkShown);
    }

    public static string BuildMessage()
    {
        CombatActiveAbilityConfigSO config =
            Resources.Load<CombatActiveAbilityConfigSO>("CombatActiveAbilityConfig");

        float freezeDuration = config != null ? config.freezeDurationSeconds : 4f;
        float freezeCooldown = config != null ? config.freezeCooldownSeconds : 75f;
        float damagePercent = config != null ? config.damagePercentOfMaxHealth * 100f : 18f;
        float damageCooldown = config != null ? config.globalDamageCooldownSeconds : 90f;

        return
            "소환 버튼 왼쪽에 전투 스킬 버튼 2개가 있습니다.\n\n" +
            $"• 시간 정지 — 필드의 모든 적과 웨이브 시간을 {freezeDuration:0.#}초간 멈춥니다. (쿨타임 {freezeCooldown:0.#}초)\n" +
            $"• 전체 피해 — 모든 적에게 최대 체력 {damagePercent:0.#}% 피해. (쿨타임 {damageCooldown:0.#}초)\n\n" +
            "적이 많이 몰릴 때 위기 탈출용으로 사용하세요.";
    }

    private static void MarkShown()
    {
        PlayerPrefs.SetInt(PrefKey, 1);
        PlayerPrefs.Save();
    }
}
