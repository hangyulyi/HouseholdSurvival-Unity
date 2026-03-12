using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlagConfirmationPanel : MonoBehaviour
{

    public GameObject confirmPanel;
    public Image confirmationImage;
    public Image[] sourceFlagImages;
    public string[] countryNames = { "USA", "Brazil", "Sweden", "India", "Kenya" };

    public CanvasGroup selectionCanvasGroup;
    public GameObject selectionMenu;

    public GameObject playerStatusPanel;
    public TMP_InputField nameInputField;
    public PlayerStatusUI playerStatusUI;

    private int selectedFlagIndex;
    
    void Start()
    {
        confirmPanel.SetActive(false);
    }

    public void ShowConfirmation(int flagIndex)
    {
        selectedFlagIndex = flagIndex;

        confirmPanel.SetActive(true);

        confirmationImage.sprite = sourceFlagImages[flagIndex].sprite;

        selectionCanvasGroup.interactable = false;
        selectionCanvasGroup.blocksRaycasts = false;
    }

    public void Confirm()
    {
        string playerName = nameInputField.text;
        playerStatusUI.SetName(playerName);

        // save country
        string selectedCountry = countryNames[selectedFlagIndex];
        GameController.Instance.SetCountry(selectedCountry);
        
        confirmPanel.SetActive(false);
        selectionMenu.SetActive(false);
        playerStatusPanel.SetActive(true);

        PauseController.SetPause(false);
    }

    public void Cancel()
    {
        confirmPanel.SetActive(false);
        selectionCanvasGroup.interactable = true;
        selectionCanvasGroup.blocksRaycasts = true;
    }
}
