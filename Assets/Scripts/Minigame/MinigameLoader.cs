using UnityEngine;

/// <summary>
/// Used by the standalone "Play Minigame" button in the UI.
/// Calls MinigameManager instead of loading a scene.
/// </summary>
public class MinigameLoader : MonoBehaviour
{
    public void loadMinigame()
    {
        if (MinigameManager.Instance != null)
            MinigameManager.Instance.OpenMinigame();
        else
            Debug.LogError("MinigameLoader: MinigameManager not found.");
    }
}
