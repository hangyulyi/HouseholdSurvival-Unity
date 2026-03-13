using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string playerName;
    public string country;

    public int money = 0;
    public int phase = 1;

    public int economicScore;
    public int socialScore;
    public int healthScore;

    public void ApplyDecision(DecisionData decision)
    {
        economicScore += decision.economic_score;
        socialScore += decision.social_score;
    }

    public void UpdateMoneyDisplay()
    {
        
    }

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

    public void AddMoney(int amount)
    {
        money += amount;
    }

    public void NextPhase()
    {
        phase++;
    }

    public void ResetGame()
    {
        PlayerPrefs.DeleteAll();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }

}
