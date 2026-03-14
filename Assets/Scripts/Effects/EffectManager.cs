using UnityEngine;

public class EffectManager : MonoBehaviour
{
    private void Awake() => GameContext.Register(this);
    private void OnDestroy() => GameContext.Unregister<EffectManager>();

    public AutoReturnParticle PlayEffect(EffectType type, Vector3 position, Transform parent = null)
    {
        var pool = GameContext.Get<PoolManager>();
        GameObject effectObj = pool.Get(type.ToString());

        if (effectObj == null) return null;

        effectObj.transform.SetParent(parent);
        effectObj.transform.position = position;

        var arp = effectObj.GetComponent<AutoReturnParticle>();
        return arp;
    }
}
