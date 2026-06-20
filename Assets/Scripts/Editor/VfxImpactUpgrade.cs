#if UNITY_EDITOR
using CoreSystem.EffectSystem;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 타워 공격 VFX 개선: 빠르고 임팩트 있는 이펙트로 교체 + 속도/크기 튜닝.
/// </summary>
public static class VfxImpactUpgrade
{
    private const string FX = "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/";
    private const string SO = "Assets/SO/Effects/";
    private const string GR = SO + "Grades/";

    [MenuItem("TowerDefense/Upgrade VFX Impact")]
    public static void ApplyFromMenu()
    {
        Apply();
        Debug.Log("[VfxImpactUpgrade] 타워 VFX 임팩트 업그레이드 완료.");
    }

    public static void Apply()
    {
        // ── BOW ────────────────────────────────────────────────
        // 빠른 슬래시/임팩트 — playbackSpeed 높여서 타격감 강화
        SetVfx(SO + "BowTowerAttackVfx.asset",      FX + "Gun Impact.prefab",            scale: 1.5f,  lifetime: 0.45f, playbackSpeed: 3.5f);
        SetVfx(GR + "RareBowAttackVfx.asset",        FX + "Ice Impact.prefab",            scale: 1.7f,  lifetime: 0.50f, playbackSpeed: 3.5f);
        SetVfx(GR + "EpicBowAttackVfx.asset",        FX + "Slash Magic.prefab",           scale: 2.1f,  lifetime: 0.45f, playbackSpeed: 4.0f);
        SetVfx(GR + "LegendaryBowAttackVfx.asset",   FX + "Lightning Strike.prefab",      scale: 2.0f,  lifetime: 0.60f, playbackSpeed: 3.6f);

        // ── CULVERIN ───────────────────────────────────────────
        // 터지는 느낌 극대화 — 빠른 폭발 플래시
        SetVfx(SO + "CulverinTowerAttackVfx.asset",  FX + "Explosion Bomb.prefab",        scale: 2.0f,  lifetime: 1.10f, playbackSpeed: 2.8f);
        SetVfx(GR + "RareCulverinAttackVfx.asset",   FX + "Fire Muzzle Flash.prefab",     scale: 2.5f,  lifetime: 0.60f, playbackSpeed: 3.5f);
        SetVfx(GR + "EpicCulverinAttackVfx.asset",   FX + "Incinerate Spell.prefab",      scale: 2.8f,  lifetime: 1.30f, playbackSpeed: 3.0f);
        SetVfx(GR + "LegendaryCulverinAttackVfx.asset", FX + "Explosion Galaxy.prefab",   scale: 3.5f,  lifetime: 2.00f, playbackSpeed: 2.2f);

        // ── MISSILE ────────────────────────────────────────────
        // 폭발 임팩트 — 큰 플래시+화염으로 터지는 느낌
        SetVfx(SO + "MissileTowerAttackVfx.asset",   FX + "Fire Impact.prefab",           scale: 1.6f,  lifetime: 0.55f, playbackSpeed: 3.5f);
        SetVfx(GR + "RareMissileAttackVfx.asset",    FX + "Plasma Ball.prefab",           scale: 2.0f,  lifetime: 0.80f, playbackSpeed: 3.2f);
        SetVfx(GR + "EpicMissileAttackVfx.asset",    FX + "Magic Explosive Spell.prefab", scale: 2.5f,  lifetime: 1.20f, playbackSpeed: 3.0f);
        SetVfx(GR + "LegendaryMissileAttackVfx.asset", FX + "Fire Tornado.prefab",        scale: 3.2f,  lifetime: 2.00f, playbackSpeed: 2.5f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void SetVfx(string soPath, string prefabPath, float scale, float lifetime, float playbackSpeed)
    {
        TowerAttackVfxDataSO data = AssetDatabase.LoadAssetAtPath<TowerAttackVfxDataSO>(soPath);
        if (data == null)
        {
            Debug.LogWarning($"[VfxImpactUpgrade] SO not found: {soPath}");
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[VfxImpactUpgrade] Prefab not found: {prefabPath}");
            return;
        }

        data.effectPrefab = prefab;
        data.scale = scale;
        data.lifetime = lifetime;
        data.playbackSpeed = playbackSpeed;
        EditorUtility.SetDirty(data);
        Debug.Log($"[VfxImpactUpgrade] {System.IO.Path.GetFileNameWithoutExtension(soPath)} → {System.IO.Path.GetFileNameWithoutExtension(prefabPath)} (x{scale}, {lifetime}s, spd{playbackSpeed})");
    }
}
#endif
