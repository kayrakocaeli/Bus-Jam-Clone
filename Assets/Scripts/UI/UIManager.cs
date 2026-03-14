using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _levelIndicatorText;
    [SerializeField] private TextMeshProUGUI _timerText;

    [Header("Panels")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _levelCompletePanel;

    [Header("Buttons")]
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _nextLevelButton;

    private int _lastDisplayedSecond = -1;

    private void Start()
    {
        var gm = GameContext.Get<GameManager>();
        if (gm != null)
        {
            gm.OnTimerUpdated += UpdateTimerUI;
            gm.OnLevelLoaded += UpdateLevelIndicator;
            gm.OnGameStateChanged += HandleGameStateChanged;
        }

        if (_restartButton) _restartButton.onClick.AddListener(OnRestartClicked);
        if (_nextLevelButton) _nextLevelButton.onClick.AddListener(OnNextLevelClicked);

        SoundManager.Instance?.PlayMusic(SoundType.GameMusic);

    }

    private void OnDestroy()
    {
        if (GameContext.TryGet<GameManager>(out var gm))
        {
            gm.OnTimerUpdated -= UpdateTimerUI;
            gm.OnLevelLoaded -= UpdateLevelIndicator;
            gm.OnGameStateChanged -= HandleGameStateChanged;
        }

        GameContext.Unregister<UIManager>();
    }

    private void HandleGameStateChanged(GameState state)
    {
        DOVirtual.DelayedCall(0.7f, () => {
            _levelCompletePanel.SetActive(state == GameState.Complete);
        });
        _gameOverPanel.SetActive(state == GameState.GameOver);
        
    }

    private void UpdateTimerUI(float timeRemaining)
    {
        int seconds = Mathf.CeilToInt(timeRemaining);

        if (seconds == _lastDisplayedSecond) return;

        _lastDisplayedSecond = seconds;
        TimeSpan time = TimeSpan.FromSeconds(Mathf.Max(0, seconds));
        _timerText.text = time.ToString(@"mm\:ss");
    }

    private void UpdateLevelIndicator(int levelIndex)
    {
        _levelIndicatorText.text = "Level " + levelIndex;
    }

    private void OnRestartClicked() => GameContext.Get<GameManager>().RestartLevel();
    private void OnNextLevelClicked() => GameContext.Get<GameManager>().NextLevel();
}