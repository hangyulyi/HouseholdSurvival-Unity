using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    public GameObject startMenuCanvas;
    public GameObject selectionCanvas;
    public GameObject confirmSelectionCanvas;
    public GameObject playerStatusCanvas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startMenuCanvas.SetActive(true);
        selectionCanvas.SetActive(false);
        playerStatusCanvas.SetActive(false);

        if(confirmSelectionCanvas != null )
        {
            confirmSelectionCanvas.SetActive(false);
        }
        PauseController.SetPause(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(startMenuCanvas.activeSelf)
        {
            return;
        }

        // check confirm isn't open, block interaction
        if (confirmSelectionCanvas != null && confirmSelectionCanvas.activeSelf)
        {
            return;
        }

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (!selectionCanvas.activeSelf && PauseController.IsGamePaused)
            {
                return;
            }
            selectionCanvas.SetActive(!selectionCanvas.activeSelf);
            PauseController.SetPause(selectionCanvas.activeSelf);
        }
    }

    public void OnPlayPressed()
    {
        startMenuCanvas.SetActive(false);
        selectionCanvas.SetActive(true);
        PauseController.SetPause(true);
    }
}
