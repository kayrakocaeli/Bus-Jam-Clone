using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public event Action<Passenger> OnPassengerTapped;

    [SerializeField] private LayerMask passengerLayer;
    private Camera _mainCamera;
    private GameManager _gameManager;

    private void Awake()
    {
        GameContext.Register(this);
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        _gameManager = GameContext.Get<GameManager>();
    }

    private void OnDestroy()
    {
        GameContext.Unregister<InputManager>();
    }

    private void Update()
    {
        // Mobile Touch Support
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // Prevent clicking through UI elements
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    return;

                HandleInput(touch.position);
            }
        }

        // Keep Mouse support for Unity Editor testing
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            HandleInput(Input.mousePosition);
        }
#endif
    }

    private void HandleInput(Vector2 screenPosition)
    {
        if (_gameManager == null) return;

        GameState state = _gameManager.GetCurrentState();

        if (state == GameState.Start)
        {
            _gameManager.ReceiveFirstInput();
        }
        else if (state == GameState.Gameplay)
        {
            HandleRaycast(screenPosition);
        }
    }

    private void HandleRaycast(Vector2 screenPosition)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, passengerLayer))
        {
            if (hit.collider.TryGetComponent(out Passenger passenger))
            {
                OnPassengerTapped?.Invoke(passenger);
            }
        }
    }
}