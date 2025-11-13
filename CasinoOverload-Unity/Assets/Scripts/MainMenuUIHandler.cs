using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIHandler : MonoBehaviour
{
    public static MainMenuUIHandler Instance;

    [Header("Main Menu Root")]
    [Tooltip("Root GameObject of the main menu UI (e.g. main menu panel canvas).")]
    [SerializeField] private GameObject mainMenuRoot;

    [Header("Main UI Texts")]
    [SerializeField] private TextMeshProUGUI CoinsText;
    [SerializeField] private TextMeshProUGUI UserIdText;
    [SerializeField] private TextMeshProUGUI UsernameText;

    [Header("Buttons")]
    [SerializeField] private Button WatchVideoAdButton;
    [SerializeField] private Button ProfileButton;
    [SerializeField] private Button ChangeAvatarButton;
    [SerializeField] private Button ShopButton;
    [SerializeField] private Button SettingsButton;          // NEW

    [Header("Popups")]
    [SerializeField] private GameObject UsernamePopup;
    [SerializeField] private ProfilePopup ProfilePopup;
    [SerializeField] private AvatarSelectionPopup AvatarSelectionPopup;
    [SerializeField] private ShopPopup ShopPopup;
    [SerializeField] private SettingsPopup SettingsPopup;    // NEW

    [Header("Avatar Objects (Single Source of Truth)")]
    [Tooltip("3D avatar characters in the scene. Only the selected one will be active.")]
    [SerializeField] private GameObject[] avatarObjects;

    [Header("Avatar Parent")]
    [Tooltip("Parent GameObject of all 3D avatars. Hidden during username, shop, settings screens.")]
    [SerializeField] private GameObject avatarParent;

    private void Awake()
    {
        Instance = this;

        // Button wiring from code only
        if (WatchVideoAdButton != null)
        {
            WatchVideoAdButton.onClick.RemoveAllListeners();
            WatchVideoAdButton.onClick.AddListener(OnWatchAdClicked);
        }

        if (ProfileButton != null)
        {
            ProfileButton.onClick.RemoveAllListeners();
            ProfileButton.onClick.AddListener(OpenProfilePopup);
        }

        if (ChangeAvatarButton != null)
        {
            ChangeAvatarButton.onClick.RemoveAllListeners();
            ChangeAvatarButton.onClick.AddListener(OpenAvatarPopup);
        }

        if (ShopButton != null)
        {
            ShopButton.onClick.RemoveAllListeners();
            ShopButton.onClick.AddListener(OpenShopPopup);
        }

        if (SettingsButton != null)
        {
            SettingsButton.onClick.RemoveAllListeners();
            SettingsButton.onClick.AddListener(OpenSettingsPopup);
        }
    }

    private void Start()
    {
        if (PlayerProfile.Instance != null)
        {
            UserIdText.text = "ID : " + PlayerProfile.Instance.Uid;

            PlayerProfile.Instance.OnCoinsChanged += UpdateMainMenuCoins;
            PlayerProfile.Instance.OnUsernameChanged += UpdateMainMenuUsername;
            PlayerProfile.Instance.OnAvatarIndexChanged += UpdateAvatarDisplay;

            UpdateMainMenuCoins(PlayerProfile.Instance.Coins);
            UpdateMainMenuUsername(PlayerProfile.Instance.Username);
            UpdateAvatarDisplay(PlayerProfile.Instance.AvatarIndex);

            bool hasUsername = !string.IsNullOrEmpty(PlayerProfile.Instance.Username);
            bool hasAvatar = PlayerProfile.Instance.AvatarIndex >= 0;

            // First-time flow: don't show main menu until username + avatar are set
            if (!hasUsername)
            {
                SetMainMenuVisible(false);
                SetAvatarParentVisible(false);
                if (UsernamePopup != null) UsernamePopup.SetActive(true);
            }
            else if (!hasAvatar)
            {
                SetMainMenuVisible(false);
                SetAvatarParentVisible(true); // avatar visible in avatar selection
                if (AvatarSelectionPopup != null) AvatarSelectionPopup.gameObject.SetActive(true);
            }
            else
            {
                SetMainMenuVisible(true);
                SetAvatarParentVisible(true);
            }
        }
        else
        {
            UserIdText.text = "ID : -";
            if (UsernameText != null) UsernameText.text = "";
            UpdateAvatarDisplay(-1);

            SetMainMenuVisible(false);
            SetAvatarParentVisible(false);
            if (UsernamePopup != null) UsernamePopup.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if (PlayerProfile.Instance != null)
        {
            PlayerProfile.Instance.OnCoinsChanged -= UpdateMainMenuCoins;
            PlayerProfile.Instance.OnUsernameChanged -= UpdateMainMenuUsername;
            PlayerProfile.Instance.OnAvatarIndexChanged -= UpdateAvatarDisplay;
        }

        if (WatchVideoAdButton != null) WatchVideoAdButton.onClick.RemoveAllListeners();
        if (ProfileButton != null) ProfileButton.onClick.RemoveAllListeners();
        if (ChangeAvatarButton != null) ChangeAvatarButton.onClick.RemoveAllListeners();
        if (ShopButton != null) ShopButton.onClick.RemoveAllListeners();
        if (SettingsButton != null) SettingsButton.onClick.RemoveAllListeners();
    }

    // -------- Visibility helpers --------

    public void SetMainMenuVisible(bool visible)
    {
        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(visible);
    }

    public void SetAvatarParentVisible(bool visible)
    {
        if (avatarParent != null)
            avatarParent.SetActive(visible);
    }

    // Called by AvatarSelectionPopup when avatar successfully chosen
    public void OnAvatarSelectionCompleted()
    {
        SetMainMenuVisible(true);
        SetAvatarParentVisible(true);
    }

    // -------- Button handlers --------

    private void OnWatchAdClicked()
    {
        if (GoogleAdsManager.Instance == null || FirebaseDatabaseBridge.Instance == null)
        {
            Debug.LogWarning("Ads or Firebase manager missing.");
            return;
        }

        GoogleAdsManager.Instance.ShowRewarded(
            onReward: r =>
            {
                _ = FirebaseDatabaseBridge.Instance.AddCoinsAsync(50);
            },
            onFail: reason =>
            {
                Debug.Log($"Rewarded not ready: {reason}");
            });
    }

    public void OpenProfilePopup()
    {
        if (ProfilePopup != null)
            ProfilePopup.gameObject.SetActive(true);
    }

    public void OpenUsernamePopup()
    {
        // Hide avatars while username popup is open
        SetAvatarParentVisible(false);

        if (UsernamePopup != null)
            UsernamePopup.SetActive(true);
    }

    public void OpenAvatarPopup()
    {
        // When avatar selection is open, hide main menu but show avatars
        SetMainMenuVisible(false);
        SetAvatarParentVisible(true);

        if (AvatarSelectionPopup != null)
            AvatarSelectionPopup.gameObject.SetActive(true);
    }

    public void OpenShopPopup()
    {
        // When shop opens, hide avatars
        SetAvatarParentVisible(false);

        if (ShopPopup != null)
            ShopPopup.gameObject.SetActive(true);
    }

    public void OpenSettingsPopup()
    {
        // When settings opens, hide avatars
        SetAvatarParentVisible(false);

        if (SettingsPopup != null)
            SettingsPopup.gameObject.SetActive(true);
    }

    // -------- UI updates --------

    public void UpdateMainMenuCoins(int coins)
    {
        if (CoinsText != null)
            CoinsText.text = "Coins : " + coins.ToString();
    }

    public void UpdateMainMenuUsername(string username)
    {
        if (UsernameText == null) return;

        if (string.IsNullOrEmpty(username))
            UsernameText.text = "";
        else
            UsernameText.text = username;
    }

    // Single place that manages which avatar object is active
    public void UpdateAvatarDisplay(int index)
    {
        if (avatarObjects == null || avatarObjects.Length == 0) return;

        bool anyActive = false;

        for (int i = 0; i < avatarObjects.Length; i++)
        {
            if (avatarObjects[i] == null) continue;

            bool shouldBeActive = (index >= 0 && index < avatarObjects.Length && i == index);
            avatarObjects[i].SetActive(shouldBeActive);
            if (shouldBeActive) anyActive = true;
        }

        if (!anyActive)
        {
            for (int i = 0; i < avatarObjects.Length; i++)
            {
                if (avatarObjects[i] != null)
                    avatarObjects[i].SetActive(false);
            }
        }
    }
}
