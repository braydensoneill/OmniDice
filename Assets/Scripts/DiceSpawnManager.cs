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

    private Dictionary<DiceType, List<GameObject>> diceList = new Dictionary<DiceType, List<GameObject>>();

    private void Start()
    {
        // Initialize dictionary
        foreach (var type in diceTypes)
        {
            diceList[type] = new List<GameObject>();
        }

        // Collect existing dice in the scene
        DiceManager[] existingDice = FindObjectsOfType<DiceManager>();
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

        foreach (var type in diceTypes)
        {
            UpdateScaleForType(type);
            UpdateDiceCountText(type);
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
        UpdateScaleForType(type);
        UpdateDiceCountText(type);

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
        UpdateScaleForType(type);
        UpdateDiceCountText(type);

        if (shakeManager != null)
            shakeManager.RefreshDiceList();
    }

    private void UpdateScaleForType(DiceType type)
    {
        int count = diceList[type].Count;
        float t = Mathf.InverseLerp(1, maxDiceCountForMinScale, count);
        float scale = Mathf.Lerp(maxScale, minScale, t);

        foreach (GameObject dice in diceList[type])
        {
            if (dice != null)
            {
                dice.transform.localScale = Vector3.one * scale;
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
}
