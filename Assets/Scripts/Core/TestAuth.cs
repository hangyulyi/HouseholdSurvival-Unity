using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

// Used for local testing only
public class DevAuthHelper : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Test Credentials (Editor only — never ships)")]
    public string testEmail = "didumb3@gmail.com";
    public string testPassword = "password";

    [Tooltip("Tick this to skip auto-login (useful if you already have a token from a previous run)")]
    public bool skipIfTokenExists = true;

    [SerializeField] private string baseUrl = "http://localhost:3000";

    void Start()
    {
        if (skipIfTokenExists && !string.IsNullOrEmpty(PlayerPrefs.GetString("token", "")))
        {
            Debug.Log("[DevAuthHelper] Token already present — skipping auto-login.");
            return;
        }

        StartCoroutine(AutoLogin());
    }

    private IEnumerator AutoLogin()
    {
        Debug.Log("[DevAuthHelper] Attempting dev login…");

        string json = $"{{\"email\":\"{testEmail}\",\"password\":\"{testPassword}\"}}";
        byte[] body = Encoding.UTF8.GetBytes(json);

        using var req = new UnityWebRequest(baseUrl + "/api/auth/login", "POST");
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[DevAuthHelper] Login failed: {req.error}\n{req.downloadHandler.text}\n\n" +
                           "Make sure the backend is running and the test account exists.\n" +
                           "Register one by POSTing to /api/auth/register with the same credentials.");
            yield break;
        }

        // Parse just the token and user_id out of the response
        string responseText = req.downloadHandler.text;
        var parsed = JsonUtility.FromJson<LoginResponse>(responseText);

        if (string.IsNullOrEmpty(parsed?.token))
        {
            Debug.LogError("[DevAuthHelper] Login response had no token:\n" + responseText);
            yield break;
        }

        PlayerPrefs.SetString("token", parsed.token);
        PlayerPrefs.SetString("userId", parsed.user.user_id.ToString());
        PlayerPrefs.Save();

        Debug.Log($"[DevAuthHelper] Logged in as '{parsed.user.email}' (userId={parsed.user.user_id}). Token stored.");
    }


    [System.Serializable]
    private class LoginResponse
    {
        public string token;
        public UserInfo user;
    }

    [System.Serializable]
    private class UserInfo
    {
        public int user_id;
        public string email;
        public string username;
    }
#endif
}
