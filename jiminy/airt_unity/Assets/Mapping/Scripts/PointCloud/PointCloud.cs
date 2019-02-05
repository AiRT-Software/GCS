#define POZYX_TRANSFORM_COORDS

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

//using System.Threading;

public class PointCloud : MonoBehaviour {

    ClientUnity clientUnity;
    public GameObject canvas;
    //Cameras to adjust on planning
    public Camera topCam;
    public Camera frontCam;
    public bool adjustCameras = true;
    string persistentDataPath;  // read and write
    public GameObject mapInfoPanel;// Panel with all the components of mapInfo
    public RectTransform mapInfoContainer, mapRender, nextButtonMap;// Panel with the info of the map that is shown at the end of planning, the panel on its side and the next button

    //Three variables used in infopanel to save the map name, the location and the file size, and the gameobjects to fill
    string mapName;
    string location;
    long fileSize;
    public GameObject mapNameInputField;
    public GameObject locationInputField;
    public GameObject boundingBoxText;
    public GameObject fileSizeText;
    public GameObject dateText;

    int NUMCLOUDFILES = 0;
    const int CLOUDS_TO_RENDER = 15000000;
    public int numAdded = 0;
    int numDrawn = 0;
    string fileName;
    public RenderTexture modelRenderer;
    byte[] allBytes;
    bool lastPCDeleted = false;
    bool allPointCloudDeleted = false;
    public GameObject prefab_cloud_go;
    public GameObject drone;
    public Sprite pointCloudThumbnail;
    //Some of this aren't here anymore
    public GameObject  ButtonBack, ButtonNext, topInfoPanel, DownInfoPanel, otherPanel, infoPanel, ScanButton, Loading;
    struct Triangle
    {
        public Vector3 p1;
        public Vector3 p2;
        public Vector3 p3;
    };
    struct BaseTriangle
    {
        public Vector4 p1;
        public Vector4 p2;
        public Vector4 p3;
        public Vector4 normal;
        public BaseTriangle(Vector4 p1, Vector4 p2, Vector4 p3, Vector4 normal)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            this.normal = normal;
        }
    };
    private BaseTriangle baseTriValues;

    // Compute shader reference
    public ComputeShader cs_create_triangles;
    
    // Uniforms
    private ComputeBuffer points;
    private ComputeBuffer normals;
    private ComputeBuffer triangles;
    private ComputeBuffer base_triangle;

    private float minX = Mathf.Infinity;
    private float minY = Mathf.Infinity;
    private float minZ = Mathf.Infinity;
    private float maxX = -Mathf.Infinity;
    private float maxY = -Mathf.Infinity;
    private float maxZ = -Mathf.Infinity;
    //Vaariable used to know which app clicked the done button and not to repeat saving metadata/mission...
    public static bool done = false;
    //This array contains the pointcloud heders to know if a PC is repetead and delete it
    SavedPointCloud savedPointCloud;
    public void GetBounds(ref Vector3 min, ref Vector3 max)
    {

        min = new Vector3(minX, minY, minZ);
        max = new Vector3(maxX, maxY, maxZ);

    }
    

    // MAIN FUNCTIONS
    void Awake()
    {

        // This property is ignored on Android and iOS. (At the moment there is no requirement for minimizing the window)
        Application.runInBackground = true;

        //data_path = Application.dataPath;
        persistentDataPath = Application.persistentDataPath + "/PersistentData/Mapping/";

        fileName = "gazebocloud_";

        FileManager.MoveAllMappingFiles();

        savedPointCloud = new SavedPointCloud();
        savedPointCloud.PointCloud = new List<SavedMeshPointCloud>();


        float planInfoX = Screen.width / 2.4f;
        float planInfoY = Screen.height - (Screen.height / 3);
        float marginX = 0.05f;
        float marginY = 0.1f;
        float fontSize = 0.04f;

        mapInfoContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(11 * Screen.width / 20, -Screen.height / 4);
        mapInfoContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 2.4f, Screen.height / 2);
        mapRender.anchoredPosition = new Vector2(Screen.width / 10, -Screen.height / 4);
        mapRender.sizeDelta = new Vector2(Screen.width / 2.5f, Screen.height / 2);
        nextButtonMap.sizeDelta = new Vector2(Screen.width * 0.075f, Screen.height * 0.05f);
        nextButtonMap.anchoredPosition = new Vector2(11 * Screen.width / 20 + Screen.width / 2.4f - nextButtonMap.sizeDelta.x, -3.1f * Screen.height / 4);
        nextButtonMap.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(-Screen.height / 60, 0);
        nextButtonMap.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width * 0.075f, Screen.height * 0.03f);
        nextButtonMap.GetChild(0).GetComponent<Text>().fontSize = Screen.height / 30;

        float mapInfoX = mapInfoContainer.sizeDelta.x;
        float mapInfoY = mapInfoContainer.sizeDelta.y;
        // Map Info Title
        mapInfoContainer.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(mapInfoX * marginX, -mapInfoY * (marginY * 0.5f));
        mapInfoContainer.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.8f, mapInfoY * 0.15f);
        mapInfoContainer.GetChild(0).GetComponent<Text>().fontSize = (int)(planInfoY * 0.05f);
        // Map Name Label
        mapInfoContainer.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(mapInfoX * marginX, -mapInfoY * (marginY * 2.0f));
        mapInfoContainer.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.15f);
        mapInfoContainer.GetChild(1).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map Location Label
        mapInfoContainer.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(mapInfoX * marginX, -mapInfoY * (marginY * 3.5f));
        mapInfoContainer.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.15f);
        mapInfoContainer.GetChild(2).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map Modification Date Label
        mapInfoContainer.GetChild(3).GetComponent<RectTransform>().anchoredPosition = new Vector2(mapInfoX * marginX, -mapInfoY * (marginY * 5.0f));
        mapInfoContainer.GetChild(3).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.15f);
        mapInfoContainer.GetChild(3).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map Size Label
        mapInfoContainer.GetChild(4).GetComponent<RectTransform>().anchoredPosition = new Vector2(mapInfoX * marginX, -mapInfoY * (marginY * 6.5f));
        mapInfoContainer.GetChild(4).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.15f);
        mapInfoContainer.GetChild(4).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map Bounding Box Label
        mapInfoContainer.GetChild(5).GetComponent<RectTransform>().anchoredPosition = new Vector2(mapInfoX * marginX, -mapInfoY * (marginY * 8.0f));
        mapInfoContainer.GetChild(5).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.15f);
        mapInfoContainer.GetChild(5).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map Name Input Field
        mapInfoContainer.GetChild(6).GetComponent<RectTransform>().anchoredPosition = new Vector2((mapInfoX / 2) + mapInfoX * marginX, -mapInfoY * (marginY * 2.0f));
        mapInfoContainer.GetChild(6).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.1f);
        mapInfoContainer.GetChild(6).GetChild(0).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        mapInfoContainer.GetChild(6).GetChild(1).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map Location Input Field
        mapInfoContainer.GetChild(7).GetComponent<RectTransform>().anchoredPosition = new Vector2((mapInfoX / 2) + mapInfoX * marginX, -mapInfoY * (marginY * 3.5f));
        mapInfoContainer.GetChild(7).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.1f);
        mapInfoContainer.GetChild(7).GetChild(0).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        mapInfoContainer.GetChild(7).GetChild(1).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map ModificationDate Text
        mapInfoContainer.GetChild(8).GetComponent<RectTransform>().anchoredPosition = new Vector2((mapInfoX / 2) + mapInfoX * marginX, -mapInfoY * (marginY * 5.0f));
        mapInfoContainer.GetChild(8).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.15f);
        mapInfoContainer.GetChild(8).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map Size Text
        mapInfoContainer.GetChild(9).GetComponent<RectTransform>().anchoredPosition = new Vector2((mapInfoX / 2) + mapInfoX * marginX, -mapInfoY * (marginY * 6.5f));
        mapInfoContainer.GetChild(9).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.15f);
        mapInfoContainer.GetChild(9).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);
        // Map Bounding Box Text
        mapInfoContainer.GetChild(10).GetComponent<RectTransform>().anchoredPosition = new Vector2((mapInfoX / 2) + mapInfoX * marginX, -mapInfoY * (marginY * 8.0f));
        mapInfoContainer.GetChild(10).GetComponent<RectTransform>().sizeDelta = new Vector2(mapInfoX * 0.4f, mapInfoY * 0.15f);
        mapInfoContainer.GetChild(10).GetComponent<Text>().fontSize = (int)(planInfoY * fontSize);

    }

    void Start()
    {
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();

        //queue = new QueueSync<PCLMsg>(10);

        //string filePath = "";
        //for (int i = 1; i < 72; i++)
        //{
        //    filePath = resourcesPath + fileName + i + ".bytes";
        //    if (File.Exists(filePath))
        //    {
        //        FileStream fs = new FileStream(filePath, FileMode.Open);
        //        BinaryReader br = new BinaryReader(fs);
        //        UnityEngine.Debug.Log(br.ReadByte());
        //        UnityEngine.Debug.Log(br.ReadByte());
        //        UnityEngine.Debug.Log(br.ReadByte());
        //        UnityEngine.Debug.Log(br.ReadInt32());
        //    }
        //    else
        //        UnityEngine.Debug.Log("Invalid Path: " + filePath);
        //
        //}

        //TextAsset[] files = Resources.LoadAll<TextAsset>("Mapping");


        //First step of the shader
        createBaseTriangle();

        NUMCLOUDFILES = FileManager.GetNumFiles(FileManager.persistentMappingDataPath);
        //UnityEngine.Debug.Log("NUMCLOUDFILES: " + NUMCLOUDFILES.ToString("##.##"));

        //for (int i = 1; i < NUMCLOUDFILES; i++)
        //{
        //    //(fileName + i);
        //    allBytes = loadMsgFromFile(fileName + i);
        //
        //    if (allBytes != null)
        //    {
        //        byte[] hdr = new byte[(int)PCLMsgOffsets.HEADER_SIZE];
        //        Buffer.BlockCopy(allBytes, 0, hdr, 0, hdr.Length);
        //
        //        PCLMsg pcl = new PCLMsg(hdr);
        //
        //        byte[] data = new byte[allBytes.Length - hdr.Length];
        //        Buffer.BlockCopy(allBytes, hdr.Length, data, 0, allBytes.Length - hdr.Length);
        //
        //        pcl.fillMesh(data);
        //        queue.Enqueue(pcl);
        //
        //        UnityEngine.Debug.Log("cloud " + i);
        //    }
        //}

    }

    void Update()
    {
        //If the app didn't click and a done mapping arrived, go to save plan
        if (done != true && MapperModule.state == MapperModule.MapperState.DONE)
        {
            MapperModule.state = MapperModule.MapperState.IDLE;
            SaveAndGoToPlanSelectionv2();
        }
        else if(done)
        {
            return;
        }
        //If a last PC deleted arrived, enter here
        if (MapperModule.lastPointCloudDeletedBool)
        {
            //If this is  the app that deleted the PC, exit
            if (lastPCDeleted)
            {
                lastPCDeleted = false;
            }
            else
            {
                if (savedPointCloud.PointCloud.Count > 0)
                {
                    Destroy(this.transform.GetChild(this.transform.childCount - 1).gameObject);
                    PointCloudID pointCloudToDelete = savedPointCloud.PointCloud[savedPointCloud.PointCloud.Count - 1].pointCloudID;
                    savedPointCloud.PointCloud.RemoveAt(savedPointCloud.PointCloud.Count - 1);

                    numAdded--;
                }
               
            }
            MapperModule.lastPointCloudDeletedBool = false;
        }
        //Same as above but for every PC
        if (MapperModule.allPointCloudsDeletedBool)
        {
            if (allPointCloudDeleted)
            {
                allPointCloudDeleted = false;
            }
            else
            {
                for (int i = 0; i < savedPointCloud.PointCloud.Count; i++)
                {
                    Destroy(this.transform.GetChild(i).gameObject);
                }
                savedPointCloud.PointCloud.Clear();

                numAdded = 0;

            }
            MapperModule.allPointCloudsDeletedBool = false;

        }
        //We obtain the message of the pointcloud with the structure that contains the points, rotation and ID
        PCLMsg pclmsg = MapperModule.DequeuePCLFrame();
        if (pclmsg != null)
        {
            //PCLMsg pclmsg = queue.Dequeue();
            //If there are too many pointclouds, delete the last from local. In our tests the app didn't seem to reach a limit.
            if (numDrawn > (CLOUDS_TO_RENDER - 1))
            {
                Destroy(GameObject.Find("cloudgo" + (numDrawn - CLOUDS_TO_RENDER)));
            }
            //Gets the points and rotation
            PCLMesh cm = pclmsg.getCloudMesh();

            // Instantiate pointclouds in gameobjects
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

            //Executes the compute shader that turns the points in triangles
            runComputeShader(ref cm.points, ref cm.normals);
            Triangle[] result = new Triangle[cm.points.Length];
            triangles.GetData(result);  //GPU-> CPU
            releaseCSBuffers();  // It is necessary to release compute buffers after using them

            // insert the compute shader results in vectors
            // TODO: from compute shader write directly in a buffer[numpoints*3] instead of buffer.p1, buffer.p2, buffer.3
            Vector3[] allVertices = new Vector3[cm.points.Length * 3];
            Vector3[] allNormals = new Vector3[cm.points.Length * 3];
            Color[] allColors = new Color[cm.points.Length * 3];
            int[] allIndices = new int[cm.points.Length * 3];
            //After obtaining the vertex from gpu, the mesh is created
            for (int i = 0, j = 0; i < cm.points.Length * 3; i += 3, j++)
            {
                allVertices[i] = result[j].p1;
                allVertices[i + 1] = result[j].p2;
                allVertices[i + 2] = result[j].p3;
                CalculateBounds(allVertices[i], allVertices[i + 1], allVertices[i + 2]);

                allIndices[i] = i;
                allIndices[i + 1] = i + 1;
                allIndices[i + 2] = i + 2;

                allNormals[i] = cm.normals[j];
                allNormals[i + 1] = cm.normals[j];
                allNormals[i + 2] = cm.normals[j];

                allColors[i] = cm.colors[j];
                allColors[i + 1] = cm.colors[j];
                allColors[i + 2] = cm.colors[j];
            }

            //TODO: replace de loop above by using Graphics.DrawProceduralIndirect (on a Camera script) and an appendBuffer in other CompShader (consumer). Tried it but didn't work. Didn't have much time to seriously try
            //https://www.digital-dust.com/single-post/2017/07/10/Marching-cubes-on-the-GPU-in-Unity
            // attach vertices, colors, and normals to the mesh
            instance.GetComponent<MeshFilter>().sharedMesh.vertices = allVertices;
            instance.GetComponent<MeshFilter>().sharedMesh.SetIndices(allIndices, MeshTopology.Triangles, 0, true);
            instance.GetComponent<MeshFilter>().sharedMesh.normals = allNormals;
            instance.GetComponent<MeshFilter>().sharedMesh.colors = allColors;
            instance.GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();
#if POZYX_TRANSFORM_COORDS

            // Converting coords from Pozyx to Unity, mm to meters, and angles from right-handed to left-handed
            Matrix4x4 toUnityCoordsMat = Matrix4x4.TRS(new Vector3(pclmsg.hdr.x * 0.001f, pclmsg.hdr.z * 0.001f, pclmsg.hdr.y * 0.001f),
                Quaternion.Euler(pclmsg.hdr.pitch * -180f / 3.141592f, pclmsg.hdr.yaw * -180f / 3.141592f, pclmsg.hdr.roll * -180f / 3.141592f),
                new Vector3(1,1,1));

            instance.transform.rotation = toUnityCoordsMat.rotation;
            instance.transform.position = new Vector3(toUnityCoordsMat.m03, toUnityCoordsMat.m13,toUnityCoordsMat.m23);
            
            //instance.transform.RotateAround(drone.transform.position, new Vector3(1, 0, 0), toUnityCoordsMat.rotation.eulerAngles.x);
            //instance.transform.RotateAround(drone.transform.position, new Vector3(0, 1, 0), -toUnityCoordsMat.rotation.eulerAngles.y);
            //instance.transform.RotateAround(drone.transform.position, new Vector3(0, 0, 1), toUnityCoordsMat.rotation.eulerAngles.z);
            //UnityEngine.Debug.Log(toUnityCoordsMat);

            //instance.transform.localScale = new Vector3(-1 * instance.transform.localScale.x, instance.transform.localScale.y, instance.transform.localScale.z);
#endif

            //The loop that checks if a pointcloud needs to be substitutedw
            numDrawn++;
            int posOfOldPointCloud = savedPointCloud.isTheCloudAlreadyIn(pclmsg.hdr.pointCloudID);
            if (posOfOldPointCloud != -1)
            {
                GameObject aux =  GameObject.Find("cloudgo" + pclmsg.hdr.pointCloudID.i + "" + pclmsg.hdr.pointCloudID.j + "" + pclmsg.hdr.pointCloudID.k + "" + pclmsg.hdr.pointCloudID.heading);
                if (aux != null)
                {
                    //Destroying the gameobject
                    Destroy(aux);
                    //UnityEngine.Debug.Log("Destroyed");
                }
                else
                {
                    //search on saved DISK
                }
                //UnityEngine.Debug.Log("Removed");
                //And remove it from the array
                savedPointCloud.PointCloud.RemoveAt(posOfOldPointCloud);
            }
            //Renaming the pointcloud to manage it better
            instance.name = "cloudgo" + pclmsg.hdr.pointCloudID.i + "" + pclmsg.hdr.pointCloudID.j + "" + pclmsg.hdr.pointCloudID.k + "" + pclmsg.hdr.pointCloudID.heading;



            //savedPointCloud.PointCloud.Add(new SavedMeshPointCloud(allVertices, allColors, Matrix4x4.TRS(new Vector3(pclmsg.hdr.x * 0.001f, pclmsg.hdr.z * 0.001f, pclmsg.hdr.y * 0.001f),
            //    Quaternion.Euler(pclmsg.hdr.pitch * -180f / 3.141592f, pclmsg.hdr.yaw * -180f / 3.141592f, pclmsg.hdr.roll * -180f / 3.141592f), new Vector3(1, 1, 1)), new Vector3(pclmsg.hdr.pointCloudID.i, pclmsg.hdr.pointCloudID.j, pclmsg.hdr.pointCloudID.k), pclmsg.hdr.pointCloudID.heading));
            //Adding the pointcloud to the array
            savedPointCloud.PointCloud.Add(new SavedMeshPointCloud(new Vector3(pclmsg.hdr.pointCloudID.i, pclmsg.hdr.pointCloudID.j, pclmsg.hdr.pointCloudID.k), pclmsg.hdr.pointCloudID.heading));


        }

        //LoadFile();

        // Código a ejecutar cuando ya hemos pintado la nube de puntos entera
        //if (numDrawn == NUMCLOUDFILES)
        //{
        //    // Ajustamos todos los parámetros de las cámaras para centrarlas sobre la nube de puntos y tratar de abarcarla entera
        //    AdjustCameras();
        //   
        //    canvas.SetActive(true);
        //
        //    numDrawn++;
        //}
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
    
    /// <summary>
    /// Calculates the pointcloud bounding box
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    /// <param name="point3"></param>
    void CalculateBounds(Vector3 point1, Vector3 point2, Vector3 point3)
    {
        // Comprueba si el punto 1 es un máximo o mínimo
        if (point1.x < minX)
            minX = point1.x;
        else if (point1.x > maxX)
            maxX = point1.x;

        if (point1.y < minY)
            minY = point1.y;
        else if (point1.y > maxY)
            maxY = point1.y;

        if (point1.z < minZ)
            minZ = point1.z;
        else if (point1.z > maxZ)
            maxZ = point1.z;

        // Comprueba si el punto 2 es un máximo o mínimo
        if (point2.x < minX)
            minX = point2.x;
        else if (point2.x > maxX)
            maxX = point2.x;

        if (point2.y < minY)
            minY = point2.y;
        else if (point2.y > maxY)
            maxY = point2.y;

        if (point2.z < minZ)
            minZ = point2.z;
        else if (point2.z > maxZ)
            maxZ = point2.z;

        // Comprueba si el punto 3 es un máximo o mínimo
        if (point3.x < minX)
            minX = point3.x;
        else if (point3.x > maxX)
            maxX = point3.x;

        if (point3.y < minY)
            minY = point3.y;
        else if (point3.y > maxY)
            maxY = point3.y;

        if (point3.z < minZ)
            minZ = point3.z;
        else if (point3.z > maxZ)
            maxZ = point3.z;
    }
    /// <summary>
    /// Adjusts the camera on Planning
    /// </summary>
    void AdjustCameras() {
        //If this is not on planning, exit
        if (!adjustCameras)
        {
            return;
        }
        // Centramos la cámara superior
        topCam.transform.position = new Vector3((maxX + minX) / 2.0f, topCam.transform.position.y, (maxZ + minZ) / 2.0f);

        // Comprobamos si el mapa de puntos es más largo en Z o en X para saber como colocar la nube de puntos
        if (Mathf.Abs(maxZ - minZ) > Mathf.Abs(maxX - minX))
        {
            // Rotamos la nube de puntos si es más larga en Z que en X
            transform.Rotate(new Vector3(0, 90, 0));

            // Centramos la cámara lateral
            frontCam.transform.position = new Vector3((maxZ + minZ) / 2.0f, (maxY + minY) / 2.0f, -900);

            // Ajustamos el orthographicSize para abarcar el máximo del mapa de puntos
            frontCam.orthographicSize = (maxZ - minZ) * (float)topCam.rect.height / (float)topCam.rect.width * 0.5f;
            topCam.orthographicSize = (maxZ - minZ) * (float)topCam.rect.height / (float)topCam.rect.width * 0.5f;

            // Ajustamos el near al punto más cercano a la cámara
            //frontCam.GetComponent<Camera>().nearClipPlane = -frontCam.transform.position.z - maxX + 0.3f;


        }
        else
        {
            // Centramos la cámara lateral
            frontCam.transform.position = new Vector3((maxX + minX) / 2.0f, (maxY + minY) / 2.0f, -900);

            // Ajustamos el orthographicSize para abarcar el máximo del mapa de puntos
            frontCam.orthographicSize = (maxX - minX) * (float)topCam.rect.height / (float)topCam.rect.width * 0.5f;
            topCam.orthographicSize = (maxX - minX) * (float)topCam.rect.height / (float)topCam.rect.width * 0.5f;

            // Ajustamos el near al punto más cercano a la cámara
            frontCam.GetComponent<Camera>().nearClipPlane = -frontCam.transform.position.z - maxZ + 0.3f;
        } 
    }
    /// <summary>
    /// Delete all pointclouds. Only called from a button
    /// </summary>
    public void DeletePointCloud()
    {
        for (int i = 0; i < savedPointCloud.PointCloud.Count; i++)
        {
            Destroy(this.transform.GetChild(i).gameObject);
        }
        clientUnity.client.SendCommand((byte)Modules.MAPPER_MODULE, (byte)MapperCommandType.MAPPER_DELETE_ALL_POINT_CLOUDS);
        savedPointCloud.PointCloud.Clear();
        allPointCloudDeleted = true;

        numAdded = 0;
    }
    /// <summary>
    /// Delete the last drawed pointcloud
    /// </summary>
    public void deleteLastPointCloud()
    {
        // A user can click faster than the quantity of pointclouds received
        if (savedPointCloud.PointCloud.Count > 0)
        {
            Destroy(this.transform.GetChild(this.transform.childCount - 1).gameObject);
            PointCloudID pointCloudToDelete = savedPointCloud.PointCloud[savedPointCloud.PointCloud.Count - 1].pointCloudID;
            clientUnity.client.sendCommand((byte)Modules.MAPPER_MODULE, (byte)MapperCommandType.MAPPER_DELETE_POINT_CLOUD, pointCloudToDelete);
            savedPointCloud.PointCloud.RemoveAt(savedPointCloud.PointCloud.Count - 1);
           // UnityEngine.Debug.Log("PointCloud Deleted: " + pointCloudToDelete.i + "" + pointCloudToDelete.j + "" + pointCloudToDelete.k + "" + pointCloudToDelete.heading);
            numAdded--;
            lastPCDeleted = true;
        }
        
    }
    /// <summary>
    /// Finishes mapping, requests a map and opens the panel to give a name and location to the map. Only called from the finish button on mapping
    /// </summary>
    public void SaveAndGoToPlanSelection()
    {
        
        clientUnity.client.SendCommand((byte)Modules.MAPPER_MODULE, (byte)MapperCommandType.MAPPER_DONE_MAPPING);
        PlanSelectionManager.askedForMaps = true;
        PlaceModel.pointCloudAskedForMetadata = false;
        clientUnity.client.sendTwoPartCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_MAP, MissionManager.guid + "\0");
        mapInfoPanel.SetActive(true);

        SaveAndGoToPlanSelectionv2();
    }
    /// <summary>
    /// Starts saving mission, metadata and thumbnail. Called from the previous function and from the update after a tablet that didn't press "done mapping"  receives a done mapping message
    /// </summary>
    void SaveAndGoToPlanSelectionv2()
    {
        done = true;

        //Remove frames form the queue as there are some residues
        MapperModule.DequeuePCLFrame();
        MapperModule.DequeuePCLFrame();
        MapperModule.DequeuePCLFrame();
        //If the map hasn't been downloaded yet, continue in this loop
        while (MapLoader.mapDownloaded == false)
        {
            //do nothing

        }

        MapLoader.mapDownloaded = false;
        //If the info panel is activated, the metadata needs to be written before coming back to Plan
        if (mapInfoPanel.activeSelf)
        {
            fileSize = new System.IO.FileInfo(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dpl.map").Length;
            fileSizeText.GetComponent<Text>().text = fileSize.ToString();
            boundingBoxText.GetComponent<Text>().text = new Vector3(maxX, maxY, maxZ).ToString();
            dateText.GetComponent<Text>().text = "" + DateTime.Now.Day + '/' + DateTime.Now.Month + '/' + DateTime.Now.Year;
            return;
        }
        SceneManager.LoadScene("General");

        //StartCoroutine(RenderTexture());
    }

    /// <summary>
    /// This function assigns the written name to the variable that will save it to the map
    /// </summary>
    public void getMapName()
    {
        mapName = mapNameInputField.GetComponent<InputField>().text;
    }
    /// <summary>
    /// Assigns the location to the variable that will be saved to the map
    /// </summary>
    public void getLocation()
    {
        location = locationInputField.GetComponent<InputField>().text;
    }
    //this function will create the metadata, upload it and go to the general scene
    public void CreateMetadataUploadItAndGoToGeneral()
    {
        StartCoroutine(RenderTexture());
    }


    //Old unused function to save PC
    void CreatePlan(string jsonPointCloud)
    {
       

        bool canBuild = true;
        

        UnityEngine.Debug.Log("Creating pointCloud scene");

        if (canBuild)
        {

            
            Matrix4x4 mat = Matrix4x4.identity;


            if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData"))
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData");

            if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Maps"))
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Maps");

            UnityEngine.Debug.Log("Creating Map: " + Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map");
            //File.Create(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map").Close();
            //File.WriteAllText(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map", jsonPointCloud, System.Text.Encoding.ASCII);

            if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Thumbnails"))
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Thumbnails");

            MapMetadata MapMetadata = new MapMetadata(MissionManager.guid, "MapMetadata_" + MissionManager.guid, "ImportedLocation_", MapMetadata.MapType.PointCloud);
            //MapMetadata.MapPath = Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dpl.map";
            MapMetadata.BoundingBox = new Vector3(maxX, maxY, maxZ);
            string metadataJson = JsonUtility.ToJson(MapMetadata);

            if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Missions"))
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Missions");

            System.IO.File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MapMetadata.Guid + ".json.metadata", metadataJson);

            Map map = new Map();
            map.Guid = MissionManager.guid;
           
            map.unityToAnchors = Matrix4x4.identity;
            mat.m00 *= 1000;
            mat.m11 *= 1000;
            mat.m22 *= 1000;
            map.unityToAnchors = mat;
            MissionManager.invMatrix = Matrix4x4.Inverse(mat);
            string mapJson = JsonUtility.ToJson(map);
            System.IO.File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + map.Guid + ".json.mission", mapJson);





            //SceneManager.LoadScene("PlanSelection");
            //guid = System.Guid.NewGuid().ToString();
            //Map m = new Map();
            //MapMetadata MapMetadata = new MapMetadata(guid, "MapMetadata_" + guid.Split('-')[0], "EmptyLocation_", MapMetadata.MapType.EmptyBox);
            //MapMetadata.BoxScale = new Vector3(width / 10, height / 10, depth / 10);
            //MapMetadata.BoundingBox = new Vector3(width, height, depth);
            //string json = JsonUtility.ToJson(MapMetadata);
            //
            //if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData"))
            //    System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData");
            //
            //if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Missions"))
            //    System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Missions");
            //
            //System.IO.File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MapMetadata.Name + ".json", json);
        }

        SceneManager.LoadScene("General");

    }
    //The function that saves the thumbnail, mission and metadata called from saveandgotoplanselection
    IEnumerator RenderTexture()
    {
        
        ButtonBack.SetActive(false);
        ButtonNext.SetActive(false);
        topInfoPanel.SetActive(false);
        DownInfoPanel.SetActive(false);
        otherPanel.SetActive(false);
        infoPanel.SetActive(false);
        ScanButton.SetActive(false);
        
       GameObject Helper = this.gameObject;
        //Assign the bounding box to the file
       MissionManager.modelBoundingBox = new Vector3(maxX, maxY, maxZ);
        //All this comments used to take a photo at the pointcloud, but for a reason unknown, it didn't center the image correctly to take the photo. TODO fix that


       // Vector3 auxPosition = Camera.main.transform.position;
       //
       // Camera.main.transform.parent.position = Vector3.zero;
       // Camera.main.transform.parent.rotation = Quaternion.Euler(0,0,0);
       // Camera.main.transform.localPosition = new Vector3(drone.transform.position.x, drone.transform.position.y + maxY * 3, drone.transform.position.z);
       // float fieldOfVIewHelper = Camera.main.fieldOfView;
       // float futureFOV = 2.0f * Mathf.Atan((Camera.main.transform.localPosition.y - (drone.transform.position.y - maxY)) * 0.5f / Vector3.Distance(Camera.main.transform.localPosition, drone.transform.position)) * Mathf.Rad2Deg;
       // futureFOV = Mathf.Clamp(futureFOV + 10.0f, 1.0f, 179.0f);
       // if (float.IsNaN(futureFOV))
       // {
       //     Camera.main.fieldOfView = 40;
       //     Camera.main.transform.localPosition = new Vector3(drone.transform.position.x, drone.transform.position.y * 3, drone.transform.position.z);
       // }
       // else
       // {
       //     Camera.main.fieldOfView = futureFOV;
       // }
       // Camera.main.transform.LookAt(drone.transform);
       // yield return new WaitForSeconds(1.0f);
       // Resolution auxResolution = Screen.currentResolution;
       // Screen.SetResolution(256, 256, true);
       // yield return new WaitForSeconds(1.0f);
       // yield return new WaitForEndOfFrame();
       // Camera.main.targetTexture = modelRenderer;
       //
       // Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
       // UnityEngine.Debug.Log("CamPos: " + Camera.main.transform.position);
       // UnityEngine.Debug.Log("CamRot: " + Camera.main.transform.rotation.eulerAngles);
       // UnityEngine.Debug.Log("Drone: " + drone.transform.position);
       // UnityEngine.Debug.Log("Fov: " + futureFOV);
       // Camera.main.Render();
       // texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
       // texture.Apply();
       // byte[] bytes = texture.EncodeToPNG();

        // save the image
        //Remember to create directories if they don't exist
        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Thumbnails"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Thumbnails");
        //Create the thumbnail from a jpg that we have. TODO change to a render of the PC
        File.WriteAllBytes(Application.persistentDataPath + "/PersistentData/Thumbnails/" + MissionManager.guid + ".jpeg.thumbnail", pointCloudThumbnail.texture.EncodeToPNG());
        //DestroyObject(texture);
       // Camera.main.targetTexture.Release();
       // Camera.main.targetTexture = null;
       // Screen.SetResolution(auxResolution.width, auxResolution.height, true);
       // Camera.main.fieldOfView = fieldOfVIewHelper;
       // Camera.main.transform.position = auxPosition;
        //string jsonPointCloud = JsonUtility.ToJson(savedPointCloud);
        
        //CreatePlan(jsonPointCloud);


        bool canBuild = true;


        //UnityEngine.Debug.Log("Creating pointCloud scene");

        if (canBuild)
        {


            Matrix4x4 mat = Matrix4x4.identity;


            if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData"))
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData");

            if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Maps"))
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Maps");

            //UnityEngine.Debug.Log("Creating Map: " + Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map");
            //File.Create(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map").Close();
            //File.WriteAllText(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map", jsonPointCloud, System.Text.Encoding.ASCII);
            //Need to set loading bigger, in the middle and to rotate it in the next while loop inside this function
            Loading.SetActive(true);
            yield return new WaitForEndOfFrame();

            if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Thumbnails"))
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Thumbnails");



            MapMetadata MapMetadata = new MapMetadata(MissionManager.guid, "MapMetadata_" + MissionManager.guid, "ImportedLocation_", MapMetadata.MapType.PointCloud);
            //MapMetadata.MapPath = Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dpl.map";
            MapMetadata.BoundingBox = new Vector3(maxX, maxY, maxZ);
            MapMetadata.Location = location;
            MapMetadata.Name = mapName;
            MapMetadata.Byte_Size = (ulong)fileSize;
            string metadataJson = JsonUtility.ToJson(MapMetadata);

            if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Missions"))
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Missions");

            System.IO.File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MapMetadata.Guid + ".json.metadata", metadataJson);
            yield return new WaitForEndOfFrame();

            Map map = new Map();
            map.Guid = MissionManager.guid;
            //Pozyx is in milimiters, need to convert to that
            map.unityToAnchors = Matrix4x4.identity;
            mat.m00 *= 1000;
            mat.m11 *= 1000;
            mat.m22 *= 1000;
            map.unityToAnchors = mat;
            MissionManager.invMatrix = Matrix4x4.Inverse(mat);
            string mapJson = JsonUtility.ToJson(map);
            System.IO.File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + map.Guid + ".json.mission", mapJson);
            
            
            //yield return new WaitForEndOfFrame();



            //SceneManager.LoadScene("PlanSelection");
            //guid = System.Guid.NewGuid().ToString();
            //Map m = new Map();
            //MapMetadata MapMetadata = new MapMetadata(guid, "MapMetadata_" + guid.Split('-')[0], "EmptyLocation_", MapMetadata.MapType.EmptyBox);
            //MapMetadata.BoxScale = new Vector3(width / 10, height / 10, depth / 10);
            //MapMetadata.BoundingBox = new Vector3(width, height, depth);
            //string json = JsonUtility.ToJson(MapMetadata);
            //
            //if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData"))
            //    System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData");
            //
            //if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Missions"))
            //    System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Missions");
            //
            //System.IO.File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MapMetadata.Name + ".json", json);
        }
        //This is for the app that clicked the done button. It exits here and won't enter more than once.
        PlanSelectionManager.uploadMapMetadata = true;
        clientUnity.client.SendCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_LIBRARY_PATH);
        done = false;
        MapperModule.state = MapperModule.MapperState.IDLE;
        SceneManager.LoadScene("General");





        // Screen.SetResolution(auxResolution.width, auxResolution.height, true);
    }
}
