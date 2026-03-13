using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;

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
    public IEnumerator LoadScenario(int scenarioId, string country, System.Action<string> callback)
    {
        string url = BASE_URL + "/api/scenarios/" + scenarioId;

        UnityWebRequest req = UnityWebRequest.Get(url);

        req.SetRequestHeader("Authorization", "Bearer " + GetToken());

        yield return req.SendWebRequest();

        callback(req.downloadHandler.text);
    }

    /*
        Store decisions
     */
    public IEnumerator SubmitDecision(int decisionId, int scenarioId)
    {
        string json = $"{{\"decision_id\":{decisionId},\"scenario_id\":{scenarioId}}}";
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest req = new UnityWebRequest(BASE_URL + "/api/decisions/submit", "POST");

        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();

        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + GetToken());

        yield return req.SendWebRequest();

        Debug.Log(req.downloadHandler.text);
    }


}
