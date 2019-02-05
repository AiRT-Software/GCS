using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Xml;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;
using System;

public class PlaceModel : MonoBehaviour {

    enum CalibrationState
    {
        startState = 0,
        anchorsCalibrated,
        daeModelLoaded,
        aligningModel,
        finishedAlignment
    }

    CalibrationState state = CalibrationState.startState;
    //Variable created just in case the app demands a file to the server and later distinguish that it was that app the one who make it
    public static bool pointCloudAskedForMetadata = false;
    //Contains the 3d model
    GameObject container;
    //In this texture the 3d rendered thumbnail is saved
    public RenderTexture modelRenderer;
    public GameObject anchorPrefab, anchorParent, anchorIDParent;
    public RectTransform scrollViewRect, contentRect;
    //The sliders rectransforms that handles the transforms
    public RectTransform sliderZoomRect, sliderPitchRect, sliderYawRect, sliderRollRect, posXInputRect, posYInputRect, posZInputRect;
    //SaveMatrixbutton and loadmatrixbutton aren't here anymore
    public RectTransform saveMatrixButton, loadMatrixButton, planningButton;
    //The sliders and inputfields that handle the transforms
    Slider sliderZoom, sliderPitch, sliderYaw, sliderRoll;
    InputField posXInput, posYInput, posZInput;
    // Compute shader reference
    public ComputeShader cs_create_triangles;
    //Loading model
    public GameObject LoadingPanel;
    int anchorID = 1;
    ClientUnity clientUnity;

    Vector3 minAnchorsBoundingBox;
    Vector3 maxAnchorsBoundingBox;
    int fontSize;

    GameObject[] anchors = new GameObject[8];
    RectTransform[] anchorIDs = new RectTransform[8];
    Vector2[] anchorIDPos = new Vector2[8];
    int hex = -1;

    bool mapAligned = false;
    //twice
    GameObject modelLoadingPanel;

    // Uniforms
    private ComputeBuffer points;
    private ComputeBuffer normals;
    private ComputeBuffer triangles;
    private ComputeBuffer base_triangle;
    private BaseTriangle baseTriValues;
    public GameObject pcPrefab;
    public GameObject prefab_cloud_go;
    void Awake()
    {

        
        modelLoadingPanel = GameObject.Find("ModelLoadingPanel");
        modelLoadingPanel.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, Screen.height / 7);
        modelLoadingPanel.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 1.5f, Screen.height / 3);

        modelLoadingPanel.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -Screen.height / 7);
        modelLoadingPanel.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.height / 8, Screen.height / 8);

        modelLoadingPanel.transform.GetChild(0).GetComponent<Text>().fontSize = (int)(Screen.width * 0.05f);
        //We wait here for the collada to be loaded, and once it's loaded, the loading panel disappears
        StartCoroutine(WaitForModel());

        GeneralSceneManager.sceneState = GeneralSceneManager.SceneState.ModelAlignment;

        BinaryFormatter bFormatter = new BinaryFormatter();



        // Open the file using the path

        string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.metadata");

        MapMetadata metadata = JsonUtility.FromJson<MapMetadata>(json);

        if (metadata.Map_type == (byte)MapMetadata.MapType.PointCloud)
        {
            //If it's a pointcloud, we use the shader to load a previous saved map
            GameObject auxGameObject = new GameObject();
            auxGameObject.name = "DaemodelChild";

            FileStream file = File.OpenRead(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dpl.map");
            createBaseTriangle();
            byte[] pclBytes = File.ReadAllBytes(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dpl.map");
            GameObject daeModel = new GameObject();
            daeModel.name = "DaeModel";
            daeModel.transform.parent = auxGameObject.transform;
            LoadPointCloud(pclBytes, ref daeModel);
        }
        else
        {
            //If it's not we used the collada loader
            FileStream file = File.OpenRead(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map");

            //var xmlWriter = new XmlTextWriter(file, Encoding.UTF8);
            //xmlWriter.
            XmlDocument doc = new XmlDocument();
            string aux;
            using (StreamReader reader = new StreamReader(file))
            {
                aux = reader.ReadToEnd();
            }
            doc.LoadXml(aux);

            if (MissionManager.importedModel)
                LoadDOC(doc, Shader.Find("Custom/ObjectsShader"));
            else
                LoadDOC(doc, Shader.Find("Airt/CullBackShader"));
        }
        
    }
    void LoadPointCloud(byte[] pcl, ref GameObject daeModel)
    {
        int numDrawn = 0;
        //PCLMsg pclmsg = queue.Dequeue();

        //More or less is the same as in PointCloud.cs, only that we have to deal with a header from the main pointcloud

        PCLMsgHeader cm = new PCLMsgHeader();
        string cabezeraBasura = BitConverter.ToString(pcl, 0, 4);
        UInt64 numberOfPCL = BitConverter.ToUInt64(pcl, 28);
        PCLMesh[] mesh;
        mesh = new PCLMesh[numberOfPCL];
        //We use this one to get the size of the pointcloud that we are loading
        int actualPointCloudSize = 0;
        //And this one is the last pointcloud which we'll use as an offset
        int pastPointCloudSize = 0;
        //36 is the header's size, so we add it as an offset always
        for (UInt64 i = 0; i < numberOfPCL; i++)
        {
            //First each pointcloud starts with an id, then the position and then the number of points. Here the number of points is an uint64, while in pointcloud it was an int
            string firma = BitConverter.ToString(pcl, 36 + pastPointCloudSize, 4);
            actualPointCloudSize += 4;
            cm.x = BitConverter.ToSingle(pcl, 36 + pastPointCloudSize + 4);
            actualPointCloudSize += 4;

            cm.y = BitConverter.ToSingle(pcl, 36 + pastPointCloudSize + 8);
            actualPointCloudSize += 4;

            cm.z = BitConverter.ToSingle(pcl, 36 + pastPointCloudSize + 12);
            actualPointCloudSize += 4;

            cm.pitch = BitConverter.ToSingle(pcl, 36 + pastPointCloudSize + 16);
            actualPointCloudSize += 4;

            cm.roll = BitConverter.ToSingle(pcl, 36 + pastPointCloudSize + 20);
            actualPointCloudSize += 4;

            cm.yaw = BitConverter.ToSingle(pcl, 36 + pastPointCloudSize + 24);
            actualPointCloudSize += 4;

            UInt64 numberOfPoints = BitConverter.ToUInt32(pcl, 36 + pastPointCloudSize + 28);
            actualPointCloudSize += 8;


            mesh[i] = new PCLMesh();
            fillMesh(ref mesh[i], pcl, cm, numberOfPoints, pastPointCloudSize, ref actualPointCloudSize);
            // Instanciate pointclouds in gameobjects
            //From here it's the same as in pointcloud.cs 
            GameObject instance = GameObject.Instantiate(prefab_cloud_go, this.transform, true);
            instance.name = "cloudgo" + numDrawn.ToString();
            instance.GetComponent<MeshFilter>().sharedMesh = new Mesh();
            instance.GetComponent<MeshFilter>().sharedMesh.name = "cmesh" + numDrawn.ToString();

            /*
            instance.GetComponent<MeshFilter>().sharedMesh.vertices = cm.points;
            instance.GetComponent<MeshFilter>().sharedMesh.SetIndices(cm.indices, MeshTopology.Points, 0, true);
            instance.GetComponent<MeshFilter>().sharedMesh.normals = cm.normals;
            instance.GetComponent<MeshFilter>().sharedMesh.colors = cm.colors;
            */
            //UnityEngine.Debug.Log("Puntets: " + cm.points[0]);
            runComputeShader(ref mesh[i].points, ref mesh[i].normals);
            Triangle[] result = new Triangle[mesh[i].points.Length];
            triangles.GetData(result);  //GPU-> CPU
            releaseCSBuffers();  // It is necessary to release compute buffers after using them

            // insert the compute shader results in vectors
            // TODO: from compute shader write directly in a buffer[numpoints*3] instead of buffer.p1, buffer.p2, buffer.3
            Vector3[] allVertices = new Vector3[mesh[i].points.Length * 3];
            Vector3[] allNormals = new Vector3[mesh[i].points.Length * 3];
            Color[] allColors = new Color[mesh[i].points.Length * 3];
            int[] allIndices = new int[mesh[i].points.Length * 3];

            for (int k = 0, l = 0; k < mesh[i].points.Length * 3; k += 3, l++)
            {
                allVertices[k] = result[l].p1;
                allVertices[k + 1] = result[l].p2;
                allVertices[k + 2] = result[l].p3;
                //CalculateBounds(allVertices[k], allVertices[k + 1], allVertices[k + 2]);

                allIndices[k] = k;
                allIndices[k + 1] = k + 1;
                allIndices[k + 2] = k + 2;

                allNormals[k] = mesh[i].normals[l];
                allNormals[k + 1] = mesh[i].normals[l];
                allNormals[k + 2] = mesh[i].normals[l];

                allColors[k] = mesh[i].colors[l];
                allColors[k + 1] = mesh[i].colors[l];
                allColors[k + 2] = mesh[i].colors[l];
            }

            //TODO: replace de loop above by using Graphics.DrawProceduralIndirect (on a Camera script) and an appendBuffer in other CompShader (consumer)
            // attach vertices, colors, and normals to the mesh
            instance.GetComponent<MeshFilter>().sharedMesh.vertices = allVertices;
            instance.GetComponent<MeshFilter>().sharedMesh.SetIndices(allIndices, MeshTopology.Triangles, 0, true);
            instance.GetComponent<MeshFilter>().sharedMesh.normals = allNormals;
            instance.GetComponent<MeshFilter>().sharedMesh.colors = allColors;
            instance.GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();

            // Converting coords from Pozyx to Unity, mm to meters, and angles from right-handed to left-handed
            Matrix4x4 toUnityCoordsMat = Matrix4x4.TRS(new Vector3(cm.x * 0.001f, cm.z * 0.001f, cm.y * 0.001f),
                Quaternion.Euler(cm.pitch * -180f / 3.141592f, cm.yaw * -180f / 3.141592f, cm.roll * -180f / 3.141592f),
                new Vector3(1, 1, 1));

            instance.transform.rotation = toUnityCoordsMat.rotation;
            instance.transform.position = new Vector3(toUnityCoordsMat.m03, toUnityCoordsMat.m13, toUnityCoordsMat.m23);

            //instance.transform.RotateAround(drone.transform.position, new Vector3(1, 0, 0), toUnityCoordsMat.rotation.eulerAngles.x);
            //instance.transform.RotateAround(drone.transform.position, new Vector3(0, 1, 0), -toUnityCoordsMat.rotation.eulerAngles.y);
            //instance.transform.RotateAround(drone.transform.position, new Vector3(0, 0, 1), toUnityCoordsMat.rotation.eulerAngles.z);
            //UnityEngine.Debug.Log(toUnityCoordsMat);

            //instance.transform.localScale = new Vector3(-1 * instance.transform.localScale.x, instance.transform.localScale.y, instance.transform.localScale.z);

            //instance.name = "cloudgo" + pclmsg.cm.pointCloudID.i + "" + pclmsg.cm.pointCloudID.j + "" + pclmsg.cm.pointCloudID.k + "" + pclmsg.cm.pointCloudID.heading;


            instance.transform.parent = daeModel.transform;
            //Here we tell where the next pointcloud will begin
            pastPointCloudSize = actualPointCloudSize;

        }









    }


    public void fillMesh(ref PCLMesh mesh, byte[] data_bytes, PCLMsgHeader header, UInt64 num_points, int offset_data, ref int tamañoPointCloudActual)
    {
        //UnityEngine.Debug.Log("offset data " + offset_data);
        //This is the same function that filled the pointcloud mesh in pointcloud, with the only difference that we add another offset because the data is in another position
        UInt64 N; //20000 * 3 < 65000 points allowed in Unity
        if (num_points > 20000) { N = 20000; }
        else { N = num_points; }
        UnityEngine.Debug.Log(N);
        mesh.points = new Vector3[N];
        mesh.normals = new Vector3[N];
        mesh.colors = new Color[N];
        mesh.indices = new int[N];

        Matrix4x4 toUnityCoordsMat = Matrix4x4.TRS(new Vector3(header.x * 0.001f, header.z * 0.001f, header.y * 0.001f),
               Quaternion.Euler(-header.pitch, -header.yaw, -header.roll),
               new Vector3(1, 1, 1));

        for (int i = 0, byte_index = 8 + offset_data; i < (int)N; byte_index += PCLMsgOffsets.POINTNORMAL_SIZE, i++)
        {
            mesh.points[i].x = BitConverter.ToSingle(data_bytes, 36 + byte_index + PCLMsgOffsets.POINT_X);
            tamañoPointCloudActual += 4;
            mesh.points[i].y = BitConverter.ToSingle(data_bytes, 36 + byte_index + PCLMsgOffsets.POINT_Z); // Changing from Pozyx to Unity
            tamañoPointCloudActual += 4;

            mesh.points[i].z = BitConverter.ToSingle(data_bytes, 36 + byte_index + PCLMsgOffsets.POINT_Y);
            tamañoPointCloudActual += 4;

#if TRANSFORM_COORDS
            points[i] = toUnityCoordsMat.MultiplyPoint(points[i]);
#elif POZYX_TRANSFORM_COORDS
            //points[i] = toUnityCoordsMat.MultiplyVector(points[i]);
#endif
            mesh.colors[i].r = data_bytes[36 + byte_index + PCLMsgOffsets.POINT_R] / 255f;
            mesh.colors[i].g = data_bytes[36 + byte_index + PCLMsgOffsets.POINT_G] / 255f;
            mesh.colors[i].b = data_bytes[36 + byte_index + PCLMsgOffsets.POINT_B] / 255f;
            //colors[i].a = msg_bytes[byte_index + PCLMsgOffsets.POINT_A] / 255f;
            mesh.colors[i].a = 1f;
            tamañoPointCloudActual += 4;

            //normals[i] = new Vector3(0.1f, 0f, 0.9f).normalized;
            mesh.normals[i].x = BitConverter.ToSingle(data_bytes, 36 + byte_index + PCLMsgOffsets.NORMAL_X);
            tamañoPointCloudActual += 4;
            mesh.normals[i].y = BitConverter.ToSingle(data_bytes, 36 + byte_index + PCLMsgOffsets.NORMAL_Z);
            tamañoPointCloudActual += 4;
            mesh.normals[i].z = BitConverter.ToSingle(data_bytes, 36 + byte_index + PCLMsgOffsets.NORMAL_Y);
            tamañoPointCloudActual += 4;

            mesh.indices[i] = i;
        }
    }
    void OnEnable()
    {
        Vector2 sliderSize, inputFieldSize, buttonSize, buttonSizeXL;
        float spacing = Screen.width / 50;

        //anchorParent.SetActive(true);
        //anchorIDParent.SetActive(true);
        anchorIDParent.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);

        minAnchorsBoundingBox = Vector3.positiveInfinity;
        maxAnchorsBoundingBox = Vector3.negativeInfinity;
        //When enabled, this scripts places the anchors
        for (int i = 0; i < CalibrationSettings.anchorConfigData.Length; i++)
        {
            Vector3 anchorPos = new Vector3(CalibrationSettings.anchorConfigData[i].position.x, CalibrationSettings.anchorConfigData[i].position.z, CalibrationSettings.anchorConfigData[i].position.y);
            anchors[i] = Instantiate(anchorPrefab, anchorPos * 0.001f, Quaternion.identity, anchorParent.transform);
            anchors[i].name = CalibrationSettings.anchorConfigData[i].id.ToString();
            anchorIDs[i] = anchorIDParent.transform.GetChild(i).GetComponent<RectTransform>();
            //if (!int.TryParse(anchors[i].name, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out hex))
            //{
            //    UnityEngine.Debug.LogWarning("Unable to parse anchor Value!");
            //}
            //anchorIDs[i].GetComponent<Text>().text = "0x" + hex.ToString("X");
            anchorIDs[i].GetComponent<Text>().text = "0x" + int.Parse(anchors[i].name).ToString("x");
            state = CalibrationState.anchorsCalibrated;
            CheckAnchorBounds(anchorPos * 0.001f);
        }

        MissionManager.anchorsBoundingBox = maxAnchorsBoundingBox - minAnchorsBoundingBox;
        MissionManager.anchorBoundsCenter = (maxAnchorsBoundingBox + minAnchorsBoundingBox) / 2;

        sliderZoom = sliderZoomRect.GetComponent<Slider>();
        sliderPitch = sliderPitchRect.GetComponent<Slider>();
        sliderYaw = sliderYawRect.GetComponent<Slider>();
        sliderRoll = sliderRollRect.GetComponent<Slider>();
        posXInput = posXInputRect.GetComponent<InputField>();
        posYInput = posYInputRect.GetComponent<InputField>();
        posZInput = posZInputRect.GetComponent<InputField>();

        sliderSize = new Vector2(Screen.width / 6, Screen.height / 50);
        inputFieldSize = new Vector2(Screen.width / 6, Screen.height / 30);
        buttonSize = new Vector2(Screen.width / 6, Screen.height / 25);
        buttonSizeXL = new Vector2(Screen.width / 8, Screen.height / 16);
        fontSize = (int)(Screen.width * 0.015);


        container = GameObject.Find("DaeModel");
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        if ((clientUnity != null) && (clientUnity.client != null) && clientUnity.client.isConnected)
            clientUnity.client.SendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_SYSTEM);
        else
            UnityEngine.Debug.LogWarning("Unable to send IPS System message");

        scrollViewRect.sizeDelta = new Vector2(Screen.width / 4, 3 * Screen.height / 4);
        scrollViewRect.anchoredPosition = new Vector2(50, scrollViewRect.anchoredPosition.y);

        contentRect.GetChild(0).GetComponent<VerticalLayoutGroup>().spacing = spacing;

        sliderZoomRect.sizeDelta = sliderSize;
        sliderPitchRect.sizeDelta = sliderSize;
        sliderYawRect.sizeDelta = sliderSize;
        sliderRollRect.sizeDelta = sliderSize;
        posXInputRect.sizeDelta = inputFieldSize;
        posYInputRect.sizeDelta = inputFieldSize;
        posZInputRect.sizeDelta = inputFieldSize;
        //saveMatrixButton.sizeDelta = buttonSize;
        //loadMatrixButton.sizeDelta = buttonSize;
        planningButton.anchoredPosition = new Vector2(6 * Screen.width / 8,  Screen.height / 7);
        planningButton.sizeDelta = buttonSizeXL;

        contentRect.sizeDelta = new Vector2(0, (sliderSize.y * 4) + (inputFieldSize.y * 3) + (buttonSize.y * 2) + buttonSizeXL.y + (spacing * 11));

        sliderZoomRect.GetChild(3).GetComponent<Text>().fontSize = fontSize;
        sliderPitchRect.GetChild(3).GetComponent<Text>().fontSize = fontSize;
        sliderYawRect.GetChild(3).GetComponent<Text>().fontSize = fontSize;
        sliderRollRect.GetChild(3).GetComponent<Text>().fontSize = fontSize;
        //loadMatrixButton.GetChild(0).GetComponent<Text>().fontSize = fontSize;
        //saveMatrixButton.GetChild(0).GetComponent<Text>().fontSize = fontSize;
        planningButton.GetChild(0).GetComponent<Text>().fontSize = fontSize;

        string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.mission");
        Map map = JsonUtility.FromJson<Map>(json);
// ## TODO CHECKEAR SI ESTA ALINEADO CON LAS ANCLAS
        mapAligned = false;
        //Shouldn't the add listener go here then?
        sliderZoom.value = map.unityToAnchors.m00 * 0.001f;
        sliderPitch.value = (int)(map.unityToAnchors.rotation.eulerAngles.x / 90);
        sliderYaw.value = (int)(map.unityToAnchors.rotation.eulerAngles.y / 90);
        sliderRoll.value = (int)(map.unityToAnchors.rotation.eulerAngles.z / 90);
        posXInput.text = (map.unityToAnchors.m03 * 0.001f).ToString();
        posYInput.text = (map.unityToAnchors.m23 * 0.001f).ToString();
        posZInput.text = (map.unityToAnchors.m13 * 0.001f).ToString();
        //We add the listener now, just in case we changed the values before (when placing the model) it doesn't activate itself and destroy the rotations
        sliderPitch.onValueChanged.AddListener(OnSliderChangedRotate);
        sliderYaw.onValueChanged.AddListener(OnSliderChangedRotate);
        sliderRoll.onValueChanged.AddListener(OnSliderChangedRotate);
        sliderZoom.onValueChanged.AddListener(OnSliderChanged);
        //container.transform.localScale = new Vector3(sliderZoom.value, sliderZoom.value, sliderZoom.value);
    }
    private void FindShader(string shaderName)
    {
        //This deactivates the shader that erases the closest fragment to the camera
        int count = 0;

        List<Material> armat = new List<Material>();

        Renderer[] arrend = (Renderer[])Resources.FindObjectsOfTypeAll(typeof(Renderer));
        foreach (Renderer rend in arrend)
        {
            foreach (Material mat in rend.sharedMaterials)
            {
                if (!armat.Contains(mat))
                {
                    armat.Add(mat);
                }
            }
        }

        foreach (Material mat in armat)
        {
            if (mat != null && mat.shader != null && mat.shader.name != null && mat.shader.name == shaderName)
            {
                mat.SetFloat("_Discard", 0);

            }
        }


    }
    void OnDisable()
    {
        if(anchorIDParent)
            anchorIDParent.SetActive(false);

        if(anchorParent)
            anchorParent.SetActive(false);
    }

    void FixedUpdate()
    {

        switch (state)
        {
            case CalibrationState.startState:
                break;
            case CalibrationState.anchorsCalibrated:
                if (GameObject.Find("DaeModel") != null)
                {
                    Camera.main.transform.position = new Vector3(MissionManager.anchorBoundsCenter.x, MissionManager.anchorBoundsCenter.y + MissionManager.anchorsBoundingBox.y * 4, MissionManager.anchorBoundsCenter.z);
                    state = CalibrationState.daeModelLoaded;
                }
                break;
            case CalibrationState.daeModelLoaded:
                //This should be done always
                if(!mapAligned)
                    PreScaleModel();
                state = CalibrationState.aligningModel;
                anchorIDParent.SetActive(true);
                anchorParent.SetActive(true);
                break;
            case CalibrationState.aligningModel:
               
                modelLoadingPanel.transform.GetChild(1).Rotate(0, 0, -200 * Time.deltaTime);
                
                if (SendJSONStateMachine.allFilesSent == true)
                    state = CalibrationState.finishedAlignment;
                break;
            case CalibrationState.finishedAlignment:
                //UnityEngine.Debug.Log("FinishedAlignment");
                state = CalibrationState.startState;
                SendJSONStateMachine.allFilesSent = false;
                LibraryModule.serverMapExists = false;
                mapAligned = false;
                pointCloudAskedForMetadata = false;
                SceneManager.LoadScene("Recording");
                
                break;
            default:
                UnityEngine.Debug.LogWarning("Unhandled alignment state");
                break;
        }

        for (int i = 0; i < anchors.Length; i++)
        {
            if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), anchors[i].GetComponent<Collider>().bounds))
            {
                anchorIDPos[i] = Camera.main.WorldToScreenPoint(anchors[i].transform.position);
                anchorIDs[i].anchoredPosition = new Vector2(anchorIDPos[i].x - 125, anchorIDPos[i].y + 25);
            }
            else
            {
                anchorIDs[i].anchoredPosition = new Vector2(-400, -400);
            }
        }

        //anchor1ID.anchoredPosition = new Vector2()
    }

    public void Change(GameObject aux)
    {
        container = aux;
        
    }

    public void LoadDOC(XmlDocument doc, Shader shader)
    {

        GameObject importedGO = new GameObject();
        ColladaImporter importer = new ColladaImporter(ref importedGO);
        StartCoroutine(importer.getMesh(doc, shader));
        importedGO.name = "DaeModelChild";
        
        //DontDestroyOnLoad(importedGO);
        //if (!Directory.Exists(Application.dataPath + "/Resources/" + "bb8.fbx") && Directory.Exists(Application.dataPath))
        //GameObject aux = Instantiate(File.OpenRead(path)) as GameObject;
        //Canvas.GetComponent<PlaceModel>().Change(mesh);

        //GameObject aux = Instantiate(AssetBundle.LoadFromFile(path) as GameObject;
        // Convert the file from a byte array into a string
        //string fileData = bFormatter.Deserialize(file) as string;
        //Mesh mesh = FastObjImporter.Instance.ImportFile(path);

    }
    //Slider for the zoom
    public void OnSliderChanged(float zoom)
    {
        if (!container)
            container = GameObject.Find("DaeModel");
        else
            container.transform.localScale = new Vector3(zoom, zoom, zoom);
    }
    //Slider for the rotations
    public void OnSliderChangedRotate(float algo)
    {
        sliderPitch.onValueChanged.RemoveAllListeners();
        sliderYaw.onValueChanged.RemoveAllListeners();
        sliderRoll.onValueChanged.RemoveAllListeners();
        if (!container)
            container = GameObject.Find("DaeModel");
        else
            container.transform.rotation = Quaternion.Euler(sliderPitch.value * 90, sliderYaw.value * 90, sliderRoll.value * 90);
        sliderPitch.onValueChanged.AddListener(OnSliderChangedRotate);
        sliderYaw.onValueChanged.AddListener(OnSliderChangedRotate);
        sliderRoll.onValueChanged.AddListener(OnSliderChangedRotate);
        
        //sliderYawn.value = container.transform.localRotation.eulerAngles.y;
        //sliderRoll.value = container.transform.localRotation.eulerAngles.z;
    }
   //This is calle by the inputfields
    public void OnSliderChangedPos()
    {

        if (posXInput.text.Equals(""))
        {
            posXInput.text = "0";
        }
        if (posYInput.text.Equals(""))
        {
            posYInput.text = "0";
        }
        if (posZInput.text.Equals(""))
        {
            posZInput.text = "0";
        }
        if (!container)
            container = GameObject.Find("DaeModel");
        else
            container.transform.position = new Vector3(float.Parse(posXInput.text), float.Parse(posYInput.text), float.Parse(posZInput.text));
        //Camera.main.transform.position = new Vector3(container.transform.position.x, container.transform.position.y + 1, container.transform.position.z - 10);
    }
    //public void SaveJson()
    //{
    //    Matrix4x4 aux = new Matrix4x4();
    //    aux.SetTRS(container.transform.position, container.transform.localRotation, container.transform.localScale);
    //    UnityEngine.Debug.Log(aux);
    //    string json = JsonUtility.ToJson(aux);
    //    File.WriteAllText("/json.json", json);
    //}
    //public void LoadJson()
    //{
    //    string jsonString = File.ReadAllText("/json.json");
    //    Matrix4x4 matr = JsonUtility.FromJson<Matrix4x4>(jsonString);
    //    container.transform.localScale = matr.lossyScale;
    //    container.transform.rotation = Quaternion.Euler(0, 0, 0);
    //    container.transform.Rotate(matr.rotation.eulerAngles);
    //    UnityEngine.Debug.Log(matr);
    //
    //    container.transform.position = new Vector3(0, 0, 0);
    //    container.transform.position = matr.MultiplyPoint3x4(container.transform.position);
    //    sliderZoom.value = container.transform.localScale.x;
    //
    //    sliderPitch.onValueChanged.RemoveAllListeners();
    //    sliderYaw.onValueChanged.RemoveAllListeners();
    //    sliderRoll.onValueChanged.RemoveAllListeners();
    //    sliderPitch.value = container.transform.localRotation.eulerAngles.x / 90;
    //    sliderYaw.value = container.transform.localRotation.eulerAngles.y / 90;
    //    sliderRoll.value = container.transform.localRotation.eulerAngles.z / 90;
    //
    //    sliderPitch.onValueChanged.AddListener(OnSliderChangedRotate);
    //    sliderYaw.onValueChanged.AddListener(OnSliderChangedRotate);
    //    sliderRoll.onValueChanged.AddListener(OnSliderChangedRotate);
    //
    //    posXInput.text = container.transform.position.x.ToString() ;
    //    posYInput.text = container.transform.position.y.ToString();
    //    posZInput.text = container.transform.position.z.ToString();
    //    //Camera.main.transform.position = new Vector3(container.transform.position.x, container.transform.position.y + 1, container.transform.position.z - 10);
    //
    //
    //}
    //Fixes the boundign box
    void CheckAnchorBounds(Vector3 anchorPos)
    {
        if (anchorPos.x < minAnchorsBoundingBox.x)
            minAnchorsBoundingBox.x = Mathf.Abs(anchorPos.x);
        if (anchorPos.y < minAnchorsBoundingBox.y)
            minAnchorsBoundingBox.y = Mathf.Abs(anchorPos.y);
        if (anchorPos.z < minAnchorsBoundingBox.z)
            minAnchorsBoundingBox.z = Mathf.Abs(anchorPos.z);

        if (anchorPos.x > maxAnchorsBoundingBox.x)
            maxAnchorsBoundingBox.x = Mathf.Abs(anchorPos.x);
        if (anchorPos.y > maxAnchorsBoundingBox.y)
            maxAnchorsBoundingBox.y = Mathf.Abs(anchorPos.y);
        if (anchorPos.z > maxAnchorsBoundingBox.z)
            maxAnchorsBoundingBox.z = Mathf.Abs(anchorPos.z);
    }
    //Preescales a model if the bounding box is too big
    void PreScaleModel()
    {
        Vector3 boundingBox = MissionManager.modelBoundingBox;
        float scale = 1.0f;
        if (boundingBox.x < boundingBox.y && boundingBox.x < boundingBox.z) // Bounding box min is X
            scale = MissionManager.anchorsBoundingBox.x / boundingBox.x;

        if (boundingBox.y < boundingBox.x && boundingBox.y < boundingBox.z) // Bounding box min is Y
            scale = MissionManager.anchorsBoundingBox.y / boundingBox.y;

        if (boundingBox.z < boundingBox.x && boundingBox.z < boundingBox.y) // Bounding box min is Z
            scale = MissionManager.anchorsBoundingBox.z / boundingBox.z;

        GameObject model = GameObject.Find("DaeModel");
        //model.transform.localScale = new Vector3(scale, scale, scale);
        //model.transform.position += MissionManager.anchorBoundsCenter;
    }
    /// <summary>
    /// The Next button in model alignment goes here. It builds the map with the matrix produced by the alignment
    /// </summary>
    public void BuildAlignedScene()
    {
        //If it is a 3d map 
        if (MissionManager.importedModel)
            RebuildImportedScene();
        else
            RebuildEmptyScene();
        //\ if it is a box or a pointcloud
    }

    void RebuildEmptyScene()
    {
        string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.mission");
        Map map = JsonUtility.FromJson<Map>(json);
        //If we want to check if the model alignment was applied, first we would need to upload the anchors and when loading the file, check that the anchors are at the same position. Until then, thsi is ignored
        map.Alignment_applied = true;
        GameObject model = GameObject.Find("DaeModel");
        // ##CAMBIAR Z POR Y, ROTACION NEGATIVA 
        if (model)
            map.unityToAnchors.SetTRS(new Vector3(model.transform.position.x, model.transform.position.z, model.transform.position.y) * 1000, model.transform.localRotation, model.transform.localScale);
        else
            map.unityToAnchors = Matrix4x4.identity;

        map.unityToAnchors.m00 *= 1000;
        map.unityToAnchors.m11 *= 1000;
        map.unityToAnchors.m22 *= 1000;

        //UnityEngine.Debug.Log("Transf Matrix * (0,0,0) = " + (map.unityToAnchors * new Vector3(0, 0, 0)));

        MissionManager.invMatrix = Matrix4x4.Inverse(map.unityToAnchors);
        string mapJson = JsonUtility.ToJson(map);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + map.Guid + ".json.mission", mapJson);
        Destroy(model);
        string file = File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.metadata");
        MapMetadata metadata = JsonUtility.FromJson<MapMetadata>(file);
        if (metadata.Map_type == (byte)MapMetadata.MapType.PointCloud)
        {
            pointCloudAskedForMetadata = true;

            // clientUnity.client.sendTwoPartCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_METADATA, MissionManager.guid + "\0");

        }
        // else
        // {
        //We check if the map is uploaded, if it is, we check if the metadata is uploaded. If one of them isn't we upload the whole plan
        PlanSelectionManager.askedForMaps = true;

        clientUnity.client.sendTwoPartCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_MAP, MissionManager.guid + "\0");
        LoadingPanel.SetActive(true);
        LoadingPanel.transform.GetChild(0).GetComponent<Text>().text = "Uploading Plan";

        //}     
        //SceneManager.LoadScene("Recording");
    }

    void RebuildImportedScene()
    {
        string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.mission");
        Map map = JsonUtility.FromJson<Map>(json);
        map.Alignment_applied = true;
        GameObject model = GameObject.Find("DaeModel");
        if (model)
            map.unityToAnchors.SetTRS(new Vector3(model.transform.position.x, model.transform.position.z, model.transform.position.y) * 1000, model.transform.localRotation, model.transform.localScale);
        else
            map.unityToAnchors = Matrix4x4.identity;

        map.unityToAnchors.m00 *= 1000;
        map.unityToAnchors.m11 *= 1000;
        map.unityToAnchors.m22 *= 1000;

        UnityEngine.Debug.Log(map.unityToAnchors + " * " + new Vector4(-1.45942223072052f, -0.00012969970703125f, 2.9674997329711916f, 1) + " = " + (map.unityToAnchors * new Vector4(-1.45942223072052f, -0.00012969970703125f, 2.9674997329711916f, 1)));

        MissionManager.invMatrix = Matrix4x4.Inverse(map.unityToAnchors);

        string mapJson = JsonUtility.ToJson(map);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + map.Guid + ".json.mission", mapJson);
        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Thumbnails"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Thumbnails");
        transform.GetChild(0).gameObject.SetActive(false);
        //This renders an image for a thumbnail
        StartCoroutine(RenderTexture());
    }

    IEnumerator RenderTexture()
    {
        GameObject Helper = GameObject.Find("DaeModel");
        //Camera.main.transform.position = new Vector3(Helper.transform.position.x, Helper.transform.position.y + MissionManager.modelBoundingBox.y * 2, Helper.transform.position.z);
        //Vector3 auxPosition = Camera.main.transform.position;
        //float fieldOfVIewHelper = Camera.main.fieldOfView;
        //float futureFOV = 2.0f * Mathf.Atan((Camera.main.transform.position.y - (Helper.transform.position.y - MissionManager.modelBoundingBox.y)) * 0.5f / Vector3.Distance(Camera.main.transform.position, Helper.transform.position)) * Mathf.Rad2Deg;
        ////It can happen that the fov is really small or not a number
        //if (float.IsNaN(futureFOV))
        //{
        //    Camera.main.fieldOfView = 40;
        //    Camera.main.transform.position = new Vector3(Helper.transform.position.x, Helper.transform.position.y + 2, Helper.transform.position.z);
        //
        //}
        //else
        //{
        //    Camera.main.fieldOfView = futureFOV;
        //}
        //Resolution auxResolution = Screen.currentResolution;
        //Screen.SetResolution(256, 256, true);
        yield return new WaitForEndOfFrame();
        //Camera.main.targetTexture = modelRenderer;
        //
        //Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        //Camera.main.Render();
        //texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        //texture.Apply();
        //byte[] bytes = texture.EncodeToPNG();
        //
        //// save the image
        //File.WriteAllBytes(Application.persistentDataPath + "/PersistentData/Thumbnails/" + MissionManager.guid + ".jpeg.thumbnail", bytes);
        //DestroyObject(texture);
        //Camera.main.targetTexture.Release();
        //Camera.main.targetTexture = null;
        //Destroy(GameObject.Find("DaeModel"));
        //Screen.SetResolution(auxResolution.width, auxResolution.height, true);
        //Camera.main.fieldOfView = fieldOfVIewHelper;
        //Camera.main.transform.position = auxPosition;

        //SceneManager.LoadScene("Recording");
        string file = File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.metadata");
        MapMetadata metadata = JsonUtility.FromJson<MapMetadata>(file);
        if (metadata.Map_type == (byte)MapMetadata.MapType.PointCloud)
        {
            pointCloudAskedForMetadata = true;

           // clientUnity.client.sendTwoPartCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_METADATA, MissionManager.guid + "\0");

        }
        // else
        // {
        PlanSelectionManager.askedForMaps = true;

        clientUnity.client.sendTwoPartCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_MAP, MissionManager.guid + "\0");
        LoadingPanel.SetActive(true);
        LoadingPanel.transform.GetChild(0).GetComponent<Text>().text = "Uploading Plan";
        //}
        // Screen.SetResolution(auxResolution.width, auxResolution.height, true);
    }

    IEnumerator WaitForModel()
    {
        //waits for the model to be loaded. Colladas take a lot of time to be read
        while (GameObject.Find("DaeModel") == null)
        {
            modelLoadingPanel.transform.GetChild(1).Rotate(0, 0, -200 * Time.deltaTime);
            yield return null;
        }
        OnSliderChanged(sliderZoom.value);
        OnSliderChangedPos();
        OnSliderChangedRotate(1);
        modelLoadingPanel.SetActive(false);
        FindShader("Custom/ObjectsShader");
    }
    void createBaseTriangle()  //called only at Start function
    {
        baseTriValues = new BaseTriangle(
            new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
            new Vector4(0.03f * 1.75f, 0.0f, 0.0f, 1.0f),
            new Vector4(0.015f * 1.75f, 0.0259807621135332f * 1.75f, 0.0f, 1.0f),
            new Vector4(0.0f, 0.0f, -1.0f, 1.0f)  //triangle normal
            );
    }
    void runComputeShader(ref Vector3[] in_points, ref Vector3[] in_normals)
    {
        int num_points = in_points.Length;

        //reserve memory
        points = new ComputeBuffer(num_points, 12);  //stride in number of bytes of each element
        normals = new ComputeBuffer(num_points, 12);
        triangles = new ComputeBuffer(num_points * 3, 36);
        base_triangle = new ComputeBuffer(1, 64);

        //fill data
        points.SetData(in_points);
        normals.SetData(in_normals);
        triangles.SetData(new Triangle[num_points]);  //CompShader will write the values

        BaseTriangle[] tmp = new BaseTriangle[1];
        tmp[0] = baseTriValues;
        base_triangle.SetData(tmp);

        //attach buffers to CompShader (uniforms)
        int kernel = cs_create_triangles.FindKernel("CreateTriangles");  //CompShader can have several main functions
        cs_create_triangles.SetBuffer(kernel, "points", points);
        cs_create_triangles.SetBuffer(kernel, "normals", normals);
        cs_create_triangles.SetBuffer(kernel, "triangles", triangles);
        cs_create_triangles.SetBuffer(kernel, "base_triangle", base_triangle);

        //dispatch (Ensure you have enough threads)
        const int thread_group_size_x = 32;  //the same in the compshader file
        int n_groups = (num_points / thread_group_size_x);
        cs_create_triangles.Dispatch(kernel, n_groups, 1, 1);

        //TODO: take into account the not included points

    }
    void releaseCSBuffers()
    {
        if (points != null)
        {
            points.Dispose();
            points.Release();
        }

        if (normals != null)
        {
            normals.Dispose();
            normals.Release();
        }

        if (triangles != null)
        {
            triangles.Dispose();
            triangles.Release();
        }

        if (base_triangle != null)
        {
            base_triangle.Dispose();
            base_triangle.Release();
        }
    }


}
