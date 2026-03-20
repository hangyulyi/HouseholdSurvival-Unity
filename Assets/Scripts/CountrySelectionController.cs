using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Handles the country selection screen.
/// On Start it fetches all country data from GET /api/countries (public, no auth needed)
/// and uses the real intro_text and difficulty_label from the database for each blurb.
/// Each country has a fixed character — the player does not enter their own name.
/// On ConfirmSelection it calls POST /api/sessions/start to initialise the session.
/// </summary>
public class CountrySelectionController : MonoBehaviour
{
    [Header("Flag Buttons")]
    public Button brazilButton;
    public Button indiaButton;
    public Button kenyaButton;
    public Button swedenButton;
    public Button usaButton;

    [Header("Flag Images")]
    public Image brazilImage;
    public Image indiaImage;
    public Image kenyaImage;
    public Image swedenImage;
    public Image usaImage;

    [Header("Map Backgrounds")]
    public GameObject brazilMap;
    public GameObject indiaMap;
    public GameObject kenyaMap;
    public GameObject swedenMap;
    public GameObject usaMap;

    [Header("Blurb")]
    public TMP_Text countryName;
    public TMP_Text countryBlurb;
    public TMP_Text difficultyText;     // optional dedicated difficulty label

    [Header("Confirm Panel")]
    public GameObject confirmPanel;
    public Image selectedFlagImage;
    public TMP_Text characterNameText; // displays e.g. "Playing as: Lucas"

    [Header("Loading / Error")]
    public GameObject loadingOverlay;
    public TMP_Text errorText;

    [Header("Player Status")]
    public PlayerStatusUI playerStatusUI;
    public GameObject playerStatusCanvas;

    [Header("Resume Prompt")]
    [Tooltip("Panel shown when a saved session exists. Has Continue and New Game buttons.")]
    public GameObject resumePromptPanel;
    public TMPro.TMP_Text resumePromptText;   // optional — shows character + country name

    // ── Fixed character per country (from the schema's intro_text characters) ──
    private static readonly Dictionary<string, string> CharacterNames = new()
    {
        { "br", "Lucas"  },
        { "in", "Arjun"  },
        { "ke", "James"  },
        { "se", "Erik"   },
        { "us", "Thomas" },
    };

    // ── Internal state ────────────────────────────────────────────────────────
    private string _selectedCountry = "Brazil";

    /// <summary>Keyed by backend country_code (br / in / ke / se / us).</summary>
    private Dictionary<string, CountryData> _countryDataByCode = new();

    // ─────────────────────────────────────────────────────────────────────────

    // ── Static flag — survives disable/enable and scene reloads ─────────────────
    // Resets only when ResetGame() is called (new game).
    private static bool _sessionCheckDone = false;

    public static void ResetSessionCheck() => _sessionCheckDone = false;

    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (resumePromptPanel) resumePromptPanel.SetActive(false);
    }

    void Start()
    {
        // Only run the session check once per application launch.
        // This prevents it re-firing when the GameObject is re-enabled
        // (e.g. after returning from the minigame scene).
        if (_sessionCheckDone) return;
        _sessionCheckDone = true;

        // GameManager has a live session — mid-session scene reload (e.g. from minigame).
        // Go straight to the map, no prompts.
        if (GameManager.Instance != null && GameManager.Instance.isSessionActive)
        {
            SkipToMap(GameManager.Instance.countryCode);
            return;
        }

        // No live session but PlayerPrefs has a saved one — app was closed and reopened.
        // Ask the player whether to continue or start fresh.
        string savedCode = PlayerPrefs.GetString("countryCode", "");
        if (!string.IsNullOrEmpty(savedCode))
        {
            ShowResumePrompt(savedCode);
            return;
        }

        // No session at all — show country selection normally.
        if (loadingOverlay) loadingOverlay.SetActive(true);
        if (errorText)      errorText.text = "";
        StartCoroutine(APIManager.Instance.GetAllCountries(OnCountriesLoaded));
    }

    // ── Resume prompt ─────────────────────────────────────────────────────────

    private void ShowResumePrompt(string savedCode)
    {
        if (resumePromptPanel == null)
        {
            // No panel — go straight to country selection
            if (loadingOverlay) loadingOverlay.SetActive(true);
            if (errorText)      errorText.text = "";
            StartCoroutine(APIManager.Instance.GetAllCountries(OnCountriesLoaded));
            return;
        }

        if (resumePromptText != null)
        {
            string name  = PlayerPrefs.GetString("playerName", "Player");
            string phase = PlayerPrefs.GetString("savedPhase", "1");
            resumePromptText.text = $"Continue as {name}?\nPhase {phase} / {GameManager.MAX_PHASES}";
        }

        resumePromptPanel.SetActive(true);
    }

    public void OnResumeContinue()
    {
        if (resumePromptPanel) resumePromptPanel.SetActive(false);

        string savedCode = PlayerPrefs.GetString("countryCode", "");
        string savedName = PlayerPrefs.GetString("playerName", "");
        string savedPhaseStr = PlayerPrefs.GetString("savedPhase", "1");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.countryCode    = savedCode;
            GameManager.Instance.playerName     = savedName;
            GameManager.Instance.isSessionActive = true;
            if (int.TryParse(savedPhaseStr, out int p))
                GameManager.Instance.phase = p;
        }

        SkipToMap(savedCode);
    }

    public void OnResumeNewGame()
    {
        if (resumePromptPanel) resumePromptPanel.SetActive(false);
        _sessionCheckDone = false;

        if (GameManager.Instance != null)
        {
            StartCoroutine(APIManager.Instance.ResetProgress(() =>
            {
                GameManager.Instance.ResetGame();
            }));
        }
        else
        {
            PlayerPrefs.DeleteKey("countryCode");
            PlayerPrefs.DeleteKey("playerName");
            PlayerPrefs.DeleteKey("savedPhase");
            PlayerPrefs.Save();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
        }
    }

    private void SkipToMap(string code)
    {
        if (GameManager.Instance != null && string.IsNullOrEmpty(GameManager.Instance.playerName))
            GameManager.Instance.playerName = PlayerPrefs.GetString("playerName", "");

        gameObject.SetActive(false);
        playerStatusCanvas.SetActive(true);

        string name = GameManager.Instance != null
            ? GameManager.Instance.playerName
            : PlayerPrefs.GetString("playerName", "");

        playerStatusUI.SetName(name);
        playerStatusUI.Refresh();
        ActivateMap(code);
    }

    private void OnCountriesLoaded(string json)
    {
        if (loadingOverlay) loadingOverlay.SetActive(false);

        try
        {
            var response = JsonUtility.FromJson<CountriesResponse>(json);
            if (response?.countries != null)
                foreach (var c in response.countries)
                    _countryDataByCode[c.country_code] = c;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("CountrySelectionController: could not load country data — " +
                             "falling back to placeholder text.\n" + e.Message);
        }

        SelectCountry("Brazil");
    }

    // ── Country selection ─────────────────────────────────────────────────────

    public void SelectCountry(string country)
    {
        _selectedCountry = country;

        Color selected = Color.white;
        Color grey = new Color(0.55f, 0.55f, 0.55f);

        brazilImage.color = grey;
        indiaImage.color = grey;
        kenyaImage.color = grey;
        swedenImage.color = grey;
        usaImage.color = grey;

        string code = GameManager.NameToCode(country);

        if (_countryDataByCode.TryGetValue(code, out CountryData data))
        {
            countryName.text = data.country_name;
            countryBlurb.text = data.intro_text;

            if (difficultyText)
                difficultyText.text = "Difficulty: " + data.difficulty_label;
            else
                countryBlurb.text += $"\n\nDifficulty: {data.difficulty_label}";
        }
        else
        {
            countryName.text = country;
            countryBlurb.text = "Loading country information…";
            if (difficultyText) difficultyText.text = "";
        }

        // Highlight the selected flag
        switch (country)
        {
            case "Brazil": brazilImage.color = selected; break;
            case "India": indiaImage.color = selected; break;
            case "Kenya": kenyaImage.color = selected; break;
            case "Sweden": swedenImage.color = selected; break;
            case "USA": usaImage.color = selected; break;
        }
    }

    // ── Confirm panel ─────────────────────────────────────────────────────────

    public void ShowConfirmPanel()
    {
        confirmPanel.SetActive(true);

        // Show the flag sprite
        selectedFlagImage.sprite = _selectedCountry switch
        {
            "Brazil" => brazilImage.sprite,
            "India" => indiaImage.sprite,
            "Kenya" => kenyaImage.sprite,
            "Sweden" => swedenImage.sprite,
            "USA" => usaImage.sprite,
            _ => selectedFlagImage.sprite
        };

        // Show the fixed character name for this country
        string code = GameManager.NameToCode(_selectedCountry);
        if (characterNameText)
        {
            string name = CharacterNames.TryGetValue(code, out var n) ? n : "Unknown";
            characterNameText.text = "Playing as: " + name;
        }
    }

    public void Cancel()
    {
        confirmPanel.SetActive(false);
    }

    /// <summary>
    /// Confirms the country choice. Uses the fixed character name for the selected country
    /// rather than a player-entered name.
    /// Requires a valid JWT in PlayerPrefs["token"] — set by React login, or DevAuthHelper in the Editor.
    /// </summary>
    public void ConfirmSelection()
    {
        string code = GameManager.NameToCode(_selectedCountry);

        // Look up the fixed character name for this country
        string characterName = CharacterNames.TryGetValue(code, out var n) ? n : "Player";
        GameManager.Instance.playerName = characterName;

        if (loadingOverlay) loadingOverlay.SetActive(true);
        if (errorText) errorText.text = "";

        StartCoroutine(APIManager.Instance.StartSession(code, characterName, OnSessionStarted));
    }

    private void OnSessionStarted(string json)
    {
        if (loadingOverlay) loadingOverlay.SetActive(false);

        SessionStartResponse response;
        try
        {
            response = JsonUtility.FromJson<SessionStartResponse>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse session response: " + e.Message + "\n" + json);
            if (errorText) errorText.text = "Failed to connect to server. Please try again.";
            return;
        }

        if (response == null || response.session == null)
        {
            Debug.LogError("Session response was null.\n" + json);
            if (errorText) errorText.text = "Server error. Please try again.";
            return;
        }

        GameManager.Instance.InitialiseFromCountryConfig(response.country_config, response.session);

        confirmPanel.SetActive(false);
        gameObject.SetActive(false);

        playerStatusUI.Refresh();
        playerStatusCanvas.SetActive(true);

        ActivateMap(response.country_config.country_code);
    }

    // ── Map helpers ───────────────────────────────────────────────────────────

    public void ActivateMap(string code)
    {
        brazilMap.SetActive(false);
        indiaMap.SetActive(false);
        kenyaMap.SetActive(false);
        swedenMap.SetActive(false);
        usaMap.SetActive(false);

        switch (code)
        {
            case "br": case "Brazil": brazilMap.SetActive(true); break;
            case "in": case "India": indiaMap.SetActive(true); break;
            case "ke": case "Kenya": kenyaMap.SetActive(true); break;
            case "se": case "Sweden": swedenMap.SetActive(true); break;
            case "us": case "USA": usaMap.SetActive(true); break;
        }
    }
}
