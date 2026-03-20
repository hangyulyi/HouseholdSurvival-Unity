using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Loads the Minigame scene ADDITIVELY on top of Main so that GameManager
/// is never destroyed and CountrySelectionController.Start() never re-runs.
///
/// Setup:
///   - Assign every root GameObject in the Main scene that should be hidden
///     during the minigame to the mainSceneObjects list (e.g. the player,
///     the tilemap Grid, the PlayerStatusCanvas, the ScenarioManager object).
///   - The minigame scene loads on top; when it unloads everything reappears.
/// </summary>
public class MinigameLoader : MonoBehaviour
{
    [Tooltip("Root GameObjects in Main that should be hidden while the minigame is running.")]
    public GameObject[] mainSceneObjects;

    public void loadMinigame()
    {
        StartCoroutine(LoadMinigameAdditive());
    }

    private IEnumerator LoadMinigameAdditive()
    {
        // Hide main scene objects
        SetMainObjects(false);

        // Load Minigame on top — Main stays loaded, GameManager untouched
        yield return SceneManager.LoadSceneAsync("Minigame", LoadSceneMode.Additive);

        // Make Minigame the active scene so its lighting and audio are used
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Minigame"));
    }

    // Called by LogicManagerScript when the player exits the minigame
    public void UnloadMinigame()
    {
        StartCoroutine(UnloadMinigameAdditive());
    }

    private IEnumerator UnloadMinigameAdditive()
    {
        // Restore Main as active scene first
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Main"));

        // Unload the minigame scene
        yield return SceneManager.UnloadSceneAsync("Minigame");

        // Show main scene objects again
        SetMainObjects(true);

        // Step-3 decisions are handled by ScenarioManager.OnEnable()
        // which fires when SetMainObjects(true) re-activates the scene above.
    }

    private void SetMainObjects(bool active)
    {
        if (mainSceneObjects == null) return;
        foreach (var obj in mainSceneObjects)
            if (obj != null) obj.SetActive(active);
    }
}