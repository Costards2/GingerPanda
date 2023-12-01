using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    private bool isPaused = false;

    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject basicinterface;

    public void ResumeGame()
    {
        //Debug.Log("Resumado");
        //EditorApplication.isPaused = false;

        pauseScreen.SetActive(false);
        basicinterface.SetActive(true);
        Cursor.visible = false;
        isPaused = false;
        Time.timeScale = 1;
    }

    public void PauseGame()
    {
        //Debug.Log("Pausado");
        //EditorApplication.isPaused = true;
        pauseScreen.SetActive(true);
        basicinterface.SetActive(false);
        Cursor.visible = true;
        isPaused = true;
        Time.timeScale = 0;
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }

    private void Update()
    {
        Debug.Log(isPaused);

        if (Input.GetKeyDown(KeyCode.Escape) && isPaused == false)
        {

            PauseGame();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isPaused)
        {
            ResumeGame();
        }
    }
}
