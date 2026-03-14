using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bus : MonoBehaviour
{
    [Header("Visuals & Animations")]
    [SerializeField] private List<Renderer> _busRenderers;
    [SerializeField] private Animator _doorAnimator;
    [SerializeField] private Transform _doorTransform;
    [SerializeField] private List<GameObject> _seatedPassengers;
    [SerializeField] private Transform _exhaustPoint;
    private AutoReturnParticle _exhaustEffect;

    public GameColors Color;
    public int Capacity { get; private set; }
    public int PassengerCount { get; private set; }

    private int _activeBoardingCount = 0;
    private static readonly int DoorOpenHash = Animator.StringToHash("doorOpen");

    private Renderer[] _seatedRenderers;

    private void Awake()
    {
        Capacity = _seatedPassengers.Count;
        _seatedRenderers = new Renderer[Capacity];

        for (int i = 0; i < Capacity; i++)
        {
            if (_seatedPassengers[i] != null)
                _seatedRenderers[i] = _seatedPassengers[i].GetComponentInChildren<Renderer>();
        }
    }

    public void Setup(GameColors color, Material mat)
    {
        Color = color;
        PassengerCount = 0;
        _activeBoardingCount = 0;

        if (mat != null && _busRenderers != null)
        {
            foreach (var ren in _busRenderers)
            {
                if (ren == null) continue;

                Material[] sharedMats = ren.sharedMaterials;
                for (int j = 0; j < sharedMats.Length; j++)
                {
                    sharedMats[j] = mat;
                }
                ren.sharedMaterials = sharedMats;
            }
        }

        for (int i = 0; i < Capacity; i++)
        {
            _seatedPassengers[i].SetActive(false);
            if (_seatedRenderers[i] != null)
                _seatedRenderers[i].sharedMaterial = mat;
        }

        if (_doorAnimator != null)
            _doorAnimator.SetBool(DoorOpenHash, false);

        StartBouncyIdle();
        _exhaustEffect = GameContext.Get<EffectManager>().PlayEffect(EffectType.ExhaustSmoke, _exhaustPoint.position, _exhaustPoint);
    }

    public bool IsFull() => PassengerCount >= Capacity;

    public void AddPassenger(Passenger passenger, Action onBoarded)
    {
        int assignedSeat = PassengerCount;
        PassengerCount++;
        _activeBoardingCount++;

        if (_doorAnimator != null)
            _doorAnimator.SetBool(DoorOpenHash, true);

        Vector3 doorPos = _doorTransform.position;
        doorPos.y = 0f;

        passenger.SetRunning(true);

        passenger.transform.DOMove(doorPos, 0.3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                passenger.SetRunning(false);
                passenger.gameObject.SetActive(false);

                if (assignedSeat < Capacity)
                {
                    Transform seatTransform = _seatedPassengers[assignedSeat].transform;
                    _seatedPassengers[assignedSeat].SetActive(true);
                    GameContext.Get<EffectManager>()?.PlayEffect(
                                            EffectType.PassengerPop,
                                            seatTransform.position,
                                            seatTransform
                                        );

                    Renderer passengerRenderer = _seatedRenderers[assignedSeat];
                    if (passengerRenderer != null && _busRenderers.Count > 0)
                    {
                        passengerRenderer.sharedMaterial = _busRenderers[0].sharedMaterial;
                    }
                }

                _activeBoardingCount--;

                if (_activeBoardingCount <= 0)
                {
                    _activeBoardingCount = 0;
                    if (_doorAnimator != null)
                        _doorAnimator.SetBool(DoorOpenHash, false);
                }

                onBoarded?.Invoke();
            });
    }

    public void DriveAway(Action onDrivenAway)
    {
        if (_doorAnimator != null)
            _doorAnimator.SetBool(DoorOpenHash, false);

        transform.DOMoveX(transform.position.x + 15f, 0.8f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                onDrivenAway?.Invoke();
            });
    }

    public void MoveToPoint(Vector3 position, float duration = 0.5f, Action onComplete = null)
    {
        transform.DOMove(position, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => onComplete?.Invoke());
    }

    private static readonly Vector3 TargetScale = new Vector3(1.02f, 0.98f, 1.02f);

    private void StartBouncyIdle()
    {
        transform.DOScale(TargetScale, 0.6f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }
}