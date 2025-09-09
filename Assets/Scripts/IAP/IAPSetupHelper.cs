using UnityEngine;

/// <summary>
/// Helper script to easily set up IAP in your scene
/// Simply add this to any GameObject in your scene
/// </summary>
public class IAPSetupHelper : MonoBehaviour
{
    [Header("IAP Setup")]
    [Tooltip("This will automatically create the RealIAPManager GameObject")]
    public bool createIAPManagerOnStart = true;

    void Start()
    {
        if (createIAPManagerOnStart)
        {
            SetupIAP();
        }
    }

    [ContextMenu("Setup IAP")]
    public void SetupIAP()
    {
        // Check if RealIAPManager already exists in the scene
        if (RealIAPManager.Instance != null)
        {
            Debug.Log("[IAPSetup] RealIAPManager already exists in the scene.");
            return;
        }

        // Create a new GameObject with RealIAPManager
        GameObject iapManager = new GameObject("RealIAPManager");
        iapManager.AddComponent<RealIAPManager>();

        Debug.Log("[IAPSetup] RealIAPManager created successfully!");
        Debug.Log("[IAPSetup] Make sure to:");
        Debug.Log("1. Create products in Google Play Console matching the product IDs");
        Debug.Log("2. Test on a real device (IAP doesn't work in the editor)");
        Debug.Log("3. Upload a signed APK to Google Play for testing");
    }
}
