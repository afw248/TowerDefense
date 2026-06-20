using UnityEngine;

namespace Tower
{
    public static class TowerIdentityPalette
    {
        public static Color GetGradeRingColor(TowerGrade grade) => grade switch
        {
            TowerGrade.Legendary => new Color(1f, 0.78f, 0.08f, 0.95f),
            TowerGrade.Epic => new Color(0.62f, 0.18f, 1f, 0.92f),
            TowerGrade.Rare => new Color(0.18f, 0.52f, 1f, 0.9f),
            _ => new Color(0.72f, 0.76f, 0.82f, 0.78f),
        };

        public static Color GetGradeLabelColor(TowerGrade grade) => grade switch
        {
            TowerGrade.Legendary => new Color(1f, 0.86f, 0.2f, 1f),
            TowerGrade.Epic => new Color(0.82f, 0.45f, 1f, 1f),
            TowerGrade.Rare => new Color(0.45f, 0.78f, 1f, 1f),
            _ => new Color(0.88f, 0.9f, 0.94f, 1f),
        };

        public static float GetGradeRingScale(TowerGrade grade) => grade switch
        {
            TowerGrade.Legendary => 1.08f,
            TowerGrade.Epic => 1.04f,
            TowerGrade.Rare => 1f,
            _ => 0.96f,
        };

        public static string GetGradeLabel(TowerGrade grade) => TowerGradeLabels.GetGradeLabel(grade);
    }
}
