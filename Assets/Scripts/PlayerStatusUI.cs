using UnityEngine;
using TMPro;

public class PlayerStatusUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text moneyText;
    public TMP_Text phaseText;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            SetName(GameManager.Instance.playerName);
        }
    }

    // Update is called once per frame
    void Update()
    {
        nameText.text = GameManager.Instance.playerName;
        moneyText.text = "$" + GameManager.Instance.money;
        phaseText.text = "Phase: " + GameManager.Instance.phase;
    }

    public void SetName(string playerName)
    {
        nameText.text = playerName;
    }
}
