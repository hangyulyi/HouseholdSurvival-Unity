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
    public TMP_Text moneyText;       // uses country currency via GameManager.FormatMoney()
    public TMP_Text healthText;
    public TMP_Text stressText;
    public TMP_Text happinessText;
    public TMP_Text debtText;        // also uses country currency

    [Header("Scores")]
    public TMP_Text totalScoreText;
    public TMP_Text economicScoreText;
    public TMP_Text socialScoreText;
    public TMP_Text healthScoreText;

    [Header("Optional Stat Bars (0–100)")]
    public Slider healthBar;
    public Slider stressBar;
    public Slider happinessBar;

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

    void Start() => Refresh();

    // ── Public API ────────────────────────────────────────────────────────────

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

        // Money and debt use the country's currency format
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
    }
}
