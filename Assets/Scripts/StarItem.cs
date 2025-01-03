using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarItem : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] private AudioClip m_StarSound;

    public static Action OnStarPicked;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnStarPicked?.Invoke();
            SoundsManager.instance.PlaySoundClip(m_StarSound, transform, 0.2f);
            Destroy(gameObject);
        }
    }
}
