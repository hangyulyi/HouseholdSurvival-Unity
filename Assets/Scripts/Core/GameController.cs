using UnityEngine;


//  GameController.Instance.SetAuthFromReact(token, userId);

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    void Awake()
    {
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

    public void SetAuthFromReact(string payload)
    {
        var parts = payload.Split('|');
        if (parts.Length < 2)
        {
            Debug.LogError("GameController.SetAuthFromReact: expected 'token|userId' format.");
            return;
        }

        string token = parts[0];
        string userId = parts[1];

        PlayerPrefs.SetString("token", token);
        PlayerPrefs.SetString("userId", userId);
        PlayerPrefs.Save();

        Debug.Log($"Auth set from React. userId={userId}");
    }

    // Using test on Unity
    public void SetAuthFromReact(string token, string userId)
    {
        PlayerPrefs.SetString("token", token);
        PlayerPrefs.SetString("userId", userId);
        PlayerPrefs.Save();
        Debug.Log($"Auth set. userId={userId}");
    }

    // set token : sendMessage("GameController", "SetToken", jwtToken)
    public void SetToken(string token)
    {
        PlayerPrefs.SetString("token", token);
        PlayerPrefs.Save();
        Debug.Log("Token set from React.");
    }

    // True if a JWT exists
    public bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(PlayerPrefs.GetString("token", ""));
    }

    // Clear credentials and reset
    public void Logout()
    {
        PlayerPrefs.DeleteKey("token");
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.Save();
        GameManager.Instance?.ResetGame();
    }
}
