using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SkinData
{
    public string skinName;
    public Dictionary<string, Material> diceMaterials; // Changed from prefabs to materials
    public Sprite previewSprite; // For the UI display

    public SkinData(string name)
    {
        skinName = name;
        diceMaterials = new Dictionary<string, Material>(); // Changed from prefabs to materials
    }
}