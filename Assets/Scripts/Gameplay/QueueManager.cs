using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class QueueManager : MonoBehaviour
{
    [SerializeField] private List<Transform> queueSlots;
    private Passenger[] _queuedPassengers;
    private int _reservedCount = 0;

    private void Awake()
    {
        GameContext.Register(this);
        _queuedPassengers = new Passenger[queueSlots.Count];
    }

    public bool HasEmptySlot()
    {
        int occupied = GetOccupiedCount();
        bool hasRoom = (occupied + _reservedCount) < queueSlots.Count;

        return hasRoom;
    }

    public void ReserveSlot() => _reservedCount++;

    public void EnqueuePassenger(Passenger passenger)
    {
        _reservedCount = Mathf.Max(0, _reservedCount - 1);

        for (int i = 0; i < _queuedPassengers.Length; i++)
        {
            if (_queuedPassengers[i] == null)
            {
                _queuedPassengers[i] = passenger;
                MoveToSlot(passenger, i);
                return;
            }
        }
    }

    private void MoveToSlot(Passenger p, int idx)
    {
        Vector3 target = queueSlots[idx].position;
        target.y = 0;
        p.SetRunning(true);
        p.transform.DOMove(target, 0.4f).SetEase(Ease.OutQuad).OnComplete(() => {
            p.SetRunning(false);
            p.transform.DORotate(Vector3.zero, 0.2f);
            GameContext.Get<BusManager>()?.CheckQueueForWaitingPassengers();
        });
    }

    public void TrySendToBus()
    {
        var busManager = GameContext.Get<BusManager>();
        if (busManager == null || busManager.CurrentBus == null) return;

        bool someoneLeft = false;
        for (int i = 0; i < _queuedPassengers.Length; i++)
        {
            var p = _queuedPassengers[i];
            if (p != null && busManager.HasAvailableBusFor(p.Color, false))
            {
                busManager.ReserveSeat();
                busManager.BoardPassenger(p);
                _queuedPassengers[i] = null;
                someoneLeft = true;
            }
        }
        if (someoneLeft) ShiftQueue();
    }

    private void ShiftQueue()
    {
        for (int i = 0; i < _queuedPassengers.Length; i++)
        {
            if (_queuedPassengers[i] == null)
            {
                for (int j = i + 1; j < _queuedPassengers.Length; j++)
                {
                    if (_queuedPassengers[j] != null)
                    {
                        _queuedPassengers[i] = _queuedPassengers[j];
                        _queuedPassengers[j] = null;
                        MoveToSlot(_queuedPassengers[i], i);
                        break;
                    }
                }
            }
        }
        DOVirtual.DelayedCall(0.4f, () => TrySendToBus());
    }
    public bool IsFull() => !HasEmptySlot();
    private int GetOccupiedCount()
    {
        int count = 0;
        foreach (var p in _queuedPassengers) if (p != null) count++;
        return count;
    }

    public bool CanAnyPassengerBoard(GameColors activeBusColor)
    {
        if (activeBusColor == GameColors.None) return false;

        foreach (var p in _queuedPassengers)
        {
            if (p != null && p.Color == activeBusColor) return true;
        }
        return false;
    }

    public int GetColorCountInQueue(GameColors color)
    {
        int count = 0;
        foreach (var p in _queuedPassengers) if (p != null && p.Color == color) count++;
        return count;
    }
}