using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DiceSpawnerManager : MonoBehaviour
{
    [Header("Dice Settings")]
    public GameObject dicePrefab;
    public Transform diceParent;
    public Vector3 spawnPosition = Vector3.zero;

    [Header("Scaling")]
    public float minScale = 0.2f;
    public float maxScale = 1f;
    public int maxDiceCountForMinScale = 40;

    [Header("UI")]
    public TextMeshProUGUI diceCountText;

    private List<GameObject> diceList = new List<GameObject>();

    private void Start()
    {
        // Collect all existing dice in the scene
        DiceManager[] existingDice = FindObjectsOfType<DiceManager>();
        foreach (DiceManager dice in existingDice)
        {
            if (!diceList.Contains(dice.gameObject))
            {
                diceList.Add(dice.gameObject);
            }
        }

        RescaleAllDice();
        UpdateDiceCountText();
    }

    public void AddDice()
    {
        if (dicePrefab == null) return;

        GameObject newDice = Instantiate(dicePrefab, spawnPosition, Random.rotation, diceParent);
        diceList.Add(newDice);
        RescaleAllDice();
        UpdateDiceCountText();
    }

    public void RemoveDice()
    {
        if (diceList.Count == 0) return;

        GameObject diceToRemove = diceList[diceList.Count - 1];
        diceList.RemoveAt(diceList.Count - 1);
        Destroy(diceToRemove);
        RescaleAllDice();
        UpdateDiceCountText();
    }

    private void RescaleAllDice()
    {
        int count = diceList.Count;
        float t = Mathf.InverseLerp(1, maxDiceCountForMinScale, count);
        float scale = Mathf.Lerp(maxScale, minScale, t);

        foreach (GameObject dice in diceList)
        {
            if (dice != null)
            {
                dice.transform.localScale = Vector3.one * scale;
            }
        }
    }

    private void UpdateDiceCountText()
    {
        if (diceCountText != null)
        {
            diceCountText.text = $"{diceList.Count}";
        }
    }
}
