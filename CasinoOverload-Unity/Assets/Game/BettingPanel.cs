using TMPro;
using UnityEngine;

public class BettingPanel : MonoBehaviour
{
    [Header("Player Money Settings")]
    public int betAmount = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI totalCashText;
    public TextMeshProUGUI betAmountText;

    private int betStep = 10;

    void Start() {
        UpdateUI();
    }

    public void IncreaseBet() {
        if (CurrencyHandler.Cash - betAmount >= betStep) {
            betAmount += betStep;
            UpdateUI();
        }
        else {
            Debug.Log("Not enough money to increase bet!");
        }
    }

    public void DecreaseBet() {
        if (betAmount - betStep >= 0) {
            betAmount -= betStep;
            UpdateUI();
        }
    }

    public void PlaceBet() {
        if (betAmount > 0) {
            Debug.Log($"Bet Placed: {betAmount}! Starting match...");

            BetHandler.StartMatch(betAmount);

            betAmount = 0; // Reset bet after placing
            UpdateUI();

            gameObject.SetActive(false);
        }
        else {
            Debug.Log("Bet amount is zero!");
        }
    }

    void UpdateUI() {
        totalCashText.text = "Cash: $" + CurrencyHandler.Cash;
        betAmountText.text = "Bet: $" + betAmount;
    }
}
