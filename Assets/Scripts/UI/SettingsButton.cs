using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class SettingsButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SettingsPanel settingsPanel;
    [SerializeField] private RectTransform gearIconTransform;

    [Header("Animation Settings")]
    [SerializeField] private float rotateDuration = 0.4f;
    [SerializeField] private Ease rotateEase = Ease.OutBack;

    [Header("Scene Settings")]
    [SerializeField] private bool isMenuScene; 

    private Button _button;
    private bool _isAnimating = false;

    private void Awake()
    {
        _button = GetComponent<Button>();

        if (gearIconTransform == null)
            gearIconTransform = GetComponent<RectTransform>();

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        if (_isAnimating || settingsPanel == null || gearIconTransform == null) return;

        _isAnimating = true;


        gearIconTransform.DOKill();

        gearIconTransform.DORotate(new Vector3(0, 0, -360), rotateDuration, RotateMode.FastBeyond360)
            .SetRelative(true)
            .SetEase(rotateEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                settingsPanel.TogglePanel(isMenuScene);
                _isAnimating = false;
            })
            .SetLink(gameObject);
    }
}