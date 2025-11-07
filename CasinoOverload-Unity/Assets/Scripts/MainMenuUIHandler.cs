using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIHandler : MonoBehaviour
{
    public static MainMenuUIHandler Instance;

    [SerializeField] private TextMeshProUGUI CoinsText;
    [SerializeField] private TextMeshProUGUI UserIdText;
    [SerializeField] private Button WatchVideoAdButton;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        UserIdText.text = "ID : " + PlayerProfile.Instance.Uid;
        PlayerProfile.Instance.OnCoinsChanged += UpdateMainMenuCoins;
        UpdateMainMenuCoins(PlayerProfile.Instance.Coins);
        WatchVideoAdButton.onClick.AddListener(delegate ()
        {
            GoogleAdsManager.Instance.ShowRewarded(
            onReward: r =>
            {
                _ = FirebaseDatabaseBridge.Instance.AddCoinsAsync(50);
            },
            onFail: reason =>
            {
                Debug.Log($"Rewarded not ready: {reason}");
            });
        });
    }

    public void UpdateMainMenuCoins(int coins)
    {
        CoinsText.text = "Coins : " + coins.ToString();
    }
}
