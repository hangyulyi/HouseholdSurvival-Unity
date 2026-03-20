using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Handles minigame scoring and exiting.
/// returnToMainGame() no longer loads a new scene — it tells MinigameLoader
/// to unload the Minigame scene additively, which restores Main as-is.
/// </summary>
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

    public void restartGame()
    {
        // Restart just the minigame scene content — reset score and reload
        playerScore = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
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
        // Find the MinigameLoader in the Main scene and tell it to unload us.
        // This keeps GameManager, all stats, and CountrySelectionController untouched.
        MinigameLoader loader = FindFirstObjectByType<MinigameLoader>();
        if (loader != null)
        {
            loader.UnloadMinigame();
        }
        else
        {
            // Fallback: MinigameLoader not found (e.g. testing Minigame scene in isolation).
            // Mark session active so CountrySelectionController skips the resume prompt.
            if (GameManager.Instance != null)
                GameManager.Instance.isSessionActive = true;
            SceneManager.LoadScene("Main");
        }
    }
}
