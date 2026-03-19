using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;

public class APIManager : MonoBehaviour
{
    public static APIManager Instance;

    // update when deployed
    [SerializeField] private string baseUrl = "http://localhost:3000";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ensure to not restart when changing scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

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


    public IEnumerator GetAllCountries(System.Action<string> callback)
    {
        using var req = UnityWebRequest.Get(baseUrl + "/api/countries");
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            callback(req.downloadHandler.text);
        else
            Debug.LogError("GetAllCountries failed: " + req.error);
    }

    public IEnumerator GetCountry(string countryCode, System.Action<string> callback)
    {
        using var req = UnityWebRequest.Get(baseUrl + "/api/countries/" + countryCode);
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            callback(req.downloadHandler.text);
        else
            Debug.LogError("GetCountry failed: " + req.error);
    }


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


    // POST /api/sessions/start

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


    // GET /api/scenarios/:id?country=code

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


    // POST /api/decisions/submit

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


    // POST /api/decisions/submit-event

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



    // GET /api/progress
    public IEnumerator GetProgress(System.Action<string> callback)
    {
        using var req = AuthorizedGet(baseUrl + "/api/progress");
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            callback(req.downloadHandler.text);
        else
            Debug.LogError("GetProgress failed: " + req.error);
    }

    /// GET /api/progress/leaderboard?country=code
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

    //POST /api/progress/reset
    public IEnumerator ResetProgress(System.Action callback)
    {
        using var req = AuthorizedPost(baseUrl + "/api/progress/reset", "{}");
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            callback?.Invoke();
        else
            Debug.LogError("ResetProgress failed: " + req.error);
    }
}
