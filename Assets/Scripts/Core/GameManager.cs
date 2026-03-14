using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public event Action<GameState> OnGameStateChanged;
    public event Action<float> OnTimerUpdated;
    public event Action<int> OnLevelLoaded;

    private GameState _currentState;
    private float _currentTimer;
    private bool _isTimerRunning;

    private GridManager _grid;
    private BusManager _bus;
    private QueueManager _queue;

    [Header("Test Setup")]
    public LevelDataSO testLevelOverride;

    private const string LevelKey = "CurrentLevel";

    private void Awake() => GameContext.Register(this);
    private void OnDestroy() => GameContext.Unregister<GameManager>();

    private void Start()
    {
        _grid = GameContext.Get<GridManager>();
        _bus = GameContext.Get<BusManager>();
        _queue = GameContext.Get<QueueManager>();

        LoadLevel();

        if (_bus != null) _bus.OnBusChanged += () => _grid.RefreshOutlines();
    }

    private void Update()
    {
        if (_currentState == GameState.Gameplay && _isTimerRunning)
        {
            _currentTimer -= Time.deltaTime;

            if (_currentTimer <= 0f)
            {
                _currentTimer = 0f;
                _isTimerRunning = false;
                ChangeState(GameState.GameOver);
            }

            OnTimerUpdated?.Invoke(_currentTimer);
        }
    }

    private void LoadLevel()
    {
        LevelDataSO levelToLoad;
        int currentLevelIndex = PlayerPrefs.GetInt(LevelKey, 1);

        if (testLevelOverride != null)
        {
            levelToLoad = testLevelOverride;
        }
        else
        {
            levelToLoad = Resources.Load<LevelDataSO>($"Levels/Level_{currentLevelIndex}");

            if (levelToLoad == null)
            {
                currentLevelIndex = 1;
                PlayerPrefs.SetInt(LevelKey, 1);
                levelToLoad = Resources.Load<LevelDataSO>($"Levels/Level_1");
            }
        }

        OnLevelLoaded?.Invoke(currentLevelIndex);

        GameContext.Get<GridManager>()?.InitializeLevel(levelToLoad);
        GameContext.Get<BusManager>()?.Initialize(levelToLoad);
        InitializeLevel(levelToLoad.levelDuration);
    }

    public void InitializeLevel(float duration)
    {
        _currentTimer = duration;
        _isTimerRunning = false;
        ChangeState(GameState.Start);
        OnTimerUpdated?.Invoke(_currentTimer);
    }

    public void ReceiveFirstInput()
    {
        if (_currentState == GameState.Start)
        {
            _isTimerRunning = true;
            ChangeState(GameState.Gameplay);
        }
    }

    public void ChangeState(GameState newState)
    {
        _currentState = newState;
        OnGameStateChanged?.Invoke(_currentState);

        switch (newState)
        {
            case GameState.Gameplay:
                SoundManager.Instance?.PlayMusic(SoundType.GameMusic);
                break;

            case GameState.Complete:
                SoundManager.Instance?.PlaySFX(SoundType.LevelWin, 0.75f);
                int nextLevel = PlayerPrefs.GetInt(LevelKey, 1) + 1;
                PlayerPrefs.SetInt(LevelKey, nextLevel);
                break;

            case GameState.GameOver:
                SoundManager.Instance?.PlaySFX(SoundType.LevelFail);
                break;
        }
    }

    public GameState GetCurrentState() => _currentState;

    public void RestartLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void NextLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    public void CheckGameOverState()
    {
        if (_queue.IsFull())
        {
            if (_bus.CurrentBus != null && _queue.CanAnyPassengerBoard(_bus.CurrentBus.Color)) return;
            if (CanAnyGridPassengerBoardDirectly()) return;

            ChangeState(GameState.GameOver);
        }
    }

    private bool CanAnyGridPassengerBoardDirectly()
    {
        var bus = GameContext.Get<BusManager>();
        var grid = GameContext.Get<GridManager>();

        if (bus.CurrentBus == null) return false;

        foreach (var p in grid.ActivePassengers)
        {
            if (p.Color == bus.CurrentBus.Color)
            {
                var path = grid.FindPathToExit(p.CurrentCoords);
                if (path != null && path.Count > 0) return true;
            }
        }
        return false;
    }
}