using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SpikeTrapController : MonoBehaviour, IRestartGame
{
    private bool m_AlreadyDead;

    private void Start()
    {
        GameManager.instance.AddRestartGame(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && m_AlreadyDead == false)
        {
            StartCoroutine(other.GetComponent<MarioLifeController>().PlayerHit(8.0f));
            m_AlreadyDead = true;
        }
    }

    public void RestartGame()
    {
        m_AlreadyDead = false;
    }
}
