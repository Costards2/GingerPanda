using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private string levelName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            LoadLevelBtn(levelName);
        }
    }

    public void LoadLevelBtn(string levelToLoad)
    {
        loadingScreen.SetActive(true);

        StartCoroutine(LoadLevelAsync(levelToLoad));
        Time.timeScale = 1;
    }

    IEnumerator LoadLevelAsync(string levelToload)
    {

        yield return new WaitForSeconds(2f);
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToload);
    }
}
