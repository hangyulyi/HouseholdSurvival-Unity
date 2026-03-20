using UnityEngine;
using TMPro;

public class LogicManagerScript : MonoBehaviour
{
    public int playerScore;
    public TMP_Text scoreText;
    public TMP_Text finalScore;
    public GameObject gameOverSceen;

    [ContextMenu("Increase Score")]
    public void addScore()
    {
        playerScore += 1;
        scoreText.text = playerScore.ToString() + "  <sprite=0>";
    }

    /// <summary>Restart button on the game over screen — full clean reset.</summary>
    public void restartGame()
    {
        if (MinigameManager.Instance != null)
            MinigameManager.Instance.OpenMinigame();
        else
            Debug.LogError("MinigameManager not found.");
    }

    public void gameOver()
    {
        finalScore.text = "<sprite=0>Collected:  " + playerScore.ToString();
        gameOverSceen.SetActive(true);

        int moneyEarned = playerScore * 5;
        if (GameManager.Instance != null)
            GameManager.Instance.AddMoney(moneyEarned);
    }

    public void returnToMainGame()
    {
        if (MinigameManager.Instance != null)
            MinigameManager.Instance.CloseMinigame();
        else
            Debug.LogError("MinigameManager not found.");
    }
}
