using UnityEngine;

/// <summary>
/// Manages showing/hiding the minigame within the Main scene.
/// No scene loading — just SetActive calls.
///
/// Setup:
///   1. Move all minigame objects under one root called "MinigameRoot", set INACTIVE.
///   2. Move all main gameplay objects under "MainGameRoot".
///   3. Attach this script to any persistent GameObject and assign both fields.
/// </summary>
public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    [Tooltip("Root containing all minigame objects. Set inactive by default.")]
    public GameObject minigameRoot;

    [Tooltip("Root containing all main gameplay objects.")]
    public GameObject mainGameRoot;

    void Awake()
    {
        Instance = this;
        if (minigameRoot) minigameRoot.SetActive(false);
    }

    /// <summary>Open the minigame. Called by MinigameLoader or ScenarioManager.</summary>
    public void OpenMinigame(DecisionData[] pendingStep3 = null)
    {
        if (pendingStep3 != null)
            GameManager.Instance.pendingPhase2Step3 = pendingStep3;

        // Destroy any sharks/pearls left over from a previous run
        foreach (var s in FindObjectsByType<SharkMove>(FindObjectsSortMode.None))
            Destroy(s.gameObject);
        foreach (var p in FindObjectsByType<PearlTriggerScript>(FindObjectsSortMode.None))
            Destroy(p.gameObject);

        // Reset LogicManager score and hide game over screen
        var logic = minigameRoot != null
            ? minigameRoot.GetComponentInChildren<LogicManagerScript>(true)
            : FindFirstObjectByType<LogicManagerScript>();
        if (logic != null)
        {
            logic.playerScore = 0;
            if (logic.scoreText) logic.scoreText.text = "0  <sprite=0>";
            if (logic.gameOverSceen) logic.gameOverSceen.SetActive(false);
        }

        if (mainGameRoot) mainGameRoot.SetActive(false);
        if (minigameRoot) minigameRoot.SetActive(true);
        // SubScript.OnEnable + spawner OnEnable fire automatically here
    }

    /// <summary>Close the minigame and return to the main game. Called by LogicManagerScript.</summary>
    public void CloseMinigame()
    {
        // Clean up any remaining sharks/pearls
        foreach (var s in FindObjectsByType<SharkMove>(FindObjectsSortMode.None))
            Destroy(s.gameObject);
        foreach (var p in FindObjectsByType<PearlTriggerScript>(FindObjectsSortMode.None))
            Destroy(p.gameObject);

        if (minigameRoot) minigameRoot.SetActive(false);
        if (mainGameRoot) mainGameRoot.SetActive(true);

        // If phase 2 step-3 decisions are pending, show them now
        if (GameManager.Instance != null &&
            GameManager.Instance.pendingPhase2Step3 != null &&
            GameManager.Instance.pendingPhase2Step3.Length > 0)
        {
            if (ScenarioManager.Instance != null)
            {
                var step3 = GameManager.Instance.pendingPhase2Step3;
                GameManager.Instance.pendingPhase2Step3 = null;
                ScenarioManager.Instance.ShowStep3FromMinigame(step3);
            }
        }
    }
}
