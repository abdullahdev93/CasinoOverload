using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UsernamePopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private Button saveButton;
    [SerializeField] private TextMeshProUGUI infoText;

    private void Awake()
    {
        if (saveButton != null)
        {
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(OnSaveClicked);
        }
    }

    private void OnEnable()
    {
        if (PlayerProfile.Instance != null && usernameInput != null)
        {
            usernameInput.text = PlayerProfile.Instance.Username ?? string.Empty;
        }

        if (infoText != null)
            infoText.text = "Choose a unique username.";
    }

    private void OnDestroy()
    {
        if (saveButton != null)
            saveButton.onClick.RemoveListener(OnSaveClicked);
    }

    private void OnSaveClicked()
    {
        _ = SaveUsernameFlow();
    }

    private async Task SaveUsernameFlow()
    {
        if (usernameInput == null || infoText == null)
            return;

        string raw = usernameInput.text.Trim();

        if (string.IsNullOrEmpty(raw))
        {
            infoText.text = "Please enter a username.";
            return;
        }

        if (FirebaseDatabaseBridge.Instance == null || !FirebaseDatabaseBridge.Instance.IsReady)
        {
            infoText.text = "Connection error. Try again.";
            return;
        }

        infoText.text = "Checking username...";

        bool ok = await FirebaseDatabaseBridge.Instance.TrySetUsernameAsync(raw);
        if (!ok)
        {
            infoText.text = "Username is taken, try again.";
            return;
        }

        infoText.text = "Username saved!";

        // After successful username:
        // - If avatar not chosen yet → go to avatar selection, with avatars visible
        // - If avatar already chosen → back to main menu with avatars visible
        if (MainMenuUIHandler.Instance != null)
        {
            MainMenuUIHandler.Instance.SetAvatarParentVisible(true);

            if (PlayerProfile.Instance != null && PlayerProfile.Instance.AvatarIndex < 0)
            {
                // First-time flow: jump into avatar selection
                MainMenuUIHandler.Instance.OpenAvatarPopup();
            }
            else
            {
                // Editing username later
                MainMenuUIHandler.Instance.SetMainMenuVisible(true);
            }
        }

        gameObject.SetActive(false);
    }
}
