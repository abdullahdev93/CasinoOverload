// FirebaseAuthentication.cs
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)] // run early
public class FirebaseAuthentication : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private bool dontDestroyOnLoad = true;

    private FirebaseAuth auth;

    private async void Awake()
    {
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        await BootAndSignIn();
    }

    private async Task BootAndSignIn()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status != DependencyStatus.Available)
        {
            Debug.LogError($"[Auth] Firebase deps not available: {status}");
            return;
        }

        auth = FirebaseAuth.DefaultInstance;

        // make sure the DB bridge exists (persistent)
        if (FirebaseDatabaseBridge.Instance == null)
            new GameObject("FirebaseDatabase").AddComponent<FirebaseDatabaseBridge>();

        if (auth.CurrentUser != null)
        {
            Debug.Log($"[Auth] Reusing cached user: {auth.CurrentUser.UserId}");
            await FirebaseDatabaseBridge.Instance.BootstrapForCurrentUserAsync(); // seed/load coins
            LoadMainMenu();
            return;
        }

        try
        {
            // version-safe: don’t touch the return object; use auth.CurrentUser instead
            var _ = await auth.SignInAnonymouslyAsync();
            Debug.Log($"[Auth] Anonymous sign-in OK. uid={auth.CurrentUser?.UserId}");
            await FirebaseDatabaseBridge.Instance.BootstrapForCurrentUserAsync(); // seed/load coins
            LoadMainMenu();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Auth] Anonymous sign-in failed: {e.Message}\n{e}");
        }
    }

    private void LoadMainMenu()
    {
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogWarning("[Auth] Main menu scene name not set.");
            return;
        }

        var active = SceneManager.GetActiveScene().name;
        if (active != mainMenuSceneName)
            SceneManager.LoadScene(mainMenuSceneName);
    }

    public string CurrentUid => auth != null ? auth.CurrentUser?.UserId : null;
}
