using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public class CheckpointController : MonoBehaviour
{
    [SerializeField] private Transform m_SpawnPoint; 

    private Animator m_CheckpointAnimator;
    private bool isChecked = false;

    [Header("Sounds")]
    [SerializeField] private AudioClip m_CheckpointSound;

    private void Start()
    {
        m_CheckpointAnimator = GetComponent<Animator>();
    }

    public Vector3 CheckPointPosition()
    {
        return m_SpawnPoint.transform.position;
    }

    private bool IsChecked()
    {
        return isChecked;
    }

    private void SetChecked(bool value)
    {
        isChecked = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && IsChecked() == false)
        {
            m_CheckpointAnimator.SetTrigger("CheckpointEntered");
            SoundsManager.instance.PlaySoundClip(m_CheckpointSound, transform, 0.2f);
            other.GetComponent<MarioController>().SetStartPosition(CheckPointPosition());
            SetChecked(true);
        }
    }
}
