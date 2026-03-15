using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles all scenario phases:
///   Phases 1–5 — fetch scenario, show decision buttons, submit choice, then show country event
///   Phase 6     — show a KPI reflection dashboard; one "Continue" button auto-submits the single decision
///   Phase 7     — auto-submits immediately; backend resolves the ending from total score
///
/// UI panels needed in the scene:
///   scenarioCanvas   — phases 1–5 choices
///   reflectionCanvas — phase 6 KPI dashboard
///   eventCanvas      — country-specific events
///   outcomeCanvas    — phase 7 final result
/// </summary>
public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance;

    // ── Phases 1–5: Scenario panel ────────────────────────────────────────────
    [Header("Scenario Panel (Phases 1–5)")]
    public GameObject scenarioCanvas;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text sdgText;
    public Transform buttonContainer;
    public GameObject decisionButtonPrefab;

    // ── Phase 6: Reflection / KPI dashboard ──────────────────────────────────
    [Header("Reflection Panel (Phase 6)")]
    public GameObject reflectionCanvas;
    public TMP_Text reflectionTitleText;
    public TMP_Text reflectionDescriptionText;
    // KPI stat fields — wire these to TMP_Text elements in your dashboard
    public TMP_Text kpiMoneyText;
    public TMP_Text kpiHealthText;
    public TMP_Text kpiStressText;
    public TMP_Text kpiHappinessText;
    public TMP_Text kpiDebtText;
    public TMP_Text kpiEconomicScoreText;
    public TMP_Text kpiSocialScoreText;
    public TMP_Text kpiHealthScoreText;
    public TMP_Text kpiTotalScoreText;
    // The single button on the reflection panel
    public Button reflectionContinueButton;

    // ── Country event panel ───────────────────────────────────────────────────
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

    // ── Phase 7: Final outcome panel ──────────────────────────────────────────
    [Header("Final Outcome Panel (Phase 7)")]
    public GameObject outcomeCanvas;
    public TMP_Text outcomeText;
    public TMP_Text outcomeTotalScoreText;  // optional — shows final score alongside outcome

    // ── Loading overlay ───────────────────────────────────────────────────────
    [Header("Loading")]
    public GameObject loadingOverlay;

    // ── Internal state ────────────────────────────────────────────────────────
    private ScenarioResponse _currentResponse;
    private CountryEventData _pendingEvent;

    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ── Public entry point ────────────────────────────────────────────────────

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

    // ── Scenario loaded ───────────────────────────────────────────────────────

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

        if (phase == 6) ShowReflectionPanel(_currentResponse);
        else if (phase == 7) AutoSubmitPhase7(_currentResponse);
        else DisplayScenario(_currentResponse);
    }

    // ── Phases 1–5: normal scenario ──────────────────────────────────────────

    private void DisplayScenario(ScenarioResponse data)
    {
        titleText.text = data.scenario.title;
        descriptionText.text = data.scenario.description;
        if (sdgText) sdgText.text = data.scenario.sdg_goal ?? "";

        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        if (data.decisions != null)
        {
            foreach (var decision in data.decisions)
            {
                var btn = Instantiate(decisionButtonPrefab, buttonContainer);
                var label = btn.GetComponentInChildren<TMP_Text>();
                if (label) label.text = decision.choice_text;

                var captured = decision;
                btn.GetComponent<Button>().onClick.AddListener(() => OnDecisionChosen(captured));
            }
        }

        scenarioCanvas.SetActive(true);
    }

    private void OnDecisionChosen(DecisionData decision)
    {
        scenarioCanvas.SetActive(false);
        if (loadingOverlay) loadingOverlay.SetActive(true);

        StartCoroutine(
            APIManager.Instance.SubmitDecision(
                decision.decision_id,
                decision.scenario_id,
                GameManager.Instance.countryCode,
                OnDecisionSubmitted
            )
        );

        if (decision.is_minigame_trigger)
            UnityEngine.SceneManagement.SceneManager.LoadScene("Minigame");
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

        // Show country event if the backend included one for this phase
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

    // ── Phase 6: Reflection / KPI dashboard ──────────────────────────────────

    private void ShowReflectionPanel(ScenarioResponse data)
    {
        if (reflectionCanvas == null)
        {
            Debug.LogWarning("ScenarioManager: reflectionCanvas not assigned — " +
                             "auto-continuing phase 6.");
            AutoSubmitReflection(data);
            return;
        }

        // Header
        if (reflectionTitleText) reflectionTitleText.text = data.scenario.title;
        if (reflectionDescriptionText) reflectionDescriptionText.text = data.scenario.description;

        // KPI stats — use FormatMoney for anything money-related
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

        // Wire the Continue button
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

        // Phase 6 has exactly one decision: "Review KPI dashboard"
        var decision = data.decisions != null && data.decisions.Length > 0
            ? data.decisions[0]
            : null;

        if (decision == null)
        {
            Debug.LogWarning("ScenarioManager: no decision found for phase 6 — skipping submit.");
            AdvancePhase();
            return;
        }

        StartCoroutine(
            APIManager.Instance.SubmitDecision(
                decision.decision_id,
                decision.scenario_id,
                GameManager.Instance.countryCode,
                OnReflectionSubmitted
            )
        );
    }

    private void OnReflectionSubmitted(string json)
    {
        if (loadingOverlay) loadingOverlay.SetActive(false);
        // Phase 6 scores are all 0 — nothing to apply, just advance
        AdvancePhase();
    }

    // ── Phase 7: auto-resolve ─────────────────────────────────────────────────

    /// <summary>
    /// Phase 7 has no player choice — the backend resolves the ending from total score.
    /// We submit the first decision in the list (doesn't matter which) and the server
    /// returns final_outcome based on the score thresholds for the player's country.
    /// </summary>
    private void AutoSubmitPhase7(ScenarioResponse data)
    {
        if (loadingOverlay) loadingOverlay.SetActive(true);

        var decision = data.decisions != null && data.decisions.Length > 0
            ? data.decisions[0]
            : null;

        if (decision == null)
        {
            Debug.LogError("ScenarioManager: no decisions found for phase 7.");
            if (loadingOverlay) loadingOverlay.SetActive(false);
            return;
        }

        StartCoroutine(
            APIManager.Instance.SubmitDecision(
                decision.decision_id,
                decision.scenario_id,
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
        catch (System.Exception e)
        {
            Debug.LogError("Phase 7 response parse error: " + e.Message);
            return;
        }

        // Apply the final score adjustments
        GameManager.Instance.ApplyAdjustedScores(response.adjusted_scores);

        string outcome = response.final_outcome ?? "Game Over";
        GameManager.Instance.finalOutcome = outcome;

        ShowFinalOutcomePanel(outcome);
    }

    // ── Country event ─────────────────────────────────────────────────────────

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
                _pendingEvent.event_id,
                choice,
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

    // ── Phase advancement ─────────────────────────────────────────────────────

    private void AdvancePhase()
    {
        GameManager.Instance.NextPhase();
    }

    // ── Final outcome ─────────────────────────────────────────────────────────

    private void ShowFinalOutcomePanel(string outcome)
    {
        if (outcomeCanvas == null)
        {
            Debug.Log("Final Outcome: " + outcome);
            return;
        }

        if (outcomeText) outcomeText.text = outcome;
        if (outcomeTotalScoreText)
            outcomeTotalScoreText.text = "Final Score: " + GameManager.Instance.totalImpactScore;

        outcomeCanvas.SetActive(true);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void HideAllPanels()
    {
        if (scenarioCanvas) scenarioCanvas.SetActive(false);
        if (reflectionCanvas) reflectionCanvas.SetActive(false);
        if (eventCanvas) eventCanvas.SetActive(false);
        if (outcomeCanvas) outcomeCanvas.SetActive(false);
    }
}
