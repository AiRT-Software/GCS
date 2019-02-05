using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UITagsConfigurationManager : MonoBehaviour, IPointerDownHandler {

    // Enum state machine for tags configuration
    public enum TagsConfigState
    {
        Idle = 0,
        SearchTagsReturn,
        SearchingTags,
        TagsFound,
        ConfiguringTags,
        ConfigurationSent,
        ConfigurationAccepted,
        ErrorReceived
    }
    bool skipClicked = false;
    public static TagsConfigState state = TagsConfigState.Idle;
    public static bool camOffsetReceived = false;
    public static bool tagInfoReceived = false;
    public static bool droneFilterReceived = false;

    public static bool camOffsetConfirmed = false;
    public static bool tagInfoConfirmed = false;

    // First panel with tags list and some labels
    public RectTransform lookingForTags, tagsThrobber, searchTags, tagsFound, tagsRect, discoverAnchorsButton, configButton, skipEverythingButton;
    // Tags list text
    public Text tagsText;
    // Input panel labels
    public RectTransform tagsConfigurationPanel, widthLabel, heightLabel, camDistLabelX, camDistLabelY, camDistLabelZ;
    public RectTransform updatePeriodLabel, movementFreedomLabel;
    // Input panel InputFields and button
    public RectTransform widthInput, heightInput, camDistInputX, camDistInputY, camDistInputZ, confirmButton, nextConfigButton, backConfigButton;
    public RectTransform updatePeriodInput, movementFreedomInput;
    // Input panel searching label and text
    public RectTransform sendingDataThrobber, sendingDataText, tagsFoundLabel;
    // Drone scene text meshes
    public TextMesh swTextMesh, nwTextMesh, neTextMesh, seTextMesh, widthTextMesh, heightTextMesh, camDistTextMesh;
    // Drone scene GO
    public GameObject droneScene, originAxis;
    public GameObject idGroupPrefab, inputParent, droneConfigParent;

    int tagsFoundCount = 0;
    bool panelInstantiated = false;
    string tagClicked = "", prevTagClicked = "";

    ClientUnity clientUnity;

    // Input fields output to be setted
    int widthValue = -1, heightValue = -1, swValue = -1, nwValue = -1, neValue = -1, seValue = -1;
    float camDistValueX = -1, camDistValueY = -1, camDistValueZ = -1;
    float updatePeriodValue = -1.0f;
    int movementFreedomValue = -1;
    Vector2 panelSize = new Vector2();

    void Awake()
    {
        GeneralSceneManager.sceneState = GeneralSceneManager.SceneState.TagsConfiguration;

        // Client for send and receive messages
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();

        // UI Settings variables
        int width = Screen.width;
        int height = Screen.height;

        // Panel to detect clicks size
        GetComponent<RectTransform>().sizeDelta = new Vector2(width  * 0.6f, height * 0.7f);

        // Tags panel settings
        panelSize = new Vector2(width * 0.3f, height * 0.5f);

        tagsConfigurationPanel.anchoredPosition = new Vector2(width * 0.6f, -height * 0.225f);
        tagsConfigurationPanel.sizeDelta = panelSize;

        // First panel (list with anchors, labels and tags settings)
        lookingForTags.anchoredPosition = new Vector2(width / 8, -height / 4);
        searchTags.anchoredPosition = new Vector2(width / 8, -height / 4);
        tagsFound.anchoredPosition = new Vector2(width / 3, -height / 6);
        tagsThrobber.anchoredPosition = new Vector2(0.0f, -height / 10f);
        tagsRect.anchoredPosition = new Vector2(0.0f, -height / 12);
        lookingForTags.sizeDelta = new Vector2(width / 6, height / 6);
        searchTags.sizeDelta = new Vector2(width / 6, height / 4);
        tagsFound.sizeDelta = new Vector2(width / 2, height / 6);
        tagsThrobber.sizeDelta = new Vector2(width / 20, width / 20);
        tagsRect.sizeDelta = new Vector2(width / 2, height / 2);
        lookingForTags.GetComponent<Text>().fontSize = (int)(width * 0.020f);
        searchTags.GetComponent<Text>().fontSize = (int)(width * 0.015f);
        tagsFound.GetComponent<Text>().fontSize = (int)(width * 0.035f);
        tagsText.GetComponent<Text>().fontSize = (int)(width * 0.02f);

        // First panel buttons settings
        discoverAnchorsButton.anchoredPosition = new Vector2(width / 8, -height / 2f);
        configButton.anchoredPosition = new Vector2(width / 8, -height / 1.5f);
        skipEverythingButton.anchoredPosition = new Vector2(width / 8, -height / 1.5f);
        discoverAnchorsButton.sizeDelta = new Vector2(width / 6, height / 12);
        skipEverythingButton.sizeDelta = new Vector2(width / 6, height / 12);

        configButton.sizeDelta = new Vector2(width / 6, height / 12);
        discoverAnchorsButton.GetChild(0).GetComponent<Text>().fontSize = (int)(width * 0.02f);
        skipEverythingButton.GetChild(0).GetComponent<Text>().fontSize = (int)(width * 0.02f);

        configButton.GetChild(0).GetComponent<Text>().fontSize = (int)(width * 0.02f);

        // Confirm config button listener
        confirmButton.GetComponent<Button>().onClick.AddListener(ConfirmTagConfiguration);
        nextConfigButton.GetComponent<Button>().onClick.AddListener(NextConfiguration);
        backConfigButton.GetComponent<Button>().onClick.AddListener(BackConfiguration);

        // Confirm button and his label/throbber size
        confirmButton.sizeDelta = new Vector2(panelSize.x * 0.4f, panelSize.y / 10);
        nextConfigButton.sizeDelta = new Vector2(panelSize.x * 0.4f, panelSize.y / 10);
        backConfigButton.sizeDelta = new Vector2(panelSize.x * 0.4f, panelSize.y / 10);
        sendingDataThrobber.sizeDelta = new Vector2(panelSize.x * 0.2f, panelSize.x * 0.2f);
        sendingDataText.sizeDelta = new Vector2(panelSize.x * 0.25f, panelSize.y / 10);

        // Confirm button and his label/throbber position
        confirmButton.anchoredPosition = new Vector2(panelSize.x * 0.55f, -panelSize.y * 1.1f);
        nextConfigButton.anchoredPosition = new Vector2(panelSize.x * 0.55f, -panelSize.y * 1.1f);
        backConfigButton.anchoredPosition = new Vector2(panelSize.x * 0.05f, -panelSize.y * 1.1f);
        sendingDataThrobber.anchoredPosition = new Vector2(panelSize.x * 0.6f, -panelSize.y * 1.1f);
        sendingDataText.anchoredPosition = new Vector2(panelSize.x * 0.85f, -panelSize.y * 1.1f);
        confirmButton.GetChild(0).GetComponent<Text>().fontSize = (int)(width * 0.02f);
        nextConfigButton.GetChild(0).GetComponent<Text>().fontSize = (int)(width * 0.02f);
        backConfigButton.GetChild(0).GetComponent<Text>().fontSize = (int)(width * 0.02f);
        sendingDataText.GetComponent<Text>().fontSize = (int)(width * 0.02f);
        



    }

    void Update()
    {

        //This if are consulted when the user presses the use last session configuration. It will enter if there is positioning and anchors.
        if (PozyxModule.positiningIsValid && skipClicked &&  CalibrationSettings.anchorConfigData[0] != null)
        {
            PozyxModule.positiningIsValid = false;
            if (GeneralSceneManager.appState == GeneralSceneManager.ApplicationState.Mapping)
                SceneManager.LoadScene("Mapping");
            else if (GeneralSceneManager.appState == GeneralSceneManager.ApplicationState.Recording)
                SceneManager.LoadScene("PlanSelection");
            return;
        }
       //If an error was received when trying to use the last session config, it will enter here
        else if (skipClicked && state == TagsConfigState.ErrorReceived)
        {
            skipEverythingButton.GetComponent<Button>().interactable = true;
            skipEverythingButton.GetChild(0).GetComponent<Text>().color = new Vector4(1, 1, 1, 1f);

            discoverAnchorsButton.GetComponent<Button>().interactable = true;
            discoverAnchorsButton.GetChild(0).GetComponent<Text>().color = new Vector4(1, 1, 1, 1f);
            lookingForTags.gameObject.SetActive(false);
            searchTags.gameObject.SetActive(true);
            state = TagsConfigState.Idle;
        }
        //Meanwhile, it will enter here to rotate the throbber
        else if(skipClicked)
        {
            tagsThrobber.transform.Rotate(0, 0, -200 * Time.deltaTime);
        }
        switch (state)
        {
            case TagsConfigState.Idle:
                // Caso para comprobación de estado
                break;
            //Not used for the moment
            case TagsConfigState.SearchTagsReturn:
                discoverAnchorsButton.GetComponent<Button>().interactable = true;
                lookingForTags.gameObject.SetActive(false);
                searchTags.gameObject.SetActive(true);
                //We leave it idle while the user configs which tag is where
                state = TagsConfigState.Idle;
                break;
            //This case happens once the user clicked on the discover button
            case TagsConfigState.SearchingTags:
                tagsThrobber.transform.Rotate(0, 0, -200 * Time.deltaTime);
                if (camOffsetReceived && tagInfoReceived && droneFilterReceived)
                    state = TagsConfigState.TagsFound;
                break;
            //Once the app gets the tag list, it enters here and displays them
            case TagsConfigState.TagsFound:
                if (!panelInstantiated) {
                    panelInstantiated = true;
                    int width = Screen.width;
                    int height = Screen.height;
                    string[] tagsFoundStr = PozyxModule.decAnchors.Split('\n');
                    tagsFoundCount = tagsFoundStr.Length - 1;
                    tagsText.text = PozyxModule.textMsg;
                    discoverAnchorsButton.gameObject.SetActive(false);
                    configButton.gameObject.SetActive(true);

                    Vector2 inputSize = new Vector2(panelSize.x * 0.3f, panelSize.y / 12);
                    Vector2 buttonSize = new Vector2(panelSize.x * 0.6f, panelSize.y / 10);
                    //All of this resizes and positions the tag selection and configurations panel
                    widthInput.GetComponent<InputField>().placeholder.GetComponent<Text>().fontSize = (int)(width * 0.018f);
                    heightInput.GetComponent<InputField>().placeholder.GetComponent<Text>().fontSize = (int)(width * 0.018f);
                    camDistInputX.GetComponent<InputField>().placeholder.GetComponent<Text>().fontSize = (int)(width * 0.018f);
                    camDistInputY.GetComponent<InputField>().placeholder.GetComponent<Text>().fontSize = (int)(width * 0.018f);
                    camDistInputZ.GetComponent<InputField>().placeholder.GetComponent<Text>().fontSize = (int)(width * 0.018f);
                    updatePeriodInput.GetComponent<InputField>().placeholder.GetComponent<Text>().fontSize = (int)(width * 0.018f);
                    movementFreedomInput.GetComponent<InputField>().placeholder.GetComponent<Text>().fontSize = (int)(width * 0.018f);

                    widthInput.GetComponent<InputField>().textComponent.GetComponent<Text>().fontSize = (int)(width * 0.018f);
                    heightInput.GetComponent<InputField>().textComponent.GetComponent<Text>().fontSize = (int)(width * 0.018f);
                    camDistInputX.GetComponent<InputField>().textComponent.GetComponent<Text>().fontSize = (int)(width * 0.018f);
                    camDistInputY.GetComponent<InputField>().textComponent.GetComponent<Text>().fontSize = (int)(width * 0.018f);
                    camDistInputZ.GetComponent<InputField>().textComponent.GetComponent<Text>().fontSize = (int)(width * 0.018f);
                    updatePeriodInput.GetComponent<InputField>().textComponent.GetComponent<Text>().fontSize = (int)(width * 0.018f);
                    movementFreedomInput.GetComponent<InputField>().textComponent.GetComponent<Text>().fontSize = (int)(width * 0.018f);

                    widthInput.GetComponent<InputField>().onValueChanged.AddListener(WidthInputText);
                    heightInput.GetComponent<InputField>().onValueChanged.AddListener(HeightInputText);
                    camDistInputX.GetComponent<InputField>().onValueChanged.AddListener(CamDistInputXText);
                    camDistInputY.GetComponent<InputField>().onValueChanged.AddListener(CamDistInputYText);
                    camDistInputZ.GetComponent<InputField>().onValueChanged.AddListener(CamDistInputZText);
                    updatePeriodInput.GetComponent<InputField>().onValueChanged.AddListener(UpdatePeriodText);
                    movementFreedomInput.GetComponent<InputField>().onValueChanged.AddListener(MovementFreedomText);

                    widthLabel.GetComponent<Text>().fontSize = (int)(width * 0.0225f);
                    heightLabel.GetComponent<Text>().fontSize = (int)(width * 0.0225f);
                    camDistLabelX.GetComponent<Text>().fontSize = (int)(width * 0.0225f);
                    camDistLabelY.GetComponent<Text>().fontSize = (int)(width * 0.0225f);
                    camDistLabelZ.GetComponent<Text>().fontSize = (int)(width * 0.0225f);
                    updatePeriodLabel.GetComponent<Text>().fontSize = (int)(width * 0.02f);
                    movementFreedomLabel.GetComponent<Text>().fontSize = (int)(width * 0.02f);

                    tagsFoundLabel.anchoredPosition = new Vector2(inputSize.x * 0.1f, inputSize.y * 1.5f);
                    tagsFoundLabel.sizeDelta = inputSize;
                    tagsFoundLabel.GetComponent<Text>().fontSize = (int)(width * 0.03f);

                    widthInput.sizeDelta = inputSize;
                    heightInput.sizeDelta = inputSize;
                    camDistInputX.sizeDelta = inputSize;
                    camDistInputY.sizeDelta = inputSize;
                    camDistInputZ.sizeDelta = inputSize;
                    updatePeriodInput.sizeDelta = inputSize;
                    movementFreedomInput.sizeDelta = inputSize;

                    widthLabel.sizeDelta = inputSize;
                    heightLabel.sizeDelta = inputSize;
                    camDistLabelX.sizeDelta = inputSize;
                    camDistLabelY.sizeDelta = inputSize;
                    camDistLabelZ.sizeDelta = inputSize;
                    updatePeriodLabel.sizeDelta = inputSize;
                    movementFreedomLabel.sizeDelta = inputSize;

                    tagsFoundCount = tagsFoundCount > 10 ? 10 : tagsFoundCount;

                    widthInput.anchoredPosition = new Vector2(panelSize.x * 0.6f, (-panelSize.y / 7) * 0 - ((panelSize.y - (inputSize.y * 7)) / (7 * 2)));
                    heightInput.anchoredPosition = new Vector2(panelSize.x * 0.6f, (-panelSize.y / 7) * 1 - ((panelSize.y - (inputSize.y * 7)) / (7 * 2)));
                    camDistInputX.anchoredPosition = new Vector2(panelSize.x * 0.6f, (-panelSize.y / 7) * 2 - ((panelSize.y - (inputSize.y * 7)) / (7 * 2)));
                    camDistInputY.anchoredPosition = new Vector2(panelSize.x * 0.6f, (-panelSize.y / 7) * 3 - ((panelSize.y - (inputSize.y * 7)) / (7 * 2)));
                    camDistInputZ.anchoredPosition = new Vector2(panelSize.x * 0.6f, (-panelSize.y / 7) * 4 - ((panelSize.y - (inputSize.y * 7)) / (7 * 2)));
                    updatePeriodInput.anchoredPosition = new Vector2(panelSize.x * 0.6f, (-panelSize.y / 7) * 5 - ((panelSize.y - (inputSize.y * 7)) / (7 * 2)));
                    movementFreedomInput.anchoredPosition = new Vector2(panelSize.x * 0.6f, (-panelSize.y / 7) * 6 - ((panelSize.y - (inputSize.y * 7)) / (7 * 2)));

                    widthLabel.anchoredPosition = new Vector2(panelSize.x * 0.1f, (-panelSize.y / 7) * 0 - ((panelSize.y - (inputSize.y * 7)) / (7 * 2)));
                    heightLabel.anchoredPosition = new Vector2(panelSize.x * 0.1f, (-panelSize.y / 7) * 1 - ((panelSize.y - (inputSize.y * 7)) / (7 * 2)));
                    camDistLabelX.anchoredPosition = new Vector2(panelSize.x * 0.1f, (-panelSize.y / 7) * 2 - ((panelSize.y - (inputSize.y * 7)) / (7 * 2)));
                    camDistLabelY.anchoredPosition = new Vector2(panelSize.x * 0.1f, (-panelSize.y / 7) * 3 - ((panelSize.y - (inputSize.y * 7)) / (7 * 2)));
                    camDistLabelZ.anchoredPosition = new Vector2(panelSize.x * 0.1f, (-panelSize.y / 7) * 4 - ((panelSize.y - (inputSize.y * 7)) / (7 * 2)));
                    updatePeriodLabel.anchoredPosition = new Vector2(panelSize.x * 0.1f, (-panelSize.y / 7) * 5 - ((panelSize.y - (inputSize.y * 7)) / (7 * 2)));
                    movementFreedomLabel.anchoredPosition = new Vector2(panelSize.x * 0.1f, (-panelSize.y / 7) * 6 - ((panelSize.y - (inputSize.y * 7)) / (7 * 2)));
                    //This creates a button for each tag found
                    for (int i = 0; i < tagsFoundCount; i++)
                    {
                        GameObject inputGO = Instantiate(idGroupPrefab, inputParent.transform);
                        inputGO.name = tagsFoundStr[i];
                        //inputGO.name = i.ToString();

                        inputGO.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = buttonSize;
                        inputGO.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(panelSize.x * 0.2f, (-panelSize.y / tagsFoundCount) * (i) - ((panelSize.y - (inputSize.y * tagsFoundCount)) / (tagsFoundCount * 2))); //  - (inputGO.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y * 1.1f)
                        inputGO.transform.GetChild(0).GetChild(0).GetComponent<Text>().fontSize = (int)(width * 0.025f);
                        //int hex = -1;
                        //if (!int.TryParse(inputGO.name, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out hex))
                        //{
                        //    UnityEngine.Debug.LogWarning("Unable to parse anchor Value!");
                        //}
                        //inputGO.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = hex.ToString("X");
                        inputGO.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "0x" + int.Parse(inputGO.name).ToString("x");
                        //And assigns a function for each tag button clicked
                        inputGO.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => SelectTagId(inputGO.name, inputGO.transform.parent));
                    }
                    //Gets the current offset of the camera
                    Vector3 camInfo = DroneModule.GetCamOffset();
                    camDistValueX = camInfo.x;
                    camDistValueY = camInfo.y;
                    camDistValueZ = camInfo.z;
                    camDistInputX.GetComponent<InputField>().text = camDistValueX.ToString();
                    camDistInputY.GetComponent<InputField>().text = camDistValueY.ToString();
                    camDistInputZ.GetComponent<InputField>().text = camDistValueZ.ToString();
                    camDistTextMesh.text = "(" + camDistValueX + ", " + camDistValueY + ", " + camDistValueZ + ")";
                    //Gets the tag configuration (which tag belongs to which part of the drone, the widht and height ...
                    ServerMessages.IPSDroneTag tagData = PozyxModule.GetTagFrame();
                    if (tagData != null)
                    {
                        widthInput.GetComponent<InputField>().text = tagData.width.ToString();
                        widthValue = tagData.width;
                        heightInput.GetComponent<InputField>().text = tagData.height.ToString();
                        heightValue = tagData.height;
                        updatePeriodInput.GetComponent<InputField>().text = PozyxModule.updatePeriod.ToString();
                        movementFreedomInput.GetComponent<InputField>().text = PozyxModule.movementFreedom.ToString();
                        //Here it is assigned to which drawing of the tag do they belong
                        for (int i = 0; i < tagsFoundCount; i++) { 
                            if(inputParent.transform.GetChild(i).name == tagData.idSW.ToString()){
                                swTextMesh.text = "0x" + tagData.idSW.ToString("x");
                                inputParent.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new Vector4(0, 1, 0, 1);
                                swValue = tagData.idSW;
                            }

                            if (inputParent.transform.GetChild(i).name == tagData.idNW.ToString())
                            {
                                nwTextMesh.text = "0x" + tagData.idNW.ToString("x");
                                inputParent.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new Vector4(0, 1, 0, 1);
                                nwValue = tagData.idNW;
                            }

                            if (inputParent.transform.GetChild(i).name == tagData.idNE.ToString())
                            {
                                neTextMesh.text = "0x" + tagData.idNE.ToString("x");
                                inputParent.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new Vector4(0, 1, 0, 1);
                                neValue = tagData.idNE;
                            }

                            if (inputParent.transform.GetChild(i).name == tagData.idSE.ToString())
                            {
                                seTextMesh.text = "0x" + tagData.idSE.ToString("x");
                                inputParent.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new Vector4(0, 1, 0, 1);
                                seValue = tagData.idSE;
                            }
                        }
                        lookingForTags.gameObject.SetActive(false);
                    }
                    else return;

                    state = TagsConfigState.ConfiguringTags;
                }
                break;
            case TagsConfigState.ConfiguringTags:
                // Caso para comprobación de estado

                break;
            case TagsConfigState.ConfigurationSent:
                //After the user presses the confirm configuration, it enters here
                sendingDataThrobber.transform.Rotate(0, 0, -200 * Time.deltaTime);
                if (camOffsetConfirmed && tagInfoConfirmed)
                    state = TagsConfigState.ConfigurationAccepted;
                break;
            case TagsConfigState.ConfigurationAccepted:
                //  We go here and start anchors calibration once the app sends all configurations
                state = TagsConfigState.Idle;
                camOffsetReceived = false;
                tagInfoReceived = false;
                droneFilterReceived = false;

                camOffsetConfirmed = false;
                tagInfoConfirmed = false;
                SceneManager.LoadScene("Calibration");
                break;
            case TagsConfigState.ErrorReceived:
                UnityEngine.Debug.LogWarning(PozyxModule.errorMsg);
                if (tagsFound.gameObject != null)
                {
                    tagsText.text = PozyxModule.errorMsg;
                    state = TagsConfigState.SearchingTags;
                }
                // ## TODO: Mostrar error recibido en escena?
                break;
        }
    }

    // Input field width listener
    void WidthInputText(string value)
    {
        widthValue = -1;
        if (!System.Int32.TryParse(value, out widthValue)) {
            UnityEngine.Debug.Log("Unable to parse width value");
            return;
        }
        widthTextMesh.text = widthValue.ToString();
    }

    // Input field height listener
    void HeightInputText(string value)
    {
        heightValue = -1;
        if (!System.Int32.TryParse(value, out heightValue))
        {
            UnityEngine.Debug.Log("Unable to parse height value");
            return;
        }
        heightTextMesh.text = heightValue.ToString();
    }

    // Input field cam dist listener
    void CamDistInputXText(string value)
    {
        camDistValueX = -1;
        if (!System.Single.TryParse(value, out camDistValueX))
        {
            UnityEngine.Debug.LogWarning("Unable to parse camera distance value");
            return;
        }
        camDistTextMesh.text = "(" + camDistValueX + ", " + camDistValueY + ", " + camDistValueZ + ")";
    }

    void CamDistInputYText(string value)
    {
        camDistValueY = -1;
        if (!System.Single.TryParse(value, out camDistValueY))
        {
            UnityEngine.Debug.LogWarning("Unable to parse camera distance value");
            return;
        }
        camDistTextMesh.text = "(" + camDistValueX + ", " + camDistValueY + ", " + camDistValueZ + ")";
    }

    void CamDistInputZText(string value)
    {
        camDistValueZ = -1;
        if (!System.Single.TryParse(value, out camDistValueZ))
        {
            UnityEngine.Debug.LogWarning("Unable to parse camera distance value");
            return;
        }
        camDistTextMesh.text = "(" + camDistValueX + ", " + camDistValueY + ", " + camDistValueZ + ")";
    }

    void UpdatePeriodText(string value)
    {
        updatePeriodValue = -1;
        if (!System.Single.TryParse(value, out updatePeriodValue))
        {
            UnityEngine.Debug.LogWarning("Unable to parse update period value");
            updatePeriodValue = 0.3f;
            return;
        }
        else
        {
            //if (updatePeriodValue != 0.000 || updatePeriodValue != 0.00 || updatePeriodValue != 0.0 || updatePeriodValue != 0)
            //{
               updatePeriodValue = Mathf.Clamp(updatePeriodValue, 0.0001f, 1.0f);
            //
            //}
            //
            //updatePeriodInput.GetComponent<InputField>().text = updatePeriodValue.ToString();
           
            PozyxModule.updatePeriod = updatePeriodValue;
        }
        //TODO: Añadir valur a estructura para enviarlo?
    }

    void MovementFreedomText(string value)
    {
        movementFreedomValue = -1;
        if (!System.Int32.TryParse(value, out movementFreedomValue))
        {
            UnityEngine.Debug.LogWarning("Unable to parse movement freedom value");
            movementFreedomValue = 1;
            return;
        }
        else
        {
            movementFreedomValue = Mathf.Clamp(movementFreedomValue, 1, 1000);
            movementFreedomInput.GetComponent<InputField>().text = movementFreedomValue.ToString();

            PozyxModule.movementFreedom = movementFreedomValue;
        }
        //TODO: Añadir valur a estructura para enviarlo?
    }

    // This function gets called once a button with the tag id is pressed
    void SelectTagId(string id, Transform parent)
    {
        GameObject anchor = GameObject.Find(tagClicked);
        //If an anchor from the 3d dron model has been selected, we enter here
        if (anchor != null && anchor.transform.parent.GetComponent<TextMesh>().text != ("0x" + int.Parse(id).ToString("x"))) { 
            for (int i = 1; i < 5; i++)
            {
                //if the anchor has the same id as the one clicked, we deselect it
                if (anchor.transform.parent.parent.GetChild(i).GetComponent<TextMesh>().text == ("0x" + int.Parse(id).ToString("x")))
                {
                    anchor.transform.parent.parent.GetChild(i).GetComponent<TextMesh>().text = "";
                }
            }
            
            for (int i = 0; i < tagsFoundCount; i++)
            {

                if (("0x" + int.Parse(parent.GetChild(i).name).ToString("x")) == anchor.transform.parent.GetComponent<TextMesh>().text)
                {
                    parent.GetChild(i).GetChild(0).GetComponent<Image>().color = new Vector4(1, 1, 1, 1);
                    //First we deselect the previous tag clicked
                    switch (prevTagClicked)
                    {
                        case "TagSW":
                            swValue = -1;
                            break;
                        case "TagNW":
                            nwValue = -1;
                            break;
                        case "TagNE":
                            neValue = -1;
                            break;
                        case "TagSE":
                            seValue = -1;
                            break;
                        default:
                            int aux = -1;
                            if (!int.TryParse(id, out aux))
                            {
                                UnityEngine.Debug.Log("Unable to parse aux Value!");
                                aux = -1;
                            }
                            if (aux == swValue)
                            {
                                swValue = -1;
                            }
                            else if (aux == nwValue)
                            {
                                nwValue = -1;
                            }
                            else if (aux == neValue)
                            {
                                neValue = -1;
                            }
                            else if(aux == seValue)
                            {
                                seValue = -1;
                            }
                            break;
                    }
                }
            }
            parent.transform.Find(id).GetChild(0).GetComponent<Image>().color = new Vector4(0, 1, 0, 1);

            anchor.transform.parent.GetComponent<TextMesh>().text = "0x" + int.Parse(id).ToString("x");
            //And now we select the tag we clicked
            switch (tagClicked)
            {
                case "TagSW":
                    if (!int.TryParse(id, out swValue)) { 
                        UnityEngine.Debug.Log("Unable to parse SW Value!");
                        swValue = -1;
                    }
                    break;
                case "TagNW":
                    if (!int.TryParse(id, out nwValue))
                    {
                        UnityEngine.Debug.Log("Unable to parse NW Value!");
                        nwValue = -1;
                    }
                    break;
                case "TagNE":
                    if (!int.TryParse(id, out neValue))
                    {
                        UnityEngine.Debug.Log("Unable to parse NE Value!");
                        neValue = -1;
                    }
                    break;
                case "TagSE":
                    if (!int.TryParse(id, out seValue))
                    {
                        UnityEngine.Debug.Log("Unable to parse SE Value!");
                        seValue = -1;
                    }
                    break;
                default:
                    UnityEngine.Debug.LogWarning("Unidentified tag!");
                    break;
            }
        }

    }

    // Confirm tag config button listener
    void ConfirmTagConfiguration()
    {
        if ((clientUnity != null) && (clientUnity.client != null) && (clientUnity.client.isConnected))
        {
            if (swValue >= 0 && nwValue >= 0 && neValue >= 0 && seValue >= 0 && widthValue >= 0 && heightValue >= 0)
            {
                state = TagsConfigState.ConfigurationSent;
                confirmButton.GetComponent<Button>().interactable = false;
                ServerMessages.DroneCamInfo camInfo = new ServerMessages.DroneCamInfo(camDistValueX, camDistValueY, camDistValueZ);
                ServerMessages.IPSDroneTag tagData = new ServerMessages.IPSDroneTag(swValue, nwValue, neValue, seValue, widthValue, heightValue, (int)camDistValueY);
                clientUnity.client.sendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_DRONETAGS_MANUAL_CONFIG, tagData);
                clientUnity.client.sendCommand((byte)Modules.DRONE_MODULE, (byte)DroneCommandType.DRONE_SET_ZCAMERA_OFFSET, camInfo);
                clientUnity.client.sendSetDroneFilter((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_SET_DRONEFILTER, 0, PozyxModule.updatePeriod, PozyxModule.movementFreedom);
            }
            else
            {
                UnityEngine.Debug.Log("Some invalid values");
                PozyxModule.errorMsg = "Some invald values";
                state = TagsConfigState.ErrorReceived;
            }
        }
    }
    //The back and next buttons
    void BackConfiguration()
    {
        tagsFoundLabel.GetComponent<Text>().text = "Tags found";
        nextConfigButton.gameObject.SetActive(true);
        inputParent.SetActive(true);
        droneConfigParent.SetActive(false);
        backConfigButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
    }

    void NextConfiguration()
    {
        tagsFoundLabel.GetComponent<Text>().text = "Drone config";
        nextConfigButton.gameObject.SetActive(false);
        inputParent.SetActive(false);
        droneConfigParent.SetActive(true);
        backConfigButton.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(true);

    }

    // Discover tags button listener
    public void DiscoverTags()
    {
        // UI Changes
        skipEverythingButton.gameObject.SetActive(false);
        discoverAnchorsButton.GetComponent<Button>().interactable = false;
        discoverAnchorsButton.GetChild(0).GetComponent<Text>().color = new Vector4(1, 1, 1, 0.4f);
        lookingForTags.gameObject.SetActive(true);
        searchTags.gameObject.SetActive(false);
        lookingForTags.GetComponent<Text>().text = "Looking \n for tags";

        // State machine changed
        state = TagsConfigState.SearchingTags;

        UnityEngine.Debug.Log("Discovering Tags!");

        // Send messages
        clientUnity.client.SendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_DISCOVER_DRONETAGS);
        clientUnity.client.SendCommand((byte)Modules.DRONE_MODULE, (byte)DroneCommandType.DRONE_GET_ZCAMERA_OFFSET);
        clientUnity.client.SendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_GET_DRONEFILTER);





    }

    // Config tags button listener
    public void ConfigTags()
    {
        configButton.parent.gameObject.SetActive(false);
        tagsConfigurationPanel.gameObject.SetActive(true);
        droneScene.SetActive(true);
        originAxis.SetActive(true);
    }
    //This is activated when an anchor is clicked, the shader is changed to iluminate that the anchor was clicked
    public void OnPointerDown(PointerEventData eventData)
    {
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit cube;
        if (Physics.Raycast(ray, out cube, LayerMask.NameToLayer("Anchor")))
        {
            if (tagClicked != "" && tagClicked != cube.transform.name)
                prevTagClicked = tagClicked;

            tagClicked = cube.transform.name;

            cube.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");
            cube.transform.parent.parent.GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");
            cube.transform.parent.parent.GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");
            cube.transform.parent.parent.GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");

            cube.transform.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
        }
    }
    //This is the button use last configuration 
    public void SkipEverything()
    {
        clientUnity.client.SendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_UPDATE_SETTINGS);
        skipClicked = true;
        // UI Changes
        skipEverythingButton.GetComponent<Button>().interactable = false;
        skipEverythingButton.GetChild(0).GetComponent<Text>().color = new Vector4(1, 1, 1, 0.4f);

        discoverAnchorsButton.GetComponent<Button>().interactable = false;
        discoverAnchorsButton.GetChild(0).GetComponent<Text>().color = new Vector4(1, 1, 1, 0.4f);
        lookingForTags.gameObject.SetActive(true);
        lookingForTags.GetComponent<Text>().text = "Obtaining \n last IPS data";
        searchTags.gameObject.SetActive(false);

    }
}
