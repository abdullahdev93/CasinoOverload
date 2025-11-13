using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI infoText; // optional

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }
    }

    private void OnEnable()
    {
        // Sync sliders from SoundManager
        if (SoundManager.Instance != null)
        {
            if (musicSlider != null)
                musicSlider.value = SoundManager.Instance.MusicVolume;

            if (sfxSlider != null)
                sfxSlider.value = SoundManager.Instance.SfxVolume;
        }

        if (infoText != null)
            infoText.text = "Adjust music and sound effects.";
    }

    private void OnMusicSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetMusicVolume(value);
    }

    private void OnSfxSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetSfxVolume(value);
    }

    private void OnCloseClicked()
    {
        // Re-enable avatars when settings closes
        if (MainMenuUIHandler.Instance != null)
            MainMenuUIHandler.Instance.SetAvatarParentVisible(true);

        gameObject.SetActive(false);
    }
}
