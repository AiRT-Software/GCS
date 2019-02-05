using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Xml;

public class PlanSelectionManager : MonoBehaviour {

    public Font Rubik;

    // RectTransforms para redimensionar interfaz en función del tamaño de la pantalla
    public RectTransform mapsPanel, plansPanel;
    public RectTransform mapsText, plansText, noPlansText;
    public RectTransform mapLeftArrow, mapRightArrow, planLeftArrow, planRightArrow;
    public RectTransform buttonsMapMask, mapsContainer, buttonsPlanMask, plansContainer;
    public RectTransform mapRender, mapInfoImage, backButtonMap, nextButtonMap;
    public RectTransform planRender, planInfoImage, planDefaultParams, backButtonPlan, backButtonPlan2, continueButtonPlan, createButtonPlan;
    public RectTransform importModelButton, emptySceneButton;
    public RectTransform deletePanel;
    public static List<string> serverMissions = new List<string>();
    public Font rubik;
    public RectTransform panelOverwriter;
    public RectTransform pleaseConnectToTheDronePanel;
    //Only activate when it needs to substitute a metadata in the server in conflict with another local
    static public bool uploadMapMetadata = false;
    // Prefab de botón para instanaciar los necesarios
    public GameObject mapButtonPrefab;

    public GameObject previewScene, modelLoadingPanel;

    public GameObject planSelection, mapInfo, planInfo, planInfo2, newMission, createEmptyScene;
    public InputField planName, planAuthor, planNotes;
    GameObject fileBrowser;
    public static bool askedForMaps = false;
    // Distancia entre botones
    static int buttonOffset;

    // Variables para el desplazamiento de los paneles de mapas y planes
    int mapIndex = 0;
    public static int mapCount = 0;
    int planIndex = 0;
    public static int planCount = 0;

    // iterador para crear los botones
    int it = 0;

    // Flechas para el desplazamiento de los paneles de mapas y planes
    Button mapLeftB, mapRightB, planLeftB, planRightB;

    // Tamaño de la pantalla
    int width;
    int height;

    // Variables para posicionar paneles y botones
    int panelSize;
    int panelPos;
    Vector2 buttonSize;

    ClientUnity clientUnity;

    public Sprite modelThumbnail, cubeThumbnail;

    public enum PanelAnimations
    {
        IDLE = 0,
        NEXT_MAP,
        PREV_MAP,
        NEXT_PATH,
        PREV_PATH,
        FIRST_PATH,
        FIRST_MAP
    }

    float animationsTime = 0.5f, currentTime = 0.0f;
    Vector2 animationOrigin, animationDestination;

    public static PanelAnimations animationsState = PanelAnimations.IDLE;
    public static uint metadatasDownloaded = 0;
    int mapButtonsCreated = 0;
    void Awake()
    {
        width = Screen.width;
        height = Screen.height;

        modelLoadingPanel.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, Screen.height / 7);
        modelLoadingPanel.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 1.5f, Screen.height / 3);

        modelLoadingPanel.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -Screen.height / 7);
        modelLoadingPanel.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.height / 8, Screen.height / 8);

        modelLoadingPanel.transform.GetChild(0).GetComponent<Text>().fontSize = (int)(Screen.width * 0.05f);

        importModelButton.sizeDelta = new Vector2(width / 5, height / 8);
        emptySceneButton.sizeDelta = new Vector2(width / 5, height / 8);
        importModelButton.anchoredPosition = new Vector2(width / 6, -height / 15);
        emptySceneButton.anchoredPosition = new Vector2(-width / 6, -height / 15);
        importModelButton.GetChild(0).GetComponent<Text>().fontSize = height / 30;
        emptySceneButton.GetChild(0).GetComponent<Text>().fontSize = height / 30;

        buttonOffset = width / 4;
        panelSize = height / 4;
        panelPos = height / 6;
        buttonSize = new Vector2(width / 5, height / 4.5f);

        // Posicionamiento y tamaño de los textos
        mapsText.anchoredPosition = new Vector2(-buttonOffset, height * 0.05f);
        mapsText.sizeDelta = new Vector2(buttonSize.x, 20);
        mapsText.gameObject.GetComponent<Text>().fontSize = height / 24;
        plansText.anchoredPosition = new Vector2(-buttonOffset, height * 0.05f);
        plansText.sizeDelta = new Vector2(buttonSize.x, 20);
        plansText.gameObject.GetComponent<Text>().fontSize = height / 24;
        noPlansText.anchoredPosition = new Vector2(-buttonOffset, -height * 0.1f);
        noPlansText.sizeDelta = new Vector2(buttonSize.x, 20);
        noPlansText.gameObject.GetComponent<Text>().fontSize = height / 24;

        // Obtención de los botones a partir del rectTransform
        mapLeftB = mapLeftArrow.gameObject.GetComponent<Button>();
        mapRightB = mapRightArrow.gameObject.GetComponent<Button>();
        planLeftB = planLeftArrow.gameObject.GetComponent<Button>();
        planRightB = planRightArrow.gameObject.GetComponent<Button>();

        // Posicionamiento y redimensión de paneles
        mapsPanel.anchoredPosition = new Vector2(0, panelPos);
        plansPanel.anchoredPosition = new Vector2(0, -panelPos);

        mapsPanel.sizeDelta = new Vector2(0, panelSize);
        plansPanel.sizeDelta = new Vector2(0, panelSize);

        // Posicionamiento y redimensión de las flechas
        mapLeftArrow.anchoredPosition = new Vector2(width / 25, 0);
        mapRightArrow.anchoredPosition = new Vector2(-width / 25, 0);
        planLeftArrow.anchoredPosition = new Vector2(width / 25, 0);
        planRightArrow.anchoredPosition = new Vector2(-width / 25, 0);

        mapLeftArrow.sizeDelta = new Vector2(width / 20, height / 16);
        mapRightArrow.sizeDelta = new Vector2(width / 20, height / 16);
        planLeftArrow.sizeDelta = new Vector2(width / 20, height / 16);
        planRightArrow.sizeDelta = new Vector2(width / 20, height / 16);

        // Redimensión de la máscara que muestra únicamente 3 botones de todos los visibles
        buttonsMapMask.sizeDelta = new Vector2(width / 1.33f, height / 3f); //antes era 4.25f
        buttonsPlanMask.sizeDelta = new Vector2(width / 1.33f, height / 3f);

        mapRender.anchoredPosition = new Vector2(width / 10, -height / 4);
        mapRender.sizeDelta = new Vector2(width / 2.5f, height / 2);
        planRender.anchoredPosition = new Vector2(width / 10, -height / 4);
        planRender.sizeDelta = new Vector2(width / 2.5f, height / 2);

        mapInfoImage.anchoredPosition = new Vector2(11 * width / 20, -height / 4);
        mapInfoImage.sizeDelta = new Vector2(width / 2.4f, height / 2);
        planInfoImage.anchoredPosition = new Vector2(11 * width / 20, -height / 6);
        planInfoImage.sizeDelta = new Vector2(width / 2.4f, height - (height / 3));
        planDefaultParams.anchoredPosition = new Vector2(width / 10, -height / 6);
        planDefaultParams.sizeDelta = new Vector2(width * 0.8f, height / 1.5f);

        backButtonPlan.anchoredPosition = new Vector2(width / 10, -3.1f * height / 4);
        backButtonPlan.sizeDelta = new Vector2(width * 0.075f, height * 0.05f);
        backButtonPlan.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(height / 90, 0);
        backButtonPlan.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(width * 0.075f, height * 0.03f);
        continueButtonPlan.sizeDelta = new Vector2(height * 0.15f, height * 0.05f);
        continueButtonPlan.anchoredPosition = new Vector2(width / 10 + width / 2.5f - continueButtonPlan.sizeDelta.x, -3.1f * height / 4);
        continueButtonPlan.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(-height / 45, 0);
        continueButtonPlan.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(width * 0.075f, height * 0.03f);

        backButtonPlan2.anchoredPosition = new Vector2(width / 8, -3f * height / 4);
        backButtonPlan2.sizeDelta = new Vector2(width * 0.075f, height * 0.05f);
        backButtonPlan2.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(height / 90, 0);
        backButtonPlan2.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(width * 0.075f, height * 0.03f);
        createButtonPlan.sizeDelta = new Vector2(height * 0.12f, height * 0.05f);
        createButtonPlan.anchoredPosition = new Vector2(width / 10 + width / 1.3f - createButtonPlan.sizeDelta.x, -3f * height / 4);
        createButtonPlan.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(-height / 45, 0);
        createButtonPlan.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(width * 0.075f, height * 0.03f);

        backButtonMap.anchoredPosition = new Vector2(width / 10, -3.1f * height / 4);
        backButtonMap.sizeDelta = new Vector2(width * 0.075f, height * 0.05f);
        backButtonMap.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(height / 60, 0);
        backButtonMap.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(width * 0.075f, height * 0.03f);
        nextButtonMap.sizeDelta = new Vector2(width * 0.075f, height * 0.05f);
        nextButtonMap.anchoredPosition = new Vector2(11 * width / 20 + width / 2.4f - nextButtonMap.sizeDelta.x, -3.1f * height / 4);
        nextButtonMap.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(-height / 60, 0);
        nextButtonMap.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(width * 0.075f, height * 0.03f);

        backButtonPlan.GetChild(0).GetComponent<Text>().fontSize = height / 30;
        continueButtonPlan.GetChild(0).GetComponent<Text>().fontSize = height / 30;
        backButtonPlan2.GetChild(0).GetComponent<Text>().fontSize = height / 30;
        createButtonPlan.GetChild(0).GetComponent<Text>().fontSize = height / 30;
        backButtonMap.GetChild(0).GetComponent<Text>().fontSize = height / 30;
        nextButtonMap.GetChild(0).GetComponent<Text>().fontSize = height / 30;

        float planInfoX = planInfoImage.sizeDelta.x;
        float planInfoY = planInfoImage.sizeDelta.y;
        float marginX = 0.05f;
        float marginY = 0.1f;
        float fontSize = 0.04f;
        // Plan Info title
        planInfoImage.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(planInfoX * marginX, -planInfoY * (marginY * 0.5f));
        planInfoImage.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(planInfoX * 0.8f, planInfoY * 0.08f);
        planInfoImage.GetChild(0).GetComponent<Text>().fontSize = (int)(planInfoY * 0.05f);
        // Plan Name Label
        planInfoImage.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(planInfoX * marginX, -planInfoY * (marginY * 1.5f));
        planInfoImage.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(planInfoX * 0.4f, planInfoY * 0.08f);
        planInfoImage.GetChild(1).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Plan Author Label
        planInfoImage.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(planInfoX * marginX, -planInfoY * (marginY * 2.5f));
        planInfoImage.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(planInfoX * 0.4f, planInfoY * 0.08f);
        planInfoImage.GetChild(2).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Plan Modification Date Label
        planInfoImage.GetChild(3).GetComponent<RectTransform>().anchoredPosition = new Vector2(planInfoX * marginX, -planInfoY * (marginY * 3.5f));
        planInfoImage.GetChild(3).GetComponent<RectTransform>().sizeDelta = new Vector2(planInfoX * 0.4f, planInfoY * 0.08f);
        planInfoImage.GetChild(3).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Plan Number of Points Label
        planInfoImage.GetChild(4).GetComponent<RectTransform>().anchoredPosition = new Vector2(planInfoX * marginX, -planInfoY * (marginY * 4.5f));
        planInfoImage.GetChild(4).GetComponent<RectTransform>().sizeDelta = new Vector2(planInfoX * 0.4f, planInfoY * 0.08f);
        planInfoImage.GetChild(4).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Plan Version Label
        planInfoImage.GetChild(5).GetComponent<RectTransform>().anchoredPosition = new Vector2(planInfoX * marginX, -planInfoY * (marginY * 5.5f));
        planInfoImage.GetChild(5).GetComponent<RectTransform>().sizeDelta = new Vector2(planInfoX * 0.4f, planInfoY * 0.08f);
        planInfoImage.GetChild(5).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Plan Name Input Field
        planInfoImage.GetChild(6).GetComponent<RectTransform>().anchoredPosition = new Vector2((planInfoX / 2) + planInfoX * marginX, -planInfoY * (marginY * 1.5f));
        planInfoImage.GetChild(6).GetComponent<RectTransform>().sizeDelta = new Vector2(planInfoX * 0.4f, planInfoY * 0.08f);
        planInfoImage.GetChild(6).GetChild(0).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        planInfoImage.GetChild(6).GetChild(1).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Plan Author Input Field
        planInfoImage.GetChild(7).GetComponent<RectTransform>().anchoredPosition = new Vector2((planInfoX / 2) + planInfoX * marginX, -planInfoY * (marginY * 2.5f));
        planInfoImage.GetChild(7).GetComponent<RectTransform>().sizeDelta = new Vector2(planInfoX * 0.4f, planInfoY * 0.08f);
        planInfoImage.GetChild(7).GetChild(0).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        planInfoImage.GetChild(7).GetChild(1).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Plan ModificationDate Text
        planInfoImage.GetChild(8).GetComponent<RectTransform>().anchoredPosition = new Vector2((planInfoX / 2) + planInfoX * marginX, -planInfoY * (marginY * 3.5f));
        planInfoImage.GetChild(8).GetComponent<RectTransform>().sizeDelta = new Vector2(planInfoX * 0.4f, planInfoY * 0.08f);
        planInfoImage.GetChild(8).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Plan Number of Points Text
        planInfoImage.GetChild(9).GetComponent<RectTransform>().anchoredPosition = new Vector2((planInfoX / 2) + planInfoX * marginX, -planInfoY * (marginY * 4.5f));
        planInfoImage.GetChild(9).GetComponent<RectTransform>().sizeDelta = new Vector2(planInfoX * 0.4f, planInfoY * 0.08f);
        planInfoImage.GetChild(9).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Plan Version Text
        planInfoImage.GetChild(10).GetComponent<RectTransform>().anchoredPosition = new Vector2((planInfoX / 2) + planInfoX * marginX, -planInfoY * (marginY * 5.5f));
        planInfoImage.GetChild(10).GetComponent<RectTransform>().sizeDelta = new Vector2(planInfoX * 0.4f, planInfoY * 0.08f);
        planInfoImage.GetChild(10).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Plan Notes
        planInfoImage.GetChild(11).GetComponent<RectTransform>().anchoredPosition = new Vector2(planInfoX * marginX, -planInfoY * (marginY * 6.5f));
        planInfoImage.GetChild(11).GetComponent<RectTransform>().sizeDelta = new Vector2(planInfoX * 0.9f, planInfoY * 0.28f);
        planInfoImage.GetChild(11).GetChild(0).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        planInfoImage.GetChild(11).GetChild(1).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);

        float planDefParamsX = planDefaultParams.sizeDelta.x;
        float planDefParamsY = planDefaultParams.sizeDelta.y;
        float fontSizeDefParams = 0.05f;
        // Plan Default Params title
        planDefaultParams.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(planDefParamsX * marginX, -planDefParamsY * (marginY * 0.5f));
        planDefaultParams.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(planDefParamsX * 0.9f, planDefParamsY * 0.2f);
        planDefaultParams.GetChild(0).GetComponent<Text>().fontSize = (int)(planDefParamsY * 0.08f);
        // Plan Default Height Label
        planDefaultParams.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(planDefParamsX * marginX, -planDefParamsY * (marginY * 2.5f));
        planDefaultParams.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(planDefParamsX * 0.3f, planDefParamsY * 0.15f);
        planDefaultParams.GetChild(1).GetComponent<Text>().fontSize = (int)(planDefParamsY * fontSizeDefParams);
        // Plan Default Speed Label
        planDefaultParams.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(planDefParamsX * marginX, -planDefParamsY * (marginY * 4.5f));
        planDefaultParams.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(planDefParamsX * 0.3f, planDefParamsY * 0.15f);
        planDefaultParams.GetChild(2).GetComponent<Text>().fontSize = (int)(planDefParamsY * fontSizeDefParams);
        // Plan Default Duration Label
        planDefaultParams.GetChild(3).GetComponent<RectTransform>().anchoredPosition = new Vector2(planDefParamsX * marginX, -planDefParamsY * (marginY * 6.5f));
        planDefaultParams.GetChild(3).GetComponent<RectTransform>().sizeDelta = new Vector2(planDefParamsX * 0.3f, planDefParamsY * 0.15f);
        planDefaultParams.GetChild(3).GetComponent<Text>().fontSize = (int)(planDefParamsY * fontSizeDefParams);
        // Plan Default Height Input
        planDefaultParams.GetChild(4).GetComponent<RectTransform>().anchoredPosition = new Vector2((planDefParamsX * 0.50f) + planDefParamsX * marginX, -planDefParamsY * (marginY * 2.5f));
        planDefaultParams.GetChild(4).GetComponent<RectTransform>().sizeDelta = new Vector2(planDefParamsX * 0.2f, planDefParamsY * 0.15f);
        planDefaultParams.GetChild(4).GetChild(0).GetComponent<Text>().fontSize = (int)(planDefParamsY * fontSizeDefParams);
        planDefaultParams.GetChild(4).GetChild(1).GetComponent<Text>().fontSize = (int)(planDefParamsY * fontSizeDefParams);
        // Plan Default Speed Input
        planDefaultParams.GetChild(5).GetComponent<RectTransform>().anchoredPosition = new Vector2((planDefParamsX * 0.50f) + planDefParamsX * marginX, -planDefParamsY * (marginY * 4.5f));
        planDefaultParams.GetChild(5).GetComponent<RectTransform>().sizeDelta = new Vector2(planDefParamsX * 0.2f, planDefParamsY * 0.15f);
        planDefaultParams.GetChild(5).GetChild(0).GetComponent<Text>().fontSize = (int)(planDefParamsY * fontSizeDefParams);
        planDefaultParams.GetChild(5).GetChild(1).GetComponent<Text>().fontSize = (int)(planDefParamsY * fontSizeDefParams);
        // Plan Default Duration Input
        planDefaultParams.GetChild(6).GetComponent<RectTransform>().anchoredPosition = new Vector2((planDefParamsX * 0.50f) + planDefParamsX * marginX, -planDefParamsY * (marginY * 6.5f));
        planDefaultParams.GetChild(6).GetComponent<RectTransform>().sizeDelta = new Vector2(planDefParamsX * 0.2f, planDefParamsY * 0.15f);
        planDefaultParams.GetChild(6).GetChild(0).GetComponent<Text>().fontSize = (int)(planDefParamsY * fontSizeDefParams);
        planDefaultParams.GetChild(6).GetChild(1).GetComponent<Text>().fontSize = (int)(planDefParamsY * fontSizeDefParams);
        // Plan Height Increase
        planDefaultParams.GetChild(7).GetComponent<RectTransform>().anchoredPosition = new Vector2((planDefParamsX * 0.72f) + planDefParamsX * marginX, -planDefParamsY * (marginY * 2.5f));
        planDefaultParams.GetChild(7).GetComponent<RectTransform>().sizeDelta = new Vector2(planDefParamsX * 0.13f, planDefParamsY * 0.15f);
        // Plan Height Decrease
        planDefaultParams.GetChild(8).GetComponent<RectTransform>().anchoredPosition = new Vector2((planDefParamsX * 0.35f) + planDefParamsX * marginX, -planDefParamsY * (marginY * 2.5f));
        planDefaultParams.GetChild(8).GetComponent<RectTransform>().sizeDelta = new Vector2(planDefParamsX * 0.13f, planDefParamsY * 0.15f);
        // Plan Speed Increase
        planDefaultParams.GetChild(9).GetComponent<RectTransform>().anchoredPosition = new Vector2((planDefParamsX * 0.72f) + planDefParamsX * marginX, -planDefParamsY * (marginY * 4.5f));
        planDefaultParams.GetChild(9).GetComponent<RectTransform>().sizeDelta = new Vector2(planDefParamsX * 0.13f, planDefParamsY * 0.15f);
        // Plan Speed Decrease
        planDefaultParams.GetChild(10).GetComponent<RectTransform>().anchoredPosition = new Vector2((planDefParamsX * 0.35f) + planDefParamsX * marginX, -planDefParamsY * (marginY * 4.5f));
        planDefaultParams.GetChild(10).GetComponent<RectTransform>().sizeDelta = new Vector2(planDefParamsX * 0.13f, planDefParamsY * 0.15f);
        // Plan Duration Increase
        planDefaultParams.GetChild(11).GetComponent<RectTransform>().anchoredPosition = new Vector2((planDefParamsX * 0.72f) + planDefParamsX * marginX, -planDefParamsY * (marginY * 6.5f));
        planDefaultParams.GetChild(11).GetComponent<RectTransform>().sizeDelta = new Vector2(planDefParamsX * 0.13f, planDefParamsY * 0.15f);
        // Plan Duration Decrease
        planDefaultParams.GetChild(12).GetComponent<RectTransform>().anchoredPosition = new Vector2((planDefParamsX * 0.35f) + planDefParamsX * marginX, -planDefParamsY * (marginY * 6.5f));
        planDefaultParams.GetChild(12).GetComponent<RectTransform>().sizeDelta = new Vector2(planDefParamsX * 0.13f, planDefParamsY * 0.15f);

        float mapInfoX = mapInfoImage.sizeDelta.x;
        float mapInfoY = mapInfoImage.sizeDelta.y;
        // Map Info Title
        mapInfoImage.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(mapInfoX * marginX, -mapInfoY * (marginY * 0.5f));
        mapInfoImage.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.8f, mapInfoY * 0.15f);
        mapInfoImage.GetChild(0).GetComponent<Text>().fontSize = (int)(planInfoY * 0.05f);
        // Map Name Label
        mapInfoImage.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(mapInfoX * marginX, -mapInfoY * (marginY * 2.0f));
        mapInfoImage.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.15f);
        mapInfoImage.GetChild(1).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map Location Label
        mapInfoImage.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(mapInfoX * marginX, -mapInfoY * (marginY * 3.5f));
        mapInfoImage.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.15f);
        mapInfoImage.GetChild(2).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map Modification Date Label
        mapInfoImage.GetChild(3).GetComponent<RectTransform>().anchoredPosition = new Vector2(mapInfoX * marginX, -mapInfoY * (marginY * 5.0f));
        mapInfoImage.GetChild(3).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.15f);
        mapInfoImage.GetChild(3).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map Size Label
        mapInfoImage.GetChild(4).GetComponent<RectTransform>().anchoredPosition = new Vector2(mapInfoX * marginX, -mapInfoY * (marginY * 6.5f));
        mapInfoImage.GetChild(4).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.15f);
        mapInfoImage.GetChild(4).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map Bounding Box Label
        mapInfoImage.GetChild(5).GetComponent<RectTransform>().anchoredPosition = new Vector2(mapInfoX * marginX, -mapInfoY * (marginY * 8.0f));
        mapInfoImage.GetChild(5).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.15f);
        mapInfoImage.GetChild(5).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map Name Input Field
        mapInfoImage.GetChild(6).GetComponent<RectTransform>().anchoredPosition = new Vector2((mapInfoX / 2) + mapInfoX * marginX, -mapInfoY * (marginY * 2.0f));
        mapInfoImage.GetChild(6).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.1f);
        mapInfoImage.GetChild(6).GetChild(0).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        mapInfoImage.GetChild(6).GetChild(1).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map Location Input Field
        mapInfoImage.GetChild(7).GetComponent<RectTransform>().anchoredPosition = new Vector2((mapInfoX / 2) + mapInfoX * marginX, -mapInfoY * (marginY * 3.5f));
        mapInfoImage.GetChild(7).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.1f);
        mapInfoImage.GetChild(7).GetChild(0).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        mapInfoImage.GetChild(7).GetChild(1).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map ModificationDate Text
        mapInfoImage.GetChild(8).GetComponent<RectTransform>().anchoredPosition = new Vector2((mapInfoX / 2) + mapInfoX * marginX, -mapInfoY * (marginY * 5.0f));
        mapInfoImage.GetChild(8).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.15f);
        mapInfoImage.GetChild(8).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map Size Text
        mapInfoImage.GetChild(9).GetComponent<RectTransform>().anchoredPosition = new Vector2((mapInfoX / 2) + mapInfoX * marginX, -mapInfoY * (marginY * 6.5f));
        mapInfoImage.GetChild(9).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.15f);
        mapInfoImage.GetChild(9).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map Bounding Box Text
        mapInfoImage.GetChild(10).GetComponent<RectTransform>().anchoredPosition = new Vector2((mapInfoX / 2) + mapInfoX * marginX, -mapInfoY * (marginY * 8.0f));
        mapInfoImage.GetChild(10).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.15f);
        mapInfoImage.GetChild(10).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);

        panelOverwriter.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 2, Screen.height / 2);
        panelOverwriter.GetChild(0).GetChild(0).GetComponent<Text>().fontSize = Screen.width / 50;
        panelOverwriter.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        panelOverwriter.GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = -new Vector2(Screen.width / 25, 0);

        panelOverwriter.GetChild(0).GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 10, Screen.height / 15);
        panelOverwriter.GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>().fontSize = Screen.width / 60;
        panelOverwriter.GetChild(0).GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 10, Screen.height / 15);
        panelOverwriter.GetChild(0).GetChild(2).GetChild(0).GetComponent<Text>().fontSize = Screen.width / 60;

        // Delete panel
        Vector2 deletePanelSizes = new Vector2(width / 2.75f, height / 2.75f);
        deletePanel.sizeDelta = deletePanelSizes;
        // Delete panel text
        deletePanel.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(deletePanelSizes.x * 0.75f, deletePanelSizes.y * 0.6f);
        deletePanel.GetChild(1).GetComponent<Text>().fontSize = width / 50;
        // Cancel button
        deletePanel.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(-deletePanelSizes.x / 5, -deletePanelSizes.y / 6);
        deletePanel.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(deletePanelSizes.x / 3.5f, deletePanelSizes.y / 8);
        deletePanel.GetChild(2).GetChild(0).GetComponent<Text>().fontSize = width / 50;
        deletePanel.GetChild(3).GetComponent<RectTransform>().anchoredPosition = new Vector2(deletePanelSizes.x / 5, -deletePanelSizes.y / 6);
        deletePanel.GetChild(3).GetComponent<RectTransform>().sizeDelta = new Vector2(deletePanelSizes.x / 3.5f, deletePanelSizes.y / 8);
        deletePanel.GetChild(3).GetChild(0).GetComponent<Text>().fontSize = width / 50;

        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Missions/"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Missions/");
        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Thumbnails/"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Thumbnails/");
        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Maps/"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Maps/");
        // ## DEBUG Creación de botones para mapas
        //mapCount = 6;
        //for (it = 0; it < mapCount; it++)
        //{
        //    GameObject button = Instantiate(mapButtonPrefab, mapsContainer.gameObject.transform);
        //    button.GetComponent<RectTransform>().anchoredPosition = new Vector2(-buttonOffset + (buttonOffset * it), 0);
        //    button.GetComponent<RectTransform>().sizeDelta = buttonSize;
        //    button.name = "Map" + it.ToString();
        //    button.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => TestId(button.name));
        //}

        fileBrowser = GameObject.Find("FileBrowserUI");
        fileBrowser.SetActive(false);
        pleaseConnectToTheDronePanel.sizeDelta = new Vector2(Screen.width / 2, Screen.height / 2);
        pleaseConnectToTheDronePanel.GetChild(0).GetComponent<Text>().fontSize = Screen.width / 20;

        //DontDestroyOnLoad(GameObject.Find("ClientUnity")); // ## TODO Hacer en la escena general 
    }
    void Start()
    {
        GeneralSceneManager.sceneState = GeneralSceneManager.SceneState.PlanSelection;
        //We start by asking if the server contains missions that the app doesn't
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        if ((clientUnity != null) && (clientUnity.client != null) && clientUnity.client.isConnected)
        {
            //clientUnity.client.sendCommand((byte)Modules.ATREYU_MODULE, (byte)AtreyuCommandType.ATREYU_ENTER_RECORDING_STATE);
            clientUnity.client.SendCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_NUMBER_PLANS_IN_DDBB);
            askedForMaps = true;
        }
    }
    

    private void Update()
    {
        //If a metadata was downloaded, we start comparing it with every other metadata
        string metadataJson =  OSModule.getMetadataFile();
        if ((clientUnity != null) && (clientUnity.client != null) && clientUnity.client.isConnected && metadataJson != null)
        {
            MapMetadata metadata = JsonUtility.FromJson<MapMetadata>(metadataJson);
            bool isTheMetadaIn = false;
            serverMissions.Add(metadata.Guid);
            
            foreach (Transform button in mapsContainer)
            {
                //If the map button is not createmap
                if (button.name == "CreateMap")
                {
                    continue;
                }
                string metadatastring = File.ReadAllText(button.name);
                MapMetadata metadata2 = JsonUtility.FromJson<MapMetadata>(metadatastring);
                //If the maps are the same map, we enter here
                if (metadata2.Name == metadata.Name)
                {
                    //We have that plan already
                    isTheMetadaIn = true;
                    //We have donwloaded every map already
                    if (PlanSelectionManager.askedForMaps == true && metadatasDownloaded == mapButtonsCreated)
                    {
                        askedForMaps = false;
                    }
                    //If the modification date is different, then there is a conflict
                    //If its 0 they are equal
                    if (metadata.Date.CompareTo(metadata2.Date) == 1)
                    {
                        mapButtonsCreated++;
                        button.GetChild(1).GetComponent<Image>().color = Color.red;
                        button.GetChild(3).gameObject.SetActive(true);
                        button.GetChild(3).GetComponent<ConflictMap>().setConflictingMaps(metadata, metadata2, panelOverwriter.gameObject, clientUnity);
                    }
                    break;
                }
            }
            if (!isTheMetadaIn)
            {
                //If the metadata is not in the app, we download a metadata, mission and thumbnail
                System.IO.File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + metadata.Guid + ".json.metadata", metadataJson);

                clientUnity.client.sendTwoPartCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_THUMBNAIL, metadata.Guid + '\0');
                askedForMaps = true;

            }

        }
        //We create a button for every metadata downloaded
        string guid = OSModule.getGuidNameToCreateButton();
        if ((clientUnity != null) && (clientUnity.client != null) && clientUnity.client.isConnected  && guid != null)
        {
            string metadataJson2 = System.IO.File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + guid + ".json.metadata");
            MapMetadata metadata = JsonUtility.FromJson<MapMetadata>(metadataJson2);
            GameObject button = Instantiate(mapButtonPrefab, mapsContainer.gameObject.transform);
            mapCount++;
            button.GetComponent<PlanSelectionButtons>().guid = guid;

            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(-buttonOffset + (buttonOffset * it), 0);
            it++;
            button.GetComponent<RectTransform>().sizeDelta = buttonSize;
            //Name of the plan to display
            GameObject nameOfPlanGameObject = new GameObject();
            nameOfPlanGameObject.transform.parent = button.transform;
            Text nameOfPlanText = nameOfPlanGameObject.AddComponent<Text>();
            nameOfPlanText.font = rubik;
            if (metadata.Name.Length > 20)
            {
                nameOfPlanText.text = metadata.Name.Substring(0, 20);
            }
            else
                nameOfPlanText.text = metadata.Name;
            nameOfPlanText.verticalOverflow = VerticalWrapMode.Overflow;

            nameOfPlanText.rectTransform.pivot = new Vector2(0, 0);
            nameOfPlanText.fontSize = Screen.width / 60;
            nameOfPlanText.rectTransform.anchorMin = new Vector2(0, 0f);
            nameOfPlanText.rectTransform.anchorMax = new Vector2(0, 0f);
            nameOfPlanText.rectTransform.sizeDelta = new Vector2(buttonSize.x, Screen.height / 20);
            nameOfPlanText.rectTransform.anchoredPosition = new Vector2(0, 0);
            button.name = Application.persistentDataPath + "/PersistentData/Missions/" + guid + ".json.metadata";
            byte[] imageBytes = File.ReadAllBytes(Application.persistentDataPath + "/PersistentData/Thumbnails/" + guid + ".jpeg.thumbnail");

            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imageBytes);
            tex.Apply();
            button.transform.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0f, 0f));
            mapButtonsCreated++;
            //We ahev downloaded every map
            if (PlanSelectionManager.askedForMaps == true && metadatasDownloaded == mapButtonsCreated )
            {
                askedForMaps = false;
            }
        }
        
    }

    void OnEnable()
    {
        //This function creates the map buttons
       // Dictionary<string, ConflictMap> names = new Dictionary<string, ConflictMap>();
       //Fist we destroy any map button that existed previously, just in case
        foreach (Transform child in mapsContainer.transform)
        {

            //if (child.GetChild(3).gameObject.activeSelf)
            //{
            //    names.Add(child.gameObject.name, child.GetChild(3).gameObject.GetComponent<ConflictMap>());
            //}
            Destroy(child.gameObject);
        }

        foreach (Transform child in plansContainer.transform)
        {
            Destroy(child.gameObject);
        }

        //UnityEngine.Debug.Log("Plan selection enabled");
        string[] mapFiles = System.IO.Directory.GetFiles(Application.persistentDataPath + "/PersistentData/Missions/", "*metadata*");
        mapCount = mapFiles.Length;
        string[] thumbnail = System.IO.Directory.GetFiles(Application.persistentDataPath + "/PersistentData/Thumbnails/", "*thumbnail*");
        int thumbnailCount = thumbnail.Length;
        //Sorting by thumbnail name and mapname in order not to assign a wrong thumbnail to a metadata
        Array.Sort(mapFiles);
        Array.Sort(thumbnail);
        for (it = 0; it < mapCount; it++)
        {
            string metadataJson = System.IO.File.ReadAllText(mapFiles[it]);
            MapMetadata metadata = JsonUtility.FromJson<MapMetadata>(metadataJson);
            GameObject button = Instantiate(mapButtonPrefab, mapsContainer.gameObject.transform);
            button.GetComponent<PlanSelectionButtons>().guid = metadata.Guid;
            button.GetComponent<PlanSelectionButtons>().pleaseConnectToTheDronePanel = pleaseConnectToTheDronePanel;
            button.GetComponent<PlanSelectionButtons>().downloadingMapPanel = modelLoadingPanel;

            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(-buttonOffset + (buttonOffset * it), 0);
            button.GetComponent<RectTransform>().sizeDelta = buttonSize;
            button.transform.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(button.GetComponent<RectTransform>().sizeDelta.x / 6, button.GetComponent<RectTransform>().sizeDelta.y / 4);
            //Name of the plan to display
            GameObject nameOfPlanGameObject = new GameObject();
            nameOfPlanGameObject.transform.parent = button.transform;
            nameOfPlanGameObject.transform.SetAsFirstSibling();
            Text nameOfPlanText = nameOfPlanGameObject.AddComponent<Text>();
            nameOfPlanText.font = rubik;
            if (metadata.Name.Length > 20)
            {
                nameOfPlanText.text = metadata.Name.Substring(0, 20);
            }
            else
                nameOfPlanText.text = metadata.Name;
            nameOfPlanText.verticalOverflow = VerticalWrapMode.Overflow;

            nameOfPlanText.rectTransform.pivot = new Vector2(0, 0) ;
            nameOfPlanText.fontSize = Screen.width / 60;
            nameOfPlanText.rectTransform.anchorMin = new Vector2(0, 0f);
            nameOfPlanText.rectTransform.anchorMax = new Vector2(0, 0f);
            nameOfPlanText.rectTransform.sizeDelta = new Vector2(buttonSize.x, Screen.height / 20);
            nameOfPlanText.rectTransform.anchoredPosition = new Vector2(0, -50);
            button.name = mapFiles[it];

            byte []imageBytes = File.ReadAllBytes(thumbnail[it]);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imageBytes);

            tex.Apply();
            button.transform.GetChild(1).GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0f, 0f));
            //if (names.ContainsKey(button.gameObject.name))
            //{
            //    button.transform.GetChild(1).GetComponent<Image>().color = Color.red;
            //    button.transform.GetChild(3).gameObject.SetActive(true);
            //    ConflictMap aux = button.transform.GetChild(3).gameObject.GetComponent<ConflictMap>();
            //    aux = names[button.gameObject.name];
            //}
        }
        //Here the new map button is created
        if (GeneralSceneManager.appState == GeneralSceneManager.ApplicationState.Planning) { 
            // Creación de un botón al final para añadir mapas
            GameObject createMapB = Instantiate(mapButtonPrefab, mapsContainer.gameObject.transform);
            createMapB.GetComponent<RectTransform>().anchoredPosition = new Vector2(-buttonOffset + (buttonOffset * it), 0);
            it++;
            createMapB.GetComponent<RectTransform>().sizeDelta = buttonSize;
            createMapB.name = "CreateMap";
            createMapB.GetComponent<PlanSelectionButtons>().newMapButton = true;
            //createMapB.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => NewMap());
            createMapB.transform.GetChild(0).GetComponent<Image>().type = Image.Type.Sliced;
            GameObject createMapText = new GameObject();
            createMapText.transform.parent = createMapB.transform.GetChild(0);
            createMapText.name = "CreateMapText";
            createMapText.AddComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            createMapText.AddComponent<Text>().text = "New Map";
            createMapText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            createMapText.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            Text mapBText = createMapText.GetComponent<Text>();
            mapBText.font = Rubik;
            mapBText.fontSize = height / 32;
            mapBText.alignment = TextAnchor.MiddleCenter;
        }
        else { mapCount--; }

        if (mapCount > 3)
        {
            mapRightB.interactable = true;
        }
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        // Activación o desactivación de las flechas en función de la posición actual en cada panel
        if (planIndex > 0)
            planLeftB.interactable = true;
        else
            planLeftB.interactable = false;

        if (mapIndex > 0)
            mapLeftB.interactable = true;
        else
            mapLeftB.interactable = false;

        if (planIndex < planCount - 2)
            planRightB.interactable = true;
        else
            planRightB.interactable = false;

        if (mapIndex < mapCount - 2)
            mapRightB.interactable = true;
        else
            mapRightB.interactable = false;

        switch (animationsState)
        {
            case PanelAnimations.IDLE:
                break;
            case PanelAnimations.NEXT_MAP:
                currentTime += (Time.deltaTime / animationsTime);
                mapsContainer.anchoredPosition = Vector2.Lerp(animationOrigin, animationDestination, currentTime / animationsTime);
                if (currentTime >= animationsTime) { 
                    mapIndex++;
                    animationsState = PanelAnimations.IDLE;
                }
                break;
            case PanelAnimations.NEXT_PATH:
                currentTime += (Time.deltaTime / animationsTime);
                plansContainer.anchoredPosition = Vector2.Lerp(animationOrigin, animationDestination, currentTime / animationsTime);
                if (currentTime >= animationsTime) { 
                    planIndex++;
                    animationsState = PanelAnimations.IDLE;
                }
                break;
            case PanelAnimations.PREV_MAP:
                currentTime += (Time.deltaTime / animationsTime);
                mapsContainer.anchoredPosition = Vector2.Lerp(animationOrigin, animationDestination, currentTime / animationsTime);
                if (currentTime >= animationsTime)
                {
                    mapIndex--;
                    animationsState = PanelAnimations.IDLE;
                }
                break;
            case PanelAnimations.PREV_PATH:
                currentTime += (Time.deltaTime / animationsTime);
                plansContainer.anchoredPosition = Vector2.Lerp(animationOrigin, animationDestination, currentTime / animationsTime);
                if (currentTime >= animationsTime)
                {
                    planIndex--;
                    animationsState = PanelAnimations.IDLE;
                }
                break;
            case PanelAnimations.FIRST_PATH:
                plansContainer.anchoredPosition = new Vector2(0, 0);
                planIndex = 0;
                animationsState = PanelAnimations.IDLE;
                break;
            case PanelAnimations.FIRST_MAP:
                mapsContainer.anchoredPosition = new Vector2(0, 0);
                mapIndex = 0;
                animationsState = PanelAnimations.FIRST_PATH;
                break;
        }
        //This loads a scene once a plan has been clicked, and if we didn't have the map, after downloading it
        if (MapLoader.mapDownloaded)
        {
            MapLoader.mapDownloaded = false;
            if (GeneralSceneManager.appState == GeneralSceneManager.ApplicationState.Planning)
            {
                GameObject.Find("UIManager").GetComponent<UIManager>().LoadCustomScene("Planning");
                //SceneManager.LoadScene("Planning");
            }
            else if (GeneralSceneManager.appState == GeneralSceneManager.ApplicationState.Recording)
            {
                GameObject.Find("UIManager").GetComponent<UIManager>().LoadCustomScene("Recording");
                //SceneManager.LoadScene("ModelAlignment");
            }
        }
		
	}

    // Desplazamos todos los mapas del panel para ver el siguiente
    public void NextMap()
    {

        if (animationsState == PanelAnimations.IDLE)
        { 
            currentTime = 0.0f;
            animationOrigin = mapsContainer.anchoredPosition;
            animationDestination = new Vector2(mapsContainer.anchoredPosition.x - buttonOffset, mapsContainer.anchoredPosition.y);
            animationsState = PanelAnimations.NEXT_MAP;
        }
        //mapsContainer.anchoredPosition -= new Vector2(buttonOffset, 0);
        //mapIndex++;
    }

    // Desplazamos todos los mapas del panel para ver el anterior
    public void PrevMap()
    {
        if (animationsState == PanelAnimations.IDLE)
        {
            currentTime = 0.0f;
            animationOrigin = mapsContainer.anchoredPosition;
            animationDestination = new Vector2(mapsContainer.anchoredPosition.x + buttonOffset, mapsContainer.anchoredPosition.y);
            animationsState = PanelAnimations.PREV_MAP;
        }
    }

    // Desplazamos todos los planes del panel para ver el siguiente
    public void NextPath()
    {
        if (animationsState == PanelAnimations.IDLE)
        {
            currentTime = 0.0f;
            animationOrigin = plansContainer.anchoredPosition;
            animationDestination = new Vector2(plansContainer.anchoredPosition.x - buttonOffset, plansContainer.anchoredPosition.y);
            animationsState = PanelAnimations.NEXT_PATH;
        }
        //plansContainer.anchoredPosition -= new Vector2(buttonOffset, 0);
        //planIndex++;
    }

    // Desplazamos todos los planes del panel para ver el anterior
    public void PrevPath()
    {
        if (animationsState == PanelAnimations.IDLE)
        {
            currentTime = 0.0f;
            animationOrigin = plansContainer.anchoredPosition;
            animationDestination = new Vector2(plansContainer.anchoredPosition.x + buttonOffset, plansContainer.anchoredPosition.y);
            animationsState = PanelAnimations.PREV_PATH;
        }
        //plansContainer.anchoredPosition += new Vector2(buttonOffset, 0);
        //planIndex--;
    }
    //From any screen that has a back button, goes back to select a map
    public void goBackToSelect()
    {
        UnityEngine.Debug.Log("BackToSelect");
        this.enabled = true;
        animationsState = PanelAnimations.FIRST_MAP;
        mapInfo.transform.GetChild(3).gameObject.SetActive(true);
        mapInfo.SetActive(false);
        planInfo.SetActive(false);
        planSelection.SetActive(true);

    }

   public void isCubeOrModel()
    {
        mapInfo.SetActive(false);

        newMission.SetActive(true);
    }
    //Creates a 3d model
   public void NewMapCreated()
   {
       mapInfo.SetActive(false);

       if (MissionManager.importedModel)
       {
           MissionManager.BuildImportedScene();

           if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Thumbnails"))
               System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Thumbnails");

           File.WriteAllBytes(Application.persistentDataPath + "/PersistentData/Thumbnails/" + MissionManager.guid + ".jpeg.thumbnail", modelThumbnail.texture.EncodeToPNG());
       }
       else
       {
           MissionManager.BuildEmptyScene();
           if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Thumbnails"))
               System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Thumbnails");

           File.WriteAllBytes(Application.persistentDataPath + "/PersistentData/Thumbnails/" + MissionManager.guid + ".jpeg.thumbnail", cubeThumbnail.texture.EncodeToPNG());
       }

       string json = File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.metadata");

       MapMetadata metadata = JsonUtility.FromJson<MapMetadata>(json);

       metadata.Name = MissionManager.mapName;
       metadata.Location = MissionManager.mapLocation;

       string metadataJson = JsonUtility.ToJson(metadata);

       if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Missions"))
           System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Missions");
       System.IO.File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + metadata.Guid + ".json.metadata", metadataJson);

       GameObject.Find("Background").GetComponent<BackgroundClicks>().enabled = false;
       GameObject.Find("Background").GetComponent<BackgroundClicks>().enabled = true;
       planSelection.transform.parent.GetComponent<PlanSelectionManager>().enabled = true;
       planSelection.SetActive(true);
   }
   /*
  public void NewModelCreated()
  {
      mapInfo.SetActive(false);

      MissionManager.BuildImportedScene();

      if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Thumbnails"))
          System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Thumbnails");

      File.WriteAllBytes(Application.persistentDataPath + "/PersistentData/Thumbnails/" + MissionManager.guid + ".jpeg.thumbnail", modelThumbnail.texture.EncodeToPNG());

      string json = File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.metadata");

      MapMetadata metadata = JsonUtility.FromJson<MapMetadata>(json);

      metadata.Name = MissionManager.mapName;
      metadata.Location = MissionManager.mapLocation;

      string metadataJson = JsonUtility.ToJson(metadata);

      if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Missions"))
          System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Missions");
      System.IO.File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + metadata.Guid + ".json.metadata", metadataJson);

      GameObject.Find("Background").GetComponent<BackgroundClicks>().enabled = false;
      GameObject.Find("Background").GetComponent<BackgroundClicks>().enabled = true;
      planSelection.transform.parent.GetComponent<PlanSelectionManager>().enabled = true;
      planSelection.SetActive(true);
  }
  */
  //Creates a box
   public void CreateEmptyScene()
    {
        MissionManager.importedModel = false;
        if(!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData");

        if (!System.IO.File.Exists(Application.persistentDataPath + "/PersistentData/cubeSimple.dae")) { 
            TextAsset ta = Resources.Load<TextAsset>("cubeSimple");
            System.IO.File.WriteAllBytes(Application.persistentDataPath + "/PersistentData/cubeSimple.dae", ta.bytes);
        }
        newMission.SetActive(false);
        createEmptyScene.SetActive(true);
        previewScene.SetActive(true);
    }

    public void LoadModel()
    {
        MissionManager.importedModel = true;
        newMission.SetActive(false);
        fileBrowser.SetActive(true);
    }

    // Función que carga el mapa seleccionado
    void LoadMap(string id)
    {

    }

    // ## Función que pasa a la ventana de carga de un mapa externo !! Revisar
    //void LoadExternalMap()
    //{
    //    
    //}

    // Funcion que carga el mapa seleccionado
    void LoadPlan(string id)
    {
        UnityEngine.Debug.Log(id);
    }

    public void BackToPlanInfo()
    {
        planInfo2.SetActive(false);
        planInfo.SetActive(true);

    }

    public void ShowDefParams()
    {
        planInfo.SetActive(false);
        planInfo2.SetActive(true);

    }
    //Fades the please connect to the drone to download a map panel
    IEnumerator FadePanel()
    {
        float alpha = 1.0f;
        pleaseConnectToTheDronePanel.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
        pleaseConnectToTheDronePanel.GetChild(0).GetComponent<Text>().color = new Color(1, 1, 1, alpha);
        pleaseConnectToTheDronePanel.gameObject.SetActive(true);
        yield return new WaitForSeconds(5f);

        while (pleaseConnectToTheDronePanel.GetComponent<Image>().color.a > 0)
        {
            alpha -= 0.5f;
            pleaseConnectToTheDronePanel.GetComponent<Image>().color = new Color( 1, 1, 1,alpha);
            pleaseConnectToTheDronePanel.GetChild(0).GetComponent<Text>().color = new Color(1,1,1, alpha);
            yield return new WaitForSeconds(0.1f);
        }
        pleaseConnectToTheDronePanel.gameObject.SetActive(false);

    }
    public void LoadNewPlan()
    {
        //Assigns the parameters that the user put on the inputfields of map info to the plan
        Path.Instance.FlightParams.height = MissionManager.planDefaultHeight;

        if (MissionManager.planName == "CreateMap")
        {
            MissionManager.planIndex = -1;

        }
        MissionManager.planName = planName.text;
        MissionManager.planAuthor = planAuthor.text;
        MissionManager.planNotes = planNotes.text;
        MissionManager.loadMap = true;
        MissionManager.modificationText = "" + DateTime.Now.Day + '/' + DateTime.Now.Month + '/' + DateTime.Now.Year.ToString();

        //MissionManager.planIndex = -1;
        Path.Instance = null;
        //If the map is not in the folder, download it
        if (!File.Exists(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map") && !File.Exists(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dpl.map"))
        {
            if (clientUnity == null || clientUnity.client == null || !clientUnity.client.isConnected)
            {
                //UnityEngine.Debug.Log("Please connect to the drone to download this map"); // ## TODO: Mostrar mensaje al usuario
                StartCoroutine(FadePanel());
                return;
            }
            else
            {

                PlanSelectionManager.askedForMaps = true;
                modelLoadingPanel.SetActive(true);
                modelLoadingPanel.transform.GetChild(0).GetComponent<Text>().text = "Downloading Map";
                clientUnity.client.sendTwoPartCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_MAP, MissionManager.guid + '\0');
            }
        }
        else
        {
            GameObject.Find("UIManager").GetComponent<UIManager>().LoadCustomScene("Planning");
            //SceneManager.LoadScene("Planning");
        }

    }
    //Removes map locally or in the server. If the map is removed locally but not in the server, the next time the tablet connects to the drone, it will download the map
    public void ConfirmRemove()
    {
        switch (ApplicationVariables.appRemoveState)
        {
            case (byte)ApplicationVariables.RemoveState.None:
                break;
            case (byte)ApplicationVariables.RemoveState.LocalMapRemove:
                UnityEngine.Debug.Log("RemoveLocalMap");

                string metadata = File.ReadAllText(ApplicationVariables.persistentDataPath + "Missions/" + MissionManager.guid + ".json.metadata");
                MapMetadata mapMetadata = JsonUtility.FromJson<MapMetadata>(metadata);
                if (mapMetadata.Map_type == (byte)MapMetadata.MapType.PointCloud)
                {
                    System.IO.File.Delete(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dpl.map");

                }
                else
                {

                    System.IO.File.Delete(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map");

                }

                System.IO.File.Delete(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.mission");
                System.IO.File.Delete(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.metadata");
                System.IO.File.Delete(Application.persistentDataPath + "/PersistentData/Thumbnails/" + MissionManager.guid + ".jpeg.thumbnail");

                foreach (Transform child in BackgroundClicks.plansContainer.transform)
                {
                    Destroy(child.gameObject);
                }

                PlanSelectionManager.mapCount--;

                Destroy(ApplicationVariables.removeGO);

                ApplicationVariables.appRemoveState = (byte)ApplicationVariables.RemoveState.None;
                ApplicationVariables.removeGO = null;
                deletePanel.transform.parent.gameObject.SetActive(false);
                OnEnable();

                break;
            case (byte)ApplicationVariables.RemoveState.ServerMapRemove:
                UnityEngine.Debug.Log("RemoveServerMap");
                ClientUnity clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
                // Requesting path will delete the files if we call it from PlanSelection scene
                clientUnity.client.SendCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_LIBRARY_PATH);

                deletePanel.transform.parent.gameObject.SetActive(false);
                break;
            case (byte)ApplicationVariables.RemoveState.PlanRemove:
                break;
        }
    }

    public void CancelRemove()
    {
        deletePanel.transform.parent.gameObject.SetActive(false);
    }

}
