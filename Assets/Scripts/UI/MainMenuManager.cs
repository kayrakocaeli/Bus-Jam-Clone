using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Button _playButton;

    private const string LEVEL_KEY = "CurrentLevel";
    private const string GAME_SCENE_NAME = "Game";

    private void Start()
    {
        int currentLevel = PlayerPrefs.GetInt(LEVEL_KEY, 1);

        if (_levelText)
            _levelText.text = "Level " + currentLevel;

        if (_playButton)
            _playButton.onClick.AddListener(PlayGame);

        SoundManager.Instance?.PlayMusic(SoundType.GameMusic);
    }

    private void OnDestroy()
    {
        if (_playButton)
            _playButton.onClick.RemoveListener(PlayGame);
    }

    private void PlayGame()
    {
        SceneManager.LoadScene(GAME_SCENE_NAME);
    }
}