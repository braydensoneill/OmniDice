using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages glow effects for selection buttons (dice selection, skin selection, etc.)
/// Ensures only one item glows at a time within each group
/// </summary>
public class SelectionGlowManager : MonoBehaviour
{
    [Header("Glow Settings")]
    public GlowSettings diceSelectionGlow = new GlowSettings 
    { 
        glowColor = new Color(0f, 0.8f, 1f, 0.8f), // Blue glow for dice
        glowSize = 15f,
        pulseSpeed = 1.5f
    };
    
    public GlowSettings skinSelectionGlow = new GlowSettings 
    { 
        glowColor = new Color(1f, 0.6f, 0f, 0.8f), // Orange glow for skins
        glowSize = 12f,
        pulseSpeed = 2f
    };
    
    [Header("Selection Groups")]
    public Transform diceSelectionContainer; // Container holding dice selection buttons
    public Transform skinSelectionContainer; // Container holding skin selection buttons
    
    // Track glow effects for each group
    private Dictionary<string, List<UIGlowEffect>> glowGroups = new Dictionary<string, List<UIGlowEffect>>();
    private Dictionary<string, int> currentSelections = new Dictionary<string, int>();
    
    // Group names
    public const string DICE_GROUP = "Dice";
    public const string SKIN_GROUP = "Skin";
    
    void Start()
    {
        InitializeGlowGroups();
        
        // Listen for selection changes
        SetupSelectionListeners();
    }
    
    void InitializeGlowGroups()
    {
        // Initialize dice selection glows
        if (diceSelectionContainer != null)
        {
            InitializeContainerGlows(diceSelectionContainer, DICE_GROUP, diceSelectionGlow);
        }
        
        // Initialize skin selection glows
        if (skinSelectionContainer != null)
        {
            InitializeContainerGlows(skinSelectionContainer, SKIN_GROUP, skinSelectionGlow);
        }
        
        Debug.Log($"[SelectionGlowManager] Initialized {glowGroups.Count} glow groups");
    }
    
    void InitializeContainerGlows(Transform container, string groupName, GlowSettings glowSettings)
    {
        List<UIGlowEffect> glowEffects = new List<UIGlowEffect>();
        
        // Find all buttons in the container
        Button[] buttons = container.GetComponentsInChildren<Button>(true);
        
        foreach (Button button in buttons)
        {
            // Add or get UIGlowEffect component
            UIGlowEffect glowEffect = button.GetComponent<UIGlowEffect>();
            if (glowEffect == null)
            {
                glowEffect = button.gameObject.AddComponent<UIGlowEffect>();
            }
            
            // Apply group-specific glow settings
            glowEffect.glowSettings = glowSettings;
            glowEffects.Add(glowEffect);
            
            Debug.Log($"[SelectionGlowManager] Added glow effect to {button.name} in group {groupName}");
        }
        
        glowGroups[groupName] = glowEffects;
        currentSelections[groupName] = -1; // No selection initially
        
        Debug.Log($"[SelectionGlowManager] Group '{groupName}' initialized with {glowEffects.Count} glow effects");
    }
    
    void SetupSelectionListeners()
    {
        // Listen for dice selection changes
        InfiniteScrollDiceSelection diceScroll = FindObjectOfType<InfiniteScrollDiceSelection>();
        if (diceScroll != null)
        {
            diceScroll.OnSelectedDiceChanged.AddListener((index) => {
                Debug.Log($"[SelectionGlowManager] Dice selection changed to index: {index}");
                SetSelection(DICE_GROUP, index);
            });
        }
        
        // Listen for skin manager changes
        if (SkinManager.Instance != null)
        {
            SkinManager.Instance.OnSkinChanged += (skinName) => {
                Debug.Log($"[SelectionGlowManager] Skin changed to: {skinName}");
                UpdateSkinSelectionGlow(skinName);
            };
        }
    }
    
    /// <summary>
    /// Set the selected item in a group, updating glow effects
    /// </summary>
    public void SetSelection(string groupName, int selectedIndex)
    {
        if (!glowGroups.ContainsKey(groupName))
        {
            Debug.LogWarning($"[SelectionGlowManager] Group '{groupName}' not found");
            return;
        }
        
        List<UIGlowEffect> groupEffects = glowGroups[groupName];
        int previousSelection = currentSelections.ContainsKey(groupName) ? currentSelections[groupName] : -1;
        
        // Hide glow on previously selected item
        if (previousSelection >= 0 && previousSelection < groupEffects.Count)
        {
            groupEffects[previousSelection].HideGlow();
            Debug.Log($"[SelectionGlowManager] Hiding glow on {groupName} index {previousSelection}");
        }
        
        // Show glow on newly selected item
        if (selectedIndex >= 0 && selectedIndex < groupEffects.Count)
        {
            groupEffects[selectedIndex].ShowGlow();
            currentSelections[groupName] = selectedIndex;
            Debug.Log($"[SelectionGlowManager] Showing glow on {groupName} index {selectedIndex}");
        }
        else
        {
            currentSelections[groupName] = -1;
            Debug.Log($"[SelectionGlowManager] Invalid selection index {selectedIndex} for group {groupName}");
        }
    }
    
    /// <summary>
    /// Update skin selection glow based on skin name
    /// </summary>
    void UpdateSkinSelectionGlow(string skinName)
    {
        // Find the skin selection UI and determine the index
        InfiniteScrollSkinSelection skinScroll = FindObjectOfType<InfiniteScrollSkinSelection>();
        if (skinScroll != null)
        {
            // You'll need to add a method to get the index of a skin by name
            // For now, this is a placeholder - you may need to implement this based on your skin selection UI
            int skinIndex = GetSkinIndex(skinName);
            if (skinIndex >= 0)
            {
                SetSelection(SKIN_GROUP, skinIndex);
            }
        }
    }
    
    /// <summary>
    /// Get the index of a skin by name (you may need to customize this)
    /// </summary>
    int GetSkinIndex(string skinName)
    {
        // This is a placeholder implementation
        // You'll need to implement this based on how your skin selection works
        if (SkinManager.Instance != null)
        {
            var availableSkins = SkinManager.Instance.GetAvailableSkins();
            for (int i = 0; i < availableSkins.Count; i++)
            {
                if (availableSkins[i].skinName == skinName)
                {
                    return i;
                }
            }
        }
        return -1;
    }
    
    /// <summary>
    /// Manually set glow on a specific button by reference
    /// </summary>
    public void SetGlowOnButton(Button button, bool showGlow)
    {
        UIGlowEffect glowEffect = button.GetComponent<UIGlowEffect>();
        if (glowEffect != null)
        {
            if (showGlow)
                glowEffect.ShowGlow();
            else
                glowEffect.HideGlow();
        }
    }
    
    /// <summary>
    /// Add a button to a glow group dynamically
    /// </summary>
    public void AddButtonToGroup(Button button, string groupName, GlowSettings glowSettings = null)
    {
        if (!glowGroups.ContainsKey(groupName))
        {
            glowGroups[groupName] = new List<UIGlowEffect>();
            currentSelections[groupName] = -1;
        }
        
        UIGlowEffect glowEffect = button.GetComponent<UIGlowEffect>();
        if (glowEffect == null)
        {
            glowEffect = button.gameObject.AddComponent<UIGlowEffect>();
        }
        
        if (glowSettings != null)
        {
            glowEffect.glowSettings = glowSettings;
        }
        
        glowGroups[groupName].Add(glowEffect);
        Debug.Log($"[SelectionGlowManager] Added button {button.name} to group {groupName}");
    }
    
    /// <summary>
    /// Clear all glows in a group
    /// </summary>
    public void ClearGroupSelection(string groupName)
    {
        if (glowGroups.ContainsKey(groupName))
        {
            foreach (UIGlowEffect effect in glowGroups[groupName])
            {
                effect.HideGlow();
            }
            currentSelections[groupName] = -1;
        }
    }
    
    /// <summary>
    /// Get current selection index for a group
    /// </summary>
    public int GetCurrentSelection(string groupName)
    {
        return currentSelections.ContainsKey(groupName) ? currentSelections[groupName] : -1;
    }
    
    // Debug methods
    [ContextMenu("Test Dice Selection 0")]
    void TestDiceSelection0() { if (Application.isPlaying) SetSelection(DICE_GROUP, 0); }
    
    [ContextMenu("Test Dice Selection 1")]
    void TestDiceSelection1() { if (Application.isPlaying) SetSelection(DICE_GROUP, 1); }
    
    [ContextMenu("Test Skin Selection 0")]
    void TestSkinSelection0() { if (Application.isPlaying) SetSelection(SKIN_GROUP, 0); }
    
    [ContextMenu("Clear All")]
    void TestClearAll() 
    { 
        if (Application.isPlaying) 
        {
            ClearGroupSelection(DICE_GROUP);
            ClearGroupSelection(SKIN_GROUP);
        }
    }
}
