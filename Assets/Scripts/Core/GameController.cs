using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    public string selectedCountry;

    public string token;
    public int userId;

    public int currentScenario = 1;
    public int totalScore = 0;

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

    // save set country
    public void SetCountry(string country)
    {
        selectedCountry = country;
        Debug.Log("Country selected: " +  country);
    }

}
