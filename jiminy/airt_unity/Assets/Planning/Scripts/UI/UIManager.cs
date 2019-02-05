using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
/// <summary>
/// This scripts scales and position the upper part of the UI (Home button, and the state where the application is) and the settings and about button
/// </summary>
public class UIManager : MonoBehaviour {

    public RectTransform topInfoPanel;
    public RectTransform readyImageRect, calibrationImageRect, mappingImageRect, planningImageRect, recordingImageRect;
    public GameObject middleBar;
    public RectTransform homeButton;

    public RectTransform bottomPanel;
    public RectTransform aboutButton, settingsButton;

    public RectTransform bottombar;
    public GameObject pclPlayer;

    public GameObject loadPanel;

    ClientUnity clientUnity;
    Image readyImage;

    void Awake()
    {
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();

        int buttonSize = Screen.height / 15;
        int buttonOffset = buttonSize / 4;

        if (topInfoPanel) { 
            // Posicion y tamaño del info panel
            topInfoPanel.anchoredPosition = new Vector2(0, -Screen.height / 20);
            topInfoPanel.sizeDelta = new Vector2(Screen.width / 2.0f, Screen.height / 20.0f);
            //topInfoPanel.sizeDelta = new Vector2(Screen.width - (2 * buttonSize) - (4 * buttonOffset), Screen.height / 8);

            //Posicion y tamaño de las imagenes del info panel
        
            float infoImageSize = topInfoPanel.sizeDelta.x/5;

            readyImageRect.anchoredPosition =           new Vector2(0 * infoImageSize, 0);
            calibrationImageRect.anchoredPosition =     new Vector2(1 * infoImageSize, 0);
            mappingImageRect.anchoredPosition =         new Vector2(2 * infoImageSize, 0);
            planningImageRect.anchoredPosition =        new Vector2(3 * infoImageSize, 0);
            recordingImageRect.anchoredPosition =       new Vector2(4 * infoImageSize, 0);

            readyImageRect.sizeDelta =          new Vector2(infoImageSize, 0);
            calibrationImageRect.sizeDelta =    new Vector2(infoImageSize, 0);
            mappingImageRect.sizeDelta =        new Vector2(infoImageSize, 0);
            planningImageRect.sizeDelta =       new Vector2(infoImageSize, 0);
            recordingImageRect.sizeDelta =      new Vector2(infoImageSize, 0);

            readyImage = readyImageRect.GetChild(0).GetComponent<Image>();

            GameObject prefabCopy = Instantiate(middleBar);

            prefabCopy.GetComponent<RectTransform>().sizeDelta = new Vector2(50, topInfoPanel.sizeDelta.y * 0.9f);
            float newBarWidth = prefabCopy.GetComponent<RectTransform>().sizeDelta.y * 9 / 92;
            prefabCopy.GetComponent<RectTransform>().anchoredPosition = new Vector2(infoImageSize * 1 - newBarWidth / 2, 0);
            Instantiate(prefabCopy, topInfoPanel.transform);
            prefabCopy.GetComponent<RectTransform>().anchoredPosition = new Vector2(infoImageSize * 2 - newBarWidth / 2, 0);
            Instantiate(prefabCopy, topInfoPanel.transform);
            prefabCopy.GetComponent<RectTransform>().anchoredPosition = new Vector2(infoImageSize * 3 - newBarWidth / 2, 0);
            Instantiate(prefabCopy, topInfoPanel.transform);
            prefabCopy.GetComponent<RectTransform>().anchoredPosition = new Vector2(infoImageSize * 4 - newBarWidth / 2, 0);
            Instantiate(prefabCopy, topInfoPanel.transform);

            Destroy(prefabCopy);

            // Posicion y tamaño del HomeButton
            //homeButton.anchoredPosition = new Vector2(-buttonSize - buttonOffset, 0);
            if (homeButton) { 
                homeButton.anchoredPosition = new Vector2(buttonSize + buttonOffset, topInfoPanel.anchoredPosition.y);
                homeButton.sizeDelta = new Vector2(buttonSize, buttonSize);
            }

        }

        if (bottombar) {
            bottombar.anchoredPosition = new Vector2(0, Screen.height / 8);
            bottombar.sizeDelta = new Vector2(-Screen.width / 15, Screen.height / 175);
        }

        if (bottomPanel)
        {
            float bottomButtonSize = Screen.height / 20.0f;
            float bottomButtonOffset = bottomButtonSize / 4;

            bottomPanel.anchoredPosition = new Vector2(0, Screen.height / 25.0f);
            bottomPanel.sizeDelta = new Vector2(Screen.width / 2.0f, bottomButtonSize);

            settingsButton.anchoredPosition = new Vector2(2 * bottomButtonSize + 3 * bottomButtonOffset, 0);
            settingsButton.sizeDelta = new Vector2(bottomButtonSize, bottomButtonSize);

            if (aboutButton)
            {
                aboutButton.anchoredPosition = new Vector2(1 * bottomButtonSize + 1 * bottomButtonOffset, 0);
                aboutButton.sizeDelta = new Vector2(bottomButtonSize, bottomButtonSize);
            }

        }
        if (loadPanel && SceneManager.GetActiveScene().name != "General")
        {
            loadPanel.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, Screen.height / 7);
            loadPanel.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 1.5f, Screen.height / 3);

            loadPanel.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -Screen.height / 7);
            loadPanel.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.height / 8, Screen.height / 8);

            loadPanel.transform.GetChild(0).GetComponent<Text>().fontSize = (int)(Screen.width * 0.05f);
        }

    }
    /// <summary>
    /// This colors the tick in the upper part of the app, to know if the app is connected to the drone
    /// </summary>
    void Update()
    {
        if (topInfoPanel != null && clientUnity != null && clientUnity.client != null && clientUnity.client.isConnected)
            readyImage.color = new Vector4(1, 1, 1, 1.0f);
        else if(topInfoPanel)
            readyImage.color = new Vector4(1, 1, 1, 0.4f);
    }
    /// <summary>
    /// Home button leads to this
    /// </summary>
    public void goBackHome()
    {
        
        SceneManager.LoadScene("General");
        
    }
    /// <summary>
    /// Loads a scene asynced
    /// </summary>
    /// <param name="sceneName"></param>
    public void LoadCustomScene(string sceneName)
    {
        loadPanel.SetActive(true);
        loadPanel.transform.GetChild(0).GetComponent<Text>().text = "Loading...";
        StartCoroutine(LoadingScene(sceneName));
        
    }

    IEnumerator LoadingScene(string sceneName)
    {
        //UnityEngine.Debug.Log("Loading scene: " + sceneName);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        yield return asyncLoad;
    }

}
