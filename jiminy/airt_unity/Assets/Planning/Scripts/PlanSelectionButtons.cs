using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System;

// TODO: Cambiar botones "artificiales" por cargado de mapas y planes, "resetear" panel de planes al cambiar el mapa seleccionado

public class PlanSelectionButtons : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {
    //This is the script assigned to a map button
    public UnityEvent onLongPress = new UnityEvent();

    public GameObject button, extraPanel;
    //White vertical bar between buttons
    public GameObject bar;
    //These are the button that appear once a map is being pressed
    public RectTransform mapInfo, mapLocalRemove, mapServerRemove, mapBar1, mapBar2;
    //Button that appear once a plan is being pressed not implemented
    public RectTransform planInfo, planEdit, planDuplicate, planRemove, planBar1, planBar2, planBar3;
    public Font Rubik;

    public GameObject planButtonPrefab;
    public GameObject deletePanel;
    //Panel that appear once you try to create a plan without a map and there is not internet to download
    public RectTransform pleaseConnectToTheDronePanel;
    //Downloading map panel gameobject
    public GameObject downloadingMapPanel;

    GameObject planSelectionGO, planInfoGO, mapInfoGO, newMissionGO, noPlansText;
    
    Button planRightB;
    Button mapLeftB, mapRightB;

    int buttonOffset = Screen.width / 4;
    public string guid = "";
    string planName = "";
    public bool newMapButton = false;
    bool newPlanButton = false;

    private bool isPointerDown = false;
    private bool longPressTriggered = false;
    private float timePressStarted;
    private float longPressDuration = 0.5f;
    void Awake()
    {
        float size = Screen.height / 4.5f;
        float panelSize = Screen.width / 5.0f;
        Vector2 barsSize = new Vector2(Screen.height / 250.0f, panelSize * 0.8f);
        
        if (mapInfo)
        {
            
            size /= 3;

            mapInfo.sizeDelta = new Vector2(0, size);
            mapLocalRemove.sizeDelta = new Vector2(0, size);
            mapServerRemove.sizeDelta = new Vector2(0, size);
            mapBar1.sizeDelta = barsSize;
            mapBar2.sizeDelta = barsSize;

            mapInfo.anchoredPosition = new Vector2(0, -0 * size);
            mapLocalRemove.anchoredPosition = new Vector2(0, -1 * size);
            mapServerRemove.anchoredPosition = new Vector2(0, -2 * size);
            mapBar1.anchoredPosition = new Vector2(panelSize * 0.1f, -1 * size);
            mapBar2.anchoredPosition = new Vector2(panelSize * 0.1f, -2 * size);

            mapInfo.GetChild(0).GetComponent<Text>().fontSize = (int)(Screen.width * 0.02f);
            mapLocalRemove.GetChild(0).GetComponent<Text>().fontSize = (int)(Screen.width * 0.02f);
            mapServerRemove.GetChild(0).GetComponent<Text>().fontSize = (int)(Screen.width * 0.02f);

        }

        if (planInfo)
        {
            size /= 4;

            planInfo.sizeDelta = new Vector2(0, size);
            planEdit.sizeDelta = new Vector2(0, size);
            planDuplicate.sizeDelta = new Vector2(0, size);
            planRemove.sizeDelta = new Vector2(0, size);
            planBar1.sizeDelta = barsSize;
            planBar2.sizeDelta = barsSize;
            planBar3.sizeDelta = barsSize;

            planInfo.anchoredPosition = new Vector2(0, -0 * size);
            planEdit.anchoredPosition = new Vector2(0, -1 * size);
            planDuplicate.anchoredPosition = new Vector2(0, -2 * size);
            planRemove.anchoredPosition = new Vector2(0, -3 * size);
            planBar1.anchoredPosition = new Vector2(panelSize * 0.1f, -1 * size);
            planBar2.anchoredPosition = new Vector2(panelSize * 0.1f, -2 * size);
            planBar3.anchoredPosition = new Vector2(panelSize * 0.1f, -3 * size);

            planInfo.GetChild(0).GetComponent<Text>().fontSize = (int)(Screen.width * 0.02f);
            planEdit.GetChild(0).GetComponent<Text>().fontSize = (int)(Screen.width * 0.02f);
            planDuplicate.GetChild(0).GetComponent<Text>().fontSize = (int)(Screen.width * 0.02f);
            planRemove.GetChild(0).GetComponent<Text>().fontSize = (int)(Screen.width * 0.02f);
        }

        planSelectionGO = GameObject.Find("PlanSelection");
        mapInfoGO = planSelectionGO.transform.parent.GetChild(4).gameObject;
        planInfoGO = planSelectionGO.transform.parent.GetChild(5).gameObject;
        newMissionGO = planSelectionGO.transform.parent.GetChild(7).gameObject;
        noPlansText = planSelectionGO.transform.GetChild(1).GetChild(4).gameObject;

        mapLeftB = planSelectionGO.transform.GetChild(0).GetChild(1).gameObject.GetComponent<Button>();
        mapRightB = planSelectionGO.transform.GetChild(0).GetChild(2).gameObject.GetComponent<Button>();
        planRightB = planSelectionGO.transform.GetChild(1).GetChild(2).gameObject.GetComponent<Button>();

        deletePanel = planSelectionGO.transform.parent.GetChild(16).gameObject;
        //Listener to know if a user has touched for a long time the button, to make appear the info/delete
        onLongPress.AddListener(LongTouch);

    }

    void Update()
    {
        //UnityEngine.Debug.Log(extraPanel.GetComponent<RectTransform>().sizeDelta.y);
        if (isPointerDown && !longPressTriggered)
        {
            if (Time.time - timePressStarted > longPressDuration)
            {
                longPressTriggered = true;
                //UnityEngine.Debug.Log("LONG PRESS!");
                onLongPress.Invoke();
            }
        }
        if (downloadingMapPanel && downloadingMapPanel.activeSelf)
        {

            downloadingMapPanel.transform.GetChild(1).Rotate(0, 0, -200 * Time.deltaTime);
        }

    }

    public void ShowOpts()
    {
        //This make the options appear, info delete local and delete on server
        if (!PlanSelectionManager.serverMissions.Contains(guid))
        {
            
            transform.GetChild(2).GetChild(4).GetComponent<Button>().interactable = false;
            transform.GetChild(2).GetChild(4).GetChild(0).GetComponent<Text>().color = new Vector4(1, 1, 1, 0.4f);
        }

        if (this.name == "Background" || newMapButton || newPlanButton)
            return;
        for (int i = 0; i < transform.parent.childCount; i++)
            transform.parent.GetChild(i).Find("ExtraPanel").gameObject.SetActive(false);
            
        extraPanel.SetActive(true);
    }

    public void InfoMap()
    {
        //This activates the panel that contains the info of the plan, name lcoation date created, BB and file size
        MissionManager.guid = guid;
        MissionManager.importedModel = true;
        Transform parent = this.transform.parent;
        string json = File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + guid + ".json.metadata");
        MapMetadata metadata = JsonUtility.FromJson<MapMetadata>(json);

        MissionManager.modificationText = "" + metadata.Date.day + '/' + metadata.Date.month + '/' + metadata.Date.year;
        MissionManager.mapName = metadata.Name;
        MissionManager.mapLocation = metadata.Location;
        MissionManager.modelBoundingBox = metadata.BoundingBox;
        MissionManager.fileSize = metadata.Byte_Size;

        while (parent.parent != null)
        {
            parent = parent.parent;
        }
        parent.GetComponent<MissionManager>().enabled = false;
        parent.GetComponent<MissionManager>().enabled = true;


        mapInfoGO.transform.GetChild(3).gameObject.SetActive(false);
        planSelectionGO.SetActive(false);
        mapInfoGO.SetActive(true);
        UnityEngine.Debug.Log("InfoMap");
    }

    public void RemoveLocalMap()
    {
        MissionManager.guid = guid;
        ApplicationVariables.appRemoveState = (byte)ApplicationVariables.RemoveState.LocalMapRemove;
        ApplicationVariables.removeGO = gameObject;
        deletePanel.SetActive(true);
        /*
        for (int i = transform.GetSiblingIndex(); i < transform.parent.childCount; i++)
        {
            transform.parent.GetChild(i).GetComponent<RectTransform>().anchoredPosition -= new Vector2(buttonOffset, 0);
        }
        if (!mapRightB.interactable)
            PlanSelectionManager.animationsState = PlanSelectionManager.PanelAnimations.FIRST_MAP;
        */
        
        
    }

    public void RemoveServerMap()
    {
        //Consults if the user really wants to delete the map from the server
        MissionManager.guid = guid;
        ApplicationVariables.appRemoveState = (byte)ApplicationVariables.RemoveState.ServerMapRemove;
        deletePanel.SetActive(true);
    }
    //Activates the info plan window 
    public void InfoPlan()
    {
        MissionManager.planIndex = int.Parse(planName);
        //MissionManager.planName = 
        planSelectionGO.SetActive(false);
        planInfoGO.SetActive(true);
        string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.mission");
        Map m = JsonUtility.FromJson<Map>(json);
        planInfoGO.transform.GetChild(1).GetChild(6).GetComponent<InputField>().text = m.Paths[int.Parse(planName)].Path_metadata.Name;
        planInfoGO.transform.GetChild(1).GetChild(7).GetComponent<InputField>().text = m.Paths[int.Parse(planName)].Path_metadata.Author;

        planInfoGO.transform.GetChild(1).GetChild(8).GetComponent<Text>().text = "" + m.Paths[int.Parse(planName)].Path_metadata.ModificationDate.day + "/" +  m.Paths[int.Parse(planName)].Path_metadata.ModificationDate.month + "/" + m.Paths[int.Parse(planName)].Path_metadata.ModificationDate.year;
        planInfoGO.transform.GetChild(1).GetChild(9).GetComponent<Text>().text = "" + m.Paths[int.Parse(planName)].Count();

        //UnityEngine.Debug.Log("InfoPlan");
    }

    public void EditPlan()
    {
        MissionManager.planIndex = int.Parse(planName);
        //UnityEngine.Debug.Log("EditPlan");
    }

    public void DuplicatePlan()
    {
        MissionManager.planIndex = int.Parse(planName);
        //UnityEngine.Debug.Log("DuplicatePlan");
    }

    public void RemovePlan()
    {
        MissionManager.planIndex = int.Parse(planName);
        //UnityEngine.Debug.Log("RemovePlan");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        timePressStarted = Time.time;
        isPointerDown = true;
        longPressTriggered = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;

        // Short touch
        if (!longPressTriggered)
        {
            //If we clicked a map, we display its plans, if not we access the panel that contains the map info to modify it 
            if (mapInfo)
            {
                if (newMapButton)
                    NewMap();
                else
                    MapButton(guid);
            }
            //Same for plans, but it loads planning or modelAlignment when we select one
            if (planInfo)
            {
                if (newPlanButton)
                    CreatePlan();
                else
                    PlanButton(planName);
            }
            UnityEngine.Debug.Log("Short touch: " + this.name);
        }
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerDown = false;
    }

    // Long touch
    public void LongTouch()
    {
        ShowOpts();
    }

    void MapButton(string id)
    {
        //With a short touch on a map button, this creates a button for each plan
        int it = 0;
        PlanSelectionManager.animationsState = PlanSelectionManager.PanelAnimations.FIRST_PATH;
        Vector2 buttonSize = new Vector2(Screen.width / 5, Screen.height / 4.5f);

        //UnityEngine.Debug.Log(id);
        MissionManager.guid = id;

        string metadataJson = System.IO.File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + id + ".json.metadata");
        MapMetadata metadata = JsonUtility.FromJson<MapMetadata>(metadataJson);
        if (metadata.Map_type == (byte)MapMetadata.MapType.EmptyBox)
            MissionManager.importedModel = false;
        else if (metadata.Map_type == (byte)MapMetadata.MapType.Model3D)
            MissionManager.importedModel = true;

        string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + id + ".json.mission");
        Map m = JsonUtility.FromJson<Map>(json);
        MissionManager.invMatrix = Matrix4x4.Inverse(m.unityToAnchors);
        // First we delete all the plans, just in case somebody changed of map, to delete the previous plans buttons
        foreach (Transform child in BackgroundClicks.plansContainer.transform)
        {
            Destroy(child.gameObject);
        }
        //We get every plan
        PlanSelectionManager.planCount = m.Paths.Count;
        if (m.Paths.Count == 0 && GeneralSceneManager.appState == GeneralSceneManager.ApplicationState.Recording)
            noPlansText.SetActive(true);
        else
            noPlansText.SetActive(false);
        string mapJson = System.IO.File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + metadata.Guid + ".json.mission");
        Map map = JsonUtility.FromJson<Map>(mapJson);
        //We print every plan
        for (it = 0; it < PlanSelectionManager.planCount; it++)
        {
            GameObject button = Instantiate(planButtonPrefab, BackgroundClicks.plansContainer.gameObject.transform);
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(-buttonOffset + (buttonOffset * it), 0);
            button.GetComponent<RectTransform>().sizeDelta = buttonSize;
            button.name = it.ToString();
            button.GetComponent<PlanSelectionButtons>().planName = it.ToString();
            button.GetComponent<PlanSelectionButtons>().pleaseConnectToTheDronePanel = pleaseConnectToTheDronePanel;
            button.GetComponent<PlanSelectionButtons>().downloadingMapPanel = downloadingMapPanel; 
            GameObject nameOfPlanGameObject = new GameObject();
            nameOfPlanGameObject.transform.parent = button.transform;

            //The name the user selected for the map is printed in the left down corner of the button
            Text nameOfPlanText = nameOfPlanGameObject.AddComponent<Text>();
            nameOfPlanText.font = Rubik;
            if (map.Paths[it].Path_metadata.Name.Length > 20)
            {
                nameOfPlanText.text = map.Paths[it].Path_metadata.Name.Substring(0, 20);
            }
            else
                nameOfPlanText.text = map.Paths[it].Path_metadata.Name;
            nameOfPlanText.verticalOverflow = VerticalWrapMode.Overflow;
            nameOfPlanText.transform.SetSiblingIndex(1);
            nameOfPlanText.rectTransform.pivot = new Vector2(0, 0);
            nameOfPlanText.fontSize = Screen.width / 60;
            nameOfPlanText.rectTransform.anchorMin = new Vector2(0, 0f);
            nameOfPlanText.rectTransform.anchorMax = new Vector2(0, 0f);
            nameOfPlanText.rectTransform.sizeDelta = new Vector2(buttonSize.x, Screen.height / 20);
            nameOfPlanText.rectTransform.anchoredPosition = new Vector2(0, -50);


            //button.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => PlanButton(button.name));
        }
        //This is for the create a plan button
        if (GeneralSceneManager.appState == GeneralSceneManager.ApplicationState.Planning) { 
            // Creación de un botón al final para añadir planes
            GameObject createPlanB = Instantiate(planButtonPrefab, BackgroundClicks.plansContainer.gameObject.transform);
            createPlanB.GetComponent<RectTransform>().anchoredPosition = new Vector2(-buttonOffset + (buttonOffset * it), 0);
            createPlanB.GetComponent<RectTransform>().sizeDelta = buttonSize;
            createPlanB.name = "CreatePlan";
            createPlanB.GetComponent<PlanSelectionButtons>().newPlanButton = true;
            //createPlanB.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => CreatePlan());
            GameObject createPlanText = new GameObject();
            createPlanText.transform.parent = createPlanB.transform.GetChild(0);
            createPlanText.name = "CreatePlanText";
            createPlanText.AddComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            createPlanText.AddComponent<Text>().text = "New Plan";
            createPlanText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            createPlanText.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            Text planBText = createPlanText.GetComponent<Text>();
            planBText.font = Rubik;
            planBText.fontSize = Screen.height / 32;
            planBText.alignment = TextAnchor.MiddleCenter;
        }
        else { PlanSelectionManager.planCount--; }
        //If there are more than 3, we create an arrow so the user can cycle between plans
        if (PlanSelectionManager.planCount > 3)
        {
            planRightB.interactable = true;
        }

    }
    IEnumerator FadePanel()
    {
        //Shows and then fades the connect to the drone panel
        float alpha = 1.0f;
        pleaseConnectToTheDronePanel.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
        pleaseConnectToTheDronePanel.GetChild(0).GetComponent<Text>().color = new Color(1, 1, 1, alpha);
        pleaseConnectToTheDronePanel.gameObject.SetActive(true);
        yield return new WaitForSeconds(5f);

        while (pleaseConnectToTheDronePanel.GetComponent<Image>().color.a > 0)
        {
            alpha -= 0.5f;
            pleaseConnectToTheDronePanel.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
            pleaseConnectToTheDronePanel.GetChild(0).GetComponent<Text>().color = new Color(1, 1, 1, alpha);
            yield return new WaitForSeconds(0.1f);
        }
        pleaseConnectToTheDronePanel.gameObject.SetActive(false);

    }
    void PlanButton(string id)
    {
        //This is the button clicked when we select an old plan
        //UnityEngine.Debug.Log(id);
        MissionManager.planIndex = int.Parse(id);
        MissionManager.loadMap = true;
        ClientUnity clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        //If the map doesn't exist, we download it 
        if (!File.Exists(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map") && !File.Exists(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dpl.map"))
        {
            if (clientUnity == null || clientUnity.client == null || !clientUnity.client.isConnected)
            {
                //UnityEngine.Debug.Log("Please connect to the drone to download this map"); // ## TODO: Mostrar mensaje al usuario
                StartCoroutine(FadePanel());

            }
            else
            {
                downloadingMapPanel.SetActive(true);
                downloadingMapPanel.transform.GetChild(0).GetComponent<Text>().text = "Downloading Map";
                PlanSelectionManager.askedForMaps = true;
                clientUnity.client.sendTwoPartCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_MAP, MissionManager.guid + "\0");
                // ##TODO Add the downloadign map that already exist and load it here

            }
            return;
        }
        if(GeneralSceneManager.appState == GeneralSceneManager.ApplicationState.Planning){
            GameObject.Find("UIManager").GetComponent<UIManager>().LoadCustomScene("Planning");
            //SceneManager.LoadScene("Planning");
        }
        else if (GeneralSceneManager.appState == GeneralSceneManager.ApplicationState.Recording) {
            GameObject.Find("UIManager").GetComponent<UIManager>().LoadCustomScene("ModelAlignment");
            //SceneManager.LoadScene("ModelAlignment");
        }
    }

    // Función que pasa a la ventana de creación de un plan
    void CreatePlan()
    {
        //UnityEngine.Debug.Log("Create Plan");
        planSelectionGO.SetActive(false);
        planInfoGO.SetActive(true);

       
       
        planInfoGO.transform.GetChild(1).GetChild(8).GetComponent<Text>().text = "" + DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year;
        //Number of points
        planInfoGO.transform.GetChild(1).GetChild(9).GetComponent<Text>().text = "" + 0;
    }

    void NewMap()
    {
        //Creates a new map with the current date and the user can modify the other parameters
        planSelectionGO.SetActive(false);
        MissionManager.modificationText = "" + DateTime.Now.Day + '/' + DateTime.Now.Month + '/' + DateTime.Now.Year.ToString();
        MissionManager.mapName ="MapName";
        MissionManager.mapLocation = "Location";
        mapInfoGO.transform.parent.GetComponent<PlanSelectionManager>().enabled = false;
        Transform parent = this.transform.parent;
        while (parent.parent != null)
        {
            parent = parent.parent;
        }
        parent.GetComponent<MissionManager>().enabled = false;
        parent.GetComponent<MissionManager>().enabled = true;

        newMissionGO.SetActive(true);
        //mapInfoGO.SetActive(true);

    }
}
