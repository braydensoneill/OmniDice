using UnityEngine;
using UnityEngine.Purchasing;
using System;

#pragma warning disable CS0618 // Suppress obsolete API warnings for Unity IAP

public class RealIAPManager : MonoBehaviour, IStoreListener
{
    public static RealIAPManager Instance { get; private set; }

    private IStoreController storeController;
    private IExtensionProvider storeExtensionProvider;

    // Product IDs - these must match your Google Play Console products
    private const string SKIN_PREFIX = "skin_";

    // Events that SkinManager will listen to
    public static event Action<string> OnPurchaseSuccessEvent;
    public static event Action<string> OnPurchaseFailedEvent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeIAP();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeIAP()
    {
        if (IsInitialized()) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Add skin products - these will be your actual Google Play products
        // You'll need to create these in Google Play Console with matching Product IDs
        // Note: "Chalk Stone White" is free and not included here
        builder.AddProduct("skin_black_gloss_red", ProductType.NonConsumable);
        builder.AddProduct("skin_chalk_stone_green", ProductType.NonConsumable);
        builder.AddProduct("skin_chrome_damaged_white", ProductType.NonConsumable);
        builder.AddProduct("skin_chrome_matte_white", ProductType.NonConsumable);
        builder.AddProduct("skin_eroded_marble_blue", ProductType.NonConsumable);
        builder.AddProduct("skin_eroded_marble_white", ProductType.NonConsumable);
        builder.AddProduct("skin_gold_fringed_marble_black", ProductType.NonConsumable);
        builder.AddProduct("skin_white_on_black_matte", ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
        Debug.Log("[RealIAP] Initializing Unity IAP...");
    }

    public bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    public void PurchaseSkin(string skinName)
    {
        if (!IsInitialized())
        {
            Debug.LogError("[RealIAP] IAP not initialized!");
            OnPurchaseFailedEvent?.Invoke(skinName);
            return;
        }

        // Convert skin name to product ID
        string productId = ConvertSkinNameToProductId(skinName);

        Product product = storeController.products.WithID(productId);

        if (product != null && product.availableToPurchase)
        {
            Debug.Log($"[RealIAP] Starting purchase for: {productId}");
            storeController.InitiatePurchase(product);
        }
        else
        {
            Debug.LogError($"[RealIAP] Product not available: {productId}");
            OnPurchaseFailedEvent?.Invoke(skinName);
        }
    }

    private string ConvertSkinNameToProductId(string skinName)
    {
        // Convert "White On Black Matte" to "skin_white_on_black_matte"
        return SKIN_PREFIX + skinName.ToLower().Replace(" ", "_").Replace("-", "_");
    }

    private string ConvertProductIdToSkinName(string productId)
    {
        // Convert "skin_white_on_black_matte" back to "White On Black Matte"
        string skinPart = productId.Replace(SKIN_PREFIX, "");
        string[] words = skinPart.Split('_');

        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            }
        }

        return string.Join(" ", words);
    }

    public string GetProductPrice(string skinName)
    {
        if (!IsInitialized()) return "$0.99";

        string productId = ConvertSkinNameToProductId(skinName);
        Product product = storeController.products.WithID(productId);

        if (product != null && product.metadata != null)
        {
            return product.metadata.localizedPriceString;
        }

        return "$0.99"; // Fallback
    }

    // Unity IAP Callbacks
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("[RealIAP] Unity IAP initialized successfully!");
        storeController = controller;
        storeExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"[RealIAP] Unity IAP initialization failed: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"[RealIAP] Unity IAP initialization failed: {error} - {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string productId = args.purchasedProduct.definition.id;
        Debug.Log($"[RealIAP] Purchase successful: {productId}");

        // Convert product ID back to skin name
        string skinName = ConvertProductIdToSkinName(productId);

        // Notify SkinManager of successful purchase
        OnPurchaseSuccessEvent?.Invoke(skinName);

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        string productId = product.definition.id;
        string skinName = ConvertProductIdToSkinName(productId);

        Debug.LogError($"[RealIAP] Purchase failed: {productId}, Reason: {failureReason}");
        OnPurchaseFailedEvent?.Invoke(skinName);
    }
}

#pragma warning restore CS0618