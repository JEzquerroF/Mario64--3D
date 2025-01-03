using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinItem : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] private AudioClip m_CoinSound;

    public static Action OnCoinPicked;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnCoinPicked?.Invoke();
            SoundsManager.instance.PlaySoundClip(m_CoinSound, transform, 0.5f);
            Destroy(gameObject);
        }
    }
}
