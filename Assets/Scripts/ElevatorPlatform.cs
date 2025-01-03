using System.Collections;
using UnityEngine;

public class ElevatorPlatform : MonoBehaviour
{
    public enum ElevatorType
    {
        Normal,
        Delayed
    }

    public ElevatorType m_ElevatorType;

    [SerializeField] private float m_DelayTime;
    [SerializeField] private Animator m_PlatformAnimator;

    private void Start()
    {
        if (m_ElevatorType == ElevatorType.Delayed)
        {
            StartCoroutine(DelayedPlatformCoroutine());
        }
        else if (m_ElevatorType == ElevatorType.Normal)
        {
            m_PlatformAnimator.SetTrigger("StartAnimation");
        }
    }

    private IEnumerator DelayedPlatformCoroutine()
    {
        yield return new WaitForSeconds(m_DelayTime);
        m_PlatformAnimator.SetTrigger("StartAnimation");
    }

    // 2 MINI ERRORES:
    // SI LO TIENES EN ONDESTROY, AL HACER UN SETPARENT SALE DEL ONDESTROY
}
