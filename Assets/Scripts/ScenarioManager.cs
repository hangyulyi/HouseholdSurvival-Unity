using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

// Manages all scenarios, accounting for phase 2 with multiple steps
public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance;

    // Phases 1–5: Scenario panel
    [Header("Scenario Panel (Phases 1–5)")]
    public GameObject scenarioCanvas;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text sdgText;
    public Transform buttonContainer;
    public GameObject decisionButtonPrefab;

    // Phase 6
    [Header("Reflection Panel (Phase 6)")]
    public GameObject reflectionCanvas;
    public TMP_Text reflectionTitleText;
    public TMP_Text reflectionDescriptionText;
    public TMP_Text kpiMoneyText;
    public TMP_Text kpiHealthText;
    public TMP_Text kpiStressText;
    public TMP_Text kpiHappinessText;
    public TMP_Text kpiDebtText;
    public TMP_Text kpiEconomicScoreText;
    public TMP_Text kpiSocialScoreText;
    public TMP_Text kpiHealthScoreText;
    public TMP_Text kpiTotalScoreText;
    public Button reflectionContinueButton;
    [Tooltip("The GameObject containing all KPI stat fields — hidden until the player taps View Stats")]
    public GameObject kpiSection;
    [Tooltip("Button that reveals kpiSection")]
    public Button viewKpiButton;

    // Country event panel
    [Header("Country Event Panel")]
    public GameObject eventCanvas;
    public TMP_Text eventTitleText;
    public TMP_Text eventDescriptionText;
    public Button eventChoiceAButton;
    public Button eventChoiceBButton;
    public Button eventChoiceCButton;
    public TMP_Text eventChoiceALabel;
    public TMP_Text eventChoiceBLabel;
    public TMP_Text eventChoiceCLabel;

    // Phase 7: Final outcome panel
    [Header("Final Outcome Panel (Phase 7)")]
    public GameObject outcomeCanvas;
    public TMP_Text outcomeText;
    public TMP_Text outcomeTotalScoreText;
    public Button replayButton;

    [Header("Loading")]
    public GameObject loadingOverlay;

    // phase 2 step 1 counter
    private const int PHASE2_STEP1_COUNT = 3;

    // Internal state 
    private ScenarioResponse _currentResponse;
    private CountryEventData _pendingEvent;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start() { }


    // Public entry point 

    public void StartScenario()
    {
        int phase = GameManager.Instance.phase;

        if (phase > GameManager.MAX_PHASES)
        {
            Debug.Log("All phases complete.");
            return;
        }

        if (loadingOverlay) loadingOverlay.SetActive(true);
        HideAllPanels();

        StartCoroutine(
            APIManager.Instance.LoadScenario(
                phase,
                GameManager.Instance.countryCode,
                OnScenarioLoaded
            )
        );
    }

    // Scenario loaded 

    private void OnScenarioLoaded(string json)
    {
        if (loadingOverlay) loadingOverlay.SetActive(false);

        try { _currentResponse = JsonUtility.FromJson<ScenarioResponse>(json); }
        catch (System.Exception e)
        {
            Debug.LogError("ScenarioManager: failed to parse scenario JSON\n" + e.Message + "\n" + json);
            return;
        }

        if (_currentResponse?.scenario == null)
        {
            Debug.LogError("ScenarioManager: scenario is null.\n" + json);
            return;
        }

        int phase = _currentResponse.scenario.phase_number;

        if (phase == 2) DisplayPhase2Step1(_currentResponse);
        else if (phase == 6) ShowReflectionPanel(_currentResponse);
        else if (phase == 7) AutoSubmitPhase7(_currentResponse);
        else DisplayScenario(_currentResponse);
    }

    // Standard scenario display (phases 1, 3, 4, 5)

    private void DisplayScenario(ScenarioResponse data)
    {
        titleText.text = data.scenario.title;
        descriptionText.text = data.scenario.description;
        if (sdgText) sdgText.text = data.scenario.sdg_goal ?? "";

        SpawnDecisionButtons(data.decisions,
            decision => OnDecisionChosen(decision, isPhase2Step1: false));

        scenarioCanvas.SetActive(true);
    }

    private void OnDecisionChosen(DecisionData decision, bool isPhase2Step1)
    {
        scenarioCanvas.SetActive(false);
        if (loadingOverlay) loadingOverlay.SetActive(true);

        StartCoroutine(
            APIManager.Instance.SubmitDecision(
                decision.decision_id,
                decision.scenario_id,
                GameManager.Instance.countryCode,
                isPhase2Step1
                    ? (System.Action<string>)(json => OnPhase2Step1Submitted(json, decision))
                    : OnDecisionSubmitted
            )
        );
    }

    private void OnDecisionSubmitted(string json)
    {
        if (loadingOverlay) loadingOverlay.SetActive(false);

        DecisionSubmitResponse response;
        try { response = JsonUtility.FromJson<DecisionSubmitResponse>(json); }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse decision submit response: " + e.Message);
            return;
        }

        GameManager.Instance.ApplyAdjustedScores(response.adjusted_scores);

        if (!string.IsNullOrEmpty(response.final_outcome))
        {
            GameManager.Instance.finalOutcome = response.final_outcome;
            ShowFinalOutcomePanel(response.final_outcome);
            return;
        }

        if (_currentResponse?.country_event != null &&
            !string.IsNullOrEmpty(_currentResponse.country_event.event_title))
        {
            _pendingEvent = _currentResponse.country_event;
            ShowCountryEvent(_pendingEvent);
        }
        else
        {
            AdvancePhase();
        }
    }

    // Phase 2: Step 1

    private void DisplayPhase2Step1(ScenarioResponse data)
    {
        titleText.text = data.scenario.title;
        descriptionText.text = data.scenario.description;
        if (sdgText) sdgText.text = data.scenario.sdg_goal ?? "";

        var allDecisions = data.decisions ?? new DecisionData[0];

        var step1 = allDecisions.Length > PHASE2_STEP1_COUNT
            ? allDecisions[..PHASE2_STEP1_COUNT]
            : allDecisions;

        SpawnDecisionButtons(step1,
            decision => OnDecisionChosen(decision, isPhase2Step1: true));

        scenarioCanvas.SetActive(true);
    }

    private void OnPhase2Step1Submitted(string json, DecisionData chosenDecision)
    {
        if (loadingOverlay) loadingOverlay.SetActive(false);

        DecisionSubmitResponse response;
        try { response = JsonUtility.FromJson<DecisionSubmitResponse>(json); }
        catch (System.Exception e)
        {
            Debug.LogError("Phase 2 step 1 parse error: " + e.Message);
            return;
        }

        GameManager.Instance.ApplyAdjustedScores(response.adjusted_scores);

        var allDecisions = _currentResponse.decisions ?? new DecisionData[0];
        var step3Decisions = allDecisions.Length > PHASE2_STEP1_COUNT
            ? allDecisions[PHASE2_STEP1_COUNT..]
            : new DecisionData[0];

        if (chosenDecision.is_minigame_trigger)
        {
            if (MinigameManager.Instance != null)
            {
                MinigameManager.Instance.OpenMinigame(step3Decisions);
            }
            else
            {
                Debug.LogError("ScenarioManager: MinigameManager not found.");
            }
        }
        else
        {
            // skip minigame, go to step 3
            ShowPhase2Step3(step3Decisions);
        }
    }

    // Phase 2: Step 3 (spend the earnings)
    public void ShowStep3FromMinigame(DecisionData[] step3) => ShowPhase2Step3(step3);

    private void ShowPhase2Step3(DecisionData[] step3Decisions)
    {
        if (step3Decisions == null || step3Decisions.Length == 0)
        {
            // ensure if no step 3, able to advance
            AfterPhase2Complete();
            return;
        }

        titleText.text = "Phase 2: How will you use the money?";
        descriptionText.text = "Decide how to allocate what you earned (or saved).";
        if (sdgText) sdgText.text = "SDG 1 – No Poverty";

        SpawnDecisionButtons(step3Decisions, OnPhase2Step3Chosen);

        scenarioCanvas.SetActive(true);
    }

    private void OnPhase2Step3Chosen(DecisionData decision)
    {
        scenarioCanvas.SetActive(false);
        if (loadingOverlay) loadingOverlay.SetActive(true);

        StartCoroutine(
            APIManager.Instance.SubmitDecision(
                decision.decision_id,
                decision.scenario_id,
                GameManager.Instance.countryCode,
                OnPhase2Step3Submitted
            )
        );
    }

    private void OnPhase2Step3Submitted(string json)
    {
        if (loadingOverlay) loadingOverlay.SetActive(false);

        DecisionSubmitResponse response;
        try { response = JsonUtility.FromJson<DecisionSubmitResponse>(json); }
        catch (System.Exception e)
        {
            Debug.LogError("Phase 2 step 3 parse error: " + e.Message);
            return;
        }

        GameManager.Instance.ApplyAdjustedScores(response.adjusted_scores);
        AfterPhase2Complete();
    }

    private void AfterPhase2Complete()
    {
        // Show the country event for phase 2 if there is one, then advance
        if (_currentResponse?.country_event != null &&
            !string.IsNullOrEmpty(_currentResponse.country_event.event_title))
        {
            _pendingEvent = _currentResponse.country_event;
            ShowCountryEvent(_pendingEvent);
        }
        else
        {
            AdvancePhase();
        }
    }

    // Phase 6: Reflection dashboard 
    private void ShowReflectionPanel(ScenarioResponse data)
    {
        if (reflectionCanvas == null)
        {
            Debug.LogWarning("ScenarioManager: reflectionCanvas not assigned — auto-continuing.");
            AutoSubmitReflection(data);
            return;
        }

        // Header (always visible)
        if (reflectionTitleText) reflectionTitleText.text = data.scenario.title;
        if (reflectionDescriptionText) reflectionDescriptionText.text = data.scenario.description;

        // KPI section — hidden until player taps "View Stats"
        if (kpiSection) kpiSection.SetActive(false);
        if (reflectionDescriptionText) reflectionDescriptionText.gameObject.SetActive(true);

        // Populate KPI values now so they're ready when revealed
        var gm = GameManager.Instance;
        if (kpiMoneyText) kpiMoneyText.text = "Money: " + gm.FormatMoney(gm.money);
        if (kpiDebtText) kpiDebtText.text = "Debt: " + gm.FormatMoney(gm.debt);
        if (kpiHealthText) kpiHealthText.text = "Health: " + gm.health + " / 100";
        if (kpiStressText) kpiStressText.text = "Stress: " + gm.stress + " / 100";
        if (kpiHappinessText) kpiHappinessText.text = "Happiness: " + gm.happiness + " / 100";
        if (kpiEconomicScoreText) kpiEconomicScoreText.text = "Economic Score: " + gm.economicScore;
        if (kpiSocialScoreText) kpiSocialScoreText.text = "Social Score: " + gm.socialScore;
        if (kpiHealthScoreText) kpiHealthScoreText.text = "Health Score: " + gm.healthScore;
        if (kpiTotalScoreText) kpiTotalScoreText.text = "Total Score: " + gm.totalImpactScore;

        // View Stats button
        if (viewKpiButton != null)
        {
            viewKpiButton.gameObject.SetActive(true);
            viewKpiButton.onClick.RemoveAllListeners();
            viewKpiButton.onClick.AddListener(() =>
            {
                if (kpiSection) kpiSection.SetActive(true);
                if (reflectionDescriptionText) reflectionDescriptionText.gameObject.SetActive(false);
                if (viewKpiButton) viewKpiButton.gameObject.SetActive(false);
            });
        }

        if (reflectionContinueButton != null)
        {
            reflectionContinueButton.onClick.RemoveAllListeners();
            reflectionContinueButton.onClick.AddListener(() => AutoSubmitReflection(data));
        }

        reflectionCanvas.SetActive(true);
    }

    private void AutoSubmitReflection(ScenarioResponse data)
    {
        if (reflectionCanvas) reflectionCanvas.SetActive(false);
        if (loadingOverlay) loadingOverlay.SetActive(true);

        var decision = data.decisions != null && data.decisions.Length > 0
            ? data.decisions[0] : null;

        if (decision == null) { AdvancePhase(); return; }

        StartCoroutine(
            APIManager.Instance.SubmitDecision(
                decision.decision_id, decision.scenario_id,
                GameManager.Instance.countryCode,
                _ => { if (loadingOverlay) loadingOverlay.SetActive(false); AdvancePhase(); }
            )
        );
    }

    // Phase 7
    private void AutoSubmitPhase7(ScenarioResponse data)
    {
        if (loadingOverlay) loadingOverlay.SetActive(true);

        var decision = data.decisions != null && data.decisions.Length > 0
            ? data.decisions[0] : null;

        if (decision == null)
        {
            Debug.LogError("ScenarioManager: no decisions for phase 7.");
            if (loadingOverlay) loadingOverlay.SetActive(false);
            return;
        }

        StartCoroutine(
            APIManager.Instance.SubmitDecision(
                decision.decision_id, decision.scenario_id,
                GameManager.Instance.countryCode,
                OnPhase7Submitted
            )
        );
    }

    private void OnPhase7Submitted(string json)
    {
        if (loadingOverlay) loadingOverlay.SetActive(false);

        DecisionSubmitResponse response;
        try { response = JsonUtility.FromJson<DecisionSubmitResponse>(json); }
        catch (System.Exception e) { Debug.LogError("Phase 7 parse error: " + e.Message); return; }

        GameManager.Instance.ApplyAdjustedScores(response.adjusted_scores);

        string outcome = response.final_outcome ?? "Game Over";
        GameManager.Instance.finalOutcome = outcome;
        ShowFinalOutcomePanel(outcome);
    }

    // Country event

    private void ShowCountryEvent(CountryEventData ev)
    {
        eventTitleText.text = ev.event_title;
        eventDescriptionText.text = ev.event_description;

        eventChoiceALabel.text = ev.choice_a_text;
        eventChoiceAButton.gameObject.SetActive(true);
        eventChoiceAButton.onClick.RemoveAllListeners();
        eventChoiceAButton.onClick.AddListener(() => OnEventChoiceSelected("a"));

        eventChoiceBLabel.text = ev.choice_b_text;
        eventChoiceBButton.gameObject.SetActive(true);
        eventChoiceBButton.onClick.RemoveAllListeners();
        eventChoiceBButton.onClick.AddListener(() => OnEventChoiceSelected("b"));

        bool hasC = !string.IsNullOrEmpty(ev.choice_c_text);
        eventChoiceCButton.gameObject.SetActive(hasC);
        if (hasC)
        {
            eventChoiceCLabel.text = ev.choice_c_text;
            eventChoiceCButton.onClick.RemoveAllListeners();
            eventChoiceCButton.onClick.AddListener(() => OnEventChoiceSelected("c"));
        }

        eventCanvas.SetActive(true);
    }

    private void OnEventChoiceSelected(string choice)
    {
        eventCanvas.SetActive(false);
        if (loadingOverlay) loadingOverlay.SetActive(true);

        StartCoroutine(
            APIManager.Instance.SubmitEventDecision(
                _pendingEvent.event_id, choice,
                GameManager.Instance.countryCode,
                OnEventDecisionSubmitted
            )
        );
    }

    private void OnEventDecisionSubmitted(string json)
    {
        if (loadingOverlay) loadingOverlay.SetActive(false);
        try
        {
            var response = JsonUtility.FromJson<EventDecisionResponse>(json);
            GameManager.Instance.ApplyEventDelta(response.delta);
        }
        catch (System.Exception e) { Debug.LogError("Event decision parse error: " + e.Message); }

        AdvancePhase();
    }

    // Replay

    public void ReplayGame()
    {
        if (replayButton) replayButton.interactable = false;
        if (loadingOverlay) loadingOverlay.SetActive(true);
        StartCoroutine(APIManager.Instance.ResetProgress(OnProgressReset));
    }

    private void OnProgressReset()
    {
        if (loadingOverlay) loadingOverlay.SetActive(false);
        HideAllPanels();
        GameManager.Instance.ResetGame();
    }

    // Helpers

    private void AdvancePhase() => GameManager.Instance.NextPhase();

    private void ShowFinalOutcomePanel(string outcome)
    {
        if (FinalResultsUI.Instance != null)
        {
            FinalResultsUI.Instance.Show();
            return;
        }

        // Fallback
        if (outcomeCanvas == null) { Debug.Log("Final Outcome: " + outcome); return; }

        if (outcomeText) outcomeText.text = outcome;
        if (outcomeTotalScoreText)
            outcomeTotalScoreText.text = "Final Score: " + GameManager.Instance.totalImpactScore;

        if (replayButton != null)
        {
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(ReplayGame);
            replayButton.gameObject.SetActive(true);
        }

        outcomeCanvas.SetActive(true);
    }

    private void SpawnDecisionButtons(DecisionData[] decisions, System.Action<DecisionData> onClick)
    {
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        if (decisions == null) return;

        foreach (var decision in decisions)
        {
            var btn = Instantiate(decisionButtonPrefab, buttonContainer);
            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label) label.text = decision.choice_text;

            var captured = decision;
            btn.GetComponent<Button>().onClick.AddListener(() => onClick(captured));
        }
    }

    private void HideAllPanels()
    {
        if (scenarioCanvas) scenarioCanvas.SetActive(false);
        if (reflectionCanvas) reflectionCanvas.SetActive(false);
        if (eventCanvas) eventCanvas.SetActive(false);
        if (outcomeCanvas) outcomeCanvas.SetActive(false);
    }
}
