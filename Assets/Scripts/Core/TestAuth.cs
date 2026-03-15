using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

/// <summary>
/// EDITOR / TESTING ONLY — auto-logs in to the backend and stores the JWT so
/// the rest of the game can make authenticated API calls without needing React.
///
/// Setup:
///   1. Create an empty GameObject called "DevAuthHelper" in your Main scene.
///   2. Attach this script.
///   3. Fill in testEmail and testPassword in the Inspector (must match a real
///      user in your database — register one first via the backend if needed).
///   4. Make sure "Only Run In Editor" is ticked so it never ships.
///
/// In a real build, React handles login and sets PlayerPrefs["token"] before
/// the Unity scene loads. This script only fills that gap during local testing.
/// </summary>
public class DevAuthHelper : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Test Credentials (Editor only — never ships)")]
    public string testEmail = "test@example.com";
    public string testPassword = "password123";

    [Tooltip("Tick this to skip auto-login (useful if you already have a token from a previous run)")]
    public bool skipIfTokenExists = true;

    // Set in Inspector to match your backend URL
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

    // Minimal response shape — only fields we need
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
