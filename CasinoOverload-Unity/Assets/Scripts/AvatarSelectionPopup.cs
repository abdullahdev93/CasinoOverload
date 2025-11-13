using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AvatarSelectionPopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI infoText;

    private int currentIndex = 0;
    private const int AvatarCount = 6; // you have 6 characters

    private void Awake()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextClicked);
        }

        if (previousButton != null)
        {
            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(OnPreviousClicked);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }
    }

    private void OnEnable()
    {
        if (PlayerProfile.Instance != null)
        {
            int savedIndex = PlayerProfile.Instance.AvatarIndex;
            if (savedIndex >= 0 && savedIndex < AvatarCount)
                currentIndex = savedIndex;
            else
                currentIndex = 0;
        }
        else
        {
            currentIndex = 0;
        }

        UpdateVisuals();

        if (infoText != null)
            infoText.text = "Choose your avatar.";
    }

    private void OnNextClicked()
    {
        if (AvatarCount <= 0) return;

        currentIndex = (currentIndex + 1) % AvatarCount;
        UpdateVisuals();
    }

    private void OnPreviousClicked()
    {
        if (AvatarCount <= 0) return;

        currentIndex--;
        if (currentIndex < 0) currentIndex = AvatarCount - 1;
        UpdateVisuals();
    }

    private void OnConfirmClicked()
    {
        _ = SaveAvatarSelection();
    }

    private async Task SaveAvatarSelection()
    {
        if (FirebaseDatabaseBridge.Instance == null || !FirebaseDatabaseBridge.Instance.IsReady)
        {
            if (infoText != null)
                infoText.text = "Connection error. Try again.";
            return;
        }

        if (infoText != null)
            infoText.text = "Saving avatar...";

        await FirebaseDatabaseBridge.Instance.SetAvatarIndexAsync(currentIndex);

        if (infoText != null)
            infoText.text = "Avatar selected!";

        if (MainMenuUIHandler.Instance != null)
            MainMenuUIHandler.Instance.OnAvatarSelectionCompleted();

        gameObject.SetActive(false);
    }

    private void UpdateVisuals()
    {
        if (MainMenuUIHandler.Instance != null)
            MainMenuUIHandler.Instance.UpdateAvatarDisplay(currentIndex);
    }
}
