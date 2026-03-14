using UnityEngine;
using DG.Tweening;

public class MenuBus : MonoBehaviour
{
    private void Start()
    {
        StartCartoonJump();
    }

    private void StartCartoonJump()
    {
        transform.DOPunchScale(new Vector3(0.02f, -0.02f, 0.02f), 0.5f, 0, 0.1f)
            .SetLoops(-1, LoopType.Restart);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}