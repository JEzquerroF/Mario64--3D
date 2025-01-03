using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface IRestartGame
{
    void RestartGame();
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private GameObject m_MarioObject;
    [SerializeField] private CharacterController m_CharacterController;
    [SerializeField] private MarioController m_MarioController;
    [SerializeField] private MarioLifeController m_MarioLifeController;

    public List<IRestartGame> m_RestartGame = new List<IRestartGame>();

    public bool m_Restart;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        MarioLifeController.OnPlayerDied += PlayerDied;
        UIController.OnMarioRevive += PlayerRevive;
    }

    private void OnDisable()
    {
        MarioLifeController.OnPlayerDied -= PlayerDied;
        UIController.OnMarioRevive -= PlayerRevive;
    }

    public void SetPlayer(MarioController l_Player)
    {
        m_MarioController = l_Player;
    }

    public MarioController GetPlayer()
    {
        return m_MarioController;
    }

    public void AddRestartGame(IRestartGame l_Restart)
    {
        m_RestartGame.Add(l_Restart);
    }

    public void EndGame()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void RestartPosition()
    {
        m_Restart = true;

        foreach (IRestartGame l_Controller in m_RestartGame)
        {
            l_Controller.RestartGame();
        }

        Cursor.lockState = CursorLockMode.Locked;
        StartCoroutine(PlayerActive());
    }

    private IEnumerator PlayerActive()
    {
        m_CharacterController.enabled = true;
        m_MarioController.enabled = true;
        m_Restart = false;
        m_MarioObject.SetActive(true);
        m_MarioLifeController.m_IsMarioDead = false;
        yield return null;
    }

    private IEnumerator PlayerDiedCoroutine()
    {
        yield return new WaitForSeconds(1.8f);
        m_MarioObject.SetActive(false);
    }

    private void PlayerDied()
    {
        StartCoroutine(PlayerDiedCoroutine());
    }

    private void PlayerRevive()
    {
        if (m_MarioLifeController.m_MarioLifes > 0)
            RestartPosition();
        else
            EndGame();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

