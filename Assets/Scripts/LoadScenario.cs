using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to any sprite/object on the map that should trigger a scenario when clicked.
/// Requires a Collider2D on the object (Box Collider 2D works fine).
///
/// Setup:
///   1. Add this script to your interactable GameObject
///   2. Add a Box Collider 2D and resize it to match the sprite
/// </summary>
public class LoadScenario : MonoBehaviour
{
    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;

        // ── FUTURE: proximity + E-key interaction ─────────────────────────────
        // Uncomment and fill in when you want E-key support:
        //
        // public float interactRadius = 1.5f;
        // public string playerTag = "Player";
        // public GameObject interactPrompt;
        // private Transform _playerTransform;
        // private bool _playerInRange;
        //
        // var playerObj = GameObject.FindGameObjectWithTag(playerTag);
        // if (playerObj != null) _playerTransform = playerObj.transform;
        // if (interactPrompt) interactPrompt.SetActive(false);
        // ─────────────────────────────────────────────────────────────────────
    }

    void Update()
    {
        

        // ── FUTURE: E-key when in range ───────────────────────────────────────
        // Uncomment when ready to add proximity interaction:
        //
        // if (_playerTransform != null)
        // {
        //     float dist = Vector2.Distance(transform.position, _playerTransform.position);
        //     bool inRange = dist <= interactRadius;
        //     if (inRange != _playerInRange)
        //     {
        //         _playerInRange = inRange;
        //         if (interactPrompt) interactPrompt.SetActive(_playerInRange);
        //     }
        //     if (_playerInRange && Input.GetKeyDown(KeyCode.E))
        //         Trigger();
        // }
        // ─────────────────────────────────────────────────────────────────────
    }

    public void Trigger()
    {
        if (ScenarioManager.Instance == null)
        {
            Debug.LogError("InteractableObject: ScenarioManager not found in scene.");
            return;
        }

        if (GameManager.Instance == null || string.IsNullOrEmpty(GameManager.Instance.countryCode))
        {
            Debug.LogWarning("InteractableObject: No country selected yet.");
            return;
        }

        ScenarioManager.Instance.StartScenario();
    }
}
