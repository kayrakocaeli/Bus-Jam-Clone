using Coffee.UIExtensions;
using UnityEngine;

public class WinPanel : MonoBehaviour
{
    [SerializeField] private UIParticle _confettiEffect;

    public void Show()
    {
        gameObject.SetActive(true);

        if (_confettiEffect != null)
        {
            _confettiEffect.Play();
        }
    }
}