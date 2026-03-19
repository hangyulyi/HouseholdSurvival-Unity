using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameLoader : MonoBehaviour
{
    public void loadMinigame()
    {
        SceneManager.LoadScene("Minigame");
    }
}
