using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ColorMaterialPair
{
    public GameColors color;
    public Material passengerMaterial;
    public Material busMaterial;
}

[CreateAssetMenu(fileName = "ColorCatalog", menuName = "BusJam/ColorCatalog")]
public class ColorCatalogSO : ScriptableObject
{
    public List<ColorMaterialPair> catalog = new();

    // Gets material for passengers
    public Material GetPassengerMaterial(GameColors color)
    {
        foreach (var pair in catalog)
        {
            if (pair.color == color) return pair.passengerMaterial;
        }
        return null;
    }

    // Gets material for buses
    public Material GetBusMaterial(GameColors color)
    {
        foreach (var pair in catalog)
        {
            if (pair.color == color) return pair.busMaterial;
        }
        return null;
    }
}