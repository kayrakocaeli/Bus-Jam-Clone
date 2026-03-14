using UnityEngine;

[ExecuteAlways]
public class CurvedWorldController : MonoBehaviour
{
    [Range(-0.05f, 0.05f)]
    public float curveStrength = 0.005f;
    public float curveOffset = 10f;

    private void Update()
    {
        // Update all CurvedWorld materials globally
        Shader.SetGlobalFloat("_GlobalCurveStrength", curveStrength);
        Shader.SetGlobalFloat("_GlobalCurveOffset", curveOffset);
    }
}