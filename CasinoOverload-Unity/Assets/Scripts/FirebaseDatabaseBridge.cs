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
    [SerializeField] private string databaseUrl = "https://casino-overload-default-rtdb.firebaseio.com/"; // put your URL here

    [SerializeField] private int startingCoins = 100;

    private FirebaseAuth auth;
    private FirebaseDatabase db;
    private DatabaseReference coinsRef;
    private string currentUid;
    private bool isReady;

    public bool IsReady => isReady && coinsRef != null;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
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

        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += OnAuthStateChanged;

        if (auth.CurrentUser != null)
            _ = InitForUid(auth.CurrentUser.UserId);
    }

    private void OnDestroy()
    {
        if (auth != null) auth.StateChanged -= OnAuthStateChanged;
    }

    private void OnAuthStateChanged(object sender, EventArgs e)
    {
        if (auth.CurrentUser == null) return;
        if (auth.CurrentUser.UserId == currentUid) return;
        _ = InitForUid(auth.CurrentUser.UserId);
    }

    // Call right after sign-in (Auth script already calls this)
    public async Task BootstrapForCurrentUserAsync()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status != DependencyStatus.Available)
        {
            Debug.LogError($"[RTDB] Deps not available: {status}");
            return;
        }

        if (auth == null) auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser == null)
        {
            Debug.LogWarning("[RTDB] Bootstrap: no user yet.");
            return;
        }

        auth.StateChanged -= OnAuthStateChanged;
        auth.StateChanged += OnAuthStateChanged;

        await InitForUid(auth.CurrentUser.UserId); // seeds 100 if missing
    }

    private async Task InitForUid(string uid)
    {
        if (string.IsNullOrEmpty(uid)) return;

        currentUid = uid;

        var profile = FindOrCreateProfile();
        profile.SetUid(uid);

        // ---- IMPORTANT: build a DB instance from URL (or fallback) ----
        db = GetDatabaseInstance();
        if (db == null)
        {
            Debug.LogError("[RTDB] Database URL missing or invalid. Set 'databaseUrl' in the Inspector.");
            return;
        }

        coinsRef = db.GetReference($"users/{uid}/coins");

        try
        {
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

            isReady = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RTDB] InitForUid failed: {ex.Message}\n{ex}");
        }
    }

    private FirebaseDatabase GetDatabaseInstance()
    {
        // If you set a URL, use it. Otherwise fall back to DefaultInstance (works only if google-services config has DatabaseURL).
        string url = databaseUrl?.Trim();
        if (!string.IsNullOrEmpty(url))
        {
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                url = "https://" + url; // be forgiving if you paste without scheme

            try { return FirebaseDatabase.GetInstance(url); }
            catch (Exception e)
            {
                Debug.LogError($"[RTDB] Invalid Database URL: {url}\n{e.Message}");
                return null;
            }
        }

        // Fallback
        try { return FirebaseDatabase.DefaultInstance; }
        catch (Exception e)
        {
            Debug.LogError($"[RTDB] DefaultInstance not configured (no DatabaseURL in google-services). Set 'databaseUrl'.\n{e.Message}");
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
        catch { return 0; }
    }

    private PlayerProfile FindOrCreateProfile()
    {
        if (PlayerProfile.Instance != null) return PlayerProfile.Instance;
        var go = new GameObject("PlayerProfile");
        return go.AddComponent<PlayerProfile>();
    }

    // ---------- Public API ----------

    public async Task<int> GetCoinsAsync()
    {
        if (!IsReady) Debug.LogWarning("[RTDB] GetCoinsAsync before ready.");
        var snap = await coinsRef.GetValueAsync();
        int coins = snap.Exists ? SnapshotToInt(snap) : 0;
        PlayerProfile.Instance?.SetCoins(coins);
        return coins;
    }

    public async Task<int> SetCoinsAsync(int coins)
    {
        if (!IsReady) Debug.LogWarning("[RTDB] SetCoinsAsync before ready.");
        coins = Mathf.Max(0, coins);
        await coinsRef.SetValueAsync(coins);
        PlayerProfile.Instance?.SetCoins(coins);
        return coins;
    }

    // Safe add using a transaction (prevents overwrite races)
    public async Task<int> AddCoinsAsync(int delta)
    {
        if (!IsReady) Debug.LogWarning("[RTDB] AddCoinsAsync before ready.");

        // If your SDK prefers RunTransactionAsync, rename accordingly.
        DataSnapshot result = await coinsRef.RunTransaction(mutable =>
        {
            long current = 0;
            if (mutable.Value is long l) current = l;
            current += delta;
            current = Mathf.Max(0, (int)current);
            mutable.Value = current;
            return TransactionResult.Success(mutable);
        });

        int updated = SnapshotToInt(result);
        PlayerProfile.Instance?.SetCoins(updated);
        return updated;
    }
}
