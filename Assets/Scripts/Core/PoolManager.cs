using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    [Serializable]
    public class PoolSetup
    {
        public string poolId;
        public GameObject prefab;
        public int defaultCapacity = 10;
        public int maxSize = 50;
    }

    [SerializeField] private List<PoolSetup> poolSetups;
    private Dictionary<string, IObjectPool<GameObject>> _pools = new();

    private void Awake()
    {
        GameContext.Register(this);
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var setup in poolSetups)
        {
            var pool = new ObjectPool<GameObject>(
                createFunc: () => CreatePooledItem(setup),
                actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj => obj.SetActive(false),
                actionOnDestroy: Destroy,
                collectionCheck: false,
                defaultCapacity: setup.defaultCapacity,
                maxSize: setup.maxSize
            );

            _pools.Add(setup.poolId, pool);

            List<GameObject> temp = new();
            for (int i = 0; i < setup.defaultCapacity; i++) temp.Add(pool.Get());
            foreach (var obj in temp) pool.Release(obj);
        }
    }

    private GameObject CreatePooledItem(PoolSetup setup)
    {
        var obj = Instantiate(setup.prefab, transform);
        var member = obj.AddComponent<PoolMember>();
        member.PoolId = setup.poolId;
        return obj;
    }

    public T Get<T>(string poolId) where T : Component
    {
        var obj = Get(poolId);
        return obj ? obj.GetComponent<T>() : null;
    }

    public GameObject Get(string poolId)
    {
        if (_pools.TryGetValue(poolId, out var pool)) return pool.Get();

        Debug.LogError($"Pool {poolId} missing!");
        return null;
    }

    public void Release(GameObject obj)
    {
        if (obj.TryGetComponent<PoolMember>(out var member))
        {
            if (_pools.TryGetValue(member.PoolId, out var pool))
            {
                pool.Release(obj);
            }
        }
    }
}

public class PoolMember : MonoBehaviour { public string PoolId; }