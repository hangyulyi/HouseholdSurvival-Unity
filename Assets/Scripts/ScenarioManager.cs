using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ScenarioManager : MonoBehaviour
{
    public GameObject scenarioCanvas;

    public TMP_Text titleText;
    public TMP_Text descriptionText;

    public Transform buttonContainer;
    public GameObject decisionButtonPrefab;
    
    public void StartScenario()
    {
        scenarioCanvas.SetActive(true);

        StartCoroutine(
            APIManager.Instance.LoadScenario(
                GameManager.Instance.phase,
                GameManager.Instance.country,
                OnScenarioLoaded
            )
        );
    }

    void OnScenarioLoaded(string json)
    {
        Debug.Log("Scenario Data: " + json);
    }

    void MakeDecision(DecisionData decision)
    {
        GameManager.Instance.ApplyDecision(decision);

        scenarioCanvas.SetActive(false);

        GameManager.Instance.NextPhase();

        MapManager.Instance.UpdateMap();
    }
}
