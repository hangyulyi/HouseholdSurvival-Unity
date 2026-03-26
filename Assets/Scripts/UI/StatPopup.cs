using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatPopup : MonoBehaviour
{
    public static StatPopup Instance;

    [Header("Popup Panel")]
    [Tooltip("The panel to show/hide. Set inactive by default in the Inspector.")]
    public GameObject popupPanel;

    [Header("Popup Header")]
    public TMP_Text popupNameText;
    public TMP_Text popupPhaseText;
    public TMP_Text popupCountryText;

    [Header("Popup Stats")]
    public TMP_Text popupMoneyText;
    public TMP_Text popupDebtText;
    public TMP_Text popupHealthText;
    public TMP_Text popupStressText;
    public TMP_Text popupHappinessText;

    [Header("Popup Stat Bars (optional, 0-100)")]
    public Slider popupHealthBar;
    public Slider popupStressBar;
    public Slider popupHappinessBar;

    [Header("Popup Scores")]
    public TMP_Text popupTotalScoreText;
    public TMP_Text popupEconomicScoreText;
    public TMP_Text popupSocialScoreText;
    public TMP_Text popupHealthScoreText;


    void Awake()
    {
        Instance = this;
        if (popupPanel) popupPanel.SetActive(false);
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.onStatsChanged.AddListener(RefreshIfOpen);
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.onStatsChanged.RemoveListener(RefreshIfOpen);
    }



    // Wire to the face Button's OnClick — toggles the popup open/closed.
    public void TogglePopup()
    {
        if (popupPanel == null) return;

        bool opening = !popupPanel.activeSelf;
        popupPanel.SetActive(opening);

        if (opening) Refresh();
    }

    // Wire to the Close button inside the popup panel
    public void ClosePopup()
    {
        if (popupPanel) popupPanel.SetActive(false);
    }


    private void RefreshIfOpen()
    {
        if (popupPanel != null && popupPanel.activeSelf)
            Refresh();
    }

    private void Refresh()
    {
        if (GameManager.Instance == null) return;
        var gm = GameManager.Instance;

        if (popupNameText) popupNameText.text = gm.playerName;
        if (popupPhaseText) popupPhaseText.text = $"Phase {gm.phase} / {GameManager.MAX_PHASES}";
        if (popupCountryText) popupCountryText.text = gm.countryCode.ToUpper();

        if (popupMoneyText) popupMoneyText.text = "Money: " + gm.FormatMoney(gm.money);
        if (popupDebtText) popupDebtText.text = "Debt: " + gm.FormatMoney(gm.debt);
        if (popupHealthText) popupHealthText.text = "Health: " + gm.health;
        if (popupStressText) popupStressText.text = "Stress: " + gm.stress;
        if (popupHappinessText) popupHappinessText.text = "Happiness: " + gm.happiness;

        if (popupHealthBar) popupHealthBar.value = gm.health;
        if (popupStressBar) popupStressBar.value = gm.stress;
        if (popupHappinessBar) popupHappinessBar.value = gm.happiness;

        if (popupTotalScoreText) popupTotalScoreText.text = "Score: " + gm.totalImpactScore;
        if (popupEconomicScoreText) popupEconomicScoreText.text = "Economic: " + gm.economicScore;
        if (popupSocialScoreText) popupSocialScoreText.text = "Social: " + gm.socialScore;
        if (popupHealthScoreText) popupHealthScoreText.text = "Health Score: " + gm.healthScore;
    }
}