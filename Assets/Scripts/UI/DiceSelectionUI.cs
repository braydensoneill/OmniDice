using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiceSelectionUI : MonoBehaviour
{
    [Header("References")]
    public InfiniteScroll diceSelection;
    public DiceSpawnManager diceSpawnManager;

    [Header("UI Elements")]
    public Button addButton;
    public Button removeButton;
    public TextMeshProUGUI selectedDiceNameText;
    public TextMeshProUGUI selectedDiceCountText;

    [Header("Settings")]
    public string defaultText = "No Dice Selected";

    private int currentSelectedIndex = 0;

    void Start()
    {
        // Subscribe to the infinite scroll selection changes
        if (diceSelection != null)
        {
            diceSelection.OnSelectedDiceChanged.AddListener(OnDiceSelectionChanged);
        }

        // Setup button listeners
        if (addButton != null)
        {
            addButton.onClick.AddListener(AddSelectedDice);
        }

        if (removeButton != null)
        {
            removeButton.onClick.AddListener(RemoveSelectedDice);
        }

        // Initialize UI
        UpdateUI();
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (diceSelection != null)
        {
            diceSelection.OnSelectedDiceChanged.RemoveListener(OnDiceSelectionChanged);
        }
    }

    public void OnDiceSelectionChanged(int selectedIndex)
    {
        currentSelectedIndex = selectedIndex;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (diceSelection == null || diceSpawnManager == null) return;

        // Update the selected dice name text
        string diceName = GetSelectedDiceName();
        if (selectedDiceNameText != null)
        {
            selectedDiceNameText.text = string.IsNullOrEmpty(diceName) ? defaultText : diceName;
        }

        // Update the count for the selected dice type
        UpdateSelectedDiceCount();

        // Enable/disable buttons based on availability
        UpdateButtonStates();
    }

    string GetSelectedDiceName()
    {
        if (diceSelection.itemList != null &&
            currentSelectedIndex >= 0 &&
            currentSelectedIndex < diceSelection.itemList.Length)
        {
            return diceSelection.itemList[currentSelectedIndex].name.Replace("(Clone)", "").Trim();
        }
        return "";
    }

    GameObject GetSelectedDicePrefab()
    {
        // Get the name of the currently selected dice from the infinite scroll
        string selectedDiceName = GetSelectedDiceName();
        if (string.IsNullOrEmpty(selectedDiceName))
            return null;

        // Find the matching dice type by name in the DiceSpawnManager
        if (diceSpawnManager.diceTypes != null)
        {
            var matchingDiceType = diceSpawnManager.diceTypes.Find(t =>
                t.name.Equals(selectedDiceName, System.StringComparison.OrdinalIgnoreCase) ||
                t.prefab.name.Equals(selectedDiceName, System.StringComparison.OrdinalIgnoreCase));

            if (matchingDiceType != null)
            {
                return matchingDiceType.prefab;
            }
        }

        return null;
    }

    void UpdateSelectedDiceCount()
    {
        if (selectedDiceCountText == null) return;

        GameObject selectedPrefab = GetSelectedDicePrefab();
        if (selectedPrefab != null)
        {
            // Find the matching dice type and get its count
            var diceType = diceSpawnManager.diceTypes.Find(t => t.prefab == selectedPrefab);
            if (diceType != null && diceType.countText != null)
            {
                selectedDiceCountText.text = diceType.countText.text;
            }
            else
            {
                selectedDiceCountText.text = "0";
            }
        }
        else
        {
            selectedDiceCountText.text = "0";
        }
    }

    void UpdateButtonStates()
    {
        GameObject selectedPrefab = GetSelectedDicePrefab();
        bool hasDiceSelected = selectedPrefab != null;

        // Both buttons are enabled if we have a valid selection
        // Let the DiceSpawnManager handle whether there are actually dice to add/remove
        if (addButton != null)
        {
            addButton.interactable = hasDiceSelected;
        }

        if (removeButton != null)
        {
            removeButton.interactable = hasDiceSelected;
        }
    }

    public void AddSelectedDice()
    {
        string selectedDiceName = GetSelectedDiceName();
        GameObject selectedPrefab = GetSelectedDicePrefab();

        Debug.Log($"Adding dice - Selected Name: '{selectedDiceName}', Prefab: '{(selectedPrefab != null ? selectedPrefab.name : "NULL")}'");

        if (selectedPrefab != null && diceSpawnManager != null)
        {
            diceSpawnManager.AddDice(selectedPrefab);
            UpdateUI(); // Refresh UI after adding
        }
        else
        {
            Debug.LogWarning($"Could not add dice - selectedPrefab is null or diceSpawnManager is null");
        }
    }

    public void RemoveSelectedDice()
    {
        string selectedDiceName = GetSelectedDiceName();
        GameObject selectedPrefab = GetSelectedDicePrefab();

        Debug.Log($"Removing dice - Selected Name: '{selectedDiceName}', Prefab: '{(selectedPrefab != null ? selectedPrefab.name : "NULL")}'");

        if (selectedPrefab != null && diceSpawnManager != null)
        {
            diceSpawnManager.RemoveDice(selectedPrefab);
            UpdateUI(); // Refresh UI after removing
        }
        else
        {
            Debug.LogWarning($"Could not remove dice - selectedPrefab is null or diceSpawnManager is null");
        }
    }

    // Public method to manually refresh the UI (useful for external calls)
    public void RefreshUI()
    {
        UpdateUI();
    }

    // Public method to get the currently selected dice name (for external access)
    public string GetCurrentSelectedDiceName()
    {
        return GetSelectedDiceName();
    }

    // Debug method to check the alignment between infinite scroll items and dice types
    [ContextMenu("Debug Lists")]
    public void DebugLists()
    {
        Debug.Log("=== INFINITE SCROLL ITEMS ===");
        if (diceSelection != null && diceSelection.itemList != null)
        {
            for (int i = 0; i < diceSelection.itemList.Length; i++)
            {
                Debug.Log($"Index {i}: {diceSelection.itemList[i].name}");
            }
        }

        Debug.Log("=== DICE SPAWN MANAGER TYPES ===");
        if (diceSpawnManager != null && diceSpawnManager.diceTypes != null)
        {
            for (int i = 0; i < diceSpawnManager.diceTypes.Count; i++)
            {
                var diceType = diceSpawnManager.diceTypes[i];
                Debug.Log($"Index {i}: Name='{diceType.name}', Prefab='{diceType.prefab.name}'");
            }
        }

        Debug.Log($"Current Selected Index: {currentSelectedIndex}");
        Debug.Log($"Current Selected Name: '{GetSelectedDiceName()}'");
        Debug.Log($"Current Selected Prefab: '{(GetSelectedDicePrefab() != null ? GetSelectedDicePrefab().name : "NULL")}'");
    }
}
