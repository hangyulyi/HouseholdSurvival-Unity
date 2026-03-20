using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


/// Populates the final results screen by calling:
///   GET /api/progress/summary   per-phase decisions, totals, session outcome
///   GET /api/countries/:code/worldbank  live World Bank indicators

public class FinalResultsUI : MonoBehaviour
{
    public static FinalResultsUI Instance;

    // Outcome header 
    [Header("Outcome Header")]
    public GameObject outcomePanel;
    public TMP_Text outcomeLabel;      
    public TMP_Text finalScoreLabel;   
    public Image outcomeIcon;       

    // Journey bars 
    [Header("Journey Bars")]
    public Slider financialBar;
    public TMP_Text financialValue;      
    public Slider socialBar;
    public TMP_Text socialValue;         
    public Slider healthBar;
    public TMP_Text healthValue;         

    //  World Bank context 
    [Header("World Bank Context")]
    public TMP_Text worldBankLabel;      
    public TMP_Text worldBankBody;       

    //  Phase decisions list 
    [Header("Key Decisions")]
    public Transform phaseRowContainer;
    public GameObject phaseRowPrefab;    // prefab: two TMP_Text children

    // Buttons 
    [Header("Buttons")]
    public Button replayButton;

    //  Loading 
    [Header("Loading")]
    public GameObject loadingOverlay;

    // Outcome colour map 
    [Header("Outcome Colours (optional)")]
    public Color colourStabilized = new Color(0.22f, 0.71f, 0.29f);
    public Color colourSurvival = new Color(0.95f, 0.77f, 0.06f);
    public Color colourPoverty = new Color(0.93f, 0.50f, 0.07f);
    public Color colourCollapse = new Color(0.86f, 0.20f, 0.18f);

    // Bar scale 
    // Sliders go from -barRange to +barRange
    [SerializeField] private float barRange = 60f;

    void Awake()
    {
        Instance = this;
        if (outcomePanel) outcomePanel.SetActive(false);
    }

    // Public entry point 
    public void Show()
    {
        if (outcomePanel) outcomePanel.SetActive(true);
        if (loadingOverlay) loadingOverlay.SetActive(true);

        // Clear previous phase rows
        if (phaseRowContainer != null)
            foreach (Transform child in phaseRowContainer)
                Destroy(child.gameObject);

        StartCoroutine(LoadAll());
    }

    // Load both endpoints in parallel 

    private IEnumerator LoadAll()
    {
        string summaryJson = null;
        string worldBankJson = null;

        bool summaryDone = false;
        bool wbDone = false;

        string code = GameManager.Instance?.countryCode ?? "";

        StartCoroutine(APIManager.Instance.GetProgressSummary(j => { summaryJson = j; summaryDone = true; }));
        StartCoroutine(APIManager.Instance.GetWorldBankData(code, j => { worldBankJson = j; wbDone = true; }));

        yield return new WaitUntil(() => summaryDone && wbDone);

        if (loadingOverlay) loadingOverlay.SetActive(false);

        PopulateSummary(summaryJson);
        PopulateWorldBank(worldBankJson);
        WireButtons();
    }

    //  Summary 

    private void PopulateSummary(string json)
    {
        ProgressSummaryResponse data;
        try { data = JsonUtility.FromJson<ProgressSummaryResponse>(json); }
        catch { Debug.LogError("FinalResultsUI: failed to parse summary\n" + json); return; }

        if (data == null) return;

        //  Outcome header 
        string ending = data.session?.final_ending
                     ?? GameManager.Instance?.finalOutcome
                     ?? "Game Over";

        if (outcomeLabel) outcomeLabel.text = ending;
        if (finalScoreLabel) finalScoreLabel.text = "Final Score: " + (data.totals?.total_score ?? 0);

        // Tint outcome label by result
        if (outcomeLabel)
            outcomeLabel.color = EndingColour(ending);

        //  Journey bars 
        int econ = data.totals?.total_economic ?? 0;
        int soc = data.totals?.total_social ?? 0;
        int health = data.totals?.total_health ?? 0;

        SetBar(financialBar, financialValue, econ, "Economic");
        SetBar(socialBar, socialValue, soc, "Social");
        SetBar(healthBar, healthValue, health, "Health");

        //  Phase decision rows 
        if (data.phases != null && phaseRowContainer != null && phaseRowPrefab != null)
        {
            foreach (var phase in data.phases)
            {
                // Skip phase 6 (reflection only, zero score) and phase 7
                if (phase.phase_number == 6 || phase.phase_number == 7) continue;

                var row = Instantiate(phaseRowPrefab, phaseRowContainer);
                var texts = row.GetComponentsInChildren<TMP_Text>();

                if (texts.Length >= 2)
                {
                    texts[0].text = $"Phase {phase.phase_number} — {phase.choice_text}";
                    int s = phase.score;
                    texts[1].text = (s >= 0 ? "+" : "") + s;
                    texts[1].color = s >= 0 ? colourStabilized : colourCollapse;
                }
            }
        }
    }

    // World Bank context

    private void PopulateWorldBank(string json)
    {
        WorldBankResponse wb;
        try { wb = JsonUtility.FromJson<WorldBankResponse>(json); }
        catch { Debug.LogWarning("FinalResultsUI: could not parse World Bank data."); return; }

        if (wb == null) return;

        string charName = GameManager.Instance?.playerName ?? "Your character";
        string countryName = CountryDisplayName(wb.country_code ?? GameManager.Instance?.countryCode ?? "");

        if (worldBankLabel)
            worldBankLabel.text = $"What happened to {charName} in {countryName}?";

        if (worldBankBody)
        {
            var lines = new System.Text.StringBuilder();

            if (wb.wb_poverty_rate > 0)
                lines.AppendLine($"Real poverty rate: {wb.wb_poverty_rate:F1}%");

            if (wb.wb_gni_per_capita > 0)
            {
                string currency = GameManager.Instance?.CurrencySymbol() ?? "$";
                int monthly = Mathf.RoundToInt(wb.wb_gni_per_capita / 12f);
                lines.AppendLine($"Your starting income was {GameManager.Instance?.FormatMoney(GameManager.Instance?.money ?? 0) ?? ""}" +
                                 $"/month — the World Bank estimates {countryName}'s GNI per capita" +
                                 $" at ${wb.wb_gni_per_capita:N0}/year.");
            }

            if (wb.wb_life_expectancy > 0)
                lines.AppendLine($"Life expectancy: {wb.wb_life_expectancy:F1} years.");

            worldBankBody.text = lines.ToString().TrimEnd();
        }
    }

    //  Helpers 

    private void SetBar(Slider slider, TMP_Text label, int value, string tag)
    {
        if (slider != null)
        {
            slider.minValue = -barRange;
            slider.maxValue = barRange;
            slider.value = Mathf.Clamp(value, -barRange, barRange);
        }
        if (label != null)
            label.text = (value >= 0 ? "" : "") + value + $" ({tag})";
    }

    private Color EndingColour(string ending)
    {
        if (ending.Contains("Stabilized")) return colourStabilized;
        if (ending.Contains("Survival")) return colourSurvival;
        if (ending.Contains("Poverty")) return colourPoverty;
        return colourCollapse;
    }

    private static string CountryDisplayName(string code) => code switch
    {
        "us" => "United States",
        "br" => "Brazil",
        "in" => "India",
        "ke" => "Kenya",
        "se" => "Sweden",
        _ => code.ToUpper()
    };

    private void WireButtons()
    {
        if (replayButton != null)
        {
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(() =>
            {
                if (ScenarioManager.Instance != null)
                    ScenarioManager.Instance.ReplayGame();
            });
        }
    }
}
