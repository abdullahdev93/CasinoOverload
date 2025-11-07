// PlayerProfile.cs
using UnityEngine;
using System;

[DisallowMultipleComponent]
public class PlayerProfile : MonoBehaviour
{
    public static PlayerProfile Instance { get; private set; }

    [SerializeField] private string uid;
    [SerializeField] private int coins;

    public string Uid => uid;
    public int Coins => coins;

    public event Action<int> OnCoinsChanged;

    const string KeyUid = "pp_uid";
    const string KeyCoins = "pp_coins";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
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

    private void LoadLocal()
    {
        if (PlayerPrefs.HasKey(KeyUid)) uid = PlayerPrefs.GetString(KeyUid);
        if (PlayerPrefs.HasKey(KeyCoins)) coins = PlayerPrefs.GetInt(KeyCoins);
    }
}
