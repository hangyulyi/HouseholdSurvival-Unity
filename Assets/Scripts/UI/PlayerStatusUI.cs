using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Displays live player stats from GameManager.
/// Subscribes to GameManager.onStatsChanged — updates automatically after every
/// backend response without needing to poll in Update.
/// </summary>
public class PlayerStatusUI : MonoBehaviour
{
    [Header("Identity")]
    public TMP_Text nameText;
    public TMP_Text phaseText;
    public TMP_Text countryText;

    [Header("Stats")]
    public TMP_Text moneyText;
    public TMP_Text healthText;
    public TMP_Text stressText;
    public TMP_Text happinessText;
    public TMP_Text debtText;

    [Header("Scores")]
    public TMP_Text totalScoreText;
    public TMP_Text economicScoreText;
    public TMP_Text socialScoreText;
    public TMP_Text healthScoreText;

    [Header("Optional Stat Bars (0-100)")]
    public Slider healthBar;
    public Slider stressBar;
    public Slider happinessBar;

    [Header("Player Face")]
    [Tooltip("The Image component on the HUD that shows the player's face")]
    public Image faceImage;
    public Sprite neutralSprite;
    public Sprite happySprite;
    public Sprite sadSprite;
    public Sprite sickSprite;
    public Sprite tiredSprite;

    [Header("Currency Font Override")]
    [Tooltip("TMP font asset that includes the rupee symbol. Applied when playing as India.")]
    public TMPro.TMP_FontAsset rupeeFont;
    private TMPro.TMP_FontAsset _defaultMoneyFont;

    [Header("Face Thresholds")]
    [Range(0, 100)] public int sickThreshold = 30;
    [Range(0, 100)] public int tiredThreshold = 70;
    [Range(0, 100)] public int sadThreshold = 30;
    [Range(0, 100)] public int happyThreshold = 70;
    [Range(0, 100)] public int happyMaxStress = 40;

    // ─────────────────────────────────────────────────────────────────────────

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.onStatsChanged.AddListener(Refresh);
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.onStatsChanged.RemoveListener(Refresh);
    }

    void Start()
    {
        if (moneyText) _defaultMoneyFont = moneyText.font;
        Refresh();
    }

    public void SetName(string playerName)
    {
        if (nameText) nameText.text = playerName;
    }

    public void Refresh()
    {
        if (GameManager.Instance == null) return;
        var gm = GameManager.Instance;

        if (nameText) nameText.text = gm.playerName;
        if (phaseText) phaseText.text = $"Phase {gm.phase} / {GameManager.MAX_PHASES}";
        if (countryText) countryText.text = gm.countryCode.ToUpper();

        // Swap to rupee font for India, restore default for all other countries
        if (moneyText != null)
        {
            bool isIndia = gm.countryCode == "in";
            moneyText.font = (isIndia && rupeeFont != null) ? rupeeFont : _defaultMoneyFont;
            if (debtText != null)
                debtText.font = moneyText.font;
        }

        if (moneyText) moneyText.text = gm.FormatMoney(gm.money);
        if (debtText) debtText.text = "Debt: " + gm.FormatMoney(gm.debt);
        if (healthText) healthText.text = "Health: " + gm.health;
        if (stressText) stressText.text = "Stress: " + gm.stress;
        if (happinessText) happinessText.text = "Happiness: " + gm.happiness;

        if (totalScoreText) totalScoreText.text = "Score: " + gm.totalImpactScore;
        if (economicScoreText) economicScoreText.text = "Econ: " + gm.economicScore;
        if (socialScoreText) socialScoreText.text = "Social: " + gm.socialScore;
        if (healthScoreText) healthScoreText.text = "Health Score: " + gm.healthScore;

        if (healthBar) healthBar.value = gm.health;
        if (stressBar) stressBar.value = gm.stress;
        if (happinessBar) happinessBar.value = gm.happiness;

        UpdateFace(gm.health, gm.stress, gm.happiness);
    }

    private void UpdateFace(int health, int stress, int happiness)
    {
        if (faceImage == null) return;

        Sprite next;
        if (health <= sickThreshold) next = sickSprite;
        else if (stress >= tiredThreshold) next = tiredSprite;
        else if (happiness <= sadThreshold) next = sadSprite;
        else if (happiness >= happyThreshold && stress < happyMaxStress) next = happySprite;
        else next = neutralSprite;

        // Fall back to neutral if the chosen sprite wasn't assigned
        if (next == null) next = neutralSprite;
        if (next != null) faceImage.sprite = next;
    }
}
