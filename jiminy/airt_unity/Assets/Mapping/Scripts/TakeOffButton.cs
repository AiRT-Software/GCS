using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//This class takes charge of managing the mapping state
public class TakeOffButton : MonoBehaviour {
    public enum TakeOffButtonEnum
    {
        Idle,
        BeingPressed,
        fiveSecondsPassed,
        Scanning,
        StopScan
    }
    public static bool receivedGUID = false;
    public static bool changedState = false;
    public static TakeOffButtonEnum state = TakeOffButtonEnum.Idle;
    float pressTime = 0, buttonWidth = 0.0f;
    RectTransform fillButton;
    ClientUnity clientUnity;
    public Transform plcParent;
    public GameObject goToSelectPlanButton; // scanningThrobber;
    bool startedMapping = false;
    public static bool mapReady = false;
    public Button homeButton;
	// Use this for initialization
	void Start () {
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();

        buttonWidth = this.GetComponent<RectTransform>().sizeDelta.x;
        fillButton = this.transform.GetChild(1).GetComponent<RectTransform>();
        //If the app is the one who started mapping, enter here
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        if ((clientUnity.client != null) && clientUnity.client.isConnected && !AtreyuModule.jumpToMappingDirectly)
        {
            clientUnity.client.SendCommand((byte)Modules.ATREYU_MODULE, (byte)AtreyuCommandType.ATREYU_ENTER_MAPPING_STATE);
        }
        //If not, the app will enter here and will check in which mapping state the drone is
        if (AtreyuModule.jumpToMappingDirectly == true)
        {
            //First, it will request the guid to know which guid should be saved
            clientUnity.client.SendCommand((byte)Modules.MAPPER_MODULE, (byte)MapperCommandType.MAPPER_REQUEST_CURRENT_GUID);
            //Then gets all pointclouds produced until the moment the app entered
            clientUnity.client.SendCommand((byte)Modules.MAPPER_MODULE, (byte)MapperCommandType.MAPPER_GET_ALL_POINTCLOUDS);
            AtreyuModule.jumpToMappingDirectly = false;
            //depending on the state, the scan button will be at scan or paused or the finished button will be active
            switch (MapperModule.state)
            {
                case MapperModule.MapperState.IDLE:
                    homeButton.interactable = false;

                  


                    transform.GetChild(0).GetComponent<Text>().text = "SCAN";
                    this.GetComponent<EventTrigger>().triggers.RemoveRange(0, this.GetComponent<EventTrigger>().triggers.Count);
                    pressTime = 0;
                    fillButton.sizeDelta = new Vector2(0.0f, 0.0f);
                    state = TakeOffButtonEnum.StopScan;
                    this.GetComponent<Button>().onClick.RemoveAllListeners();
                    this.GetComponent<Button>().onClick.AddListener(startScan);
                    UIMapManager.droneFlying = true;

                    break;
                case MapperModule.MapperState.READY:
                    homeButton.interactable = false;

                   

                    transform.GetChild(0).GetComponent<Text>().text = "SCAN";
                    this.GetComponent<EventTrigger>().triggers.RemoveRange(0, this.GetComponent<EventTrigger>().triggers.Count);
                    pressTime = 0;
                    fillButton.sizeDelta = new Vector2(0.0f, 0.0f);
                    state = TakeOffButtonEnum.StopScan;
                    this.GetComponent<Button>().onClick.RemoveAllListeners();
                    this.GetComponent<Button>().onClick.AddListener(startScan);
                    UIMapManager.droneFlying = true;

                    break;
                case MapperModule.MapperState.START:
                    homeButton.interactable = false;
                    this.GetComponent<EventTrigger>().triggers.RemoveRange(0, this.GetComponent<EventTrigger>().triggers.Count);
                    pressTime = 0;
                    fillButton.sizeDelta = new Vector2(0.0f, 0.0f);
                   //clientUnity.client.sendCommand((byte)Modules.MAPPER_MODULE, (byte)MapperCommandType.MAPPER_START_MAPPING);
                    this.GetComponent<Button>().onClick.RemoveAllListeners();
                    this.GetComponent<Button>().onClick.AddListener(stopScan);
                    startedMapping = true;

                    state = TakeOffButtonEnum.Scanning;
                    pressTime = 0;
                    UnityEngine.Debug.Log("Scanning");
                    goToSelectPlanButton.SetActive(false);
                    this.transform.GetChild(0).GetComponent<Text>().text = "PAUSE";
                    UIMapManager.droneFlying = true;

                    break;
                case MapperModule.MapperState.PAUSED:
                    homeButton.interactable = false;
                    this.GetComponent<EventTrigger>().triggers.RemoveRange(0, this.GetComponent<EventTrigger>().triggers.Count);
                    pressTime = 0;
                    fillButton.sizeDelta = new Vector2(0.0f, 0.0f);
                    state = TakeOffButtonEnum.StopScan;
                    this.GetComponent<Button>().onClick.RemoveAllListeners();
                    this.GetComponent<Button>().onClick.AddListener(startScan);

                    state = TakeOffButtonEnum.StopScan;
                    pressTime = 0;
                    fillButton.sizeDelta = new Vector2(0.0f, 0.0f);
                    UnityEngine.Debug.Log("Stopped");
                    this.transform.GetChild(0).GetComponent<Text>().text = "SCAN";
                    UIMapManager.droneFlying = true;
                    startedMapping = true;

                    break;
                case MapperModule.MapperState.DONE:
                    break;
                default:
                    break;
            }
            
        }
        else
            state = TakeOffButtonEnum.fiveSecondsPassed;
    }

    // Update is called once per frame
    void Update () {

        //If this isn't the tablet that paused/started the scann or finished mapping, it will enter here to change state
        if (changedState)
        {
            switch (MapperModule.state)
            {
                case MapperModule.MapperState.IDLE:
                    break;
                case MapperModule.MapperState.READY:
                    break;
                case MapperModule.MapperState.START:
                    
                    this.GetComponent<Button>().onClick.RemoveAllListeners();
                    this.GetComponent<Button>().onClick.AddListener(stopScan);
                    startedMapping = true;

                    state = TakeOffButtonEnum.Scanning;
                    pressTime = 0;
                    UnityEngine.Debug.Log("Scanning");
                    goToSelectPlanButton.SetActive(false);
                    this.transform.GetChild(0).GetComponent<Text>().text = "PAUSE";
                    //scanningThrobber.SetActive(true);
                    //GameObject.Find("CameraToggleButton").GetComponent<Button>().interactable = false;
                    break;
                case MapperModule.MapperState.PAUSED:
                    homeButton.interactable = false;
                    this.GetComponent<EventTrigger>().triggers.RemoveRange(0, this.GetComponent<EventTrigger>().triggers.Count);
                    pressTime = 0;
                    fillButton.sizeDelta = new Vector2(0.0f, 0.0f);
                    state = TakeOffButtonEnum.StopScan;
                    this.GetComponent<Button>().onClick.RemoveAllListeners();
                    this.GetComponent<Button>().onClick.AddListener(startScan);

                    state = TakeOffButtonEnum.StopScan;
                    pressTime = 0;
                    fillButton.sizeDelta = new Vector2(0.0f, 0.0f);
                    UnityEngine.Debug.Log("Stopped");
                    this.transform.GetChild(0).GetComponent<Text>().text = "SCAN";
                    UIMapManager.droneFlying = true;
                    startedMapping = true;
                    goToSelectPlanButton.SetActive(true);

                    break;
                case MapperModule.MapperState.DONE:
                    break;
                default:
                    break;
            }
            changedState = false;
        }


        //manages the mapping state
        switch (state)
        {
            case TakeOffButtonEnum.Idle:
                break;
                //not used anymore
            case TakeOffButtonEnum.BeingPressed:
                pressTime += Time.deltaTime;
                fillButton.sizeDelta = new Vector2((pressTime * buttonWidth) / 5.0f, 0.0f);
                if (pressTime >= 5.0f)
                    state = TakeOffButtonEnum.fiveSecondsPassed;
                break;
            
            case TakeOffButtonEnum.fiveSecondsPassed:
                //UnityEngine.Debug.Log("5 sec pressed");
                // ##TODO Desactiva todo el panel de warning y llamar a función que carga el camino
                //clientUnity.client.sendCommand((byte)Modules.FCS_MULTIPLEXER_MODULE, (byte)FCSMultiplexerCommandType.FCS_TAKEOFF);

                // ## DEBUG QUITAR LUEGO
                //MissionManager.guid = Guid.NewGuid().ToString();
                //Home button stops being interactable because exiting in the middle of mapping stops the user from doing anything to the drone
                homeButton.interactable = false;
                //If the guid is null, we create a new map. If we want to modify a pointcloud, we load a map. Right now there is no way to modify a pointcloud
                if (MissionManager.guid == "" )
                {
                    MissionManager.guid = Guid.NewGuid().ToString();

                    clientUnity.client.sendTwoPartCommand((byte)Modules.MAPPER_MODULE, (byte)MapperCommandType.MAPPER_CREATE_MAP, MissionManager.guid + "\0");

                }
                else
                {
                    clientUnity.client.sendTwoPartCommand((byte)Modules.MAPPER_MODULE, (byte)MapperCommandType.MAPPER_LOAD_MAP, MissionManager.guid + "\0");
                }
                
                //The button allow now to scan
                transform.GetChild(0).GetComponent<Text>().text = "SCAN";
                this.GetComponent<EventTrigger>().triggers.RemoveRange(0, this.GetComponent<EventTrigger>().triggers.Count);
                pressTime = 0;
                fillButton.sizeDelta = new Vector2(0.0f, 0.0f);
                state = TakeOffButtonEnum.StopScan;
                this.GetComponent<Button>().onClick.RemoveAllListeners();
                this.GetComponent<Button>().onClick.AddListener(startScan);
                UIMapManager.droneFlying = true;

                break;
            case TakeOffButtonEnum.Scanning:
                pressTime += Time.deltaTime;
                //plcParent.transform.Rotate(0, 24 * Time.deltaTime, 0);
                //scanningThrobber.transform.Rotate(0, 0, -200 * Time.deltaTime);
                //fillButton.sizeDelta = new Vector2((pressTime * buttonWidth) / 15.0f, 0.0f);
                //This is the amount of time we want to allow the user to scan. Once the time is up, we pause mapping
                if (pressTime >= 10000000)
                {
                    goToSelectPlanButton.SetActive(true);
                    fillButton.sizeDelta = new Vector2(0.0f, 0.0f);
                    pressTime = 0;
                    clientUnity.client.SendCommand((byte)Modules.MAPPER_MODULE, (byte)MapperCommandType.MAPPER_PAUSE_MAPPING);
                    state = TakeOffButtonEnum.StopScan;
                    this.GetComponent<Button>().onClick.RemoveAllListeners();
                    this.GetComponent<Button>().onClick.AddListener(startScan);
                    //SAVE TO DISK CLOUD
                }
                break;
            case TakeOffButtonEnum.StopScan:
                //if (!mapReady)
                //{
                //    this.GetComponent<Button>().interactable = false;
                //}
                //else
                //{
                //    this.GetComponent<Button>().interactable = true;
                //}
                break;
            default:
                break;
        }
            
    }
    //Starts mapping. First time needs to be an start, subsequent presses of the button are resumes
    public void startScan()
    {
        
        if (!startedMapping)
        {
            clientUnity.client.SendCommand((byte)Modules.MAPPER_MODULE, (byte)MapperCommandType.MAPPER_START_MAPPING);

            startedMapping = true;
        }
        else
        {
            clientUnity.client.SendCommand((byte)Modules.MAPPER_MODULE, (byte)MapperCommandType.MAPPER_RESUME_MAPPING);

        }
        //clientUnity.client.sendCommand((byte)Modules.MAPPER_MODULE, (byte)MapperCommandType.MAPPER_START_MAPPING);
        this.GetComponent<Button>().onClick.RemoveAllListeners();
        this.GetComponent<Button>().onClick.AddListener(stopScan);
        state = TakeOffButtonEnum.Scanning;
        pressTime = 0;
        UnityEngine.Debug.Log("Scanning");
        goToSelectPlanButton.SetActive(false);
        this.transform.GetChild(0).GetComponent<Text>().text = "PAUSE";
        //scanningThrobber.SetActive(true);
        //GameObject.Find("CameraToggleButton").GetComponent<Button>().interactable = false;

    }
    //Pauses scan
    public void stopScan()
    {
        goToSelectPlanButton.SetActive(true);

        clientUnity.client.SendCommand((byte)Modules.MAPPER_MODULE, (byte)MapperCommandType.MAPPER_PAUSE_MAPPING);
        this.GetComponent<Button>().onClick.RemoveAllListeners();
        this.GetComponent<Button>().onClick.AddListener(startScan);
        state = TakeOffButtonEnum.StopScan;
        pressTime = 0;
        fillButton.sizeDelta = new Vector2(0.0f, 0.0f);
        UnityEngine.Debug.Log("Stopped");
        this.transform.GetChild(0).GetComponent<Text>().text = "SCAN";
        //scanningThrobber.SetActive(false);
        //GameObject.Find("CameraToggleButton").GetComponent<Button>().interactable = true;
    }
    //old buttons to manually take of the dron. Not used anymore
    public void onStartTakeOff()
    {

        state = TakeOffButtonEnum.BeingPressed;

    }
    public void onPressedUpButton()
    {
        this.GetComponent<Button>().onClick.AddListener(startScan);

    }
    public void onCancelTakeOff()
    {

        {
            pressTime = 0;
            fillButton.sizeDelta = new Vector2(0.0f, 0.0f);
            state = TakeOffButtonEnum.Idle;
        }
        

    }


}
