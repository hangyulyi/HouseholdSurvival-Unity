using UnityEngine;

/// <summary>
/// Thin bridge that receives auth credentials from the React frontend
/// and exposes a clean entry point for the rest of the Unity game.
///
/// React should call one of these methods (via Unity's JSlib or
/// by setting PlayerPrefs before the scene loads):
///
///   GameController.Instance.SetAuthFromReact(token, userId);
///
/// or pre-populate PlayerPrefs "token" and "userId" before the scene loads.
/// </summary>
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

    /// <summary>
    /// Called by the React host page (WebGL build) after the player logs in.
    /// Stores credentials in PlayerPrefs so APIManager can attach them to every request.
    /// </summary>
    public void SetAuthFromReact(string token, string userId)
    {
        PlayerPrefs.SetString("token", token);
        PlayerPrefs.SetString("userId", userId);
        PlayerPrefs.Save();
        Debug.Log("Auth credentials set from React.");
    }

    /// <summary>True if a JWT exists — does not verify expiry.</summary>
    public bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(PlayerPrefs.GetString("token", ""));
    }

    /// <summary>Clear credentials and return to the Main scene.</summary>
    public void Logout()
    {
        PlayerPrefs.DeleteKey("token");
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }
}
