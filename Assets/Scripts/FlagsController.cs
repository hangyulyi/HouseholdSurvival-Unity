using UnityEngine;
using UnityEngine.UI;

public class FlagsController : MonoBehaviour
{

    public Image[] flagImages;
    public GameObject[] details;

    public FlagConfirmationPanel confirmationPanel;

    private int activeFlagIndex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActivateFlag(0); // default to first 
    }

    public void ActivateFlag(int flagNo)
    {
        activeFlagIndex = flagNo;
        for(int i = 0; i < details.Length; i++)
        {
            details[i].SetActive(false);
            flagImages[i].color = Color.gray;
        }
        details[flagNo].SetActive(true);
        flagImages[flagNo].color = Color.white;
    }

    public void OnConfirmButttonPressed()
    {
        confirmationPanel.ShowConfirmation(activeFlagIndex);
    }
}
