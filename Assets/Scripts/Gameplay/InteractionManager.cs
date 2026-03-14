using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;

public class InteractionManager : MonoBehaviour
{
    private GridManager _grid;
    private BusManager _bus;
    private QueueManager _queue;
    private InputManager _input;
    private GameManager _gameManager;

    private int _activeMovements = 0;
    [SerializeField] private float sitAnimationDelay = 0.5f;

    private void Awake() => GameContext.Register(this);

    private void Start()
    {
        _grid = GameContext.Get<GridManager>();
        _bus = GameContext.Get<BusManager>();
        _queue = GameContext.Get<QueueManager>();
        _input = GameContext.Get<InputManager>();
        _gameManager = GameContext.Get<GameManager>();

        if (_input != null) _input.OnPassengerTapped += HandlePassengerTapped;
    }

    private void OnDestroy()
    {
        if (_input != null) _input.OnPassengerTapped -= HandlePassengerTapped;
        GameContext.Unregister<InteractionManager>();
    }

    private void HandlePassengerTapped(Passenger passenger)
    {
        if (_gameManager.GetCurrentState() != GameState.Gameplay || !passenger.IsInteractable) return;

        var path = _grid.FindPathToExit(passenger.CurrentCoords);
        if (path == null || path.Count == 0)
        {
            SoundManager.Instance?.PlaySFX(SoundType.PassangerAngryClick);
            FailFeedback(passenger);
            return;
        }

        SoundManager.Instance?.PlaySFX(SoundType.PassangerClick);

        if (_bus.HasAvailableBusFor(passenger.Color, true))
        {
            PreparePassengerForMove(passenger);
            _bus.ReserveSeat();
            ExecuteMovement(passenger, path, () => _bus.BoardPassenger(passenger));
        }
        else if (_queue.HasEmptySlot())
        {
            PreparePassengerForMove(passenger);
            _queue.ReserveSlot();
            ExecuteMovement(passenger, path, () => _queue.EnqueuePassenger(passenger));
        }
        else
        {
            SoundManager.Instance?.PlaySFX(SoundType.PassangerAngryClick);
            ExecuteReturnMove(passenger, path);
        }
    }

    private void PreparePassengerForMove(Passenger p)
    {
        p.IsInteractable = false;
        _grid.GetTile(p.CurrentCoords).HasPassenger = false;
    }

    private void ExecuteMovement(Passenger p, List<Tile> path, Action onArrival)
    {
        _activeMovements++;
        p.SetOutline(false);
        _grid.RefreshOutlines();

        p.MoveAlongPath(path, () => {
            onArrival?.Invoke();

            DOVirtual.DelayedCall(sitAnimationDelay, () => {
                _activeMovements = Mathf.Max(0, _activeMovements - 1);

                if (_activeMovements == 0) _gameManager.CheckGameOverState();
            });
        });
    }

    private void ExecuteReturnMove(Passenger p, List<Tile> path)
    {
        p.IsInteractable = false;
        p.ShowAngryFeedback();

        p.MoveAlongPath(path, () => {
            p.transform.DORotate(new Vector3(0, 30f, 0), 0.1f)
                .SetLoops(4, LoopType.Yoyo)
                .OnComplete(() => {
                    p.ReturnToOriginalTile(path, () => {
                        var tile = _grid.GetTile(p.CurrentCoords);
                        if (tile != null) tile.HasPassenger = true;
                        p.IsInteractable = true;
                        _grid.RefreshOutlines();
                    });
                });
        });
    }

    private void FailFeedback(Passenger p)
    {
        p.ShowAngryFeedback();

        p.IsInteractable = false;
        p.transform.DOComplete();

        p.transform.DORotate(new Vector3(0, 20f, 0), 0.1f)
            .SetLoops(2, LoopType.Yoyo)
            .OnComplete(() =>
            {
                p.transform.DORotate(Vector3.zero, 0.1f);
                p.IsInteractable = true;
            });
    }
}