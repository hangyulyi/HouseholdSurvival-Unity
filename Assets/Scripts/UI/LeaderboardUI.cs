using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Fetches and displays the leaderboard from GET /api/progress/leaderboard.
///
/// </summary>
public class LeaderboardUI : MonoBehaviour
{
    [Header("Filter")]
    public Toggle filterByCountryToggle;

    [Header("Row")]
    public Transform rowContainer;
    public GameObject rowPrefab;

    [Header("Status")]
    public TMP_Text statusText;
    public GameObject loadingOverlay;

    void OnEnable()
    {
        LoadLeaderboard();
    }

    public void LoadLeaderboard()
    {
        if (loadingOverlay) loadingOverlay.SetActive(true);

        string code = "";
        if (filterByCountryToggle != null && filterByCountryToggle.isOn)
            code = GameManager.Instance?.countryCode ?? "";

        StartCoroutine(APIManager.Instance.GetLeaderboard(code, OnLeaderboardLoaded));
    }

    private void OnLeaderboardLoaded(string json)
    {
        if (loadingOverlay) loadingOverlay.SetActive(false);

        LeaderboardResponse response;
        try
        {
            response = JsonUtility.FromJson<LeaderboardResponse>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Leaderboard parse error: " + e.Message);
            if (statusText) statusText.text = "Failed to load leaderboard.";
            return;
        }

        // Clear old rows
        foreach (Transform child in rowContainer)
            Destroy(child.gameObject);

        if (response.leaderboard == null || response.leaderboard.Length == 0)
        {
            if (statusText) statusText.text = "No entries yet.";
            return;
        }

        if (statusText) statusText.text = "";

        for (int i = 0; i < response.leaderboard.Length; i++)
        {
            var entry = response.leaderboard[i];
            var row = Instantiate(rowPrefab, rowContainer);

            // Expects first TMP_Text child = rank+name, second = score
            var texts = row.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 2)
            {
                texts[0].text = $"{i + 1}. {entry.username ?? entry.email} [{entry.country_code?.ToUpper()}]";
                texts[1].text = entry.total_score.ToString();
            }
        }
    }
}
