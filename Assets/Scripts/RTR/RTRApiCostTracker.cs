using UnityEngine;
using TMPro;

public class RTRApiCostTracker : MonoBehaviour
{
    [Header("Session Costs (USD)")]
    [SerializeField] private double llmCostUsdTotal;
    [SerializeField] private double elevenLabsCostUsdTotal;

    [Header("UI")]
    [SerializeField] private TMP_Text costText;

    public double TotalCostUsd =>
        llmCostUsdTotal + elevenLabsCostUsdTotal;

    private void Start()
    {
        RefreshUI();
    }

    public void RecordLLMCost(double costUsd)
    {
        if (double.IsNaN(costUsd) ||
            double.IsInfinity(costUsd) ||
            costUsd < 0)
        {
            Debug.LogWarning("Invalid LLM cost. Ignoring it.");
            return;
        }

        llmCostUsdTotal += costUsd;
        RefreshUI();
    }

    public void RecordElevenLabsAudioCost(
        double audioSeconds,
        double usdPerAudioHour)
    {
        if (double.IsNaN(audioSeconds) ||
            double.IsInfinity(audioSeconds) ||
            double.IsNaN(usdPerAudioHour) ||
            double.IsInfinity(usdPerAudioHour) ||
            audioSeconds < 0 ||
            usdPerAudioHour < 0)
        {
            Debug.LogWarning("Invalid ElevenLabs cost input.");
            return;
        }

        elevenLabsCostUsdTotal +=
            (audioSeconds / 3600.0) * usdPerAudioHour;

        RefreshUI();
    }

    public void ResetSessionCosts()
    {
        llmCostUsdTotal = 0;
        elevenLabsCostUsdTotal = 0;

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (costText != null)
        {
            // costText.text = $"${TotalCostUsd:F6}";

            double usdToInrRate = 95.15;
            double totalCostInr = TotalCostUsd * usdToInrRate;

            costText.text = $"₹{totalCostInr:F6}";
        }
    }
}