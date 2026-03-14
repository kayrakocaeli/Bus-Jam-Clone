using System.Collections.Generic;
using UnityEngine;

public class MainMenuEnvironment : MonoBehaviour
{
    [Header("Generation Settings")]
    public RoadChunk roadPrefab;
    public int chunkCount = 15;
    public float scrollSpeed = 5f;

    private List<RoadChunk> _activeChunks = new();
    private Transform _cameraTransform;

    // --- Editor Preview Methods ---

    [ContextMenu("Preview Road")]
    private void PreviewRoad()
    {
        ClearRoad();

        RoadChunk lastChunk = Instantiate(roadPrefab, transform);
        lastChunk.transform.position = Vector3.zero;
        _activeChunks.Add(lastChunk);

        for (int i = 1; i < chunkCount; i++)
        {
            RoadChunk lastActive = _activeChunks[_activeChunks.Count - 1];
            RoadChunk newChunk = Instantiate(roadPrefab, transform);
            AlignChunks(lastActive.exitPoint, newChunk);
            _activeChunks.Add(newChunk);
        }
    }

    [ContextMenu("Clear Road")]
    private void ClearRoad()
    {
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in transform)
        {
            toDestroy.Add(child.gameObject);
        }

        foreach (var obj in toDestroy)
        {
            DestroyImmediate(obj);
        }

        _activeChunks.Clear();
    }

    // --- Logic ---

    private void Start()
    {
        if (transform.childCount > 0)
        {
            _activeChunks.Clear();
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent<RoadChunk>(out var rc))
                    _activeChunks.Add(rc);
            }
        }
        else
        {
            GenerateInitialRoad();
        }

        if (Camera.main != null) _cameraTransform = Camera.main.transform;
    }

    private void GenerateInitialRoad()
    {
        _activeChunks.Clear();
        RoadChunk firstChunk = Instantiate(roadPrefab, transform);
        firstChunk.transform.position = Vector3.zero;
        _activeChunks.Add(firstChunk);

        for (int i = 1; i < chunkCount; i++)
        {
            SpawnNextChunk();
        }
    }

    private void SpawnNextChunk()
    {
        RoadChunk lastActive = _activeChunks[_activeChunks.Count - 1];
        RoadChunk newChunk = Instantiate(roadPrefab, transform);
        AlignChunks(lastActive.exitPoint, newChunk);
        _activeChunks.Add(newChunk);
    }

    private void AlignChunks(Transform targetAnchor, RoadChunk chunkToMove)
    {
        Vector3 offset = chunkToMove.transform.position - chunkToMove.entryPoint.position;
        chunkToMove.transform.position = targetAnchor.position + offset;
    }

    private void Update()
    {
        if (!Application.isPlaying) return;

        foreach (var chunk in _activeChunks)
        {
            chunk.transform.Translate(Vector3.back * scrollSpeed * Time.deltaTime, Space.World);
        }

        if (_cameraTransform != null && _activeChunks.Count > 0)
        {
            if (_activeChunks[0].transform.position.z < _cameraTransform.position.z - 15f)
            {
                RecycleFirstChunk();
            }
        }
    }

    private void RecycleFirstChunk()
    {
        RoadChunk chunkToMove = _activeChunks[0];
        _activeChunks.RemoveAt(0);

        RoadChunk currentLast = _activeChunks[_activeChunks.Count - 1];
        AlignChunks(currentLast.exitPoint, chunkToMove);
        _activeChunks.Add(chunkToMove);

        if (chunkToMove.TryGetComponent<RoadChunk>(out var rc))
        {
            rc.GenerateHouses();
        }
    }
}