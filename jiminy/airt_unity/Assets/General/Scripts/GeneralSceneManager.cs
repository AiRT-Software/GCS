using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GeneralSceneManager : MonoBehaviour {

    public GameObject splashScreen;
    public GameObject selectFlightType;
    public GameObject updatePanel;
    public RectTransform loadingRotate;
    int rotation = -180;

    public GameObject updateProgressBar;

    public RectTransform airtLogo;
    public RectTransform partnerLogos;

    private ClientUnity clientUnity;

    bool retry = true;
    bool versionCheck = false;
    public Button fpvButton;
    public Button mappingButton, recordingButton;

    public RectTransform turnOffOCS, update, backButton, creditsTitle, AIrtLogo, CreditsText, EULogo;

    public Text connectingToOCS;
    public GameObject offlinePanel, FCPanel, RCPanel, GimbalPanel; 
    public enum SceneState
    {
        General = 0,
        TagsConfiguration,
        Calibration,
        PlanSelection,
        ModelAlignment,
        Planning,
        Mapping,
        Recording
    }

    public enum ApplicationState
    {
        Start = 0,
        Planning,
        Mapping,
        Recording
    };

    public static ApplicationState appState;
    public static SceneState sceneState;
    public Text updateText;
    enum CurrentState
    {
        start = 0,
        versionReq,
        versionCheck,
        askUpdateApp,
        getUpdatePath,
        updatingApp,
        upToDate,
        waitOrIdle
    }

    CurrentState state = CurrentState.start;

    void Awake()
    {
        appState = ApplicationState.Start;
        sceneState = SceneState.General;
        // Logos size
        //airtLogo.sizeDelta = new Vector2(Screen.width/3.0f, Screen.height/6.0f);
        //partnerLogos.sizeDelta = new Vector2(Screen.width - 50.0f, Screen.height / 8.0f);

        //byte[] ta = System.IO.File.ReadAllBytes(Application.persistentDataPath + "/PrimerPiso.mb");
        //for (int i = 0; i < ta.Length; i++)
        //{
        //string s = System.BitConverter.ToString(ta, 0);
        //UnityEngine.Debug.Log(s);
        //}


        //turnOffOCS.sizeDelta = new Vector2(Screen.width / 16, Screen.height / 32);


        //TurnOffOCS button text font size
        turnOffOCS.GetChild(0).GetComponent<Text>().fontSize = Screen.width / 120;


        turnOffOCS.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(-turnOffOCS.GetChild(1).GetComponent<RectTransform>().anchoredPosition.x + 20, turnOffOCS.GetChild(0).GetComponent<RectTransform>().anchoredPosition.y);
        //turnOffOCS.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(turnOffOCS.sizeDelta.x, 0);

        //update.sizeDelta = new Vector2(Screen.width / 16, Screen.height / 18);

        //Update text and the version text
        update.GetChild(0).GetComponent<Text>().fontSize = Screen.width / 80;
        updateText.fontSize = Screen.width / 110;

        //backButton.sizeDelta = new Vector2(Screen.width / 16, Screen.height / 18);
        //backButton.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 60, Screen.height / 60);




    }

    void Start()
    {
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        GetVersionWithoutUpdating();
    }

    private IEnumerator WaitAndPrint(float waitTime)
    {
        
        yield return new WaitForSeconds(waitTime);
        splashScreen.SetActive(false);
        selectFlightType.SetActive(true);
        MissionManager.guid = "";

    }
    //The back button on the no connection panel calls this
    public void closeNoCOnnectionPanel()
    {
        offlinePanel.SetActive(false);
        FCPanel.SetActive(false);
        RCPanel.SetActive(false);
        GimbalPanel.SetActive(false);
    }
    void Update()
    {
        if (splashScreen.activeSelf)
        {
            //loadingRotate.localRotation ## Usar este método para girar la rueda de X en X grados, no de forma constante
            loadingRotate.Rotate(new Vector3(0, 0, rotation * Time.deltaTime));
        }
        //UnityEngine.Debug.Log(clientUnity);
        //UnityEngine.Debug.Log(clientUnity.client);
        //UnityEngine.Debug.Log(clientUnity.client.isConnected);
        //If another tablet is mapping, go to mapping
        if (AtreyuModule.jumpToMappingDirectly && sceneState != SceneState.Mapping && CalibrationSettings.anchorConfigData[0] != null)
        {
            SceneManager.LoadScene("Mapping");

        }
        //This makes the buttons interactables only if the client is connected to the drone. Doesn't make sense to go to mapping if the client isn't connected
        if (clientUnity != null && clientUnity.client != null && clientUnity.client.isConnected)
        {
            mappingButton.interactable = true;
            recordingButton.interactable = true;
            turnOffOCS.GetComponent<Button>().interactable = true;
            update.GetComponent<Button>().interactable = true;
            fpvButton.interactable = true;
        }
        else
        {
            mappingButton.interactable = false;
            recordingButton.interactable = false;
            turnOffOCS.GetComponent<Button>().interactable = false;
            update.GetComponent<Button>().interactable = false;
            fpvButton.interactable = false;


        }

        switch (state) {

            case CurrentState.start:
                //Keeps the loading screen from the beggining 2 seconds to show the logo 
                StartCoroutine(WaitAndLoad());

                break;

            case CurrentState.versionReq:

                if (retry) { 
                    GetVersion();
                    StartCoroutine(WaitForResponse());
                }
                
                break;

            case CurrentState.versionCheck:
                VersionCheck();
                break;

            case CurrentState.askUpdateApp:
                updatePanel.SetActive(true);
                state = CurrentState.waitOrIdle;
                break;

            case CurrentState.getUpdatePath:
                if (retry) {
                    //The update file can only be called from the persistent data path, but we can only get it from the resources folder, so we move it
                    FileManager.MoveDebFile();
                    clientUnity.client.SendCommand((byte)Modules.ATREYU_MODULE, (byte)AtreyuCommandType.ATREYU_QUERY_UPDATE_PACKAGE_PATHNAME);
                    StartCoroutine(WaitForResponse());
                    state = CurrentState.updatingApp;
                    connectingToOCS.text = "Updating";
                    updateProgressBar.SetActive(true);
                    //StartCoroutine(WaitForResponse());
                }
                break;

            case CurrentState.updatingApp:
                if (retry) {
                    StartCoroutine(CheckConnection());
                    updateProgressBar.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(updateProgressBar.GetComponent<RectTransform>().sizeDelta.x * ((float)OSModule.bytesSent / (float)OSModule.updateDebFileSize), updateProgressBar.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y);
                }
                break;

            case CurrentState.upToDate:
                
                // ## Comprobar si se ha calibrado y sino pasar a otra escena
                //SceneManager.LoadScene("Calibration");

                // ## DEBUG ONLY

                selectFlightType.SetActive(true);
                splashScreen.SetActive(false);
                state = CurrentState.waitOrIdle;
                updateProgressBar.SetActive(false);

                connectingToOCS.text = "Connecting to OCS";

                break;

            case CurrentState.waitOrIdle:
                // Idle state, waiting for user data
                break;

            default:
                UnityEngine.Debug.Log("Unknow state reached");
                Application.Quit();
                break;

        };
    }
    //Checks the version
    void GetVersion()
    {
        //UnityEngine.Debug.Log("GetVersion");
        if (clientUnity != null && clientUnity.client != null && clientUnity.client.isConnected) {
            clientUnity.client.SendCommand((byte)Modules.ATREYU_MODULE, (byte)AtreyuCommandType.ATREYU_QUERY_SERVER_VERSION);
            state = CurrentState.versionCheck;
            UnityEngine.Debug.Log("Changing state to version check");
            //UnityEngine.Debug.Log("Version Get: " + AtreyuManager.majorV + "." + AtreyuManager.minorV + "." + AtreyuManager.patchV);
        }
    }
    void GetVersionWithoutUpdating()
    {
        //UnityEngine.Debug.Log("GetVersion");
        //UnityEngine.Debug.Log(clientUnity);
        //UnityEngine.Debug.Log(clientUnity.client);
        //UnityEngine.Debug.Log(clientUnity.client.isConnected);
        if (clientUnity != null && clientUnity.client != null )
        {
            clientUnity.client.SendCommand((byte)Modules.ATREYU_MODULE, (byte)AtreyuCommandType.ATREYU_QUERY_SERVER_VERSION);
            //UnityEngine.Debug.Log("Version Get: " + AtreyuManager.majorV + "." + AtreyuManager.minorV + "." + AtreyuManager.patchV);
        }
    }
    void VersionCheck() {
        //UnityEngine.Debug.Log("VersionCheck");
        //IF the version is not updated, we start to send the update
        if (AtreyuModule.versionUpdated)
        {
            if (AtreyuModule.EqualVersions()){
                //UnityEngine.Debug.Log("Up to date");
                state = CurrentState.upToDate;
            }
            else {
                UnityEngine.Debug.Log("Need to update app");
                state = CurrentState.getUpdatePath;
            }
        }
    }

    public void ChangeToMapping()
    {
        //IF NOT CALIBRATED -> Calibrate, else mapping
        appState = ApplicationState.Mapping;
        GameObject.Find("UIManager").GetComponent<UIManager>().LoadCustomScene("TagsConfiguration");
        clientUnity.client.SendCommand((byte)Modules.FPV_MODULE, (byte)FPVCommandType.FPV_STOP_STREAMING);
        //SceneManager.LoadScene("TagsConfiguration");
    }
    public void ChangeToRecording()
    {
        //IF NOT CALIBRATED -> Calibrate, else mapping
        
        splashScreen.SetActive(true);
        state = CurrentState.versionReq;
        selectFlightType.SetActive(false);
        appState = ApplicationState.Recording;
        GameObject.Find("UIManager").GetComponent<UIManager>().LoadCustomScene("TagsConfiguration");

        //SceneManager.LoadScene("TagsConfiguration");
    }
    public void ChangeToPlanning()
    {
        //IF NOT CALIBRATED -> Calibrate, else mapping
        appState = ApplicationState.Planning;
        GameObject.Find("UIManager").GetComponent<UIManager>().LoadCustomScene("PlanSelection");
        clientUnity.client.SendCommand((byte)Modules.FPV_MODULE, (byte)FPVCommandType.FPV_STOP_STREAMING);

        //SceneManager.LoadScene("PlanSelection");
    }

    public void UpdateApp() {
        UnityEngine.Debug.Log("UpdateApp");
        updatePanel.SetActive(false);
        state = CurrentState.getUpdatePath;
    }

    public void CancelUpdate()
    {
        UnityEngine.Debug.Log("CancelUpdate");
        Application.Quit();
    }

    private IEnumerator WaitForResponse()
    {
        retry = false;
        yield return new WaitForSeconds(1.0f);
        retry = true;
    }

    private IEnumerator CheckConnection()
    {
        retry = false;
        yield return new WaitForSeconds(2.0f);
        retry = true;
        if (OSModule.isUpdated && clientUnity.client.isConnected) { 
            state = CurrentState.upToDate;
        }
        if (!clientUnity.client.isConnected)
        {
            offlinePanel.SetActive(true);
            state = CurrentState.upToDate;
        }
    }

    private IEnumerator WaitAndLoad()
    {
        yield return new WaitForSeconds(2.0f);
        splashScreen.SetActive(false);
        selectFlightType.SetActive(true);
        state = CurrentState.waitOrIdle;
    }

    // ## DEBUG ONLY!
    public void ManualUpdate()
    {
        GameObject.Find("GoToDifferentConfigs").SetActive(false);
        splashScreen.SetActive(true);
        state = CurrentState.versionReq;
        selectFlightType.SetActive(false);
    }
}
