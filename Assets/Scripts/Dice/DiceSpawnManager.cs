using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class DiceType
{
    public string name;
    public GameObject prefab;
    public TextMeshProUGUI countText;
}

public class DiceSpawnManager : MonoBehaviour
{
    [Header("External References")]
    public ShakeManager shakeManager;
    public DiceSelectionUI diceSelectionUI;

    [Header("Dice Types")]
    public List<DiceType> diceTypes;
    public Transform diceParent;
    public Vector3 spawnPosition = Vector3.zero;

    [Header("Scaling")]
    public float minScale = 0.2f;
    public float maxScale = 1f;
    public int maxDiceCountForMinScale = 40;
    [SerializeField] private float scaleCurve = 2.0f; // Higher values = more dramatic early scaling
    [SerializeField] private float scaleTransitionSpeed = 5.0f; // How fast dice resize (higher = faster)

    [Header("UI")]
    public TextMeshProUGUI totalDiceCountText;

    private Dictionary<DiceType, List<GameObject>> diceList = new Dictionary<DiceType, List<GameObject>>();
    private float targetScale = 1.0f; // The scale all dice should lerp towards
    private Dictionary<GameObject, float> currentScales = new Dictionary<GameObject, float>(); // Track individual dice scales

    private void Start()
    {
        // Initialize dictionary
        foreach (var type in diceTypes)
        {
            diceList[type] = new List<GameObject>();
        }

        // Collect existing dice in the scene using the modern API
        DiceManager[] existingDice = FindObjectsByType<DiceManager>(FindObjectsSortMode.None);
        foreach (DiceManager dice in existingDice)
        {
            foreach (var type in diceTypes)
            {
                if (dice.gameObject.name.StartsWith(type.prefab.name))
                {
                    diceList[type].Add(dice.gameObject);
                    break;
                }
            }
        }

        UpdateScaleForAllDice();

        foreach (var type in diceTypes)
        {
            UpdateDiceCountText(type);
        }

        UpdateTotalDiceCount();
    }

    private void Update()
    {
        // Smoothly lerp all dice towards target scale
        var dicesToRemove = new List<GameObject>();
        var scaleUpdates = new Dictionary<GameObject, float>();

        foreach (var kvp in currentScales)
        {
            GameObject dice = kvp.Key;
            float currentScale = kvp.Value;

            if (dice == null)
            {
                dicesToRemove.Add(dice);
                continue;
            }

            // Lerp towards target scale
            float newScale = Mathf.Lerp(currentScale, targetScale, scaleTransitionSpeed * Time.deltaTime);
            scaleUpdates[dice] = newScale;

            // Apply the lerped scale
            dice.transform.localScale = Vector3.one * newScale;
        }

        // Apply all scale updates after iteration
        foreach (var kvp in scaleUpdates)
        {
            currentScales[kvp.Key] = kvp.Value;
        }

        // Clean up null references
        foreach (GameObject dice in dicesToRemove)
        {
            currentScales.Remove(dice);
        }
    }

    public void AddDice(GameObject prefab)
    {
        DiceType type = diceTypes.Find(t => t.prefab == prefab);
        if (type == null)
        {
            Debug.LogWarning("No matching DiceType found for this prefab.");
            return;
        }

        GameObject newDice = Instantiate(type.prefab, spawnPosition, Random.rotation, diceParent);
        diceList[type].Add(newDice);

        // Apply current skin to the newly spawned dice
        if (SkinManager.Instance != null)
        {
            SkinManager.Instance.ApplySkinToDice(newDice);
        }

        UpdateScaleForAllDice();
        UpdateDiceCountText(type);
        UpdateTotalDiceCount();

        // Notify DiceSelectionUI to refresh
        if (diceSelectionUI != null)
            diceSelectionUI.RefreshUI();

        if (shakeManager != null)
            shakeManager.RefreshDiceList();
    }

    public void RemoveDice(GameObject prefab)
    {
        DiceType type = diceTypes.Find(t => t.prefab == prefab);
        if (type == null || !diceList.ContainsKey(type) || diceList[type].Count == 0)
        {
            Debug.LogWarning("No dice of this type to remove.");
            return;
        }

        GameObject diceToRemove = diceList[type][diceList[type].Count - 1];
        diceList[type].RemoveAt(diceList[type].Count - 1);
        Destroy(diceToRemove);

        UpdateScaleForAllDice();
        UpdateDiceCountText(type);
        UpdateTotalDiceCount();

        // Notify DiceSelectionUI to refresh
        if (diceSelectionUI != null)
            diceSelectionUI.RefreshUI();

        if (shakeManager != null)
            shakeManager.RefreshDiceList();
    }

    public void ClearDice()
    {
        foreach (var type in diceTypes)
        {
            List<GameObject> diceToRemove = diceList[type];
            foreach (GameObject dice in diceToRemove)
            {
                if (dice != null)
                {
                    Destroy(dice);
                }
            }
            diceToRemove.Clear();
            UpdateDiceCountText(type);
        }

        UpdateScaleForAllDice();
        UpdateTotalDiceCount();

        // Notify DiceSelectionUI to refresh
        if (diceSelectionUI != null)
            diceSelectionUI.RefreshUI();

        if (shakeManager != null)
            shakeManager.RefreshDiceList();
    }

    private void UpdateScaleForAllDice()
    {
        int totalCount = 0;
        foreach (var type in diceTypes)
        {
            totalCount += diceList[type].Count;
        }

        // Use exponential scaling for more dramatic early changes
        float normalizedCount = Mathf.InverseLerp(1, maxDiceCountForMinScale, totalCount);

        // Apply exponential curve - higher scaleCurve = more dramatic early scaling
        float exponentialT = Mathf.Pow(normalizedCount, 1.0f / scaleCurve);

        // Calculate target scale but don't apply it immediately
        targetScale = Mathf.Lerp(maxScale, minScale, exponentialT);

        // Initialize current scales for new dice
        foreach (var type in diceTypes)
        {
            var diceTypeList = diceList[type];
            for (int i = diceTypeList.Count - 1; i >= 0; i--)
            {
                GameObject dice = diceTypeList[i];
                if (dice != null)
                {
                    // Initialize scale tracking for new dice
                    if (!currentScales.ContainsKey(dice))
                    {
                        currentScales[dice] = dice.transform.localScale.x;
                    }
                }
                else
                {
                    // Remove null references during iteration
                    diceTypeList.RemoveAt(i);
                }
            }
        }
    }

    private void UpdateDiceCountText(DiceType type)
    {
        if (type.countText != null)
        {
            type.countText.text = diceList[type].Count.ToString();
        }
    }

    private void UpdateTotalDiceCount()
    {
        int totalCount = 0;
        foreach (var type in diceTypes)
        {
            totalCount += diceList[type].Count;
        }

        if (totalDiceCountText != null)
        {
            totalDiceCountText.text = totalCount.ToString();
        }
    }

    // Method to get the count of a specific dice type
    public int GetDiceCount(DiceType diceType)
    {
        if (diceType != null && diceList.ContainsKey(diceType))
        {
            return diceList[diceType].Count;
        }
        return 0;
    }

    // Optional wrappers for Unity UI buttons
    public void AddDiceByIndex(int index)
    {
        if (index >= 0 && index < diceTypes.Count)
        {
            AddDice(diceTypes[index].prefab);
        }
    }

    public void RemoveDiceByIndex(int index)
    {
        if (index >= 0 && index < diceTypes.Count)
        {
            RemoveDice(diceTypes[index].prefab);
        }
    }

    // Method to add dice based on the currently selected dice from infinite scroll
    public void AddSelectedDice(int selectedIndex)
    {
        AddDiceByIndex(selectedIndex);
    }

    // Method to remove dice based on the currently selected dice from infinite scroll
    public void RemoveSelectedDice(int selectedIndex)
    {
        RemoveDiceByIndex(selectedIndex);
    }
}
