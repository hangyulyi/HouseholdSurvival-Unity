using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

    public void restartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void gameOver()
    {
        finalScore.text = "<sprite=0>Collected:  " + playerScore.ToString();
        gameOverSceen.SetActive(true);

        // Convert pearls collected to money and apply to GameManager
        int moneyEarned = playerScore * 5;
        if (GameManager.Instance != null)
            GameManager.Instance.AddMoney(moneyEarned);
    }

    public void returnToMainGame()
    {
        // LoadScene("Main") is safe — GameManager is DontDestroyOnLoad so
        // countryCode and all stats survive the scene transition.
        // CountrySelectionController.Start() detects the saved countryCode
        // and skips straight to the map rather than showing country selection.
        SceneManager.LoadScene("Main");
    }
}
