using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Displays live player stats from GameManager.
/// Subscribes to GameManager.onStatsChanged so it updates immediately
/// whenever the backend returns adjusted scores.
///
/// Attach to: the PlayerStatus HUD canvas object.
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

    [Header("Score")]
    public TMP_Text totalScoreText;
    public TMP_Text economicScoreText;
    public TMP_Text socialScoreText;
    public TMP_Text healthScoreText;

    [Header("Optional Stat Bars")]
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

    // ── Public ────────────────────────────────────────────────────────────────

    public void SetName(string playerName)
    {
        if (nameText) nameText.text = playerName;
    }

    /// <summary>Re-read all values from GameManager and push to UI.</summary>
    public void Refresh()
    {
        if (GameManager.Instance == null) return;
        var gm = GameManager.Instance;

        if (nameText) nameText.text = gm.playerName;
        if (phaseText) phaseText.text = $"Phase {gm.phase} / {GameManager.MAX_PHASES}";
        if (countryText) countryText.text = gm.countryCode.ToUpper();

        if (moneyText) moneyText.text = "$" + gm.money;
        if (healthText) healthText.text = "Health: " + gm.health;
        if (stressText) stressText.text = "Stress: " + gm.stress;
        if (happinessText) happinessText.text = "Happiness: " + gm.happiness;
        if (debtText) debtText.text = "Debt: $" + gm.debt;

        if (totalScoreText) totalScoreText.text = "Score: " + gm.totalImpactScore;
        if (economicScoreText) economicScoreText.text = "Econ: " + gm.economicScore;
        if (socialScoreText) socialScoreText.text = "Social: " + gm.socialScore;
        if (healthScoreText) healthScoreText.text = "Health Score: " + gm.healthScore;

        // Sliders (0–100 range)
        if (healthBar) healthBar.value = gm.health;
        if (stressBar) stressBar.value = gm.stress;
        if (happinessBar) happinessBar.value = gm.happiness;
    }
}
