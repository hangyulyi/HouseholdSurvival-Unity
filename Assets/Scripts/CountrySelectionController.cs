using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountrySelectionController : MonoBehaviour
{
    [Header("Flag Buttons")]
    public Button brazilButton;
    public Button indiaButton;
    public Button kenyaButton;
    public Button swedenButton;
    public Button usaButton;

    [Header("Flag Images")]
    public Image brazilImage;
    public Image indiaImage;
    public Image kenyaImage;
    public Image swedenImage;
    public Image usaImage;

    [Header("Map Backgrounds")]
    public GameObject brazilMap;
    public GameObject indiaMap;
    public GameObject kenyaMap;
    public GameObject swedenMap;
    public GameObject usaMap;
    

    [Header("Blurb")]
    public TMP_Text countryName;
    public TMP_Text countryBlurb;

    [Header("Confirm Panel")]
    public GameObject confirmPanel;
    public Image selectedFlagImage;
    public TMP_InputField nameInput;

    [Header("Player Status")]
    public PlayerStatusUI playerStatusUI;
    public GameObject playerStatusCanvas;

    string selectedCountry;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // init selected
        SelectCountry("Brazil");
    }

    public void SelectCountry(string country)
    {
        selectedCountry = country;

        Color selected = Color.white;
        Color grey = Color.grey;

        // Init as grey
        brazilImage.color = grey;
        indiaImage.color = grey;
        kenyaImage.color = grey;
        swedenImage.color = grey;
        usaImage.color = grey;
    
        if(country == "Brazil")
        {
            brazilImage.color = selected;
            countryName.text = "Brazil";
            countryBlurb.text = "In Brazil's urban environments, financial stability can be unpredictable." +
                "Inflation, job instability and economic fluctuations affect daily life. Strong family and community ties often help through challenges\n\n\n" +
                "System Type: Volatile Economy\n\n" +
                "Safety Net: Low-medium\n\n" +
                "Difficulty: Medium-hard";
        }

        if (country == "India")
        {
            indiaImage.color = selected;
            countryName.text = "India";
            countryBlurb.text = "In urban India, income often supports more than just yourself. Many workers contribute financially to parents or extended family." +
                "Healthcare can be affordable but varies in quailty and education can improve opportunities if you can afford the time and fees\n\n\n" +
                "System Type: Family-Responsibility Economy\n\n" +
                "Safety Net: Low\n\n" +
                "Difficulty: Medium";
        }

        if (country == "Kenya")
        {
            kenyaImage.color = selected;
            countryName.text = "Kenya";
            countryBlurb.text = "Many people in Kenya rely on informal work and community networks rather than stable employment." +
                "Income can fluctuate and access to healthcare and education may be limited\n\n\n" +
                "System Type: Informal Economy\n\n" +
                "Safety Net: Very low\n\n" +
                "Difficulty: Hard";
        }

        if (country == "Sweden")
        {
            swedenImage.color = selected;
            countryName.text = "Sweden";
            countryBlurb.text = "Sweden provides strong social support systems such as healthcare, unemployment benefits and education assistance." +
                "While taxes are higher, government programs help protect citizens from extreme financial crisis\n\n\n" +
                "System Type: Welfare State\n\n" +
                "Safety Net: High\n\n" +
                "Difficulty: Easier";
        }

        if (country == "USA")
        {
            usaImage.color = selected;
            countryName.text = "United States of America";
            countryBlurb.text = "Life in the US offers opportunity but basic needs can be expensive. Healthcare, housing and education costs create constant financial pressure." +
                "While career growth and training can lead to higher income, one medical emergency or job loss can quickly lead to debt\n\n\n" +
                "System Type: Working-Poor Economy\n\n" +
                "Safety Net: Low\n\n" +
                "Difficulty: Medium";
        }
    }

    public void ShowConfirmPanel()
    {
        confirmPanel.SetActive(true);

        if (selectedCountry == "Brazil")
            selectedFlagImage.sprite = brazilImage.sprite;

        if (selectedCountry == "India")
            selectedFlagImage.sprite = indiaImage.sprite;

        if (selectedCountry == "Kenya")
            selectedFlagImage.sprite = kenyaImage.sprite;

        if (selectedCountry == "Sweden")
            selectedFlagImage.sprite = swedenImage.sprite;

        if (selectedCountry == "USA")
            selectedFlagImage.sprite = usaImage.sprite;
    }

    public void ConfirmSelection()
    {
        GameManager.Instance.country = selectedCountry;
        GameManager.Instance.playerName = nameInput.text;

        // save state

        //PlayerPrefs.SetInt("HasSelectedCountry", 1);
        //PlayerPrefs.SetString("PlayerName", nameInput.text);
        // PlayerPrefs.Save();

        ActivateMap(selectedCountry);

        playerStatusUI.SetName(nameInput.text);
        playerStatusCanvas.SetActive(true);

        confirmPanel.SetActive(false);

        gameObject.SetActive(false);
    }

    public void ActivateMap(string country)
    {
        brazilMap.SetActive(false);
        indiaMap.SetActive(false);
        kenyaMap.SetActive(false);
        swedenMap.SetActive(false);
        usaMap.SetActive(false);

        switch (country)
        {
            case "Brazil": brazilMap.SetActive(true); break;
            case "India": indiaMap.SetActive(true); break;
            case "Kenya": kenyaMap.SetActive(true); break;
            case "Sweden": swedenMap.SetActive(true); break;
            case "USA": usaMap.SetActive(true); break;
        }
    }

    public void Cancel()
    {
        confirmPanel.SetActive(false);
    }

    void Awake()
    {
        // check if game has already started
        // 1 = true, 0 = false (default)
        //if(PlayerPrefs.GetInt("HasSelectedCountry", 0) == 1)
        //{
        //    playerStatusCanvas.SetActive(true);

        //    //string savedName = PlayerPrefs.GetString("PlayerName", "Player");
        //    //playerStatusUI.SetName(savedName);

        //    playerStatusUI.SetName(GameManager.Instance.playerName);

        //    gameObject.SetActive(false);
        //}
        if (GameManager.Instance == null) return;

        if (!string.IsNullOrEmpty(GameManager.Instance.country))
        {
            gameObject.SetActive(false);

            playerStatusCanvas.SetActive(true);
            playerStatusUI.SetName(GameManager.Instance.playerName);

            ActivateMap(GameManager.Instance.country);
        }
    }
}
