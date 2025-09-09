using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InfiniteScrollSkinSelection : InfiniteScrollBase<string>
{
    [Header("Skin-Specific Settings")]
    public string defaultSkinName = "default";

    [Header("UI References")]
    public TextMeshProUGUI skinNameText; // Reference to your "Skin name" TMPro text field
    public Button applyButton; // Reference to your "Apply" button
    public Button purchaseButton; // Reference to purchase/unlock button
    public TextMeshProUGUI skinStatusText; // Shows "OWNED" or "LOCKED" or price

    [Header("Button Colors")]
    public Color ownedButtonColor = new Color(1f, 0.5f, 0f, 1f); // Orange (255/255, 125/255, 0/255, 1)
    public Color purchaseButtonColor = new Color(0f, 0.8f, 0f, 1f); // Green (0, 225/255, 0, 1)

    [Header("Skin Item Settings")]
    public Vector2 itemSize = new Vector2(200, 200); // Size of each skin item
    public Font textFont; // Font for skin name labels (optional)

    private List<SkinData> loadedSkins = new List<SkinData>();
    private string purchaseInProgressSkin = ""; // Track which skin is being purchased

    protected override void Start()
    {
        LoadSkinItems();
        base.Start();

        // Setup the apply button
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(ApplySkin);
        }

        // Setup the purchase button
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(PurchaseSkin);
        }

        // Subscribe to skin changes to update UI when skins are purchased
        if (SkinManager.Instance != null)
        {
            SkinManager.Instance.OnSkinChanged += OnSkinChanged;
            SkinManager.Instance.OnSkinUnlocked += OnSkinUnlocked;
        }

        // Update UI with initial selection
        UpdateSkinDisplay();
    }

    private void OnSkinChanged(string skinName)
    {
        // Update the UI whenever a skin changes (including purchases)
        Debug.Log($"[UI] OnSkinChanged triggered for: {skinName}");
        UpdateSkinDisplay();
    }

    private void OnSkinUnlocked(string skinName)
    {
        // Update the UI whenever a skin is unlocked/purchased
        Debug.Log($"[UI] OnSkinUnlocked triggered for: {skinName}");

        // Clear purchase in progress flag
        if (purchaseInProgressSkin == skinName)
        {
            purchaseInProgressSkin = "";
            Debug.Log($"[UI] Purchase completed for: {skinName}, clearing purchase flag");
        }

        UpdateSkinDisplay();
        Debug.Log($"UI updated: Skin '{skinName}' was unlocked!");
    }
    private void LoadSkinItems()
    {
        Debug.Log("LoadSkinItems() called");

        if (SkinManager.Instance == null)
        {
            Debug.LogError("SkinManager not found! Make sure it exists in the scene.");
            return;
        }

        loadedSkins = SkinManager.Instance.GetAvailableSkins();
        Debug.Log($"Found {loadedSkins.Count} skins from SkinManager");

        // Use existing UI items from the Inspector instead of creating new ones
        List<RectTransform> skinUIItems = new List<RectTransform>();

        // Get all existing children from the content panel
        for (int i = 0; i < contentPanelTransform.childCount; i++)
        {
            Transform child = contentPanelTransform.GetChild(i);
            RectTransform rectTransform = child.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                // Update the name to match skin data if available
                if (i < loadedSkins.Count)
                {
                    child.name = loadedSkins[i].skinName;

                    // Update the text if it exists
                    TextMeshProUGUI skinText = child.GetComponentInChildren<TextMeshProUGUI>();
                    if (skinText != null)
                    {
                        skinText.text = loadedSkins[i].skinName;
                    }

                    Debug.Log($"Updated existing UI item for skin: {loadedSkins[i].skinName}");
                }

                skinUIItems.Add(rectTransform);
            }
        }

        // Convert to array for the base class
        itemList = skinUIItems.ToArray();
        Debug.Log($"LoadSkinItems completed. Using {itemList.Length} existing skin UI items");

        // Temporary fallback: if no items found, create a dummy item to prevent crash
        if (itemList.Length == 0)
        {
            Debug.LogWarning("No skin UI items found! Creating a temporary default item to prevent crash.");
            GameObject tempItem = new GameObject("TempSkin");
            RectTransform tempRect = tempItem.AddComponent<RectTransform>();
            tempRect.sizeDelta = itemSize;
            tempItem.transform.SetParent(contentPanelTransform, false);
            itemList = new RectTransform[] { tempRect };
        }
    }

    protected override string GetItemName(RectTransform item)
    {
        return item.name.Replace("(Clone)", "").Trim();
    }

    protected override string GetChildItemName(Transform child)
    {
        return child.name.Replace("(Clone)", "").Trim();
    }

    protected override void OnItemSelected(int index)
    {
        string selectedSkinName = GetSelectedItemName();
        Debug.Log($"Skin selected: {selectedSkinName} (Index: {index})");

        // Update the skin display
        UpdateSkinDisplay();
    }

    private void UpdateSkinDisplay()
    {
        string selectedSkinName = GetSelectedSkinName();

        if (skinNameText != null)
        {
            skinNameText.text = selectedSkinName;
        }

        // Update ownership status and button states
        bool isOwned = SkinManager.Instance != null && SkinManager.Instance.IsOwned(selectedSkinName);

        // If purchase is in progress for this skin, treat as not owned until purchase completes
        if (purchaseInProgressSkin == selectedSkinName)
        {
            Debug.Log($"[UI] Purchase in progress for {selectedSkinName}, treating as not owned");
            isOwned = false;
        }

        Debug.Log($"[UI] UpdateSkinDisplay: Skin '{selectedSkinName}' - IsOwned: {isOwned}");

        // Update status text
        if (skinStatusText != null)
        {
            if (isOwned)
            {
                skinStatusText.text = "OWNED";
            }
            else
            {
                string price = "$0.99"; // Default fallback
                if (SkinManager.Instance != null)
                {
                    price = SkinManager.Instance.GetSkinPrice(selectedSkinName);
                }
                skinStatusText.text = $"LOCKED - {price}";
            }
        }

        // Update apply button based on ownership
        if (applyButton != null)
        {
            applyButton.interactable = true; // Always interactable now

            // Get button text component
            TextMeshProUGUI buttonText = applyButton.GetComponentInChildren<TextMeshProUGUI>();

            if (isOwned)
            {
                // Owned skin: "Apply" with orange background
                if (buttonText != null)
                {
                    Debug.Log($"[UI] Setting button text to 'Apply' for skin: {selectedSkinName}");
                    buttonText.text = "Apply";
                    buttonText.ForceMeshUpdate(); // Force immediate text update
                }

                // Set orange color
                ColorBlock colors = applyButton.colors;
                colors.normalColor = ownedButtonColor;
                colors.highlightedColor = ownedButtonColor; // Same color for hover
                colors.pressedColor = ownedButtonColor * 0.8f; // Darker when pressed for feedback
                colors.selectedColor = ownedButtonColor; // Same color for selected
                colors.disabledColor = ownedButtonColor * 0.5f; // Dimmed for disabled
                applyButton.colors = colors;
                Debug.Log($"[UI] Set button color to ORANGE for owned skin: {selectedSkinName}");
            }
            else
            {
                // Locked skin: "Purchase" with green background
                if (buttonText != null)
                {
                    Debug.Log($"[UI] Setting button text to 'Purchase' for skin: {selectedSkinName}");
                    buttonText.text = "Purchase";
                    buttonText.ForceMeshUpdate(); // Force immediate text update
                }

                // Set green color
                ColorBlock colors = applyButton.colors;
                colors.normalColor = purchaseButtonColor;
                colors.highlightedColor = purchaseButtonColor; // Same color for hover
                colors.pressedColor = purchaseButtonColor * 0.8f; // Darker when pressed for feedback
                colors.selectedColor = purchaseButtonColor; // Same color for selected
                colors.disabledColor = purchaseButtonColor * 0.5f; // Dimmed for disabled
                applyButton.colors = colors;
                Debug.Log($"[UI] Set button color to GREEN for locked skin: {selectedSkinName}");
            }
        }

        // Hide the separate purchase button since we're using the apply button for both
        if (purchaseButton != null)
        {
            purchaseButton.gameObject.SetActive(false);
        }
    }

    private void UpdateSkinNameDisplay()
    {
        // Legacy method - redirects to new method
        UpdateSkinDisplay();
    }

    public void ApplySkin()
    {
        string selectedSkinName = GetSelectedSkinName();

        if (SkinManager.Instance != null)
        {
            // Check if skin is owned
            if (SkinManager.Instance.IsOwned(selectedSkinName))
            {
                // Apply the owned skin
                SkinManager.Instance.SetCurrentSkin(selectedSkinName);
                Debug.Log($"Applied skin: {selectedSkinName}");

                // Apply materials to all existing dice in the scene
                ApplyMaterialsToAllDice(selectedSkinName);

                // Update all dice prefabs with the new skin
                UpdateDicePrefabsWithSkin(selectedSkinName);
            }
            else
            {
                // Trigger purchase for locked skin
                PurchaseSkin();
            }
        }
    }

    public void PurchaseSkin()
    {
        string selectedSkinName = GetSelectedSkinName();
        Debug.Log($"[UI] PurchaseSkin called for: {selectedSkinName}");

        if (SkinManager.Instance == null)
        {
            Debug.LogError("SkinManager not found!");
            return;
        }

        if (SkinManager.Instance.IsOwned(selectedSkinName))
        {
            Debug.LogWarning($"Skin '{selectedSkinName}' is already owned!");
            return;
        }

        // Mark this skin as being purchased (prevents UI updates during purchase)
        purchaseInProgressSkin = selectedSkinName;
        Debug.Log($"[UI] Purchase in progress for: {selectedSkinName}");

        // Use real IAP system
        Debug.Log($"Initiating purchase for skin: {selectedSkinName}");
        SkinManager.Instance.PurchaseSkin(selectedSkinName);

        // The purchase result will be handled automatically by the OnSkinUnlocked event
        // which will update the UI when the purchase completes
    }

    // TEST METHOD - Remove this in production
    [ContextMenu("Unlock Current Skin (Testing Only)")]
    public void UnlockCurrentSkinForTesting()
    {
        string selectedSkinName = GetSelectedSkinName();
        if (SkinManager.Instance != null && !SkinManager.Instance.IsOwned(selectedSkinName))
        {
            SkinManager.Instance.UnlockSkin(selectedSkinName);
            UpdateSkinDisplay();
            Debug.Log($"[TESTING] Unlocked skin: {selectedSkinName}");
        }
    }

    private void ApplyMaterialsToAllDice(string skinName)
    {
        // Find all dice in the scene using the DiceCreator's DieSides component
        InnerDriveStudios.DiceCreator.DieSides[] allDice = FindObjectsByType<InnerDriveStudios.DiceCreator.DieSides>(FindObjectsSortMode.None);

        int appliedCount = 0;

        foreach (var dieSides in allDice)
        {
            // Get the dice type from the GameObject name (e.g., "D6", "D20", etc.)
            string diceTypeName = GetDiceTypeFromName(dieSides.gameObject.name);

            if (!string.IsNullOrEmpty(diceTypeName))
            {
                // Load the material for this dice type and skin
                Material skinMaterial = Resources.Load<Material>($"Skins/{skinName}/{diceTypeName}");

                if (skinMaterial != null)
                {
                    // Apply the material to the dice
                    MeshRenderer renderer = dieSides.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.material = skinMaterial;
                        appliedCount++;
                        Debug.Log($"Applied {skinName} material to {diceTypeName} dice: {dieSides.gameObject.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Material not found for skin '{skinName}' and dice type '{diceTypeName}'");
                }
            }
        }

        Debug.Log($"Applied skin '{skinName}' to {appliedCount} dice in the scene");
    }

    private void UpdateDicePrefabsWithSkin(string skinName)
    {
        // Find the DiceSpawnManager to access dice prefabs
        DiceSpawnManager spawnManager = FindFirstObjectByType<DiceSpawnManager>();
        if (spawnManager == null)
        {
            Debug.LogWarning("UpdateDicePrefabsWithSkin: DiceSpawnManager not found");
            return;
        }

        int updatedPrefabs = 0;

        // Update each dice type prefab with the new skin material
        foreach (var diceType in spawnManager.diceTypes)
        {
            if (diceType.prefab != null)
            {
                string diceTypeName = GetDiceTypeFromName(diceType.prefab.name);

                if (!string.IsNullOrEmpty(diceTypeName))
                {
                    Material skinMaterial = Resources.Load<Material>($"Skins/{skinName}/{diceTypeName}");

                    if (skinMaterial != null)
                    {
                        MeshRenderer renderer = diceType.prefab.GetComponent<MeshRenderer>();
                        if (renderer != null)
                        {
                            renderer.sharedMaterial = skinMaterial; // Use sharedMaterial for prefabs
                            updatedPrefabs++;
                            Debug.Log($"Updated prefab '{diceType.prefab.name}' with {skinName} material");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Material not found for skin '{skinName}' and dice type '{diceTypeName}'");
                    }
                }
            }
        }

        Debug.Log($"Updated {updatedPrefabs} dice prefabs with skin '{skinName}'");
    }

    private string GetDiceTypeFromName(string gameObjectName)
    {
        // Extract dice type from GameObject name (handles names like "D6(Clone)", "Classic", etc.)
        string cleanName = gameObjectName.Replace("(Clone)", "").Trim();

        // Check against known dice types (sorted by length to avoid partial matches)
        string[] knownDiceTypes = { "Classic", "D2", "D3", "D4", "D6", "D8", "D10", "D10-00", "D12", "D20" };

        // Sort by length (longest first) to avoid D2 matching D20, D10 matching D10-00
        var sortedDiceTypes = new List<string>(knownDiceTypes);
        sortedDiceTypes.Sort((a, b) => b.Length.CompareTo(a.Length));

        foreach (string diceType in sortedDiceTypes)
        {
            if (cleanName.Equals(diceType, System.StringComparison.OrdinalIgnoreCase) ||
                cleanName.StartsWith(diceType, System.StringComparison.OrdinalIgnoreCase))
            {
                return diceType;
            }
        }

        return null;
    }

    protected override void SetInitialPosition()
    {
        // Find the index of default skin to center it initially
        int defaultIndex = -1;
        for (int i = 0; i < itemList.Length; i++)
        {
            if (itemList[i].name.ToLower().Contains(defaultSkinName.ToLower()))
            {
                defaultIndex = i;
                currentSelectedIndex = defaultIndex;
                break;
            }
        }

        // If default skin is not owned, try to find the free skin
        if (SkinManager.Instance != null && !SkinManager.Instance.IsOwned(defaultSkinName))
        {
            for (int i = 0; i < itemList.Length; i++)
            {
                string skinName = itemList[i].name;
                if (SkinManager.Instance.IsOwned(skinName))
                {
                    currentSelectedIndex = i;
                    break;
                }
            }
        }

        base.SetInitialPosition();
    }

    public string GetSelectedSkinName()
    {
        return GetSelectedItemName();
    }

    public int GetSelectedSkinIndex()
    {
        return GetSelectedIndex();
    }

    void OnDestroy()
    {
        if (applyButton != null)
        {
            applyButton.onClick.RemoveListener(ApplySkin);
        }

        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveListener(PurchaseSkin);
        }

        // Unsubscribe from skin changes
        if (SkinManager.Instance != null)
        {
            SkinManager.Instance.OnSkinChanged -= OnSkinChanged;
            SkinManager.Instance.OnSkinUnlocked -= OnSkinUnlocked;
        }
    }
}