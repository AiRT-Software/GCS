using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// Manages the recording scene
/// </summary>
public class UIRecordingManager : MonoBehaviour {

    ClientUnity clientUnity;

    public GameObject point;
    public GameObject parent;
    public Camera topCam;
    public GameObject recordingPanel;
    public RectTransform WarningPanel, warningRecStartPanel, warningBatteryLevel;
    public GameObject drone;
    public GameObject anchorParent;
    public Text warningTitle, warningText, warningRecStartTitle, warningRecStartText, batteryWarningText, batteryWarningText2;
    public RectTransform warningAccept, warningCancel, warningTakeOff, recPlayPause, recBack, batteryWarningOK, batteryWarningCancel, batteryWarningOK2;
    public Material lineMat;
    public RectTransform droneHasFinishedPanel, droneHasFinishedText, droneHasFinishedButton;
    byte anchorID = 1;
    Path path;
    public GameObject topInfoPanel;
    public RectTransform fpvRender;
    public static RegistrationMatrix registrationMatrix;

    public RectTransform backRect;
    RectTransform playPauseRect;
    Image playPauseImage;
    public Sprite playSprite, pauseSprite; 
    


    List<GameObject> waypoints = new List<GameObject>();
    public GameObject playButton;
    bool active = false;
    int indexWaypoints = 0;
    int indexMiddlePoints = 0;
    float time = 0;
    Vector3 actualWaypoint;
    Vector3 nextWaypoint;
    int TotalindexMiddlePoints = 0;
    GameObject modelLoadingPanel;

    public static bool flightEnded = false;
    public static bool receivingIPSData = false, isRecState = false; // ## TODO Poner a false otra vez cuando se deje de hacer recording
    public enum FlyingStates
    {
        IDLE = 0,
        STARTFLYING,
        FLYING
    }
    public static FlyingStates states = FlyingStates.IDLE;
    //This variable is used to know if somebody qith another tablet already entered in recording scene
    bool firstRec = true;

    void Awake()
    {
        GeneralSceneManager.sceneState = GeneralSceneManager.SceneState.Recording;


        topInfoPanel.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = new Color(1,1,1,0.4f);
        topInfoPanel.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0.4f);
        topInfoPanel.transform.GetChild(2).GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0.4f);
        topInfoPanel.transform.GetChild(3).GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0.4f);

        modelLoadingPanel = GameObject.Find("ModelLoadingPanel");
        modelLoadingPanel.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, Screen.height / 7);
        modelLoadingPanel.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 1.5f, Screen.height / 3);

        modelLoadingPanel.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -Screen.height / 7);
        modelLoadingPanel.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.height / 8, Screen.height / 8);

        modelLoadingPanel.transform.GetChild(0).GetComponent<Text>().fontSize = (int)(Screen.width * 0.05f);

        StartCoroutine(WaitForModel());


        fpvRender.sizeDelta = new Vector2(Screen.width / 4, Screen.height / 3);

        warningAccept.sizeDelta = new Vector2(Screen.width / 6, Screen.height / 10);
        warningCancel.sizeDelta = new Vector2(Screen.width / 6, Screen.height / 10);
        warningTakeOff.sizeDelta = new Vector2(Screen.width / 6, Screen.height / 10);

        warningAccept.anchoredPosition = new Vector2(Screen.width / 8, 0);
        warningCancel.anchoredPosition = new Vector2(-Screen.width / 8, 0);
        WarningPanel.anchoredPosition = new Vector2(0, Screen.height / 4);
        WarningPanel.sizeDelta = new Vector2(WarningPanel.sizeDelta.x, 3 * Screen.height / 4);
        warningTitle.fontSize = Screen.height / 16;
        warningTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(warningTitle.GetComponent<RectTransform>().anchoredPosition.x, Screen.height / 12);
        warningText.fontSize = Screen.height / 28;
        warningText.rectTransform.sizeDelta = new Vector2(warningText.rectTransform.sizeDelta.x, Screen.height / 4);
        warningText.rectTransform.anchoredPosition = new Vector2(warningText.rectTransform.anchoredPosition.x, -2 * Screen.height / 12);

        warningRecStartPanel.anchoredPosition = new Vector2(0, Screen.height / 4);
        warningRecStartPanel.sizeDelta = new Vector2(WarningPanel.sizeDelta.x, 3 * Screen.height / 6);
        warningRecStartTitle.fontSize = Screen.height / 16;
        warningRecStartTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(warningTitle.GetComponent<RectTransform>().anchoredPosition.x, Screen.height / 12);
        warningRecStartText.fontSize = Screen.height / 28;
        warningRecStartText.rectTransform.sizeDelta = new Vector2(warningText.rectTransform.sizeDelta.x, Screen.height / 4);
        warningRecStartText.rectTransform.anchoredPosition = new Vector2(warningText.rectTransform.anchoredPosition.x, -2 * Screen.height / 12);

        

        playPauseImage = playButton.GetComponent<Image>();
        playPauseRect = playButton.GetComponent<RectTransform>();

        playPauseRect.sizeDelta = new Vector2(Screen.height / 16, Screen.height / 16);
        backRect.sizeDelta = new Vector2(Screen.height / 16, Screen.height / 16);
        playPauseRect.anchoredPosition = new Vector2(Screen.width / 20, 0);
        backRect.anchoredPosition = new Vector2(-Screen.width / 20, 0);
        recordingPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.width / 20, 2 * Screen.height / 8);
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        droneHasFinishedPanel.gameObject.SetActive(false);
        droneHasFinishedPanel.sizeDelta = new Vector2(droneHasFinishedPanel.sizeDelta.x, Screen.height / 16);
        droneHasFinishedPanel.anchoredPosition = new Vector2(0, 2 * Screen.height / 8);
        droneHasFinishedText.GetComponent<Text>().fontSize = Screen.width / 60;
        droneHasFinishedButton.sizeDelta = new Vector2(Screen.width / 8, Screen.height / 16);

        //Create Path
        path = Path.Instance;
        string jsonString = File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.mission");
        Map map = JsonUtility.FromJson<Map>(jsonString);
        //UnityEngine.Debug.Log("Guid: " + MissionManager.guid);
        //UnityEngine.Debug.Log("PlanIndex: " + MissionManager.planIndex);
        //UnityEngine.Debug.Log("PathsCount: " + map.Paths.Count);
        Path anotherPath = map.Paths[MissionManager.planIndex];
        //We get the matrix that will transform from unity to pozyx to send it
        registrationMatrix = new RegistrationMatrix();
        for (int i = 0; i < 16; i++)
        {
            registrationMatrix.elems[i] = map.unityToAnchors[i];

            UnityEngine.Debug.Log(registrationMatrix.elems[i]);
        }
        registrationMatrix.rowMajor = 0;
        //path.setPath(anotherPath);
        //If the drone isn't in recording state
        if (!isRecState)
        {
            //We enter recording state, which will send the matrix and the plan
            clientUnity.client.SendCommand((byte)Modules.ATREYU_MODULE, (byte)AtreyuCommandType.ATREYU_ENTER_RECORDING_STATE);
            //clientUnity.client.sendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_SYSTEM);
        }
        else
        {
            //else we start to look at the drone travel
            FlyButtonStateMachine.state = FlyButtonStateMachine.buttonState.ACTIVATED;
            isRecState = false;
        }
        //for (int i = 0; i < path.Count(); i++)
        //{
        //    GameObject auxPoint = Instantiate(point, path.GetPoint(i).PointPosition, Quaternion.Euler(0, 0, 0), parent.transform);
        //    path.GetPoint(i).PointTransf = auxPoint.transform;
        //    auxPoint.SetActive(true);
        //    auxPoint.AddComponent<PathPoint>().createPathPointWithPoint(path.GetPoint(i), 0,0);
        //    waypoints.Add(auxPoint);
        //}
        //topCam.gameObject.GetComponent<PostRenderFront>().enabled = true;

        //lineMat.SetPass(0);
        ////WarningPanel.gameObject.SetActive(false);
        //for (int i = 0; i < path.Count() - 1; i++)
        //{
        //    CatmullRomSpline.DisplayCatmullRomSpline2(path, i, ref path.middlePointsTop, topCam.orthographicSize / 2.0f, true, topCam);
        //}
        ////modelMaterial.SetFloat("_Discard", 0);

       // waypoints.Sort((IComparer<GameObject>)new SortPointsByID());
       // indexWaypoints = 0;
       // indexMiddlePoints = 0;
       // TotalindexMiddlePoints = 0;
       // drone.transform.position = waypoints[0].transform.position;
       // actualWaypoint = waypoints[0].transform.position;
       // nextWaypoint = path.middlePointsTop[0];
       // if (waypoints[0].GetComponent<PathPoint>().Poi != null)
       // {
       //     drone.transform.LookAt(waypoints[0].GetComponent<PathPoint>().Poi);
       // }
       // else
       // {
       //     drone.transform.rotation = Quaternion.Euler(0, 0, 0);
       //
       // }
        //recordingPanel.SetActive(true);

    }
    //Once the drone finishes the path, a button appears which, once it is clicked, will call this function
    public void finish()
    {
        clientUnity.client.SendCommand((byte)Modules.PLAN_EXECUTOR_MODULE, (byte)PlanExecutorCommandType.PLAN_EXEC_EXIT);
        isRecState = false;
        registrationMatrix = null;
        FlyButtonStateMachine.state = FlyButtonStateMachine.buttonState.DEACTIVATED;
        SceneManager.LoadScene("General");
    }
    public void Begin()
    {
        //Pass to manual mode
    }
    private class SortPointsByID : IComparer<GameObject>
    {

        int IComparer<GameObject>.Compare(GameObject a, GameObject b)
        {
            PathPoint p1 = ((GameObject)a).GetComponent<PathPoint>();
            PathPoint p2 = ((GameObject)b).GetComponent<PathPoint>();
            if (p1.Id > p2.Id)
                return 1;
            else if (p1.Id < p2.Id)
                return -1;
            else
                return 0;
        }

    }
    //This may no exist as pausing and playing the drone might be impossible?
    public void StartOrStop()
    {
        
        if (active){
            clientUnity.client.SendCommand((byte)Modules.PLAN_EXECUTOR_MODULE, (byte)PlanExecutorCommandType.PLAN_EXEC_PAUSE);
            playPauseImage.sprite = playSprite;
        }
        else {
            if (firstRec)
            {
                clientUnity.client.SendCommand((byte)Modules.PLAN_EXECUTOR_MODULE, (byte)PlanExecutorCommandType.PLAN_EXEC_START);
                firstRec = false;
            }
            else
                clientUnity.client.SendCommand((byte)Modules.PLAN_EXECUTOR_MODULE, (byte)PlanExecutorCommandType.PLAN_EXEC_RESUME);

            playPauseImage.sprite = pauseSprite;
        }

        active = !active;

        //UnityEngine.Debug.Log("Started/Stopped");
        

    }
    public void Back()
    {
        //clientUnity.client.sendCommand((byte)Modules.PLAN_EXECUTOR_MODULE, (byte)PlanExecutorCommandType.);

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        //We get the drone position to reflect it on the screen
        ServerMessages.IPSFrameData frameData = DroneModule.DequeueIPSDronFrame();
        if (frameData != null)
        {
            drone.transform.position = frameData.position;
            drone.transform.rotation = Quaternion.Euler(frameData.rotation);
            //Camera.main.transform.position = new Vector3(drone.transform.position.x, drone.transform.position.y + 10, drone.transform.position.z) ;

        }
        //We get the anchors to paint them
        ServerMessages.IPSFrameAnchorData anchorData = PozyxModule.DequeueIPSAnchorFrame();
        if (anchorData != null)
        {
            anchorParent.transform.Find("Anchor" + anchorID).position = anchorData.position;
            anchorID++;
            if (anchorID == 9)
                UnityEngine.Debug.Log("All anchors positioned");
        }
        //Here we should do a takeoff
        switch (states)
        {
            case FlyingStates.IDLE:
                break;
            case FlyingStates.STARTFLYING:
                time += Time.deltaTime;
                if (time >= 2.0f)
                {
                    clientUnity.client.SendCommand((byte)Modules.PLAN_EXECUTOR_MODULE, (byte)PlanExecutorCommandType.PLAN_EXEC_TAKEOFF);


                    states = FlyingStates.FLYING;
                }
                break;
            case FlyingStates.FLYING:
                
                break;
            default:
                break;
        }
        //If the drone reached its destination, we activate the button that returns to general
        if (flightEnded)
        {
            flightEnded = false;
            droneHasFinishedPanel.gameObject.SetActive(true);
            warningRecStartPanel.gameObject.SetActive(false);
            recordingPanel.gameObject.SetActive(false);

        }
        /*
        if (indexWaypoints >= waypoints.Count || TotalindexMiddlePoints >= path.middlePointsTop.Count)
        {
            active = false;

        }
        if (active && time < 1)
        {
            drone.transform.position = Vector3.Lerp(actualWaypoint, nextWaypoint, time);
            //camToTravel.transform.position = nextWaypoint * Time.deltaTime * 0.001f;
            if (waypoints[indexWaypoints].GetComponent<PathPoint>().BlockDirection)
            {
                drone.transform.rotation = Quaternion.Euler(waypoints[indexWaypoints].GetComponent<PathPoint>().GimbalRotation.z, waypoints[indexWaypoints].GetComponent<PathPoint>().GimbalRotation.y - 90, waypoints[indexWaypoints].GetComponent<PathPoint>().GimbalRotation.x);
                //blockedDIrection = true;
            }
            else if (!waypoints[indexWaypoints].GetComponent<PathPoint>().BlockDirection)
            {
                drone.transform.LookAt(waypoints[indexWaypoints].GetComponent<PathPoint>().Poi);
            }

            time += Time.deltaTime;
        }
        if (active && time >= 1)
        {
            actualWaypoint = nextWaypoint;
            UnityEngine.Debug.Log(waypoints[indexWaypoints].GetComponent<PathPoint>().Segments);
            if (waypoints[indexWaypoints].GetComponent<PathPoint>().Segments <= 3)
            {
                if (waypoints.Count - 1 > indexWaypoints)
                {
                    indexWaypoints++;
                    TotalindexMiddlePoints++;
                    nextWaypoint = waypoints[indexWaypoints].transform.position;
                    indexMiddlePoints = 0;
                }
                //blockedDIrection = false;

            }
            else
            {
                if (waypoints[indexWaypoints].GetComponent<PathPoint>().Segments == indexMiddlePoints + 1)
                {
                    indexWaypoints++;
                    TotalindexMiddlePoints++;
                    if (TotalindexMiddlePoints < path.middlePointsTop.Count)
                        nextWaypoint = path.middlePointsTop[TotalindexMiddlePoints];
                    indexMiddlePoints = 0;
                    //blockedDIrection = false;

                }
                else if (path.middlePointsTop.Count > TotalindexMiddlePoints)
                {
                    indexMiddlePoints++;
                    TotalindexMiddlePoints++;
                    if (TotalindexMiddlePoints < path.middlePointsTop.Count)
                        nextWaypoint = path.middlePointsTop[TotalindexMiddlePoints];
                }
            }


            time = 0;
        }
        */
    }

   
    public void CoroutineCaller(IEnumerator func)
    {
        StartCoroutine(func);
    }

    IEnumerator WaitForModel()
    {

        while (GameObject.Find("DaeModel") == null)
        {
            modelLoadingPanel.transform.GetChild(1).Rotate(0, 0, -200 * Time.deltaTime);
            yield return null;
        }
        modelLoadingPanel.SetActive(false);

    }
}
