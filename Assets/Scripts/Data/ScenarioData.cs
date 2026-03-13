using UnityEngine;

[System.Serializable]
public class ScenarioData
{
    public int scenario_id;
    public string title;
    public string description;
    public DecisionData[] decisions;

}

[System.Serializable]
public class DecisionData
{
    public int decision_id;
    public string choice_text;
    public int impact_score;
    public int economic_score;
    public int social_score;
}