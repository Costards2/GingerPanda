using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject painelMenu;
    [SerializeField] private GameObject paineBasic;


    public void MainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Jogar()
    {
        SceneManager.LoadScene("Test Scene (Programmer)");
    }

    public void Config()
    {
        painelMenu.SetActive(true);
        paineBasic.SetActive(false);
    }

    public void Voltar()
    {
        painelMenu.SetActive(false);
        paineBasic.SetActive(true);
    }

    public void Sair()
    {
        Debug.Log("Sair");
        Application.Quit();
    }
}
