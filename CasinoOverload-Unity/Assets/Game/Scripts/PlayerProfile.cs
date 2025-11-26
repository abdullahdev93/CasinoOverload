// PlayerProfile.cs
using UnityEngine;
using System;

[DisallowMultipleComponent]
public class PlayerProfile : MonoBehaviour
{
    public static PlayerProfile Instance { get; private set; }

    [SerializeField] private string uid;
    [SerializeField] private int coins;
    [SerializeField] private string username;
    [SerializeField] private int avatarIndex = -1; // 0..5, -1 = not chosen

    public string Uid => uid;
    public int Coins => coins;
    public string Username => username;
    public int AvatarIndex => avatarIndex;

    public event Action<int> OnCoinsChanged;
    public event Action<string> OnUsernameChanged;
    public event Action<int> OnAvatarIndexChanged;

    private const string KeyUid = "pp_uid";
    private const string KeyCoins = "pp_coins";
    private const string KeyUsername = "pp_username";
    private const string KeyAvatarIndex = "pp_avatarIndex";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadLocal();
    }

    public void SetUid(string newUid)
    {
        if (string.IsNullOrEmpty(newUid)) return;

        uid = newUid;
        PlayerPrefs.SetString(KeyUid, uid);
        PlayerPrefs.Save();
    }

    public void SetCoins(int newCoins)
    {
        coins = Mathf.Max(0, newCoins);
        PlayerPrefs.SetInt(KeyCoins, coins);
        PlayerPrefs.Save();
        OnCoinsChanged?.Invoke(coins);
    }

    public void AddCoinsLocal(int delta) => SetCoins(coins + delta);

    public void SetUsername(string newUsername)
    {
        if (string.IsNullOrEmpty(newUsername)) return;
        if (username == newUsername) return;

        username = newUsername;
        PlayerPrefs.SetString(KeyUsername, username);
        PlayerPrefs.Save();
        OnUsernameChanged?.Invoke(username);
    }

    public void SetAvatarIndex(int newIndex)
    {
        if (newIndex < 0) newIndex = -1;
        avatarIndex = newIndex;
        PlayerPrefs.SetInt(KeyAvatarIndex, avatarIndex);
        PlayerPrefs.Save();
        OnAvatarIndexChanged?.Invoke(avatarIndex);
    }

    private void LoadLocal()
    {
        if (PlayerPrefs.HasKey(KeyUid)) uid = PlayerPrefs.GetString(KeyUid);
        if (PlayerPrefs.HasKey(KeyCoins)) coins = PlayerPrefs.GetInt(KeyCoins);
        if (PlayerPrefs.HasKey(KeyUsername)) username = PlayerPrefs.GetString(KeyUsername);
        if (PlayerPrefs.HasKey(KeyAvatarIndex)) avatarIndex = PlayerPrefs.GetInt(KeyAvatarIndex);
        else avatarIndex = -1;
    }
}
