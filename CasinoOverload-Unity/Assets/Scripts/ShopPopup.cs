// ShopPopup.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPopup : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button smallButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button largeButton;
    [SerializeField] private Button premiumButton;
    [SerializeField] private Button closeButton;

    [Header("Texts (optional)")]
    [SerializeField] private TextMeshProUGUI infoText;

    private void Awake()
    {
        if (smallButton != null)
        {
            smallButton.onClick.RemoveAllListeners();
            smallButton.onClick.AddListener(OnSmallClicked);
        }

        if (mediumButton != null)
        {
            mediumButton.onClick.RemoveAllListeners();
            mediumButton.onClick.AddListener(OnMediumClicked);
        }

        if (largeButton != null)
        {
            largeButton.onClick.RemoveAllListeners();
            largeButton.onClick.AddListener(OnLargeClicked);
        }

        if (premiumButton != null)
        {
            premiumButton.onClick.RemoveAllListeners();
            premiumButton.onClick.AddListener(OnPremiumClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseClicked);
        }
    }

    private void OnEnable()
    {
        if (infoText != null)
            infoText.text = "Buy coin bundles to boost your balance!";
    }

    private void OnSmallClicked()
    {
        if (IAPManager.Instance == null)
        {
            Debug.LogWarning("[Shop] IAPManager missing.");
            return;
        }
        IAPManager.Instance.BuySmall();
    }

    private void OnMediumClicked()
    {
        if (IAPManager.Instance == null)
        {
            Debug.LogWarning("[Shop] IAPManager missing.");
            return;
        }
        IAPManager.Instance.BuyMedium();
    }

    private void OnLargeClicked()
    {
        if (IAPManager.Instance == null)
        {
            Debug.LogWarning("[Shop] IAPManager missing.");
            return;
        }
        IAPManager.Instance.BuyLarge();
    }

    private void OnPremiumClicked()
    {
        if (IAPManager.Instance == null)
        {
            Debug.LogWarning("[Shop] IAPManager missing.");
            return;
        }
        IAPManager.Instance.BuyPremium();
    }

    private void OnCloseClicked()
    {
        // Re-enable avatars when shop closes
        if (MainMenuUIHandler.Instance != null)
            MainMenuUIHandler.Instance.SetAvatarParentVisible(true);

        gameObject.SetActive(false);
    }
}
