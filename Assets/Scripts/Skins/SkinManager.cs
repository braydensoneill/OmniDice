using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SkinManager : MonoBehaviour
{
    [Header("Skin Settings")]
    public string skinsPath = "Assets/Resources/Skins";

    [Header("Dice Types")]
    public string[] diceTypes = { "Classic", "D2", "D3", "D4", "D6", "D8", "D10", "D10-00", "D12", "D20" };

    private List<SkinData> availableSkins = new List<SkinData>();
    private string currentSelectedSkin = "default";

    public static SkinManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load the current skin from PlayerPrefs on startup
            currentSelectedSkin = PlayerPrefs.GetString("SelectedSkin", "default");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadAvailableSkins();
    }

    void LoadAvailableSkins()
    {
        availableSkins.Clear();

        // Check if we're in editor and can use Directory operations
#if UNITY_EDITOR
        string fullSkinsPath = skinsPath.Replace("Assets/", Application.dataPath + "/");
        if (Directory.Exists(fullSkinsPath))
        {
            string[] skinDirectories = Directory.GetDirectories(fullSkinsPath);

            foreach (string skinDir in skinDirectories)
            {
                string skinName = Path.GetFileName(skinDir);
                LoadSkinData(skinName);
            }
        }
        else
        {
            Debug.LogWarning($"Skins directory not found: {fullSkinsPath}");
            Debug.LogWarning($"Please make sure your skins folder exists at: {skinsPath}");
        }
#else
        // In build, we need to manually specify skin names or use a different loading method
        // For now, we'll try to load some common skin names
        string[] commonSkinNames = { "default", "wood", "metal", "stone", "glass" };
        
        foreach (string skinName in commonSkinNames)
        {
            LoadSkinData(skinName);
        }
#endif

        Debug.Log($"Loaded {availableSkins.Count} skins");
    }

    private void LoadSkinData(string skinName)
    {
        SkinData skinData = new SkinData(skinName);
        bool hasMaterials = false;

        // Load dice materials for this skin
        foreach (string diceType in diceTypes)
        {
            Material diceMaterial = Resources.Load<Material>($"Skins/{skinName}/{diceType}");

            if (diceMaterial != null)
            {
                skinData.diceMaterials[diceType] = diceMaterial;
                hasMaterials = true;
            }
        }

        // Only add skin if it has at least one dice material
        if (hasMaterials)
        {
            // Load preview sprite (optional)
            skinData.previewSprite = Resources.Load<Sprite>($"Skins/{skinName}/preview");

            // If no preview sprite, try to use the first available dice as preview
            if (skinData.previewSprite == null && skinData.diceMaterials.Count > 0)
            {
                // You could generate a preview here or use a default icon
                Debug.Log($"No preview sprite found for skin: {skinName}");
            }

            availableSkins.Add(skinData);
            Debug.Log($"Successfully loaded skin: {skinName} with {skinData.diceMaterials.Count} dice materials");
        }
        else
        {
            Debug.LogWarning($"No dice materials found for skin: {skinName}");
        }
    }

    public List<SkinData> GetAvailableSkins()
    {
        return availableSkins;
    }

    public SkinData GetSkinByName(string skinName)
    {
        return availableSkins.Find(skin => skin.skinName.Equals(skinName, System.StringComparison.OrdinalIgnoreCase));
    }

    public void SetCurrentSkin(string skinName)
    {
        SkinData skin = GetSkinByName(skinName);
        if (skin != null)
        {
            currentSelectedSkin = skinName;

            // Save to PlayerPrefs for persistence
            PlayerPrefs.SetString("SelectedSkin", skinName);
            PlayerPrefs.Save();

            Debug.Log($"Current skin set to: {skinName}");

            // Optionally notify other systems that the skin has changed
            OnSkinChanged?.Invoke(skinName);
        }
        else
        {
            Debug.LogWarning($"Attempted to set invalid skin: {skinName}");
        }
    }

    public string GetCurrentSkin()
    {
        return currentSelectedSkin;
    }

    public Material GetDiceMaterial(string skinName, string diceType)
    {
        SkinData skin = GetSkinByName(skinName);
        if (skin != null && skin.diceMaterials.ContainsKey(diceType))
        {
            return skin.diceMaterials[diceType];
        }

        Debug.LogWarning($"Dice material not found for skin: {skinName}, dice type: {diceType}");
        return null;
    }

    // Get current skin's dice material
    public Material GetCurrentDiceMaterial(string diceType)
    {
        return GetDiceMaterial(currentSelectedSkin, diceType);
    }

    // Check if a skin exists
    public bool SkinExists(string skinName)
    {
        return GetSkinByName(skinName) != null;
    }

    // Get all available dice types for a specific skin
    public List<string> GetAvailableDiceTypes(string skinName)
    {
        SkinData skin = GetSkinByName(skinName);
        if (skin != null)
        {
            return new List<string>(skin.diceMaterials.Keys);
        }
        return new List<string>();
    }

    // Apply current skin to a specific dice GameObject
    public void ApplySkinToDice(GameObject diceObject)
    {
        if (diceObject == null)
        {
            Debug.LogWarning("ApplySkinToDice: diceObject is null");
            return;
        }

        Debug.Log($"ApplySkinToDice: Processing dice '{diceObject.name}' with current skin '{currentSelectedSkin}'");

        // Get the dice type from the GameObject name
        string diceTypeName = GetDiceTypeFromName(diceObject.name);

        if (string.IsNullOrEmpty(diceTypeName))
        {
            Debug.LogWarning($"ApplySkinToDice: Could not determine dice type from name '{diceObject.name}'");
            return;
        }

        Debug.Log($"ApplySkinToDice: Detected dice type '{diceTypeName}'");

        Material skinMaterial = GetCurrentDiceMaterial(diceTypeName);

        if (skinMaterial == null)
        {
            Debug.LogWarning($"ApplySkinToDice: No material found for skin '{currentSelectedSkin}' and dice type '{diceTypeName}'");
            return;
        }

        Debug.Log($"ApplySkinToDice: Found material '{skinMaterial.name}'");

        MeshRenderer renderer = diceObject.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogWarning($"ApplySkinToDice: No MeshRenderer found on dice '{diceObject.name}'");
            return;
        }

        renderer.material = skinMaterial;
        Debug.Log($"Applied current skin '{currentSelectedSkin}' material '{skinMaterial.name}' to dice: {diceObject.name}");
    }

    private string GetDiceTypeFromName(string gameObjectName)
    {
        // Extract dice type from GameObject name (handles names like "D6(Clone)", "Classic", etc.)
        string cleanName = gameObjectName.Replace("(Clone)", "").Trim();
        Debug.Log($"GetDiceTypeFromName: Original name '{gameObjectName}' → Clean name '{cleanName}'");

        // Sort dice types by length (longest first) to avoid partial matches
        var sortedDiceTypes = new List<string>(diceTypes);
        sortedDiceTypes.Sort((a, b) => b.Length.CompareTo(a.Length));

        Debug.Log($"GetDiceTypeFromName: Checking against sorted types: [{string.Join(", ", sortedDiceTypes)}]");

        // Check against known dice types (longest matches first)
        foreach (string diceType in sortedDiceTypes)
        {
            bool exactMatch = cleanName.Equals(diceType, System.StringComparison.OrdinalIgnoreCase);
            bool startsWithMatch = cleanName.StartsWith(diceType, System.StringComparison.OrdinalIgnoreCase);

            Debug.Log($"GetDiceTypeFromName: Testing '{diceType}' → Exact: {exactMatch}, StartsWith: {startsWithMatch}");

            if (exactMatch || startsWithMatch)
            {
                Debug.Log($"GetDiceTypeFromName: MATCHED! Returning '{diceType}'");
                return diceType;
            }
        }

        Debug.LogWarning($"GetDiceTypeFromName: No match found for '{cleanName}'");
        return null;
    }

    // Event for when skin changes (other systems can subscribe to this)
    public System.Action<string> OnSkinChanged;

    // Refresh/reload all skins
    public void RefreshSkins()
    {
        LoadAvailableSkins();
    }

    // Debug method to print all loaded skins
    [ContextMenu("Debug Print All Skins")]
    public void DebugPrintAllSkins()
    {
        Debug.Log($"=== LOADED SKINS ({availableSkins.Count}) ===");
        foreach (SkinData skin in availableSkins)
        {
            Debug.Log($"Skin: {skin.skinName}");
            foreach (var kvp in skin.diceMaterials)
            {
                Debug.Log($"  - {kvp.Key}: {kvp.Value.name}");
            }
            Debug.Log($"  - Preview: {(skin.previewSprite != null ? skin.previewSprite.name : "None")}");
        }
    }
}