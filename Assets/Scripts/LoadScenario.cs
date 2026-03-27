using UnityEngine;
using UnityEngine.EventSystems;


// handle scenarios (move on to next phase)

public class LoadScenario : MonoBehaviour
{
    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;

        // public float interactRadius = 1.5f;
        // public string playerTag = "Player";
        // public GameObject interactPrompt;
        // private Transform _playerTransform;
        // private bool _playerInRange;
        //
        // var playerObj = GameObject.FindGameObjectWithTag(playerTag);
        // if (playerObj != null) _playerTransform = playerObj.transform;
        // if (interactPrompt) interactPrompt.SetActive(false);
    }

    void Update()
    {
        

        // E-key when in range 
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

        if (PlayerPrefs.GetString("gameCompleted") == "true")
        {
            Debug.Log("Game already completed — showing final UI again.");

            return;
        }

        ScenarioManager.Instance.StartScenario();
    }
}
