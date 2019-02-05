using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// This class adjust the interface on planning, contains the function that saves the mission and changes between panels
/// </summary>

public class MainPlanningUI : MonoBehaviour {

    public RectTransform rect;
    public RectTransform buttonPanel;
    public RectTransform editPathButton, editWaypointButton, POIButton, editCurvesButton, previewButton;
    public RectTransform eraseButton, eraseButtonPoi; // Se usa para cambiar el panel (padre del objeto)
    public RectTransform pointsPanel, POIPanel;
    public GameObject frontCam;
    public GameObject TopCam;
    public RectTransform isPointButton, isGimballButton, isRecCamButton, pointsButtonsPanel;
    public RectTransform selectPoinTypeText;
    public RectTransform pointTypePanel;

    public RectTransform makeWaypointButton, makeStopButton, inputFieldStopTime;
    public RectTransform speedInputField, speedText;
    public RectTransform photoIcon, videoIcon;
    public RectTransform panelPhotoOrVideo;
    public RectTransform scrollViewParameters, scrollViewPanel;
    public RectTransform[] photoParametersInputs, videoParametersInputs;
    public RectTransform[] photoParametersTexts, videoParametersTexts;
    public GameObject POIEditor;
    public GameObject POIPanelButtons;
    public GameObject PreviewPanel;
    public RectTransform barra1, barra2;
    public GameObject EditCurvePanel;
    public RectTransform HeightText, TimeText;
    public GameObject barPrefab;
    public GameObject OriginAxis;
    public RectTransform saveButton;
    public Transform viewFrames;
    float scaleFactor = 0.2f;
    Vector2 sizeScrollHeightPhoto;
    Vector2 sizeScrollHeightVideo;

    GameObject modelLoadingPanel;

    public RectTransform checkBoxActivateCamera, buttonActivateRecCamCanvas;

    void Awake()
    {
        modelLoadingPanel = GameObject.Find("ModelLoadingPanel");
        modelLoadingPanel.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, Screen.height / 7);
        modelLoadingPanel.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 1.5f, Screen.height / 3);

        modelLoadingPanel.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -Screen.height / 7);
        modelLoadingPanel.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.height / 8, Screen.height / 8);

        modelLoadingPanel.transform.GetChild(0).GetComponent<Text>().fontSize = (int)(Screen.width * 0.05f);

        StartCoroutine(WaitForModel());
        GeneralSceneManager.sceneState = GeneralSceneManager.SceneState.Planning;

        rect.sizeDelta = new Vector2(Screen.width, Screen.height * 0.75f);
        EditCurvePanel.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height * 0.75f); ;

    // Posicion del panel de los botones (siempre por debajo de las ventanas de planning)
    //buttonPanel.anchoredPosition = new Vector2(0, (Screen.height / 4) + (buttonSize / 2) + buttonOffset);
        buttonPanel.sizeDelta = new Vector2(Screen.width / 2.0f, Screen.height / 20.0f);
        buttonPanel.anchoredPosition = new Vector2(0, Screen.height / 25.0f);

        float buttonPos = buttonPanel.sizeDelta.x / 5;

        // Tamaño de los botones, ancho y alto dependen del alto de la ventana para que los botones sean cuadrados
        //int buttonSize = Screen.height / 15;
        editPathButton.sizeDelta =      new Vector2(buttonPos, 0);
        editWaypointButton.sizeDelta =  new Vector2(buttonPos, 0);
        POIButton.sizeDelta =           new Vector2(buttonPos, 0);
        //editCurvesButton.sizeDelta =    new Vector2(buttonPos, 0);
        previewButton.sizeDelta =       new Vector2(buttonPos, 0);
        eraseButton.sizeDelta =         new Vector2(buttonPos, Screen.height / 25.0f);
        eraseButtonPoi.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(buttonPos * 1.5f, buttonPos / 3);
        


        // Button position
        editPathButton.anchoredPosition =       new Vector2(0 * buttonPos, 0);
        editWaypointButton.anchoredPosition =   new Vector2(1 * buttonPos, 0);
        POIButton.anchoredPosition =            new Vector2(2 * buttonPos, 0);
        //editCurvesButton.anchoredPosition =     new Vector2(3 * buttonPos, 0);
        previewButton.anchoredPosition =        new Vector2(3 * buttonPos, 0);
        eraseButton.anchoredPosition =          new Vector2(-buttonPos, buttonPanel.anchoredPosition.y);
        eraseButtonPoi.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(4 * buttonPos, buttonPanel.anchoredPosition.y);

        PreviewPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 2, Screen.height / 2);

        HeightText.position = new Vector2(EditCurvePanel.GetComponent<RectTransform>().sizeDelta.x / 3 - 50f, HeightText.position.y);
        TimeText.position = new Vector2(EditCurvePanel.GetComponent<RectTransform>().sizeDelta.x / 3 - 50f, TimeText.position.y);

        barPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(5, buttonPanel.sizeDelta.y * 0.9f);
        float bottomBarWidth = barPrefab.GetComponent<RectTransform>().sizeDelta.y * 9 / 92;
        barPrefab.GetComponent<RectTransform>().anchoredPosition = new Vector2(buttonPos * 1 - bottomBarWidth / 2, 0);
        Instantiate(barPrefab, buttonPanel.transform);
        barPrefab.GetComponent<RectTransform>().anchoredPosition = new Vector2(buttonPos * 2 - bottomBarWidth / 2, 0);
        Instantiate(barPrefab, buttonPanel.transform);
        barPrefab.GetComponent<RectTransform>().anchoredPosition = new Vector2(buttonPos * 3 - bottomBarWidth / 2, 0);
        Instantiate(barPrefab, buttonPanel.transform);
        //barPrefab.GetComponent<RectTransform>().anchoredPosition = new Vector2(buttonPos * 4 - bottomBarWidth / 2, 0);
        //Instantiate(barPrefab, buttonPanel.transform);

        //Posicion de los paneles path, point, poi...
        pointsPanel.sizeDelta = new Vector2(Screen.width/2, Screen.height / 2);
        pointsButtonsPanel.sizeDelta = new Vector2(0, pointsPanel.sizeDelta.y / 6);
        float pointsButtonSize = pointsPanel.sizeDelta.x / 3;
        float newBarWidth = barra1.sizeDelta.y * 9 / 92;

        isPointButton.sizeDelta = new Vector2(pointsButtonSize, pointsButtonsPanel.sizeDelta.y - pointsButtonsPanel.sizeDelta.y * 0.25f);
        isPointButton.anchoredPosition = new Vector2(0 * pointsButtonSize, 0.0f);

        isGimballButton.sizeDelta = new Vector2(pointsButtonSize, pointsButtonsPanel.sizeDelta.y - pointsButtonsPanel.sizeDelta.y * 0.25f);
        isGimballButton.anchoredPosition = new Vector2(1 * pointsButtonSize, 0.0f);

        isRecCamButton.sizeDelta = new Vector2(pointsButtonSize, pointsButtonsPanel.sizeDelta.y - pointsButtonsPanel.sizeDelta.y * 0.25f);
        isRecCamButton.anchoredPosition = new Vector2(2 * pointsButtonSize, 0.0f);
        pointTypePanel.sizeDelta = new Vector2(0, pointsPanel.sizeDelta.y / 10);
        selectPoinTypeText.GetComponent<Text>().fontSize = Screen.width / 60;
        selectPoinTypeText.anchoredPosition = new Vector2(20, pointTypePanel.sizeDelta.y);

        barra1.sizeDelta = new Vector2(50, pointsButtonsPanel.sizeDelta.y - (pointsButtonsPanel.sizeDelta.y * 0.3f));
        barra2.sizeDelta = new Vector2(50, pointsButtonsPanel.sizeDelta.y - (pointsButtonsPanel.sizeDelta.y * 0.3f));
        barra1.anchoredPosition = new Vector2(pointsButtonSize * 1 - newBarWidth / 2, 0);
        barra2.anchoredPosition = new Vector2(pointsButtonSize * 2 - newBarWidth / 2, 0);

        makeWaypointButton.sizeDelta = new Vector2(pointsPanel.sizeDelta.x / 6, isPointButton.sizeDelta.y);
        makeWaypointButton.anchoredPosition = new Vector2(0 * pointsButtonSize + 50 + makeWaypointButton.sizeDelta.x / 2, 0.0f);

        makeWaypointButton.GetChild(0).GetComponent<Text>().fontSize = Screen.width / 60;
        makeStopButton.sizeDelta = new Vector2(pointsPanel.sizeDelta.x / 6, isPointButton.sizeDelta.y);
        makeStopButton.GetChild(0).GetComponent<Text>().fontSize = Screen.width / 60;

        inputFieldStopTime.sizeDelta = new Vector2(pointsPanel.sizeDelta.x / 6, 0);
        inputFieldStopTime.GetChild(1).GetComponent<Text>().fontSize = Screen.width / 60;

        speedText.anchoredPosition = new Vector2(2 * pointsPanel.sizeDelta.x / 6, pointsPanel.sizeDelta.y / 6);
        speedText.GetComponent<Text>().fontSize = Screen.width / 60; ;

        speedInputField.anchoredPosition = new Vector2(3 * pointsPanel.sizeDelta.x / 6, pointsPanel.sizeDelta.y / 6);
        speedInputField.sizeDelta = new Vector2( pointsPanel.sizeDelta.x / 8, pointsPanel.sizeDelta.y / 10);
        speedInputField.GetChild(1).GetComponent<Text>().fontSize = Screen.width / 60;
        speedInputField.GetComponent<InputField>().text = "" + MissionManager.planDefaultSpeed;
        //RECCAMPARAMETERS
        panelPhotoOrVideo.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 5 * pointsPanel.sizeDelta.y /6);
        panelPhotoOrVideo.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        panelPhotoOrVideo.sizeDelta = new Vector2(0, panelPhotoOrVideo.transform.parent.GetComponent<RectTransform>().sizeDelta.y / 4);
        photoIcon.sizeDelta = new Vector2(panelPhotoOrVideo.transform.parent.GetComponent<RectTransform>().sizeDelta.y / 4, 0);
        photoIcon.anchoredPosition = new Vector2(-panelPhotoOrVideo.transform.parent.GetComponent<RectTransform>().sizeDelta.y / 4, 0);
        videoIcon.anchoredPosition = new Vector2(panelPhotoOrVideo.transform.parent.GetComponent<RectTransform>().sizeDelta.y / 4, 0);
        videoIcon.sizeDelta = new Vector2(panelPhotoOrVideo.transform.parent.GetComponent<RectTransform>().sizeDelta.y / 4, 0);
        scrollViewParameters.sizeDelta = new Vector2(0, 3 * panelPhotoOrVideo.transform.parent.GetComponent<RectTransform>().sizeDelta.y / 4);

        //, scrollViewPanel, total number of paremters by its size, on here, make it the size;
        /*
        for (int i = 0; i < photoParametersInputs.Length; i++)
        {
            photoParametersInputs[i].sizeDelta = new Vector2(panelPhotoOrVideo.transform.parent.GetComponent<RectTransform>().sizeDelta.y / 2, panelPhotoOrVideo.transform.parent.GetComponent<RectTransform>().sizeDelta.y / 6);
            photoParametersInputs[i].anchoredPosition = new Vector2(4 * pointsPanel.sizeDelta.x / 6, (-photoParametersInputs[i].sizeDelta.y -50) * i  );
            photoParametersTexts[i].GetComponent<Text>().fontSize = Screen.width / 60;
            photoParametersTexts[i].anchoredPosition = new Vector2(2 * pointsPanel.sizeDelta.x / 8, (-photoParametersInputs[i].sizeDelta.y - 50) * i);
            if (photoParametersInputs[i].GetComponent<InputField>() != null)
            {
                photoParametersInputs[i].GetComponent<InputField>().textComponent.fontSize = Screen.width / 80;
                photoParametersInputs[i].GetComponent<InputField>().placeholder.GetComponent<Text>().fontSize = Screen.width / 80;

            }
            else if (photoParametersInputs[i].GetComponent<Dropdown>() != null)
            {
                photoParametersInputs[i].GetComponent<Dropdown>().captionText.fontSize = Screen.width / 80;
                photoParametersInputs[i].GetComponent<Dropdown>().itemText.fontSize = Screen.width / 80;
                photoParametersInputs[i].GetComponent<Dropdown>().template.sizeDelta = new Vector2(0, Screen.height / 8);
                photoParametersInputs[i].GetComponent<Dropdown>().template.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(0, Screen.height / 20);
                photoParametersInputs[i].GetComponent<Dropdown>().template.GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(0, photoParametersInputs[i].GetComponent<Dropdown>().template.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta.y);

            }
            else if (photoParametersInputs[i].GetComponent<Toggle>() != null)
            {
                photoParametersInputs[i].sizeDelta = new Vector2(panelPhotoOrVideo.transform.parent.GetComponent<RectTransform>().sizeDelta.y / 16, panelPhotoOrVideo.transform.parent.GetComponent<RectTransform>().sizeDelta.y / 16);
            }
            photoParametersInputs[i].gameObject.SetActive(false);
            photoParametersTexts[i].gameObject.SetActive(false);
        }
       
        for (int i = 0; i < videoParametersInputs.Length; i++)
        {
            videoParametersInputs[i].sizeDelta = new Vector2(panelPhotoOrVideo.transform.parent.GetComponent<RectTransform>().sizeDelta.y / 2, panelPhotoOrVideo.transform.parent.GetComponent<RectTransform>().sizeDelta.y / 6);
            videoParametersInputs[i].anchoredPosition = new Vector2(4 * pointsPanel.sizeDelta.x / 6, (-videoParametersInputs[i].sizeDelta.y - 50) * i );
            videoParametersTexts[i].GetComponent<Text>().fontSize = Screen.width / 60;
            videoParametersTexts[i].anchoredPosition = new Vector2(2 * pointsPanel.sizeDelta.x / 8, (-videoParametersInputs[i].sizeDelta.y - 50) * i);
            if (videoParametersInputs[i].GetComponent<InputField>() != null)
            {
                videoParametersInputs[i].GetComponent<InputField>().textComponent.fontSize = Screen.width / 80;
                videoParametersInputs[i].GetComponent<InputField>().placeholder.GetComponent<Text>().fontSize = Screen.width / 80;

            }
            else if(videoParametersInputs[i].GetComponent<Dropdown>() != null)
            {
                videoParametersInputs[i].GetComponent<Dropdown>().captionText.fontSize = Screen.width / 80;
                videoParametersInputs[i].GetComponent<Dropdown>().itemText.fontSize = Screen.width / 80;
                videoParametersInputs[i].GetComponent<Dropdown>().template.sizeDelta = new Vector2(0, Screen.height / 8);
                videoParametersInputs[i].GetComponent<Dropdown>().template.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(0, Screen.height / 20);
                videoParametersInputs[i].GetComponent<Dropdown>().template.GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(0, videoParametersInputs[i].GetComponent<Dropdown>().template.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta.y );

            }
            else if (videoParametersInputs[i].GetComponent<Toggle>() != null)
            {
                videoParametersInputs[i].sizeDelta = new Vector2(panelPhotoOrVideo.transform.parent.GetComponent<RectTransform>().sizeDelta.y / 16, panelPhotoOrVideo.transform.parent.GetComponent<RectTransform>().sizeDelta.y / 16);
            }


        }
        */
        checkBoxActivateCamera.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(pointsPanel.sizeDelta.x / 16, pointsPanel.sizeDelta.y / 10);
        checkBoxActivateCamera.GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(pointsPanel.sizeDelta.x / 14, pointsPanel.sizeDelta.x / 14);
        checkBoxActivateCamera.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(50, 0);
        checkBoxActivateCamera.GetChild(1).GetComponent<Text>().fontSize = Screen.width / 60;

        checkBoxActivateCamera.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, pointsPanel.sizeDelta.y /2);

        buttonActivateRecCamCanvas.anchoredPosition = new Vector2(0, pointsPanel.sizeDelta.y / 6);
        buttonActivateRecCamCanvas.sizeDelta = new Vector2(pointsPanel.sizeDelta.x / 4, pointsPanel.sizeDelta.y / 6);
        buttonActivateRecCamCanvas.GetChild(0).GetComponent<Text>().fontSize = Screen.width / 60;

        sizeScrollHeightPhoto = new Vector2(0, photoParametersInputs.Length * (photoParametersInputs[0].sizeDelta.y + 50));
        sizeScrollHeightVideo = new Vector2(0, videoParametersInputs.Length * (videoParametersInputs[0].sizeDelta.y + 50));

        scrollViewParameters.GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = sizeScrollHeightVideo;
        POIPanel.anchoredPosition = new Vector2(Screen.width / 2.75f, Screen.height / 25.0f);
        POIPanel.sizeDelta = new Vector2(Screen.width / 6.0f, Screen.height / 20.0f);
        POIPanel.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(POIPanel.sizeDelta.x / 4, 0);
        POIPanel.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(POIPanel.sizeDelta.x / 15, 0);
        POIPanel.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(POIPanel.sizeDelta.x / 4, 0);
        POIPanel.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(POIPanel.sizeDelta.x / 4, 0);

        saveButton.anchoredPosition = new Vector2(-Screen.height / 12, -Screen.height / 20);
        saveButton.sizeDelta = new Vector2(Screen.height / 15, Screen.height / 15);

        saveButton.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -Screen.height / 18);
        saveButton.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 10, Screen.height / 20);
        saveButton.GetChild(0).GetComponent<Text>().fontSize = (int)(Screen.width * 0.02f);

        viewFrames.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, Screen.height / 8);
        viewFrames.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.width / 2, Screen.height / 8);
        viewFrames.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 2, Screen.height * 3 / 4);
        viewFrames.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 2, Screen.height * 3 / 4);

    }
    /*public void changeToVideoMode()
    {
        scrollViewParameters.GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = sizeScrollHeightVideo;
        for (int i = 0; i < photoParametersInputs.Length; i++)
        {
            photoParametersInputs[i].gameObject.SetActive(false);
            photoParametersTexts[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < videoParametersInputs.Length; i++)
        {
            videoParametersInputs[i].gameObject.SetActive(true);
            videoParametersTexts[i].gameObject.SetActive(true);
        }
    }
    public void changeToPhotoMode()
    {
        scrollViewParameters.GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = sizeScrollHeightPhoto;
        for (int i = 0; i < photoParametersInputs.Length; i++)
        {
            photoParametersInputs[i].gameObject.SetActive(true);
            photoParametersTexts[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < videoParametersInputs.Length; i++)
        {
            videoParametersInputs[i].gameObject.SetActive(false);
            videoParametersTexts[i].gameObject.SetActive(false);
        }

    }*/

    /// <summary>
    /// Activates the panel that lets the user create and delete waypoints
    /// </summary>
    public void ActivatePath()
    {
        Color col = editPathButton.GetChild(0).GetComponent<Button>().image.color;
        //Every button on the panel is deselected except for the button that belongs to the panel where the app is
        editPathButton.GetChild(0).GetComponent<Button>().image.color = new Color(col.r, col.g, col.b, 1);
        editWaypointButton.GetChild(0).GetComponent<Button>().image.color = new Color(col.r, col.g, col.b, 0.5f);
        POIButton.GetChild(0).GetComponent<Button>().image.color = new Color(col.r, col.g, col.b, 0.5f);
        previewButton.GetChild(0).GetComponent<Button>().image.color = new Color(col.r, col.g, col.b, 0.5f);
        //Necessary activations and deactivations
        if (EditCurvePanel.activeSelf)
        {
            EditCurvePanel.GetComponent<CurveEditor>().DeActivate();
        }
        if (pointsPanel.gameObject.activeSelf)
        {
            pointsPanel.gameObject.GetComponent<ChangePointType>().Deactivate();
        }
        if (POIEditor.activeSelf)
        {
            POIEditor.GetComponent<POIEditor>().Deactivate();
        }
        if (PreviewPanel.activeSelf)
        {
            PreviewPanel.GetComponent<VirtualTravel>().DeActivate();
            //GameObject pointSelected = rect.gameObject.GetComponent<PointClicker>().Deactivate();
            pointsPanel.gameObject.GetComponent<ChangePointType>().Activate(null, 1);

        }
        pointsPanel.gameObject.SetActive(false);
        POIEditor.SetActive(false);
        POIPanelButtons.SetActive(false);
        PreviewPanel.SetActive(false);
        EditCurvePanel.SetActive(false);

        rect.gameObject.SetActive(true);
        frontCam.SetActive(true);
        gameObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        TopCam.GetComponent<Camera>().rect = new Rect(0, 0.125f, 0.5f, 0.75f);

    }
    /// <summary>
    /// Same as above for the edit points info panel
    /// </summary>
    public void ActivatePoints()
    {
        Color col= editPathButton.GetChild(0).GetComponent<Button>().image.color;
        editPathButton.GetChild(0).GetComponent<Button>().image.color = new Color(col.r, col.g,col.b, 0.5f);
        editWaypointButton.GetChild(0).GetComponent<Button>().image.color = new Color(col.r, col.g, col.b, 1f);
        POIButton.GetChild(0).GetComponent<Button>().image.color = new Color(col.r, col.g, col.b, 0.5f);
        previewButton.GetChild(0).GetComponent<Button>().image.color = new Color(col.r, col.g, col.b, 0.5f);

        if (EditCurvePanel.activeSelf)
        {
            EditCurvePanel.GetComponent<CurveEditor>().DeActivate();
        }
        if (rect.gameObject.activeSelf)
        {
            GameObject pointselected = rect.gameObject.GetComponent<PointClicker>().Deactivate();
            pointsPanel.gameObject.GetComponent<ChangePointType>().Activate(pointselected, 0);
        }
        if (POIEditor.activeSelf)
        {

            POIEditor.GetComponent<POIEditor>().Deactivate();
            GameObject pointselected = rect.gameObject.GetComponent<PointClicker>().Deactivate();
            pointsPanel.gameObject.GetComponent<ChangePointType>().Activate(pointselected, 0);
        }
        else if (PreviewPanel.activeSelf)
        {
            PreviewPanel.GetComponent<VirtualTravel>().DeActivate();

        }
        POIPanelButtons.SetActive(false);
        rect.gameObject.SetActive(false);
        frontCam.SetActive(false);
        eraseButton.gameObject.SetActive(false);
        POIEditor.SetActive(false);
        PreviewPanel.SetActive(false);
        EditCurvePanel.SetActive(false);

        pointsPanel.gameObject.SetActive(true);
        gameObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        gameObject.GetComponent<Canvas>().worldCamera = Camera.main;
        TopCam.GetComponent<Camera>().rect = new Rect(0, 0.125f, 0.5f, 0.75f);

    }
   /// <summary>
   /// Same as above for the panel that creates POI
   /// </summary>
    public void ActivatePoi()
    {
        Color col = editPathButton.GetChild(0).GetComponent<Button>().image.color;
        editPathButton.GetChild(0).GetComponent<Button>().image.color = new Color(col.r, col.g, col.b, 0.5f);
        editWaypointButton.GetChild(0).GetComponent<Button>().image.color = new Color(col.r, col.g, col.b, 0.5f);
        POIButton.GetChild(0).GetComponent<Button>().image.color = new Color(col.r, col.g, col.b, 1f);
        previewButton.GetChild(0).GetComponent<Button>().image.color = new Color(col.r, col.g, col.b, 0.5f);
        if (POIEditor.activeSelf)
        {
            return;
        }
        if (EditCurvePanel.activeSelf)
        {
            EditCurvePanel.GetComponent<CurveEditor>().DeActivate();
        }
        if (rect.gameObject.activeSelf)
        {
           // GameObject pointSelected = rect.gameObject.GetComponent<PointClicker>().Deactivate();
            pointsPanel.gameObject.GetComponent<ChangePointType>().Activate(null, 1);
            pointsPanel.gameObject.GetComponent<ChangePointType>().Deactivate();
            
        }
        else if (pointsPanel.gameObject.activeSelf)
        {
            pointsPanel.gameObject.GetComponent<ChangePointType>().Deactivate();
        }
        else if (PreviewPanel.activeSelf)
        {
            PreviewPanel.GetComponent<VirtualTravel>().DeActivate();

        }
        else if (POIEditor.activeSelf)
        {
            PreviewPanel.GetComponent<VirtualTravel>().DeActivate();

        }
        rect.gameObject.SetActive(false);
        eraseButton.gameObject.SetActive(false);
        pointsPanel.gameObject.SetActive(false);
        PreviewPanel.SetActive(false);
        EditCurvePanel.SetActive(false);

        POIEditor.SetActive(true);
        gameObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        frontCam.SetActive(true);
        POIPanelButtons.SetActive(true);
        POIEditor.GetComponent<POIEditor>().Activate();
        TopCam.GetComponent<Camera>().rect = new Rect(0, 0.125f, 0.5f, 0.75f);

    }

    public void ActivateCurve()
    {
       //if (rect.gameObject.activeSelf)
       //{
       //    rect.gameObject.GetComponent<PointClicker>().Deactivate();
       //}
       //else if (POIEditor.activeSelf)
       //{
       //    POIEditor.GetComponent<POIEditor>().Deactivate();
       //}
       //else if (PreviewPanel.activeSelf)
       //{
       //    PreviewPanel.GetComponent<VirtualTravel>().DeActivate();
       //
       //}
       //rect.gameObject.SetActive(false);
       //frontCam.SetActive(false);
       //POIPanelButtons.SetActive(false);
       //eraseButton.gameObject.SetActive(false);
       //pointsPanel.gameObject.SetActive(false);
       //POIEditor.SetActive(false);
       //PreviewPanel.SetActive(false);
       //TopCam.GetComponent<Camera>().rect = new Rect(0.05f, 0.38f, 0.15f, 0.34f) ;
       //
       //EditCurvePanel.SetActive(true);
       //EditCurvePanel.GetComponent<CurveEditor>().Activate();
       //
       //gameObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
       //gameObject.GetComponent<Canvas>().worldCamera = Camera.main;
    }
    /// <summary>
    /// Same for the panel that activates the preview
    /// </summary>
    public void ActivatePreview()
    {
        Color col = editPathButton.GetChild(0).GetComponent<Button>().image.color;
        editPathButton.GetChild(0).GetComponent<Button>().image.color = new Color(col.r, col.g, col.b, 0.5f);
        editWaypointButton.GetChild(0).GetComponent<Button>().image.color = new Color(col.r, col.g, col.b, 0.5f);
        POIButton.GetChild(0).GetComponent<Button>().image.color = new Color(col.r, col.g, col.b, 0.5f);
        previewButton.GetChild(0).GetComponent<Button>().image.color = new Color(col.r, col.g, col.b, 1f);
        if (EditCurvePanel.activeSelf)
        {
            EditCurvePanel.GetComponent<CurveEditor>().DeActivate();
        }
        if (rect.gameObject.activeSelf)
        {
            rect.gameObject.GetComponent<PointClicker>().Deactivate();
        }
        if (POIEditor.activeSelf)
        {
            POIEditor.GetComponent<POIEditor>().Deactivate();
        }
        rect.gameObject.SetActive(false);
        frontCam.SetActive(false);
        POIPanelButtons.SetActive(false);
        eraseButton.gameObject.SetActive(false);
        pointsPanel.gameObject.SetActive(false);
        POIEditor.SetActive(false);
        EditCurvePanel.SetActive(false);
        TopCam.GetComponent<Camera>().rect = new Rect(0, 0.125f, 0.5f, 0.75f);

        PreviewPanel.SetActive(true);
        gameObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        PreviewPanel.GetComponent<VirtualTravel>().Activate();
    }

    void Update()
    {
        OriginAxis.transform.localScale = new Vector3(scaleFactor * TopCam.GetComponent<Camera>().orthographicSize, scaleFactor * TopCam.GetComponent<Camera>().orthographicSize, scaleFactor * TopCam.GetComponent<Camera>().orthographicSize);
    }
    /// <summary>
    /// This function is called by the button with a floppy disk in planning
    /// </summary>
    public void SaveButton()
    {
        SavePath();
        StartCoroutine(ShowSavedText());
    }
    /// <summary>
    /// This saves the Mission into a mission.json and updates the metadata date
    /// </summary>
    public static void SavePath()
    {
       


        string metadataJson = File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.metadata");
        string mapJson = File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.mission");
        MapMetadata metadata = JsonUtility.FromJson<MapMetadata>(metadataJson);
        Map map = JsonUtility.FromJson<Map>(mapJson);
        // ## TODO: Cambiar esto al botón de guardar el camino
        // Application.persistentDataPath + "/PersistentData/Missions/" + MapMetadata.Name + ".json"
        metadata.Date = new MapMetadata.AirtDateTime((byte)DateTime.Now.Day, (byte)DateTime.Now.Month, DateTime.Now.Year, (byte)DateTime.Now.Second, (byte)DateTime.Now.Minute, (byte)DateTime.Now.Hour);
        //If the plan was new, enter here
        if (MissionManager.planIndex == map.Paths.Count)
        {
            Path path = Path.Instance;
            //This adds the gimbal and reccam parameters to the metadata
            changeParameters(path);
            path.Path_metadata.Name = MissionManager.planName;
            path.Path_metadata.Author = MissionManager.planAuthor;
            path.Path_metadata.Notas = MissionManager.planNotes;
            path.FlightParams.height = MissionManager.planDefaultHeight;
            path.FlightParams.speed = MissionManager.planDefaultSpeed;
            path.FlightParams.duration = MissionManager.planDefaultDuration;
            map.AddPath(path);
        }
        //If it is a previously created plan, not newn enter here
        else
        {
            Path path = map.Paths[MissionManager.planIndex];
            Path actualPath = Path.Instance;
            //To modify a path, because it's static, these steps are necessary
            path.setPath(actualPath);
            path.clearRc();
            changeParameters(path);

            path.Path_metadata.Name = MissionManager.planName;
            path.Path_metadata.Author = MissionManager.planAuthor;
            path.Path_metadata.Notas = MissionManager.planNotes;
            map.Paths[MissionManager.planIndex].setPath(path);
        }
        mapJson = JsonUtility.ToJson(map);
        metadataJson = JsonUtility.ToJson(metadata);

        File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + metadata.Guid + ".json.metadata", metadataJson);
        File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + map.Guid + ".json.mission", mapJson);
    }

    IEnumerator ShowSavedText()
    {
        saveButton.GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        saveButton.GetChild(0).gameObject.SetActive(false);
    }
    /// <summary>
    /// waits for the dae to be loaded before deactivating the loading model screen
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForModel() {

        while (GameObject.Find("DaeModel") == null)
        {
            modelLoadingPanel.transform.GetChild(1).Rotate(0, 0, -200 * Time.deltaTime);
            yield return null;
        }
        modelLoadingPanel.SetActive(false);

    }
    /// <summary>
    /// Adds the reccam and gimbal parameters to the mission file
    /// </summary>
    /// <param name="path"></param>
    static void changeParameters(Path path)
    {
        int lastIndexRecCam = -1;
        int lastIndexGb = 0;

        for (int i = 0; i < path.Count(); i++)
        {
            //path.GetPoint(i).PointPosition = new Vector3(path.GetPoint(i).PointPosition.x, path.GetPoint(i).PointPosition.z, path.GetPoint(i).PointPosition.y);
            //The first waypoint always needs to have a gimbal parameter
            if (i == 0)
            {
                if (path.NeedsToUpdateGbParameters(0) == -1)
                {
                    GimballParameters gb = new GimballParameters();
                    gb.id_pointer = 0;
                    gb.mode = 1;
                    path.addNewParamGbAtBeggining(gb);
                }
            }
            //First point
            RecCamParameters rc = path.GetPoint(i).PointTransf.gameObject.GetComponent<PathPoint>().Rc;

            if (lastIndexRecCam == -1 && rc.edited)
            {

                rc.id_pointer = i;
                if (!rc.active)
                {
                    continue;
                }
                if (rc.switchToRec == 1)
                {
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SWITCH_TO_REC }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SWITCH_TO_REC);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_MOVIE_FORMAT, rc.resolution }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_MOVIE_FORMAT);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.resolution);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_WB, rc.autoManualWB }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_WB);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.autoManualWB);
                    if (rc.autoManualWB == 254)
                    {
                        rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_MANUAL_WB_TINT, rc.WBTint }));
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_MANUAL_WB_TINT);
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.WBTint);
                    }
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_ISO, rc.ISO }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_ISO);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.ISO);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_SHARPNESS, rc.sharpness }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_SHARPNESS);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.sharpness);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_CONTRAST, rc.contrast[0], rc.contrast[1], rc.contrast[2], rc.contrast[3] }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_CONTRAST);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.contrast[1]);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.contrast[0]);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_AE_METER_MODE, rc.AE }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_AE_METER_MODE);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.AE);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_SATURATION, rc.saturation[0], rc.saturation[1], rc.saturation[2], rc.saturation[3] }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_SATURATION);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.saturation[1]);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.saturation[0]);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_BRIGHTNESS, rc.brightness[0], rc.brightness[1], rc.brightness[2], rc.brightness[3] }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_BRIGHTNESS);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.brightness[1]);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.brightness[0]);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_ROTATION, rc.upsideDown }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_ROTATION);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.upsideDown);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_IRIS, rc.irisAperture }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_IRIS);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.irisAperture);



                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_START_REC }));

                    //rc.reccam_parameters[rc.reccam_parameters.Count-1].Add((byte)RCamCommandType.RCAM_START_REC);

                }
                if (rc.switchToRec == 0)
                {
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SWITCH_TO_STILL }));
                    //rc.reccam_parameters[0].Add((byte)RCamCommandType.RCAM_SWITCH_TO_STILL);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_SIZE, rc.megaPixels }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_SIZE);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.megaPixels);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_WB, rc.autoManualWB }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_WB);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.autoManualWB);
                    if (rc.autoManualWB == 254)
                    {
                        rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_MANUAL_WB_TINT, rc.WBTint }));
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_MANUAL_WB_TINT);
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.WBTint);
                    }
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_ISO, rc.ISO }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_ISO);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.ISO);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_SHARPNESS, rc.sharpness }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_SHARPNESS);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.sharpness);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_CONTRAST, rc.contrast[0], rc.contrast[1], rc.contrast[2], rc.contrast[3] }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_CONTRAST);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.contrast[1]);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.contrast[0]);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_AE_METER_MODE, rc.AE }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_AE_METER_MODE);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.AE);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_SATURATION, rc.saturation[0], rc.saturation[1], rc.saturation[2], rc.saturation[3] }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_SATURATION);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.saturation[1]);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.saturation[0]);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_QUALITY, rc.photoQuality }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_QUALITY);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.photoQuality);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_BRIGHTNESS, rc.brightness[0], rc.brightness[1], rc.brightness[2], rc.brightness[3] }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_BRIGHTNESS);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.brightness[1]);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.brightness[0]);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_ROTATION, rc.upsideDown }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_ROTATION);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.upsideDown);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_IRIS, rc.irisAperture }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_IRIS);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.irisAperture);
                    if (rc.burstMode == 0)
                    {
                        if (rc.AF == 0)
                        {
                            rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_CAPTURE }));

                        }
                        else
                        {
                            rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_CAPTURE_AF }));

                        }
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_CAPTURE);

                    }
                    else
                    {
                        rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_BURST, rc.burstMode }));

                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_BURST);
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.burstMode);
                        rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_BURST_SPEED, rc.burstSpeed }));

                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_BURST_SPEED);
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.burstSpeed);
                        rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_BURST_CAPTURE_START }));

                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_BURST_CAPTURE_START);

                    }
                }

                path.addNewParamRcAtBeggining(rc);
                lastIndexRecCam = i;
            }

            //Rest of points
            else if (rc.edited)
            {
               
                RecCamParameters lastRecCam = path.GetPoint(lastIndexRecCam).PointTransf.gameObject.GetComponent<PathPoint>().Rc;
                if (!rc.active && !lastRecCam.active)
                {
                    continue;
                }
                rc.id_pointer = i;
                if (rc.switchToRec == 1)
                {
                    if (lastRecCam.switchToRec == 0)
                    {
                        if (lastRecCam.burstMode == 1)
                        {

                            rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_BURST_CAPTURE_CANCEL }));
                            //rc.reccam_parameters[0].Add((byte)RCamCommandType.RCAM_BURST_CAPTURE_CANCEL);
                            if (rc.active == false)
                            {
                                path.addNewParamRc(rc);
                                lastIndexRecCam = i;
                                continue;
                            }
                        }
                        
                    }
                    


                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SWITCH_TO_REC }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SWITCH_TO_REC);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_MOVIE_FORMAT, rc.resolution }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_MOVIE_FORMAT);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.resolution);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_WB, rc.autoManualWB }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_WB);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.autoManualWB);
                    if (rc.autoManualWB == 254)
                    {
                        rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_MANUAL_WB_TINT, rc.WBTint }));
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_MANUAL_WB_TINT);
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.WBTint);
                    }
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_ISO, rc.ISO }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_ISO);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.ISO);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_SHARPNESS, rc.sharpness }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_SHARPNESS);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.sharpness);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_CONTRAST, rc.contrast[0], rc.contrast[1], rc.contrast[2], rc.contrast[3] }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_CONTRAST);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.contrast[1]);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.contrast[0]);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_AE_METER_MODE, rc.AE }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_AE_METER_MODE);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.AE);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_SATURATION, rc.saturation[0], rc.saturation[1], rc.saturation[2], rc.saturation[3] }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_SATURATION);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.saturation[1]);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.saturation[0]);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_BRIGHTNESS, rc.brightness[0], rc.brightness[1], rc.brightness[2], rc.brightness[3] }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_BRIGHTNESS);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.brightness[1]);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.brightness[0]);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_ROTATION, rc.upsideDown }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_ROTATION);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.upsideDown);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_IRIS, rc.irisAperture }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_IRIS);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.irisAperture);
                    /////////////////////////////////////////////////////////////////
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_START_REC }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_START_REC);

                }

                else if (rc.switchToRec == 0)
                {
                    if (lastRecCam.switchToRec == 1)
                    {

                        rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_STOP_REC }));
                        if (rc.active == false)
                        {
                            path.addNewParamRc(rc);
                            lastIndexRecCam = i;
                            continue;
                        }
                    }
                    else
                    {
                        if (lastRecCam.burstMode == 1 && rc.burstMode == 0)
                        {

                            rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_BURST_CAPTURE_CANCEL }));
                            //rc.reccam_parameters[0].Add((byte)RCamCommandType.RCAM_BURST_CAPTURE_CANCEL);
                            if (rc.active == false)
                            {
                                path.addNewParamRc(rc);
                                lastIndexRecCam = i;
                                continue;
                            }
                        }
                    }


                    //rc.reccam_parameters[0].Add((byte)RCamCommandType.RCAM_STOP_REC);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SWITCH_TO_STILL }));
                    //rc.reccam_parameters[0].Add((byte)RCamCommandType.RCAM_SWITCH_TO_STILL);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_SIZE, rc.megaPixels }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_SIZE);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.megaPixels);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_WB, rc.autoManualWB }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_WB);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.autoManualWB);
                    if (rc.autoManualWB == 254)
                    {
                        rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_MANUAL_WB_TINT, rc.WBTint }));
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_MANUAL_WB_TINT);
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.WBTint);
                    }
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_ISO, rc.ISO }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_ISO);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.ISO);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_SHARPNESS, rc.sharpness }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_SHARPNESS);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.sharpness);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_CONTRAST, rc.contrast[0], rc.contrast[1], rc.contrast[2], rc.contrast[3] }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_CONTRAST);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.contrast[1]);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.contrast[0]);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_AE_METER_MODE, rc.AE }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_AE_METER_MODE);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.AE);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_SATURATION, rc.saturation[0], rc.saturation[1], rc.saturation[2], rc.saturation[3] }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_SATURATION);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.saturation[1]);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.saturation[0]);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_QUALITY, rc.photoQuality }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_QUALITY);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.photoQuality);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_BRIGHTNESS, rc.brightness[0], rc.brightness[1], rc.brightness[2], rc.brightness[3] }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_BRIGHTNESS);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.brightness[1]);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.brightness[0]);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_ROTATION, rc.upsideDown }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_ROTATION);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.upsideDown);
                    rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_IRIS, rc.irisAperture }));
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_IRIS);
                    //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.irisAperture);
                    if (rc.burstMode == 0)
                    {
                        if (rc.AF == 0)
                        {
                            rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_CAPTURE }));

                        }
                        else
                        {
                            rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_CAPTURE_AF }));

                        }

                    }
                    else
                    {
                        rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_BURST, rc.burstMode }));

                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_BURST);
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.burstMode);
                        rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_BURST_SPEED, rc.burstSpeed }));

                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamCommandType.RCAM_SET_CONFIG);
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add((byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_BURST_SPEED);
                        //rc.reccam_parameters[rc.reccam_parameters.Count - 1].Add(rc.burstSpeed);
                        rc.reccam_parameters.Add(new ByteArray(new byte[] { (byte)RCamCommandType.RCAM_BURST_CAPTURE_START }));


                    }

                }


                path.addNewParamRc(rc);
                lastIndexRecCam = i;

            }

        }
    }


}
