using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour, IStoreListener
{
    public static IAPManager Instance { get; private set; }

    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    // Track processed transaction IDs so we don't grant coins twice
    private static readonly HashSet<string> processedTransactions = new HashSet<string>();

    // Product IDs – use these in both Google Play and App Store
    public const string PRODUCT_SMALL = "com.casinooverload.purchase.coins_small";
    public const string PRODUCT_MEDIUM = "com.casinooverload.purchase.coins_medium";
    public const string PRODUCT_LARGE = "com.casinooverload.purchase.coins_large";
    public const string PRODUCT_PREMIUM = "com.casinooverload.purchase.coins_premium";

    // Coin amounts
    private const int SMALL_COINS = 1000;
    private const int MEDIUM_COINS = 2000;
    private const int LARGE_COINS = 5000;
    private const int PREMIUM_COINS = 10000;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[IAP] Duplicate IAPManager found, destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (storeController == null)
        {
            InitializePurchasing();
        }
    }

    public bool IsInitialized => storeController != null && storeExtensionProvider != null;

    public void InitializePurchasing()
    {
        if (IsInitialized)
            return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Register consumable products
        builder.AddProduct(PRODUCT_SMALL, ProductType.Consumable);
        builder.AddProduct(PRODUCT_MEDIUM, ProductType.Consumable);
        builder.AddProduct(PRODUCT_LARGE, ProductType.Consumable);
        builder.AddProduct(PRODUCT_PREMIUM, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }

    // -------- Public buy methods (for UI) --------

    public void BuySmall() => BuyProductID(PRODUCT_SMALL);
    public void BuyMedium() => BuyProductID(PRODUCT_MEDIUM);
    public void BuyLarge() => BuyProductID(PRODUCT_LARGE);
    public void BuyPremium() => BuyProductID(PRODUCT_PREMIUM);

    private void BuyProductID(string productId)
    {
        if (!IsInitialized)
        {
            Debug.LogWarning("[IAP] BuyProductID failed: IAP not initialized.");
            return;
        }

        Product product = storeController.products.WithID(productId);

        if (product != null && product.availableToPurchase)
        {
            Debug.Log($"[IAP] Purchasing: {product.definition.id}");
            storeController.InitiatePurchase(product);
        }
        else
        {
            Debug.LogWarning($"[IAP] BuyProductID: Product not found or not available: {productId}");
        }
    }

    // -------- IStoreListener implementation --------

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("[IAP] Initialization successful.");
        storeController = controller;
        storeExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"[IAP] Initialization failed: {error}");
    }

#if UNITY_2019_3_OR_NEWER
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"[IAP] Initialization failed: {error} - {message}");
    }
#endif

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string id = args.purchasedProduct.definition.id;
        string txId = args.purchasedProduct.transactionID;

        Debug.Log($"[IAP] ProcessPurchase: productId={id} txId={txId}");

        // 🔒 Guard: if we've already handled this transaction, skip
        if (!string.IsNullOrEmpty(txId))
        {
            if (processedTransactions.Contains(txId))
            {
                Debug.LogWarning($"[IAP] Duplicate ProcessPurchase detected for txId={txId}, ignoring (no extra coins).");
                return PurchaseProcessingResult.Complete;
            }

            processedTransactions.Add(txId);
        }
        else
        {
            Debug.LogWarning("[IAP] ProcessPurchase has empty transactionID, still processing but cannot dedupe.");
        }

        if (id == PRODUCT_SMALL)
            GrantCoins(SMALL_COINS);
        else if (id == PRODUCT_MEDIUM)
            GrantCoins(MEDIUM_COINS);
        else if (id == PRODUCT_LARGE)
            GrantCoins(LARGE_COINS);
        else if (id == PRODUCT_PREMIUM)
            GrantCoins(PREMIUM_COINS);
        else
            Debug.LogWarning($"[IAP] ProcessPurchase: Unhandled product: {id}");

        return PurchaseProcessingResult.Complete; // consumables
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.LogWarning($"[IAP] Purchase failed: {product.definition.id} - {reason}");
    }

    private async void GrantCoins(int amount)
    {
        if (amount <= 0) return;

        try
        {
            // Prefer DB as source of truth
            if (FirebaseDatabaseBridge.Instance != null && FirebaseDatabaseBridge.Instance.IsReady)
            {
                await FirebaseDatabaseBridge.Instance.AddCoinsAsync(amount);
                Debug.Log($"[IAP] Granted {amount} coins via Firebase DB.");
            }
            else if (FirebaseDatabaseBridge.Instance != null)
            {
                // If bridge exists but not "ready", still attempt
                await FirebaseDatabaseBridge.Instance.AddCoinsAsync(amount);
                Debug.Log($"[IAP] Granted {amount} coins via Firebase DB (bridge not marked ready).");
            }
            else if (PlayerProfile.Instance != null)
            {
                // Fallback: local only (offline / no DB)
                PlayerProfile.Instance.AddCoinsLocal(amount);
                Debug.Log($"[IAP] Granted {amount} coins locally (no Firebase bridge).");
            }
            else
            {
                Debug.LogWarning("[IAP] No FirebaseDatabaseBridge or PlayerProfile to grant coins to.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[IAP] Failed to grant coins: {e.Message}");
        }
    }
}
