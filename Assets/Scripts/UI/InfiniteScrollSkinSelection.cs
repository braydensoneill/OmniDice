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

    [Header("Skin Item Settings")]
    public Vector2 itemSize = new Vector2(200, 200); // Size of each skin item
    public Font textFont; // Font for skin name labels (optional)

    private List<SkinData> loadedSkins = new List<SkinData>();

    protected override void Start()
    {
        LoadSkinItems();
        base.Start();

        // Setup the apply button
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(ApplySkin);
        }

        // Update UI with initial selection
        UpdateSkinNameDisplay();
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

        // Update the skin name display
        UpdateSkinNameDisplay();
    }

    private void UpdateSkinNameDisplay()
    {
        if (skinNameText != null)
        {
            skinNameText.text = GetSelectedSkinName();
        }
    }

    public void ApplySkin()
    {
        string selectedSkinName = GetSelectedSkinName();

        if (SkinManager.Instance != null)
        {
            SkinManager.Instance.SetCurrentSkin(selectedSkinName);
            Debug.Log($"Applied skin: {selectedSkinName}");

            // Apply materials to all existing dice in the scene
            ApplyMaterialsToAllDice(selectedSkinName);

            // Update all dice prefabs with the new skin
            UpdateDicePrefabsWithSkin(selectedSkinName);
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
    }
}