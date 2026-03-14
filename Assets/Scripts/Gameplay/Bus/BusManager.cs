using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BusManager : MonoBehaviour
{
    public event Action OnBusChanged;

    [Header("Queue Settings")]
    [SerializeField] private ColorCatalogSO colorCatalog;
    [SerializeField] private Vector3 firstBusPosition = new Vector3(0, 0, 5);
    [SerializeField] private float busSpacing = 7f;
    [SerializeField] private int maxVisibleBuses = 3;

    private Queue<GameColors> _busSequence = new();
    private List<Bus> _activeBuses = new();

    public Bus CurrentBus => _activeBuses.Count > 0 ? _activeBuses[0] : null;

    private int _reservedSeats = 0;
    private bool _isBusTransitioning = false;

    private void Awake() => GameContext.Register(this);
    private void OnDestroy() => GameContext.Unregister<BusManager>();

    public void Initialize(LevelDataSO levelData)
    {
        _busSequence.Clear();
        foreach (var color in levelData.busSpawnSequence)
            _busSequence.Enqueue(color);

        _isBusTransitioning = true;

        int initialSpawnCount = Mathf.Min(maxVisibleBuses, _busSequence.Count);
        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnBusInQueue(i);
        }

        DOVirtual.DelayedCall(0.6f, () =>
        {
            _isBusTransitioning = false;
            OnBusChanged?.Invoke();
            CheckQueueForWaitingPassengers();
        });
    }

    private Vector3 GetQueuePosition(int index)
    {
        return firstBusPosition + new Vector3(-index * busSpacing, 0f, 0f);
    }

    private void SpawnBusInQueue(int slotIndex)
    {
        if (_busSequence.Count == 0) return;

        var nextColor = _busSequence.Dequeue();
        var busObj = GameContext.Get<PoolManager>().Get("Bus");

        Vector3 targetPos = GetQueuePosition(slotIndex);

        Vector3 spawnPos = targetPos + new Vector3(-10, 0, 0);

        busObj.transform.position = spawnPos;
        Bus bus = busObj.GetComponent<Bus>();
        bus.Setup(nextColor, colorCatalog.GetBusMaterial(nextColor));

        _activeBuses.Add(bus);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(SoundType.BusArrive);

        bus.MoveToPoint(targetPos, 0.6f);
    }

    public void CheckBusState()
    {
        if (CurrentBus != null && CurrentBus.IsFull())
        {
            _isBusTransitioning = true;
            Bus departingBus = _activeBuses[0];
            _activeBuses.RemoveAt(0);
            _reservedSeats = 0;

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundType.BusDepart);

            departingBus.DriveAway(() =>
            {
                ShiftBusesForward();
            });
        }
    }

    private void ShiftBusesForward()
    {
        if (_activeBuses.Count == 0 && _busSequence.Count == 0)
        {
            GameContext.Get<GameManager>().ChangeState(GameState.Complete);
            return;
        }

        _isBusTransitioning = true;

        for (int i = 0; i < _activeBuses.Count; i++)
        {
            _activeBuses[i].MoveToPoint(GetQueuePosition(i), 0.5f);
        }

        if (_busSequence.Count > 0)
        {
            SpawnBusInQueue(_activeBuses.Count);
        }

        DOVirtual.DelayedCall(0.5f, () =>
        {
            _isBusTransitioning = false;
            OnBusChanged?.Invoke();
            CheckQueueForWaitingPassengers();
        });
    }

    public bool HasAvailableBusFor(GameColors color, bool isCheckingFromGrid = false)
    {
        if (_isBusTransitioning || CurrentBus == null || CurrentBus.Color != color)
            return false;

        int inQueueCount = isCheckingFromGrid
            ? GameContext.Get<QueueManager>().GetColorCountInQueue(color)
            : 0;

        return (CurrentBus.PassengerCount + _reservedSeats + inQueueCount) < CurrentBus.Capacity;
    }

    public void ReserveSeat() => _reservedSeats++;

    public void BoardPassenger(Passenger passenger)
    {
        _reservedSeats--;
        CurrentBus.AddPassenger(passenger, CheckBusState);
    }

    public void CheckQueueForWaitingPassengers()
    {
        GameContext.Get<QueueManager>()?.TrySendToBus();
    }
}