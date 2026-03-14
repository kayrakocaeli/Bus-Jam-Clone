using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AutoReturnParticle : MonoBehaviour
{
    private ParticleSystem _ps;
    private PoolManager _pool;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
        _pool = GameContext.Get<PoolManager>();
    }

    private void OnEnable()
    {
        var main = _ps.main;
        if (!main.loop)
        {
            Invoke(nameof(ReturnToPool), main.duration + main.startLifetime.constantMax);
        }
    }

    public void StopAndReturn()
    {
        _ps.Stop();
        CancelInvoke();
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (gameObject.activeSelf) _pool.Release(gameObject);
    }
}