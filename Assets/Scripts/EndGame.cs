using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    [SerializeField] private Animator m_EndGameAnimator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            StartCoroutine(GoToMainMenu());
    }

    private IEnumerator GoToMainMenu()
    {
        m_EndGameAnimator.SetTrigger("EndGame");
        yield return new WaitForSeconds(5.0f);
        GameManager.instance.EndGame();
    }
}