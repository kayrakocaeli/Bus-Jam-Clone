using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Passenger : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _reservationHat;
    [SerializeField] private Renderer _meshRenderer;

    [Header("Feedback UI")]
    [SerializeField] private GameObject angryEmojiObject;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 8f;

    public GameColors Color { get; private set; }
    public Vector2Int CurrentCoords;
    public bool IsReserved;
    public bool IsSecret;
    public bool IsInteractable { get; set; } = true;

    private bool _isMoving;
    private MaterialPropertyBlock _propBlock;

    private static readonly int RunningHash = Animator.StringToHash("Running");
    private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");

    private void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
    }

    public void Setup(GameColors color, Vector2Int startCoords, Material mat, bool isReserved = false, bool isSecret = false)
    {
        Color = color;
        CurrentCoords = startCoords;
        IsReserved = isReserved;
        IsSecret = isSecret;

        _isMoving = false;
        IsInteractable = true;

        if (_reservationHat != null)
            _reservationHat.SetActive(IsReserved);

        if (_meshRenderer != null && mat != null && !IsSecret)
            _meshRenderer.sharedMaterial = mat;

        transform.rotation = Quaternion.identity;
    }

    public void MoveAlongPath(List<Tile> path, Action onComplete = null)
    {
        if (path == null || path.Count == 0 || _isMoving) return;

        _isMoving = true;
        SetRunning(true);

        int count = path.Count;
        Vector3[] waypoints = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = path[i].transform.position;
            waypoints[i] = new Vector3(pos.x, 0f, pos.z);
        }

        float duration = count / _moveSpeed;

        transform.DOPath(waypoints, duration, PathType.Linear)
            .SetLookAt(0.01f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                CurrentCoords = path[count - 1].Coordinates;
                _isMoving = false;
                SetRunning(false);

                transform.DORotate(Vector3.zero, 0.1f);
                onComplete?.Invoke();
            });
    }

    public void SetRunning(bool isRunning)
    {
        if (_animator != null)
            _animator.SetBool(RunningHash, isRunning);
    }
    public void ReturnToOriginalTile(List<Tile> path, Action onReturned)
    {
        List<Tile> returnPath = new List<Tile>(path);
        returnPath.Reverse();

        MoveAlongPath(returnPath, () => {
            IsInteractable = true;
            onReturned?.Invoke();
        });
    }

    public void SetOutline(bool state)
    {
        if (_meshRenderer == null) return;

        _meshRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(OutlineWidthId, state ? 0.02f : 0f);
        _meshRenderer.SetPropertyBlock(_propBlock);
    }

    public void ShowAngryFeedback()
    {
        if (angryEmojiObject == null) return;

        SpriteRenderer emojiRenderer = angryEmojiObject.GetComponent<SpriteRenderer>();
        if (emojiRenderer == null) return;

        angryEmojiObject.transform.DOKill();
        emojiRenderer.DOKill();

        angryEmojiObject.SetActive(true);

        Color c = emojiRenderer.color;
        c.a = 1f;
        emojiRenderer.color = c;

        angryEmojiObject.transform.localPosition = new Vector3(0, 2.5f, 0);

        Sequence seq = DOTween.Sequence();

        seq.Append(angryEmojiObject.transform.DOLocalMoveY(3.5f, 0.8f).SetEase(Ease.OutQuad));
        seq.Join(emojiRenderer.DOFade(0f, 0.8f));

        seq.OnComplete(() => angryEmojiObject.SetActive(false));

        seq.SetLink(gameObject);
    }
}