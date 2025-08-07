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

    [Header("Dice Types")]
    public List<DiceType> diceTypes;
    public Transform diceParent;
    public Vector3 spawnPosition = Vector3.zero;

    [Header("Scaling")]
    public float minScale = 0.2f;
    public float maxScale = 1f;
    public int maxDiceCountForMinScale = 40;

    [Header("UI")]
    public TextMeshProUGUI totalDiceCountText;

    private Dictionary<DiceType, List<GameObject>> diceList = new Dictionary<DiceType, List<GameObject>>();

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

        UpdateScaleForAllDice();
        UpdateDiceCountText(type);
        UpdateTotalDiceCount();

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

        if (shakeManager != null)
            shakeManager.RefreshDiceList();
    }

    public void RemoveAllDice()
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

        float t = Mathf.InverseLerp(1, maxDiceCountForMinScale, totalCount);
        float scale = Mathf.Lerp(maxScale, minScale, t);
        Vector3 scaleVector = Vector3.one * scale;

        // Apply scale to all dice in one pass
        foreach (var type in diceTypes)
        {
            var diceTypeList = diceList[type];
            for (int i = diceTypeList.Count - 1; i >= 0; i--)
            {
                GameObject dice = diceTypeList[i];
                if (dice != null)
                {
                    dice.transform.localScale = scaleVector;
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
