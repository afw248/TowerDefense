using TMPro;

using UnityEngine;

using UnityEngine.UI;



public class SummonButtonUi : MonoBehaviour

{

    [SerializeField] private Button button;

    [SerializeField] private TextMeshProUGUI labelText;

    [SerializeField] private TextMeshProUGUI costText;

    [SerializeField] private TextMeshProUGUI probabilityText;

    [SerializeField] private TileManager tileManager;



    private void Awake()

    {

        tileManager ??= FindFirstObjectByType<TileManager>();

        button ??= GetComponent<Button>();

        labelText ??= transform.Find("Text (TMP)")?.GetComponent<TextMeshProUGUI>();



        TextMeshProUGUI[] costLabels = GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI candidate in costLabels)

        {

            if (candidate == labelText || candidate == probabilityText || candidate.name != "SummonCostText")

                continue;



            costText = candidate;

            break;

        }



        costText ??= transform.Find("SummonCostText")?.GetComponent<TextMeshProUGUI>();

        ResolveProbabilityText();



        if (button != null)

            button.onClick.AddListener(HandleClick);

    }



    private void OnEnable()

    {

        if (EconomyManager.Instance != null)

            EconomyManager.Instance.OnSummonCostChanged += Refresh;



        if (ArchetypeUpgradeManager.Instance != null)
            ArchetypeUpgradeManager.Instance.Changed += Refresh;

        WaveManager.WaveStarted += HandleWaveStarted;
    }



    private void OnDisable()

    {

        if (EconomyManager.Instance != null)

            EconomyManager.Instance.OnSummonCostChanged -= Refresh;



        if (ArchetypeUpgradeManager.Instance != null)

            ArchetypeUpgradeManager.Instance.Changed -= Refresh;

        WaveManager.WaveStarted -= HandleWaveStarted;
    }



    private void OnDestroy()

    {

        if (button != null)

            button.onClick.RemoveListener(HandleClick);

    }



    private void HandleWaveStarted(int wave)
    {
        Refresh();
    }

    private void HandleClick()

    {

        if (tileManager == null)

            return;



        SummonResult result = tileManager.TrySummonTower();



        switch (result)

        {

            case SummonResult.AtCapacity:

                WarningMessageUi.Instance?.Show("타워가 가득 찼습니다!");

                break;

            case SummonResult.NotEnoughGold:

                WarningMessageUi.Instance?.Show("코인이 부족합니다!");

                break;

            case SummonResult.NoSpace:

                WarningMessageUi.Instance?.Show("빈 타일이 없습니다!");

                break;

            case SummonResult.Success:

                Refresh();

                break;

        }

    }



    public void Refresh()

    {

        EconomyManager economy = EconomyManager.Instance;



        int cost = economy != null ? economy.GetSummonCost() : 0;

        tileManager ??= FindFirstObjectByType<TileManager>();
        bool canAfford = economy == null || economy.CanAfford(cost);
        bool atCapacity = tileManager != null && tileManager.IsAtCapacity;
        bool canSummon = canAfford && !atCapacity;



        if (costText != null)

        {

            costText.text = $"G {cost}";

            UiFonts.ApplyNexon(costText);

            costText.color = canSummon

                ? GameHudTheme.GoldText

                : new Color(0.85f, 0.45f, 0.45f, 1f);

        }



        if (labelText != null)

        {

            labelText.text = "소환";

            UiFonts.ApplyNexon(labelText);

            labelText.color = GameHudTheme.BodyText;

        }



        RefreshProbabilityText();



        if (button != null)

            button.interactable = canSummon;



        Image bg = GetComponent<Image>();

        if (bg != null)

            bg.color = canSummon

                ? GameHudTheme.PanelBackground

                : new Color(0.22f, 0.10f, 0.10f, 0.98f);

    }



    private void ResolveProbabilityText()
    {
        if (probabilityText != null)
            return;

        Transform playerSpawn = transform.parent;
        probabilityText = playerSpawn?.Find("SummonProbabilityPanel/SummonProbabilityText")
            ?.GetComponent<TextMeshProUGUI>();
    }

    private void RefreshProbabilityText()
    {
        ResolveProbabilityText();

        if (probabilityText == null)
            return;



        tileManager ??= FindFirstObjectByType<TileManager>();
        int currentWave = FindFirstObjectByType<WaveManager>()?.CurrentWave ?? 0;
        EconomyConfigSO config = EconomyManager.Instance?.Config;
        int summonUpgradeLevel = ArchetypeUpgradeManager.Instance?.GetSummonUpgradeLevel() ?? 0;
        AllPlayerListSO allTowers = tileManager != null ? tileManager.AllTowers : null;

        probabilityText.text = allTowers != null
            ? SummonGradeOdds.FormatDisplayText(allTowers, currentWave, config, summonUpgradeLevel)
            : string.Empty;
        UiFonts.ApplyNexon(probabilityText);
        probabilityText.color = new Color(0.82f, 0.88f, 0.95f, 1f);
    }
}


