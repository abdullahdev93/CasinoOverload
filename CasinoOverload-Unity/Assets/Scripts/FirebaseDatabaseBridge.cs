// FirebaseDatabaseBridge.cs
using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-90)]
public class FirebaseDatabaseBridge : MonoBehaviour
{
    public static FirebaseDatabaseBridge Instance { get; private set; }

    [Header("Realtime Database")]
    [Tooltip("Full RTDB URL. Example: https://your-project-id-default-rtdb.asia-southeast1.firebasedatabase.app")]
    [SerializeField] private string databaseUrl = "https://casino-overload-default-rtdb.firebaseio.com/";

    [SerializeField] private int startingCoins = 100;

    private FirebaseAuth auth;
    private FirebaseDatabase db;
    private DatabaseReference userRef;
    private DatabaseReference coinsRef;
    private DatabaseReference avatarRef;

    private string currentUid;
    private bool isReady;

    public bool IsReady => isReady && userRef != null && coinsRef != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status != DependencyStatus.Available)
        {
            Debug.LogError($"[RTDB] Firebase deps not available: {status}");
            return;
        }

        auth ??= FirebaseAuth.DefaultInstance;
        auth.StateChanged += OnAuthStateChanged;

        if (auth.CurrentUser != null)
        {
            _ = InitForUid(auth.CurrentUser.UserId);
        }
    }

    private void OnDestroy()
    {
        if (auth != null)
            auth.StateChanged -= OnAuthStateChanged;
    }

    private void OnAuthStateChanged(object sender, EventArgs e)
    {
        if (auth.CurrentUser == null) return;
        if (auth.CurrentUser.UserId == currentUid) return;

        _ = InitForUid(auth.CurrentUser.UserId);
    }

    // Called by FirebaseAuthentication after sign-in
    public async Task BootstrapForCurrentUserAsync()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status != DependencyStatus.Available)
        {
            Debug.LogError($"[RTDB] Deps not available: {status}");
            return;
        }

        auth ??= FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser == null)
        {
            Debug.LogWarning("[RTDB] Bootstrap: no user yet.");
            return;
        }

        auth.StateChanged -= OnAuthStateChanged;
        auth.StateChanged += OnAuthStateChanged;

        await InitForUid(auth.CurrentUser.UserId);
    }

    private async Task InitForUid(string uid)
    {
        if (string.IsNullOrEmpty(uid)) return;

        currentUid = uid;

        var profile = FindOrCreateProfile();
        profile.SetUid(uid);

        db = GetDatabaseInstance();
        if (db == null)
        {
            Debug.LogError("[RTDB] Database URL missing or invalid. Set 'databaseUrl' in the Inspector.");
            return;
        }

        userRef = db.GetReference($"users/{uid}");
        coinsRef = userRef.Child("coins");
        avatarRef = userRef.Child("avatarIndex");

        try
        {
            // Coins
            var snap = await coinsRef.GetValueAsync();
            if (snap.Exists && snap.Value != null)
            {
                int coins = SnapshotToInt(snap);
                profile.SetCoins(coins);
                Debug.Log($"[RTDB] Loaded coins={coins} for uid={uid}");
            }
            else
            {
                await coinsRef.SetValueAsync(startingCoins);
                profile.SetCoins(startingCoins);
                Debug.Log($"[RTDB] First sign-in. Seeded coins={startingCoins} for uid={uid}");
            }

            // Username
            var usernameSnap = await userRef.Child("username").GetValueAsync();
            if (usernameSnap.Exists && usernameSnap.Value != null)
            {
                string existingUsername = usernameSnap.Value.ToString();
                profile.SetUsername(existingUsername);
                Debug.Log($"[RTDB] Loaded username='{existingUsername}' for uid={uid}");
            }

            // Avatar index
            var avatarSnap = await avatarRef.GetValueAsync();
            int avatarIndex = -1;
            if (avatarSnap.Exists && avatarSnap.Value != null)
            {
                int.TryParse(avatarSnap.Value.ToString(), out avatarIndex);
            }
            profile.SetAvatarIndex(avatarIndex);

            isReady = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RTDB] InitForUid failed: {ex.Message}\n{ex}");
        }
    }

    private FirebaseDatabase GetDatabaseInstance()
    {
        string url = databaseUrl?.Trim();
        if (!string.IsNullOrEmpty(url))
        {
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                url = "https://" + url;

            try
            {
                return FirebaseDatabase.GetInstance(url);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RTDB] Invalid Database URL: {url}\n{e.Message}");
                return null;
            }
        }

        try
        {
            return FirebaseDatabase.DefaultInstance;
        }
        catch (Exception e)
        {
            Debug.LogError($"[RTDB] DefaultInstance not configured. Set 'databaseUrl'.\n{e.Message}");
            return null;
        }
    }

    private static int SnapshotToInt(DataSnapshot snap)
    {
        try
        {
            if (snap.Value is long l) return (int)l;
            if (snap.Value is double d) return Mathf.RoundToInt((float)d);
            int.TryParse(snap.Value.ToString(), out int parsed);
            return parsed;
        }
        catch
        {
            return 0;
        }
    }

    private PlayerProfile FindOrCreateProfile()
    {
        if (PlayerProfile.Instance != null) return PlayerProfile.Instance;

        var go = new GameObject("PlayerProfile");
        return go.AddComponent<PlayerProfile>();
    }

    // -------- Coins API --------

    public async Task<int> GetCoinsAsync()
    {
        if (!IsReady)
            Debug.LogWarning("[RTDB] GetCoinsAsync before ready.");

        var snap = await coinsRef.GetValueAsync();
        int coins = snap.Exists ? SnapshotToInt(snap) : 0;
        PlayerProfile.Instance?.SetCoins(coins);
        return coins;
    }

    public async Task<int> SetCoinsAsync(int coins)
    {
        if (!IsReady)
            Debug.LogWarning("[RTDB] SetCoinsAsync before ready.");

        coins = Mathf.Max(0, coins);
        await coinsRef.SetValueAsync(coins);
        PlayerProfile.Instance?.SetCoins(coins);
        return coins;
    }

    public async Task<int> AddCoinsAsync(int delta)
    {
        if (!IsReady)
            Debug.LogWarning("[RTDB] AddCoinsAsync before ready.");

        DataSnapshot result = await coinsRef.RunTransaction(mutable =>
        {
            long current = 0;
            if (mutable.Value is long l) current = l;
            current += delta;
            if (current < 0) current = 0;
            mutable.Value = current;
            return TransactionResult.Success(mutable);
        });

        int updated = SnapshotToInt(result);
        PlayerProfile.Instance?.SetCoins(updated);
        return updated;
    }

    // -------- Username API --------

    public async Task<bool> TrySetUsernameAsync(string username)
    {
        if (!IsReady || db == null || userRef == null)
        {
            Debug.LogWarning("[RTDB] TrySetUsernameAsync called before ready.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(username)) return false;

        string trimmed = username.Trim();
        string key = trimmed.ToLowerInvariant();

        try
        {
            var usernameRef = db.GetReference($"usernames/{key}");
            var snap = await usernameRef.GetValueAsync();

            if (snap.Exists && snap.Value != null)
            {
                string existingUid = snap.Value.ToString();
                if (!string.IsNullOrEmpty(existingUid) && existingUid != currentUid)
                {
                    Debug.Log($"[RTDB] Username '{trimmed}' already in use by uid={existingUid}");
                    return false;
                }
            }

            await usernameRef.SetValueAsync(currentUid);
            await userRef.Child("username").SetValueAsync(trimmed);

            PlayerProfile.Instance?.SetUsername(trimmed);

            Debug.Log($"[RTDB] Username '{trimmed}' saved for uid={currentUid}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RTDB] TrySetUsernameAsync failed: {ex.Message}\n{ex}");
            return false;
        }
    }

    public async Task<string> GetUsernameAsync()
    {
        if (!IsReady || userRef == null)
        {
            Debug.LogWarning("[RTDB] GetUsernameAsync before ready.");
            return PlayerProfile.Instance != null ? PlayerProfile.Instance.Username : null;
        }

        try
        {
            var snap = await userRef.Child("username").GetValueAsync();
            if (snap.Exists && snap.Value != null)
            {
                string uname = snap.Value.ToString();
                PlayerProfile.Instance?.SetUsername(uname);
                return uname;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RTDB] GetUsernameAsync failed: {ex.Message}\n{ex}");
        }

        return null;
    }

    // -------- Avatar API --------

    public async Task<int> SetAvatarIndexAsync(int index)
    {
        if (!IsReady || avatarRef == null)
        {
            Debug.LogWarning("[RTDB] SetAvatarIndexAsync before ready.");
            PlayerProfile.Instance?.SetAvatarIndex(index);
            return index;
        }

        await avatarRef.SetValueAsync(index);
        PlayerProfile.Instance?.SetAvatarIndex(index);
        return index;
    }

    public async Task<int> GetAvatarIndexAsync()
    {
        if (!IsReady || avatarRef == null)
        {
            Debug.LogWarning("[RTDB] GetAvatarIndexAsync before ready.");
            return PlayerProfile.Instance != null ? PlayerProfile.Instance.AvatarIndex : -1;
        }

        var snap = await avatarRef.GetValueAsync();
        int index = -1;
        if (snap.Exists && snap.Value != null)
        {
            int.TryParse(snap.Value.ToString(), out index);
        }

        PlayerProfile.Instance?.SetAvatarIndex(index);
        return index;
    }
}
