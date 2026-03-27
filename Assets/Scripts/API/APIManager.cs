using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;

// Singleton HTTP client for the Household Survival backend.
// React handles login and stores the JWT + userId in PlayerPrefs before loading the Unity scene.
// Expected PlayerPrefs keys:  "token"  (JWT string)
//                             "userId" (int as string)

public class APIManager : MonoBehaviour
{
    public static APIManager Instance;


    [SerializeField] private string baseUrl = "https://household-survival-production.up.railway.app";

    void Awake()
    {
        transform.SetParent(null);

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Auth helpers

    private string GetToken() => PlayerPrefs.GetString("token", "");

    private UnityWebRequest AuthorizedGet(string url)
    {
        var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + GetToken());
        return req;
    }

    private UnityWebRequest AuthorizedPost(string url, string json)
    {
        byte[] body = Encoding.UTF8.GetBytes(json);
        var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + GetToken());
        return req;
    }

    // ────────────────────────────────────────────────────────────────────────
    // COUNTRIES
    // ────────────────────────────────────────────────────────────────────────

    // GET /api/countries  no auth required, returns CountriesResponse
    public IEnumerator GetAllCountries(System.Action<string> callback)
    {
        using var req = UnityWebRequest.Get(baseUrl + "/api/countries");
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            callback(req.downloadHandler.text);
        else
            Debug.LogError("GetAllCountries failed: " + req.error);
    }

    // GET /api/countries/:code — returns country data + its events
    public IEnumerator GetCountry(string countryCode, System.Action<string> callback)
    {
        using var req = UnityWebRequest.Get(baseUrl + "/api/countries/" + countryCode);
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            callback(req.downloadHandler.text);
        else
            Debug.LogError("GetCountry failed: " + req.error);
    }

    // GET /api/countries/:code/events/:phase
    public IEnumerator GetCountryEvent(string countryCode, int phase, System.Action<string> callback)
    {
        string url = $"{baseUrl}/api/countries/{countryCode}/events/{phase}";
        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            callback(req.downloadHandler.text);
        else
            Debug.LogError("GetCountryEvent failed: " + req.error);
    }

    // ────────────────────────────────────────────────────────────────────────
    // SESSIONS
    // ────────────────────────────────────────────────────────────────────────

    /// POST /api/sessions/start
    /// Call this when the player confirms their country selection.
    /// Returns a SessionStartResponse containing session info + country starting stats.
    public IEnumerator StartSession(string countryCode, string characterName, System.Action<string> callback)
    {
        string json = $"{{\"country_code\":\"{countryCode}\",\"character_name\":\"{characterName}\"}}";
        using var req = AuthorizedPost(baseUrl + "/api/sessions/start", json);
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            callback(req.downloadHandler.text);
        else
            Debug.LogError("StartSession failed: " + req.error + "\n" + req.downloadHandler.text);
    }

    // ────────────────────────────────────────────────────────────────────────
    // SCENARIOS
    // ────────────────────────────────────────────────────────────────────────

    /// GET /api/scenarios/:id?country=code
    /// scenarioId matches phase number (phase 1 = scenario_id 1, etc.)
    /// Returns ScenarioResponse with scenario, decisions, and optional country_event.
    /// Requires auth token
    public IEnumerator LoadScenario(int scenarioId, string countryCode, System.Action<string> callback)
    {
        string url = $"{baseUrl}/api/scenarios/{scenarioId}?country={countryCode}";
        using var req = AuthorizedGet(url);
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            callback(req.downloadHandler.text);
        else
            Debug.LogError("LoadScenario failed: " + req.error + "\n" + req.downloadHandler.text);
    }

    // ────────────────────────────────────────────────────────────────────────
    // DECISIONS
    // ────────────────────────────────────────────────────────────────────────

    // POST /api/decisions/submit Submits a scenario decision and returns adjusted scores + optional final outcome
    public IEnumerator SubmitDecision(int decisionId, int scenarioId, string countryCode,
                                      System.Action<string> callback)
    {
        string json = $"{{\"decision_id\":{decisionId},\"scenario_id\":{scenarioId},\"country_code\":\"{countryCode}\"}}";
        using var req = AuthorizedPost(baseUrl + "/api/decisions/submit", json);
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            callback(req.downloadHandler.text);
        else
            Debug.LogError("SubmitDecision failed: " + req.error + "\n" + req.downloadHandler.text);
    }

    // POST /api/decisions/submit-event Submits a country-specific event choice
    public IEnumerator SubmitEventDecision(int eventId, string chosenChoice, string countryCode,
                                           System.Action<string> callback)
    {
        string json = $"{{\"event_id\":{eventId},\"chosen_choice\":\"{chosenChoice}\",\"country_code\":\"{countryCode}\"}}";
        using var req = AuthorizedPost(baseUrl + "/api/decisions/submit-event", json);
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            callback(req.downloadHandler.text);
        else
            Debug.LogError("SubmitEventDecision failed: " + req.error + "\n" + req.downloadHandler.text);
    }

    // ────────────────────────────────────────────────────────────────────────
    // PROGRESS
    // ────────────────────────────────────────────────────────────────────────

    // GET /api/progress  returns all phase progress for this player
    public IEnumerator GetProgress(System.Action<string> callback)
    {
        using var req = AuthorizedGet(baseUrl + "/api/progress");
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            callback(req.downloadHandler.text);
        else
            Debug.LogError("GetProgress failed: " + req.error);
    }

    // GET /api/progress/leaderboard?country=code
    public IEnumerator GetLeaderboard(string countryCode, System.Action<string> callback)
    {
        string url = baseUrl + "/api/progress/leaderboard";
        if (!string.IsNullOrEmpty(countryCode))
            url += "?country=" + countryCode;

        using var req = AuthorizedGet(url);
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            callback(req.downloadHandler.text);
        else
            Debug.LogError("GetLeaderboard failed: " + req.error);
    }

    // POST /api/progress/reset  wipes all progress for this player
    public IEnumerator ResetProgress(System.Action callback)
    {
        using var req = AuthorizedPost(baseUrl + "/api/progress/reset", "{}");
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            callback?.Invoke();
        else
            Debug.LogError("ResetProgress failed: " + req.error);
    }

    // GET /api/progress/summary  phase decisions + totals + session for results screen
    public IEnumerator GetProgressSummary(System.Action<string> callback)
    {
        using var req = AuthorizedGet(baseUrl + "/api/progress/summary");
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            callback(req.downloadHandler.text);
        else
        {
            Debug.LogError("GetProgressSummary failed: " + req.error);
            callback?.Invoke(null);
        }
            
            
    }

    // GET /api/countries/:code/worldbank  live World Bank indicators
    public IEnumerator GetWorldBankData(string countryCode, System.Action<string> callback)
    {
        using var req = UnityWebRequest.Get(baseUrl + "/api/countries/" + countryCode + "/worldbank");
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            callback(req.downloadHandler.text);
        else
        {
            Debug.LogError("GetWorldBankData failed: " + req.error);
            callback?.Invoke(null);
        }
            
    }

}