using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using SimpleJSON;

public class APIManager : MonoBehaviour
{
    public static APIManager Instance;

    // update when deployed
    private string BASE_URL = "https://localhost:4000";

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    string GetToken()
    {
        return PlayerPrefs.GetString("token", "");
    }

    /*
        Manage Scenarios
     */
    public IEnumerator LoadScenario(int scenarioId, System.Action<string,string,JSONArray> callback)
    {
        string url = BASE_URL + "/api/scenarios/" + scenarioId;

        UnityWebRequest req = UnityWebRequest.Get(url);

        req.SetRequestHeader("Authorization", "Bearer " + GetToken());

        yield return req.SendWebRequest();

        // Successful request
        if (req.result == UnityWebRequest.Result.Success)
        {
            var data = JSON.Parse(req.downloadHandler.text);

            string title = data["scenario"]["title"];
            string description = data["scenario"]["description"];

            JSONArray decisions = data["decisions"].AsArray;

            callback(title, description, decisions);
        }
        else
        {
            Debug.LogError(req.downloadHandler.text);
        }
    }

    /*
        Store decisions
     */
    public IEnumerator SubmitDecision(int decisionId, int scenarioId, System.Action<int, int, int, int> callback)
    {
        string json = $"{{\"decision_id\":{decisionId},\"scenario_id\":{scenarioId}}}";
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest req = new UnityWebRequest(BASE_URL + "/api/decisions/submit", "POST");

        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();

        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + GetToken());

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var data = JSON.Parse (req.downloadHandler.text);
            var decision = data["chosen_decision"];

            int impact = decision["impact_score"].AsInt;
            int economic = decision["economic_score"].AsInt;
            int social = decision["social_score"].AsInt;
            int environment = decision["environmental_score"].AsInt;

            callback(impact, economic, social, environment);
        }
        else
        {
            Debug.LogError(req.downloadHandler.text);
        }
    }


}
