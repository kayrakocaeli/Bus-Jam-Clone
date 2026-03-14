using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }

    private void PlaySound()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(SoundType.ButtonClick);
    }
}