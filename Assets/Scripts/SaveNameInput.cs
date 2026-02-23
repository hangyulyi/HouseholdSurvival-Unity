using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveNameInput : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public TMP_Text playerName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerName.text = PlayerPrefs.GetString("player_name");
    }

    public void SetName()
    {
        playerName.text = nameInputField.text;
        PlayerPrefs.SetString("player_name", playerName.text);
        PlayerPrefs.Save();
    }
}
