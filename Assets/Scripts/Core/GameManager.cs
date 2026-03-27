using UnityEngine;
using UnityEngine.Events;


// Central game state. Persists across scenes.
// Starting stats are populated from the backend when a session begins.

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Player identity 
    public string playerName = "";
    public string countryCode = "";
    public int sessionId = -1;

    // Game phase 
    public int phase = 1;
    public const int MAX_PHASES = 7;

    // Player stats 
    public int money = 0;
    public int health = 70;
    public int stress = 30;
    public int happiness = 60;
    public int debt = 0;

    // Running scores 
    public int economicScore = 0;
    public int socialScore = 0;
    public int healthScore = 0;
    public int environmentalScore = 0;
    public int totalImpactScore = 0;

    // Country multipliers 
    public float healthcareCostMult = 1f;
    public float educationAccessMult = 1f;
    public float safetyNetMult = 1f;

    // Final outcome 
    public string finalOutcome = "";

    //  Phase 2: step-3 decisions saved across the minigame scene load 
    // ScenarioManager stores these here before loading the Minigame scene.
    // On return, ScenarioManager.Start() picks them up and shows them.
    [HideInInspector] public DecisionData[] pendingPhase2Step3 = null;

    // UI event 
    public UnityEvent onStatsChanged = new UnityEvent();


    // Multiplier applied to economic_score when updating the displayed money value.
    // economic_score values range roughly -20 to +20 — without scaling this is
    // nearly invisible against a starting_money of 90–600.
    // Default of 5 makes each decision feel like a meaningful money swing.

    [Tooltip("How much each economic score point moves the displayed money amount")]
    public int moneyScaleFactor = 1;


    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    // Session init 

    public void InitialiseFromCountryConfig(CountryData config, SessionData session)
    {
        countryCode = config.country_code;
        sessionId = session.session_id;

        money = config.starting_money;
        health = config.starting_health;
        stress = config.starting_stress;
        happiness = config.starting_happiness;
        debt = config.starting_debt;

        healthcareCostMult = config.healthcare_cost_mult;
        educationAccessMult = config.education_access_mult;
        safetyNetMult = config.safety_net_mult;

        economicScore = socialScore = healthScore = environmentalScore = totalImpactScore = 0;
        phase = 1;
        finalOutcome = "";

        // Persist so CountrySelectionController can read it after a scene reload
        PlayerPrefs.SetString("countryCode", countryCode);
        PlayerPrefs.SetString("playerName", playerName);
        PlayerPrefs.SetString("savedPhase", "1");
        PlayerPrefs.DeleteKey("gameCompleted");
        PlayerPrefs.Save();

        onStatsChanged.Invoke();
    }

    // Score application 
    public void ApplyAdjustedScores(AdjustedScores scores)
    {
        economicScore += scores.economic_score;
        socialScore += scores.social_score;
        healthScore += scores.health_score;
        environmentalScore += scores.environmental_score;
        totalImpactScore += scores.impact_score;

        money = Mathf.Clamp(money + scores.economic_score * moneyScaleFactor, -99999, 99999);
        health = Mathf.Clamp(health + scores.health_score, 0, 100);
        happiness = Mathf.Clamp(happiness + scores.social_score, 0, 100);
        stress = Mathf.Clamp(stress - scores.social_score / 2, 0, 100);

        onStatsChanged.Invoke();
    }

    public void ApplyEventDelta(EventDelta delta)
    {
        economicScore += delta.economic;
        socialScore += delta.social;
        healthScore += delta.health;
        totalImpactScore += delta.impact;

        money = Mathf.Clamp(money + delta.economic * moneyScaleFactor, -99999, 99999);
        health = Mathf.Clamp(health + delta.health, 0, 100);
        happiness = Mathf.Clamp(happiness + delta.social, 0, 100);

        onStatsChanged.Invoke();
    }

    // Phase 

    public void NextPhase()
    {
        if (phase > MAX_PHASES)
        {
            return;
        }
        phase++;
        PlayerPrefs.SetString("savedPhase", phase.ToString());
        PlayerPrefs.Save();
        onStatsChanged.Invoke();
    }

    public bool IsGameOver() => phase > MAX_PHASES;

    // Currency formatting

    /// <summary>
    /// Returns the money value formatted with the correct currency symbol/code
    /// for the currently selected country.
    ///   us  →  $400
    ///   br  →  R$220
    ///   in  →  ₹180
    ///   ke  →  KSh90
    ///   se  →  600 kr
    /// </summary>
    public string FormatMoney(int amount)
    {
        // handle negative
        if (amount < 0)
        {
            int abs = Mathf.Abs(amount);
            return countryCode switch
            {
                "us" => $"-${abs}",
                "br" => $"-R${abs}",
                "in" => $"-₹{abs}",
                "ke" => $"-KSh{abs}",
                "se" => $"-{abs} kr",
                _ => $"-${abs}"
            };
        }
            return countryCode switch
        {
            "us" => $"${amount}",
            "br" => $"R${amount}",
            "in" => $"₹{amount}",
            "ke" => $"KSh{amount}",
            "se" => $"{amount} kr",
            _ => $"${amount}"   // fallback
        };
    }

    public string CurrencySymbol()
    {
        return countryCode switch
        {
            "us" => "$",
            "br" => "R$",
            "in" => "₹",
            "ke" => "KSh",
            "se" => "kr",
            _ => "$"
        };
    }

    // Helpers 

    public void AddMoney(int amount)
    {
        money = Mathf.Clamp(money + amount, -99999, 99999);
        onStatsChanged.Invoke();
    }

    public static string NameToCode(string name) => name switch
    {
        "Brazil" => "br",
        "India" => "in",
        "Kenya" => "ke",
        "Sweden" => "se",
        "USA" => "us",
        _ => name.ToLower()
    };

    public void ResetGame()
    {
        playerName = ""; countryCode = ""; sessionId = -1; phase = 1;
        money = 0; health = 70; stress = 30; happiness = 60; debt = 0;
        economicScore = socialScore = healthScore = environmentalScore = totalImpactScore = 0;
        finalOutcome = "";
        pendingPhase2Step3 = null;
        PlayerPrefs.DeleteKey("countryCode");
        PlayerPrefs.DeleteKey("savedPhase");
        PlayerPrefs.DeleteKey("playerName");
        PlayerPrefs.DeleteKey("gameCompleted");
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }
}
