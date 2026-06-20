using Tower;

public static class TowerGradeLabels
{
    public static string GetGradeLabel(TowerGrade grade) => grade switch
    {
        TowerGrade.Rare => "레어",
        TowerGrade.Epic => "에픽",
        TowerGrade.Legendary => "전설",
        _ => "노말"
    };

    public static string GetArchetypeLabel(TowerArchetype archetype) => archetype switch
    {
        TowerArchetype.Culverin => "대포",
        TowerArchetype.Missile => "미사일",
        _ => "석궁"
    };

    public static string GetMergeLabel(TowerGrade fromGrade)
    {
        if (fromGrade >= TowerGrade.Legendary)
            return string.Empty;

        TowerGrade toGrade = fromGrade + 1;
        return $"{GetGradeLabel(fromGrade)}→{GetGradeLabel(toGrade)}";
    }

    public static string GetMergeChanceUpgradeLabel(TowerGrade fromGrade)
    {
        if (fromGrade >= TowerGrade.Legendary)
            return string.Empty;

        return $"확률: {GetMergeLabel(fromGrade)}";
    }

    public static string GetMergeUnlockLabel(TowerGrade fromGrade)
    {
        if (fromGrade >= TowerGrade.Legendary)
            return string.Empty;

        return $"해금: {GetMergeLabel(fromGrade)}";
    }
}
