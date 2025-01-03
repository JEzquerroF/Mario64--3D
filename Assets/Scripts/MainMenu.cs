using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject m_PlayMenu;
    [SerializeField] private GameObject m_ExitMenu;
    [SerializeField] private GameObject m_MainMenu;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PlayButton()
    {
        m_ExitMenu.SetActive(false);
        m_PlayMenu.SetActive(true);
    }

    public void ExitButton()
    {
        m_ExitMenu.SetActive(true);
        m_PlayMenu.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        m_PlayMenu.SetActive(false);
        m_ExitMenu.SetActive(false);
        m_MainMenu.SetActive(true);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
				Application.Quit();
        #endif
    }

    public void LoadScene(string scene)
    {
        if (scene != "")
        {
            SceneManager.LoadSceneAsync(scene);
        }
    }
}
