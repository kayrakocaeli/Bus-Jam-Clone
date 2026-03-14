using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SettingsPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform panelContent;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Buttons")]
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private Button resetProgressButton;
    [SerializeField] private Button closeButton;

    private bool _isOpen = false;
    private bool _isAnimating = false;

    private void Awake()
    {
        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);

        closeButton.onClick.AddListener(Hide);
        backToMenuButton.onClick.AddListener(BackToMenu);
        resetProgressButton.onClick.AddListener(ResetProgress);

        _isOpen = false;

        panelContent.localScale = Vector3.zero;
    }

    public void TogglePanel(bool isMenuScene)
    {
        if (_isAnimating) return;

        if (_isOpen)
            Hide();
        else
            Show(isMenuScene);
    }

    public void Show(bool isMenuScene)
    {
        _isAnimating = true;
        _isOpen = true;
        gameObject.SetActive(true);

        backToMenuButton.gameObject.SetActive(!isMenuScene);
        resetProgressButton.gameObject.SetActive(isMenuScene);

        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        panelContent.DOKill();
        panelContent.localScale = Vector3.zero;
        panelContent.DOScale(Vector3.one, 0.3f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() => _isAnimating = false);
    }

    public void Hide()
    {
        _isAnimating = true;
        _isOpen = false;

        panelContent.DOKill();
        panelContent.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => {
                gameObject.SetActive(false);
                _isAnimating = false;
            });
    }

    private void OnMusicSliderChanged(float value) => SoundManager.Instance?.SetMusicVolume(value);
    private void OnSFXSliderChanged(float value) => SoundManager.Instance?.SetSFXVolume(value);

    private void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    private void ResetProgress()
    {
        PlayerPrefs.SetInt("CurrentLevel", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}