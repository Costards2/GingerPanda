using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProximoLevel : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private string levelName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            LoadLevelBtn(levelName);
        }
    }

    public void LoadLevelBtn(string levelToLoad)
    {
        loadingScreen.SetActive(true);

        StartCoroutine(LoadLevelAsync(levelToLoad));
    }

    IEnumerator LoadLevelAsync(string levelToload)
    {

        yield return new WaitForSeconds(2f);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToload);

        // This code is the visual part of the slider, and It's working fine, however the game is too fast to load so 
        // I had to do the "WaitForSeconds" to give the sentaion of loading
        //while (!loadOperation.isDone)
        //{
        //    float progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
        //    loadingSlider.value = progressValue;
        //    yield return null;
        //}
    }

}
