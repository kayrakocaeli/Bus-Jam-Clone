using UnityEngine;
using UnityEditor;

public class ShaderConverter : Editor
{
    [MenuItem("Tools/Convert Materials To Curved World")]
    public static void ConvertSelectedMaterials()
    {
        // Get all materials in the selected folder(s)
        Material[] materials = Selection.GetFiltered<Material>(SelectionMode.DeepAssets);

        if (materials.Length == 0)
        {
            Debug.LogWarning("Please select a folder containing materials in the Project window.");
            return;
        }

        Shader curvedShader = Shader.Find("Custom/CurvedWorld");

        if (curvedShader == null)
        {
            Debug.LogError("Custom/CurvedWorld shader not found!");
            return;
        }

        int changedCount = 0;

        foreach (Material mat in materials)
        {
            if (mat.shader != curvedShader)
            {
                mat.shader = curvedShader;
                EditorUtility.SetDirty(mat);
                changedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Success! Converted {changedCount} materials to Curved World.");
    }
}