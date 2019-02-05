using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AnchorsCalibration : MonoBehaviour {

    public enum CalibrationState
    {
        IDLE = 0,
        DISCOVERING,
        RECEIVING_DISCOVER_ANCHORS,
        RECEIVED_DISCOVER_ANCHORS,
        DISCOVERED,
        MANUAL_ACCEPTED,
        AUTOCALIBRATION_RECEIVED,
        AUTOCALIBRATION_ACCEPTED,
        AUTOCALIBRATION_FINNISHED,
        CALIBRATION_PREVIEW,
        TRY_START_POSITIONING,
        POSITIONING_CONFIRMED,
        LAST_REQ_FAILED
    }

    public static CalibrationState state = CalibrationState.IDLE;

    public Camera cam;

    public GameObject calibrationTypePanel, discoverPanel;
    public GameObject anchorsView, discoverButton, configAnchorsButton, acceptConfigButton, startingLabel, searchingLabel, anchorTextList;
    public Button confirmManual, editConfig, redoAutocalibration, confirmAuto, endEditConfig;
    public GameObject drone, anchorParent, configScene, cubeClick;

    public GameObject autocalibratingThrobber;
    public RectTransform anchor1ID, anchor2ID, anchor3ID, anchor4ID, anchor5ID, anchor6ID, anchor7ID, anchor8ID;
    public GameObject anchor1GO, anchor2GO, anchor3GO, anchor4GO, anchor5GO, anchor6GO, anchor7GO, anchor8GO;

    public GameObject anchorList, anchorCubeParent;
    public Text dicoveredAnchors;

    Vector2 anchor1Pos, anchor2Pos, anchor3Pos, anchor4Pos, anchor5Pos, anchor6Pos, anchor7Pos, anchor8Pos;

    ClientUnity clientUnity;

    public static bool autoCalib = true;

    int anchorID = 0;
    int anchorReadCount = 0;
    bool firstPositionTry;

	// Use this for initialization
	void Start () {
        //Boolean to put the text waiting for positioning
        firstPositionTry = true;
        //State machine to know at which scene we are
        GeneralSceneManager.sceneState = GeneralSceneManager.SceneState.Calibration;

        anchorCubeParent.transform.GetChild(0).GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");
        //Find the clientunity that is in every scene and comunicates with the drone
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        //Initialize transformation matrix
        Matrix4x4 id = Matrix4x4.identity;
        id.m00 = id.m11 = id.m22 = 1000;
        MissionManager.invMatrix = Matrix4x4.Inverse(id);
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        switch (state)
        {
            case CalibrationState.IDLE:
                //The spinning wheel(throbber) rotates
                if(autocalibratingThrobber.activeInHierarchy)
                    autocalibratingThrobber.transform.Rotate(0, 0, -200 * Time.deltaTime);
                break;
            case CalibrationState.DISCOVERING:
                searchingLabel.transform.GetChild(0).Rotate(0, 0, -200 * Time.deltaTime);
                break;
            case CalibrationState.RECEIVING_DISCOVER_ANCHORS:

                List<int> anchorsIDList = PozyxModule.GetAnchorList();
                {
                    //UnityEngine.Debug.Log("ReadingAnchorList");
                    dicoveredAnchors.text = "";
                    for (int i = 0; i < anchorsIDList.Count; i++)
                    {
                        //Writing the anchor list received inside the TextBoxes that can be clicked, with the HorizontalGroup name
                        //UnityEngine.Debug.Log("Reading Anchor List Element: " + i);
                        //anchorParent.transform.Find("Cube" + anchorID).position = anchorData.position;
                        //anchor1Pos = cam.WorldToScreenPoint(anchorData.position);
                        //transform.Find("Cube" + anchorID + "ID").GetComponent<RectTransform>().anchoredPosition = new Vector2(anchor1Pos.x - 40, anchor1Pos.y - Screen.height * 0.15f);
                        dicoveredAnchors.text += "Anchor: " + i + " ID: " + "0x" + anchorsIDList[i].ToString("x") + "\n";
                        anchorList.transform.Find("HorizontalGroup" + (i + 1)).GetChild(0).GetChild(0).GetComponent<Text>().text = "0x" + anchorsIDList[i].ToString("x");
                        anchorList.transform.Find("HorizontalGroup" + (i + 1)).GetChild(0).name = anchorsIDList[i].ToString();                
               
                    }

                    //UnityEngine.Debug.Log("FinishedReadingAnchorList");
                    //Activate the button that lets the user give the correct position to the anchors
                    configAnchorsButton.GetComponent<Button>().interactable = true;
                    configAnchorsButton.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1, 1, 1, 1);
                    //UnityEngine.Debug.Log("All anchors positioned");
                    searchingLabel.SetActive(false);
                    state = CalibrationState.RECEIVED_DISCOVER_ANCHORS;
            
                }
                break;
            case CalibrationState.RECEIVED_DISCOVER_ANCHORS:
                //Having done previously that, is this state necessary?
                if (PozyxModule.anchorsReceived)
                {
                    PozyxModule.anchorsReceived = false;
                    state = CalibrationState.DISCOVERED;
                }
                break;
            case CalibrationState.DISCOVERED:

                anchorReadCount = 0;
                for (int i = 0; i < 8; i++)
                {
                    ServerMessages.IPSFrameAnchorData anchorData = CalibrationSettings.GetAnchorData(i);
                    //anchorList.transform.Find("HorizontalGroup" + (anchorData.order + 1)).GetChild(0).GetChild(0).GetComponent<Text>().text = anchorData.id.ToString();
                    //anchorList.transform.Find("HorizontalGroup" + (anchorData.order + 1)).GetChild(0).name = anchorData.id.ToString();
                
                    //Writing the corresponding id of an anchor to a cube
                    anchorCubeParent.transform.Find("Anchor" + (anchorData.order + 1) + "Cube").GetChild(0).GetComponent<TextMesh>().text = "0x" + anchorData.id.ToString("x");
                    anchorCubeParent.transform.Find("Anchor" + (anchorData.order + 1) + "Cube").GetChild(1).localPosition = anchorData.position;
                    for (int j = 1; j < 9; j++)
                    {
                        //UnityEngine.Debug.Log(anchorData.order);
                        //Writing the received position of the anchors
                        if (anchorList.transform.Find("HorizontalGroup" + j).GetChild(0).name == anchorData.id.ToString()) {
                            anchorList.transform.Find("HorizontalGroup" + j).GetChild(1).GetComponent<InputField>().text = anchorData.position.x.ToString();
                            anchorList.transform.Find("HorizontalGroup" + j).GetChild(2).GetComponent<InputField>().text = anchorData.position.y.ToString();
                            anchorList.transform.Find("HorizontalGroup" + j).GetChild(3).GetComponent<InputField>().text = anchorData.position.z.ToString();
                            anchorList.transform.Find("HorizontalGroup" + j).GetChild(0).GetComponent<Image>().color = new Vector4(0, 255, 0, 150);
                            anchorReadCount++;
                            //Writing the received position of the anchor list when autocalibration is pressed
                            if (autoCalib && (anchorData.order == 1 || anchorData.order == 5))
                            {
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(1).GetComponent<InputField>().text = "";
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(1).GetComponent<InputField>().placeholder.GetComponent<Text>().text = "AUTO";
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(1).GetComponent<InputField>().interactable = false;
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(2).GetComponent<InputField>().text = "0";
                            }
                            else if (autoCalib && (anchorData.order == 3 || anchorData.order == 7))
                            {
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(1).GetComponent<InputField>().text = "0";
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(2).GetComponent<InputField>().text = "";
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(2).GetComponent<InputField>().placeholder.GetComponent<Text>().text = "AUTO";
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(2).GetComponent<InputField>().interactable = false;
                            }
                            else if (autoCalib && (anchorData.order == 2 || anchorData.order == 6))
                            {
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(1).GetComponent<InputField>().text = "";
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(1).GetComponent<InputField>().placeholder.GetComponent<Text>().text = "AUTO";
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(1).GetComponent<InputField>().interactable = false;
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(2).GetComponent<InputField>().text = "";
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(2).GetComponent<InputField>().placeholder.GetComponent<Text>().text = "AUTO";
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(2).GetComponent<InputField>().interactable = false;
                            }
                            else if(autoCalib)
                            {
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(1).GetComponent<InputField>().text = "0";
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(2).GetComponent<InputField>().text = "0";
                            }
                        }
                    
                    }
                }

                //UnityEngine.Debug.Log("Anclas coincidentes: " + anchorReadCount);
                //Activates button to confirm configuration
                if (anchorReadCount == 8) {
                    UnityEngine.Debug.Log("PozyxModuleAnchorsReady");
                    confirmManual.interactable = true;
                    confirmManual.transform.GetChild(0).GetComponent<Text>().color = new Vector4(255, 255, 255, 255);
                }

                state = CalibrationState.IDLE;

                //anchorParent.transform.Find("Cube" + anchorID).position = anchorData.position;
                //anchor1Pos = cam.WorldToScreenPoint(anchorData.position);
                //transform.Find("Cube" + anchorID + "ID").GetComponent<RectTransform>().anchoredPosition = new Vector2(anchor1Pos.x - 40, anchor1Pos.y - Screen.height * 0.15f);
                //dicoveredAnchors.text += "Tag: " + anchorID + " ID: " + anchorData.id + "\n";
                //anchorList.transform.Find("Anchor" + anchorID).GetChild(0).GetComponent<Text>().text = "Anchor ID: " + anchorData.id;
                //anchorList.transform.Find("Anchor" + anchorID).name = anchorData.id.ToString();

                //configAnchorsButton.GetComponent<Button>().interactable = true;
                //configAnchorsButton.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1, 1, 1, 1);
                //UnityEngine.Debug.Log("All anchor data received and ready to be used");
                //searchingLabel.SetActive(false);
                //ipsCommandSent = false;
                break;
            case CalibrationState.MANUAL_ACCEPTED:
                //Sending the configuration and wait for positioning
                clientUnity.client.SendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_UPDATE_SETTINGS);
                state = CalibrationState.TRY_START_POSITIONING;
                break;
            case CalibrationState.AUTOCALIBRATION_RECEIVED:

                autocalibratingThrobber.transform.Rotate(0, 0, -200 * Time.deltaTime);
                anchorReadCount = 0;

                for (int i = 0; i < 8; i++)
                {
                    ServerMessages.IPSFrameAnchorData anchorData = CalibrationSettings.GetAnchorData(i);
                    //anchorList.transform.Find("HorizontalGroup" + (anchorData.order + 1)).GetChild(0).GetChild(0).GetComponent<Text>().text = anchorData.id.ToString();
                    //anchorList.transform.Find("HorizontalGroup" + (anchorData.order + 1)).GetChild(0).name = anchorData.id.ToString();


                    anchorCubeParent.transform.Find("Anchor" + (anchorData.order + 1) + "Cube").GetChild(0).GetComponent<TextMesh>().text = "0x" + anchorData.id.ToString("x");
                    anchorCubeParent.transform.Find("Anchor" + (anchorData.order + 1) + "Cube").GetChild(1).localPosition = anchorData.position;
                    //Receiving the anchors position from autocalibration
                    for (int j = 1; j < 9; j++)
                    {
                        if (anchorList.transform.Find("HorizontalGroup" + j).GetChild(0).name == anchorData.id.ToString())
                        {
                            anchorList.transform.Find("HorizontalGroup" + j).GetChild(1).GetComponent<InputField>().text = anchorData.position.x.ToString();
                            anchorList.transform.Find("HorizontalGroup" + j).GetChild(2).GetComponent<InputField>().text = anchorData.position.y.ToString();
                            anchorList.transform.Find("HorizontalGroup" + j).GetChild(3).GetComponent<InputField>().text = anchorData.position.z.ToString();
                            anchorList.transform.Find("HorizontalGroup" + j).GetChild(0).GetComponent<Button>().interactable = false;
                            anchorList.transform.Find("HorizontalGroup" + j).GetChild(1).GetComponent<InputField>().interactable = false;
                            anchorList.transform.Find("HorizontalGroup" + j).GetChild(2).GetComponent<InputField>().interactable = false;
                            anchorList.transform.Find("HorizontalGroup" + j).GetChild(3).GetComponent<InputField>().interactable = false;
                            anchorList.transform.Find("HorizontalGroup" + j).GetChild(0).GetComponent<Image>().color = new Vector4(0, 255, 0, 150);
                            anchorReadCount++;
                            //Painting orange the autocalibrated anchors
                            if (anchorData.order == 1 || anchorData.order == 2 || anchorData.order == 5 || anchorData.order == 6)
                            {
                                ColorBlock cb = anchorList.transform.Find("HorizontalGroup" + j).GetChild(1).GetComponent<InputField>().colors;
                                cb.disabledColor = new Vector4(1.0f, 0.6f, 0.0f, 0.4f);
                                cb.normalColor = new Vector4(1.0f, 0.8f, 0.4f, 1.0f);
                                cb.highlightedColor = new Vector4(1.0f, 0.9f, 0.7f, 1.0f);
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(1).GetComponent<InputField>().colors = cb;
                            }
                            if (anchorData.order == 2 || anchorData.order == 3 || anchorData.order == 6 || anchorData.order == 7)
                            {
                                ColorBlock cb = anchorList.transform.Find("HorizontalGroup" + j).GetChild(2).GetComponent<InputField>().colors;
                                cb.disabledColor = new Vector4(1.0f, 0.6f, 0.0f, 0.4f);
                                cb.normalColor = new Vector4(1.0f, 0.8f, 0.4f, 1.0f);
                                cb.highlightedColor = new Vector4(1.0f, 0.9f, 0.7f, 1.0f);
                                anchorList.transform.Find("HorizontalGroup" + j).GetChild(2).GetComponent<InputField>().colors = cb;
                            }
                        }

                    }
                }

                if (anchorReadCount == 8)
                {
                    //Habilitating the three buttons
                    //UnityEngine.Debug.Log("AnchorsAutocalibratedReady");
                    // ## Se puede sacar fuera del if todo
                    confirmManual.gameObject.SetActive(false);
                    editConfig.gameObject.SetActive(true);
                    redoAutocalibration.gameObject.SetActive(true);
                    confirmAuto.gameObject.SetActive(true);
                    editConfig.interactable = true;
                    editConfig.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    redoAutocalibration.interactable = true;
                    redoAutocalibration.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    confirmAuto.interactable = true;
                    confirmAuto.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    //acceptConfig.interactable = true;
                    //acceptConfig.transform.GetChild(0).GetComponent<Text>().color = new Vector4(255, 255, 255, 255);
                }

                state = CalibrationState.AUTOCALIBRATION_FINNISHED;

                break;
            case CalibrationState.AUTOCALIBRATION_ACCEPTED:
                autocalibratingThrobber.transform.Rotate(0, 0, -200 * Time.deltaTime);
                // Used to check the current state
                break;
            case CalibrationState.AUTOCALIBRATION_FINNISHED:
                // Used to check the current state
                autocalibratingThrobber.transform.parent.gameObject.SetActive(false);
                break;
            case CalibrationState.CALIBRATION_PREVIEW:
                //Nobody uses this state ??
                ServerMessages.IPSFrameData frameData = DroneModule.DequeueIPSDronFrame();
                if (frameData != null)
                {
                    //UnityEngine.Debug.Log("Position: " + frameData.position);
                    //UnityEngine.Debug.Log("Rotation: " + frameData.rotation);
                    drone.transform.position = frameData.position;
                    drone.transform.rotation = Quaternion.Euler(frameData.rotation);
                }

                anchor1Pos = cam.WorldToScreenPoint(anchor1GO.transform.position);
                anchor2Pos = cam.WorldToScreenPoint(anchor2GO.transform.position);
                anchor3Pos = cam.WorldToScreenPoint(anchor3GO.transform.position);
                anchor4Pos = cam.WorldToScreenPoint(anchor4GO.transform.position);
                anchor5Pos = cam.WorldToScreenPoint(anchor5GO.transform.position);
                anchor6Pos = cam.WorldToScreenPoint(anchor6GO.transform.position);
                anchor7Pos = cam.WorldToScreenPoint(anchor7GO.transform.position);
                anchor8Pos = cam.WorldToScreenPoint(anchor8GO.transform.position);

                // ## TODO: Incluir en el if de arriba buscando cada anchorID según el ID actual y asociar su posición
                anchor1ID.anchoredPosition = new Vector2(anchor1Pos.x - 40, anchor1Pos.y - Screen.height * 0.15f);
                anchor2ID.anchoredPosition = new Vector2(anchor2Pos.x - 40, anchor2Pos.y - Screen.height * 0.15f);
                anchor3ID.anchoredPosition = new Vector2(anchor3Pos.x - 40, anchor3Pos.y - Screen.height * 0.15f);
                anchor4ID.anchoredPosition = new Vector2(anchor4Pos.x - 40, anchor4Pos.y - Screen.height * 0.15f);
                anchor5ID.anchoredPosition = new Vector2(anchor5Pos.x - 40, anchor5Pos.y - Screen.height * 0.15f);
                anchor6ID.anchoredPosition = new Vector2(anchor6Pos.x - 40, anchor6Pos.y - Screen.height * 0.15f);
                anchor7ID.anchoredPosition = new Vector2(anchor7Pos.x - 40, anchor7Pos.y - Screen.height * 0.15f);
                anchor8ID.anchoredPosition = new Vector2(anchor8Pos.x - 40, anchor8Pos.y - Screen.height * 0.15f);
                break;
            case CalibrationState.TRY_START_POSITIONING:
                //Check for positioning
                if (PozyxModule.positiningIsValid){
                    firstPositionTry = true;
                    state = CalibrationState.POSITIONING_CONFIRMED;
                }
                else
                {
                    if (firstPositionTry) { 
                        autocalibratingThrobber.transform.parent.GetComponent<Text>().text = "Waiting for positioning";
                        autocalibratingThrobber.transform.parent.gameObject.SetActive(true);
                        firstPositionTry = false;
                    }
                    autocalibratingThrobber.transform.Rotate(0, 0, -200 * Time.deltaTime);
                }
                break;
            case CalibrationState.POSITIONING_CONFIRMED:
                //Change scene, positioning received
                state = CalibrationState.IDLE;
                autocalibratingThrobber.transform.parent.gameObject.SetActive(false);
                //UnityEngine.Debug.Log(GeneralSceneManager.appState);
                //You can access the scene from both buttons, as calibrating is always necessary, that is way there is a button to use last calibration and skip the process
                if (GeneralSceneManager.appState == GeneralSceneManager.ApplicationState.Mapping)
                    SceneManager.LoadScene("Mapping");
                else if (GeneralSceneManager.appState == GeneralSceneManager.ApplicationState.Recording)
                    SceneManager.LoadScene("PlanSelection");
                break;
            case CalibrationState.LAST_REQ_FAILED:
                UnityEngine.Debug.Log("LastReqFailed");
                if (discoverPanel.activeInHierarchy)
                { // Something failed discovering anchors, retry
                    UnityEngine.Debug.Log("DiscoverFailed");
                    dicoveredAnchors.text = PozyxModule.errorMsg;
                    state = CalibrationState.DISCOVERING;
                }
                else
                { // Something failed with anchors configuration, let the user change the config and try again
                    UnityEngine.Debug.Log("ConfigFailed");
                    state = CalibrationState.IDLE;
                    autocalibratingThrobber.transform.parent.gameObject.SetActive(false);
                    if (confirmManual.gameObject.activeInHierarchy)
                    {
                        confirmManual.interactable = true;
                        confirmManual.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    }
                    else if (confirmAuto.gameObject.activeInHierarchy)
                    {
                        confirmAuto.interactable = true;
                        confirmAuto.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                        editConfig.interactable = true;
                        editConfig.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                        redoAutocalibration.interactable = true;
                        redoAutocalibration.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    }
                }
                //discoverButton.SetActive(true);
                //configAnchorsButton.SetActive(false);
                //startingLabel.SetActive(true);
                //searchingLabel.SetActive(false);
                
                break;
        }		
	}

    public void DiscoverAnchors()
    {
        //Discover button pressed, send a IPS command to search for anchors
        dicoveredAnchors.text = "";
        if (state != CalibrationState.DISCOVERING && (clientUnity.client != null) && clientUnity.client.isConnected)
        {
            clientUnity.client.SendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_SYSTEM);
            state = CalibrationState.DISCOVERING;
            discoverButton.SetActive(false);
            configAnchorsButton.SetActive(true);
            startingLabel.SetActive(false);
            searchingLabel.SetActive(true);
        }
    }

    public void Autocalibrate()
    {
        anchorsView.SetActive(true);
    }

    public void AnchorsConfig()
    {
        //Config button pressed
        int fontSize = (int)(Screen.width * 0.018f);
        bool first = true;

        if(autoCalib)
            confirmManual.transform.GetChild(0).GetComponent<Text>().text = "Autocalibrate";
        else
            confirmManual.transform.GetChild(0).GetComponent<Text>().text = "Confirm\nConfiguration";

        //configAnchorsButton.SetActive(false);
        //anchorTextList.SetActive(false);
        //searchingLabel.SetActive(false);
        discoverPanel.SetActive(false);
        anchorList.SetActive(true);
        acceptConfigButton.SetActive(true);
        configScene.SetActive(true);
        cubeClick.SetActive(true);
        //Resizing the textboxes which contain the anchor's data
        foreach (Transform child in anchorList.transform)
        {
            child.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2((child.parent.GetComponent<RectTransform>().sizeDelta.x / 4) - 20, child.GetComponent<RectTransform>().sizeDelta.y - 1);

            for (int i = 1; i < 4; i++)
            {
                child.GetChild(i).GetComponent<RectTransform>().sizeDelta = new Vector2((child.parent.GetComponent<RectTransform>().sizeDelta.x / 4) - 20, child.GetComponent<RectTransform>().sizeDelta.y - 1);
                if (!first)
                {
                    child.GetChild(i).GetComponent<InputField>().placeholder.GetComponent<Text>().fontSize = fontSize;
                    child.GetChild(i).GetComponent<InputField>().textComponent.fontSize = fontSize;
                }
            }
            if(first)
                first = false;
        }

    }

    public void ManualCalibration()
    {
        autoCalib = false;
        calibrationTypePanel.SetActive(false);
        discoverPanel.SetActive(true);
    }

    public void AutoCalibration()
    {
        autoCalib = true;
        calibrationTypePanel.SetActive(false);
        discoverPanel.SetActive(true);
    }

    public void EditConfiguration()
    {
        //Edit anchor data on auto pozyx
        for (int j = 1; j < 9; j++)
        {
            anchorList.transform.Find("HorizontalGroup" + j).GetChild(0).GetComponent<Button>().interactable = true;
            anchorList.transform.Find("HorizontalGroup" + j).GetChild(1).GetComponent<InputField>().interactable = true;
            anchorList.transform.Find("HorizontalGroup" + j).GetChild(2).GetComponent<InputField>().interactable = true;
            anchorList.transform.Find("HorizontalGroup" + j).GetChild(3).GetComponent<InputField>().interactable = true;
        }

        editConfig.gameObject.SetActive(false);
        redoAutocalibration.gameObject.SetActive(false);
        confirmAuto.gameObject.SetActive(false);
        endEditConfig.gameObject.SetActive(true);
    }

    public void EndEditConfiguration()
    {
        //End the mode 
        for (int j = 1; j < 9; j++)
        {
            anchorList.transform.Find("HorizontalGroup" + j).GetChild(0).GetComponent<Button>().interactable = false;
            anchorList.transform.Find("HorizontalGroup" + j).GetChild(1).GetComponent<InputField>().interactable = false;
            anchorList.transform.Find("HorizontalGroup" + j).GetChild(2).GetComponent<InputField>().interactable = false;
            anchorList.transform.Find("HorizontalGroup" + j).GetChild(3).GetComponent<InputField>().interactable = false;
        }

        editConfig.gameObject.SetActive(true);
        redoAutocalibration.gameObject.SetActive(true);
        confirmAuto.gameObject.SetActive(true);
        endEditConfig.gameObject.SetActive(false);
    }
}
