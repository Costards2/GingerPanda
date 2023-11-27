using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AsyncLoader : MonoBehaviour
{

    [Header("Menu Screens")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenu;

    [Header("Slider")]
    [SerializeField] private Slider loadingSlider;

    public float duration = 1.5f;

    public void LoadLevelBtn(string levelToLoad)
    {
        mainMenu.SetActive(false);
        loadingScreen.SetActive(true);

        StartCoroutine(LoadLevelAsync(levelToLoad));
    }

    IEnumerator LoadLevelAsync(string levelToload)
    {
        //Codigos de forçat careegamento de barra 

        //loadingSlider.value = 0.30f;
        //yield return new WaitForSeconds(0.50f);

        //loadingSlider.value = 0.65f;
        //yield return new WaitForSeconds(0.50f);

        //loadingSlider.value = 0.88f;
        //yield return new WaitForSeconds(0.50f);

        //loadingSlider.value = 1f;
        //yield return new WaitForSeconds(0.50f);

        //float elapsedTime = 0f;
        //float startValue = 0f;
        //float endValue = 1f;

        //while (elapsedTime < duration)
        //{
        //    loadingSlider.value = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
        //    elapsedTime += Time.deltaTime;
        //    yield return null;
        //}

        //loadingSlider.value = endValue;

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
