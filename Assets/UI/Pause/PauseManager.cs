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
        EditorApplication.isPaused = false;
        //Time.timeScale = 1;
        pauseScreen.SetActive(false);
        basicinterface.SetActive(true);
        Cursor.visible = false;
        isPaused = false;
    }

    public void PauseGame()
    {
        //Debug.Log("Pausado");
        EditorApplication.isPaused = true;
        //Time.timeScale = 0;
        pauseScreen.SetActive(true);
        basicinterface.SetActive(false);
        Cursor.visible = true;
        isPaused = true;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    private void Update()
    {
        Debug.Log(isPaused);

        if (Input.GetKeyDown(KeyCode.Escape) && isPaused == false)
        {
            PauseGame();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isPaused == true)
        {
            ResumeGame();
        }
    }
}
