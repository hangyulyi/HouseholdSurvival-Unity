using UnityEngine;
using TMPro;

public class PlayerStatusUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    

    public void SetName(string newName)
    {
        nameText.text = newName;
    }
}
