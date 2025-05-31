using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DiceTypeUI
{
    public GameObject dicePrefab; // The prefab for this dice type
    public TextMeshProUGUI countText;        // UI Text element to display this type's count
}

public class DiceSpawnManager : MonoBehaviour
{
    [Header("Assign your Dice types and their associated count texts")]
    public List<DiceTypeUI> diceTypes = new List<DiceTypeUI>();

    // Keep track of all spawned dice in the scene
    private List<GameObject> spawnedDiceInstances = new List<GameObject>();

    private void Start()
    {
        // Optionally pre-populate spawnedDiceInstances by finding all existing dice in the scene
        foreach (DiceTypeUI typeUI in diceTypes)
        {
            GameObject[] foundDice = GameObject.FindGameObjectsWithTag(typeUI.dicePrefab.tag); // assumes each dice type has a unique tag
            foreach (GameObject dice in foundDice)
            {
                if (dice.name.Replace("(Clone)", "") == typeUI.dicePrefab.name)
                {
                    spawnedDiceInstances.Add(dice);
                }
            }
        }

        UpdateDiceCounts();
    }

    // Add a new dice of the given prefab type
    public void AddDice(GameObject dicePrefabToAdd)
    {
        if (dicePrefabToAdd == null)
        {
            Debug.LogWarning("Dice Prefab to Add is null. Ensure the button's OnClick is set correctly.");
            return;
        }

        // Spawn the dice at a calculated position
        GameObject newDice = Instantiate(dicePrefabToAdd, GetSpawnPosition(), Quaternion.identity);
        spawnedDiceInstances.Add(newDice);

        UpdateDiceCounts();
    }

    // Remove one instance of the specified dice type
    public void RemoveDice(GameObject dicePrefabToRemove)
    {
        if (dicePrefabToRemove == null)
        {
            Debug.LogWarning("Dice Prefab to Remove is null. Ensure the button's OnClick is set correctly.");
            return;
        }

        GameObject instanceToRemove = null;

        // Find and remove the most recently added matching dice
        for (int i = spawnedDiceInstances.Count - 1; i >= 0; i--)
        {
            if (spawnedDiceInstances[i] != null &&
                spawnedDiceInstances[i].name.Replace("(Clone)", "") == dicePrefabToRemove.name)
            {
                instanceToRemove = spawnedDiceInstances[i];
                spawnedDiceInstances.RemoveAt(i);
                break;
            }
        }

        if (instanceToRemove != null)
        {
            Destroy(instanceToRemove);
        }
        else
        {
            Debug.LogWarning($"No {dicePrefabToRemove.name} found to remove.");
        }

        UpdateDiceCounts();
    }

    // Remove all spawned dice from the scene
    public void ClearAllDice()
    {
        foreach (GameObject dice in spawnedDiceInstances)
        {
            if (dice != null)
                Destroy(dice);
        }

        spawnedDiceInstances.Clear();
        UpdateDiceCounts();
    }

    // Get a position to spawn the new dice
    private Vector3 GetSpawnPosition()
    {
        if (spawnedDiceInstances.Count > 0)
        {
            Vector3 lastPos = spawnedDiceInstances[spawnedDiceInstances.Count - 1].transform.position;
            return lastPos + new Vector3(0, 1.5f, 0); // Stack vertically
        }
        else
        {
            return new Vector3(0, 5, 0); // Starting position
        }
    }

    // Updates the count text for each dice type based on currently spawned instances
    private void UpdateDiceCounts()
    {
        foreach (DiceTypeUI typeUI in diceTypes)
        {
            int count = 0;

            foreach (GameObject dice in spawnedDiceInstances)
            {
                if (dice != null && dice.name.Replace("(Clone)", "") == typeUI.dicePrefab.name)
                {
                    count++;
                }
            }

            if (typeUI.countText != null)
            {
                typeUI.countText.text = count.ToString();
            }
        }
    }
}
