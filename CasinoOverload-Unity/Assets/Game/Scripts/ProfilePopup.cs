// ProfilePopup.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button editUsernameButton;

    [Header("Refs")]
    [SerializeField] private GameObject usernamePopup; // UsernamePopup panel

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        if (editUsernameButton != null)
        {
            editUsernameButton.onClick.RemoveAllListeners();
            editUsernameButton.onClick.AddListener(OnEditUsernameClicked);
        }
    }

    private void OnEnable()
    {
        if (PlayerProfile.Instance != null)
        {
            PlayerProfile.Instance.OnCoinsChanged += OnCoinsChanged;
            PlayerProfile.Instance.OnUsernameChanged += OnUsernameChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (PlayerProfile.Instance != null)
        {
            PlayerProfile.Instance.OnCoinsChanged -= OnCoinsChanged;
            PlayerProfile.Instance.OnUsernameChanged -= OnUsernameChanged;
        }
    }

    private void Refresh()
    {
        if (PlayerProfile.Instance == null) return;

        if (usernameText != null)
            usernameText.text = string.IsNullOrEmpty(PlayerProfile.Instance.Username)
                ? "-"
                : PlayerProfile.Instance.Username;

        if (coinsText != null)
            coinsText.text = PlayerProfile.Instance.Coins.ToString();
    }

    private void OnCoinsChanged(int coins)
    {
        if (coinsText != null)
            coinsText.text = coins.ToString();
    }

    private void OnUsernameChanged(string newUsername)
    {
        if (usernameText != null)
            usernameText.text = string.IsNullOrEmpty(newUsername) ? "-" : newUsername;
    }

    private void OnCloseClicked()
    {
        MainMenuUIHandler.Instance.SetAvatarParentVisible(true);
        gameObject.SetActive(false);
    }

    private void OnEditUsernameClicked()
    {
        gameObject.SetActive(false);

        if (usernamePopup != null)
            usernamePopup.SetActive(true);
    }
}
