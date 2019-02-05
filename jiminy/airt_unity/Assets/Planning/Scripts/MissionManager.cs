using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;
//This class handles a few global variables and also builds the 3d map and cube
public class MissionManager : MonoBehaviour {

    public InputField widthInput, heightInput, depthInput;
    public TextMesh widthText, heightText, depthText;
    
    static float width, height, depth;
    //The guid that will be used for any query to the vurrent guid
    public static string guid = "";
    //The data of the plan that is going to be created
    public static string planName = "", planAuthor = "", planNotes = "";
    public static int planDefaultHeight = 0, planDefaultSpeed = 1, planDefaultDuration = 60;
    public static int planIndex = -1;
    //Booleans to distinguish states when downloading maps
    public static bool loadMap = false;
    public static bool importedModel = false;
    public static Matrix4x4 invMatrix;
    public static Vector3 modelBoundingBox = new Vector3(); // (Width, Height, and Depth) of bounding box
    public static Vector3 anchorsBoundingBox; // (Width, Height, and Depth) of bounding box
    public static Vector3 modelBoundsCenter = new Vector3(); // (x, y, z) of bounding box center
    public static Vector3 anchorBoundsCenter; // (x, y, z) of bounding box center
    public Sprite cubeThumbnail, modelThumbnail;
    public GameObject canvas;
    public static string mapName = "";
    public static string mapLocation = "";
    public InputField inputMapName;
    public InputField inputMapLocation;
    public Text modificationTextGui, boundingBoxGui, mapSizeGui;
    public static string modificationText;
    public static ulong fileSize = 0;
    public static bool loadedMap = false;
    //This is the cube collada. When the user creates a cube it concatenates this text along with the size the user decided for each axis
    static string startXML = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"1.4.1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\n<asset>\n<contributor>\n<author>Blender User</author>\n<authoring_tool>Blender 2.79.0 commit date:2017-09-11, commit time:10:43, hash:5bd8ac9</authoring_tool>\n</contributor>\n<created>2018-04-24T15:32:52</created>\n<modified>2018-04-24T15:32:52</modified>\n<unit name=\"meter\" meter=\"1\"/>\n<up_axis>Z_UP</up_axis>\n</asset>\n<library_images/>\n<library_geometries>\n<geometry id=\"Cube-mesh\" name=\"Cube\">\n<mesh>\n<source id=\"Cube-mesh-positions\">\n<float_array id=\"Cube-mesh-positions-array\" count=\"24\">";
    static string midXML = "</float_array>\n<technique_common>\n<accessor source=\"#Cube-mesh-positions-array\" count=\"8\" stride=\"3\">\n<param name=\"X\" type=\"float\"/>\n<param name=\"Y\" type=\"float\"/>\n<param name=\"Z\" type=\"float\"/>\n</accessor>\n</technique_common>\n</source>\n<source id=\"Cube-mesh-normals\">\n<float_array id=\"Cube-mesh-normals-array\" count=\"36\">0 0 1 0 0 -1 -1 0 0 0 1 0 1 0 0 0 -1 0</float_array>\n<technique_common>\n<accessor source=\"#Cube-mesh-normals-array\" count=\"12\" stride=\"3\">\n<param name=\"X\" type=\"float\"/>\n<param name=\"Y\" type=\"float\"/>\n<param name=\"Z\" type=\"float\"/>\n</accessor>\n</technique_common>\n</source>\n<vertices id=\"Cube-mesh-vertices\">\n<input semantic=\"POSITION\" source=\"#Cube-mesh-positions\"/>\n</vertices>\n<triangles count=\"12\">\n<input semantic=\"VERTEX\" source=\"#Cube-mesh-vertices\" offset=\"0\"/>\n<input semantic=\"NORMAL\" source=\"#Cube-mesh-normals\" offset=\"1\"/>\n<p>0 0 2 0 3 0 7 1 5 1 4 1 4 2 1 2 0 2 5 3 2 3 1 3 2 4 7 4 3 4 0 5 7 5 4 5 0 6 1 6 2 6 7 7 6 7 5 7 4 8 5 8 1 8 5 9 6 9 2 9 2 10 6 10 7 10 0 11 3 11 7 11</p>\n</triangles>\n</mesh>\n</geometry>\n</library_geometries>\n<library_controllers/>\n<library_visual_scenes>\n<visual_scene id=\"Scene\" name=\"Scene\">\n<node id=\"Cube\" name=\"Cube\" type=\"NODE\">\n<matrix sid=\"transform\">";
    static string endXML = "</matrix>\n<instance_geometry url=\"#Cube-mesh\" name=\"Cube\"/>\n</node>\n</visual_scene>\n</library_visual_scenes>\n<scene>\n<instance_visual_scene url=\"#Scene\"/>\n</scene>\n</COLLADA>";

    public GameObject planSelection, createEmptyScene, previewScene, mapInfo;

    ClientUnity clientUnity;
    
    //Sets the name that the player wrote on the input field
    public void setName()
    {
        mapName = inputMapName.text;

    }
    //Sets the location that the player wrote on the input field
    public void setLocation()
    {
        mapLocation = inputMapLocation.text;

    }

    private void OnEnable()
    {
        //Before activating the panel that this script is attached to, we add the map name, location, filesize and bounding box that the map has and here we write it in the input field with the map
        //info. If it didn't have anything, the default values will appear
        modificationTextGui.text = modificationText;
        if (guid != "") {
            boundingBoxGui.text = modelBoundingBox.x.ToString("0.00") + "x" + modelBoundingBox.y.ToString("0.00") + "x" + modelBoundingBox.z.ToString("0.00");
            mapSizeGui.text = fileSize + "KB";
        }
        
        inputMapName.text = mapName;
        inputMapLocation.text = mapLocation;

    }
    //Builds a 3d model
    public static void BuildImportedScene()
    {
        bool canBuild = true;
        // ## Check info for invalid values

        if (canBuild)
        {
            MapMetadata MapMetadata;
            importedModel = true;
            //guid = System.Guid.NewGuid().ToString();
            if (mapName == "")
            {

                mapName = "Default3dModel";

            }
            if (mapLocation == "")
            {
                mapLocation = "Default Location";
            }
            MapMetadata = new MapMetadata(guid, mapName, mapLocation, MapMetadata.MapType.Model3D);
            if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Maps"))
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Maps");

            File.Copy(FileBrowser.importedPath, Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map");
         

            // Fill with real map path
            
            //MapMetadata.MapPath = Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map";
            MapMetadata.Map_type = (byte)MapMetadata.MapType.Model3D;
            // ## Vector3 empty just for debug
            MapMetadata.BoundingBox = MissionManager.modelBoundingBox;

            System.IO.FileInfo fileInfo = new System.IO.FileInfo(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map");
            ulong mapFileSize = (ulong)fileInfo.Length;
            mapFileSize = mapFileSize / 1024;
            MissionManager.fileSize = mapFileSize;
            MapMetadata.Byte_Size = mapFileSize;

            string metadataJson = JsonUtility.ToJson(MapMetadata);

            if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData"))
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData");

            if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Missions"))
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Missions");

            System.IO.File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MapMetadata.Guid + ".json.metadata", metadataJson);

            Map map = new Map();
            map.Guid = guid;

            GameObject model = GameObject.Find("DaeModel");
            if (model)
                map.unityToAnchors.SetTRS(model.transform.position, model.transform.localRotation, model.transform.localScale);
            else
                map.unityToAnchors = Matrix4x4.identity;

            map.unityToAnchors.m00 *= 1000;
            map.unityToAnchors.m11 *= 1000;
            map.unityToAnchors.m22 *= 1000;
            invMatrix = Matrix4x4.Inverse(map.unityToAnchors);
            string mapJson = JsonUtility.ToJson(map);
            System.IO.File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + map.Guid + ".json.mission", mapJson);

            loadMap = false;
        }

    }
    //BUilds an empty box
    public static void BuildEmptyScene()
    { 
        string vertexXP = "", vertexYP = "", vertexZP = "";
        string vertexXN = "", vertexYN = "", vertexZN = "";

        guid = System.Guid.NewGuid().ToString();

        vertexXP = (width / 2).ToString();
        vertexYP = (depth / 2).ToString();
        vertexZP = (height / 2).ToString();
        vertexXN = (-width / 2).ToString();
        vertexYN = (-depth / 2).ToString();
        vertexZN = (-height / 2).ToString();
        string vertexDAE =
            vertexXP + " " + vertexYP + " " + vertexZN + " " +
            vertexXP + " " + vertexYN + " " + vertexZN + " " +
            vertexXN + " " + vertexYN + " " + vertexZN + " " +
            vertexXN + " " + vertexYP + " " + vertexZN + " " +
            vertexXP + " " + vertexYP + " " + vertexZP + " " +
            vertexXP + " " + vertexYN + " " + vertexZP + " " +
            vertexXN + " " + vertexYN + " " + vertexZP + " " +
            vertexXN + " " + vertexYP + " " + vertexZP;

        UnityEngine.Debug.Log("Creating empty scene");
        MissionManager.modelBoundingBox = new Vector3(width, height, depth);

        Matrix4x4 mat = Matrix4x4.identity;
        string matString = mat.ToString().Replace("\n", " ").Replace("\t", " ");
        string myCube = startXML + vertexDAE + midXML + matString + endXML;

        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData");

        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Maps"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Maps");

       // UnityEngine.Debug.Log("Creating Map: " + Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map");
        File.Create(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map").Close();
        File.WriteAllText(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map", myCube, System.Text.Encoding.ASCII);

        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Thumbnails"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Thumbnails");

        //UnityEngine.Debug.Log("Creating Thumbnail");
        
        MapMetadata MapMetadata = new MapMetadata(guid, mapName , mapLocation, MapMetadata.MapType.EmptyBox);
        //MapMetadata.MapPath = Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map";
        MapMetadata.BoundingBox = new Vector3(width, height, depth);

        FileInfo fileInfo = new System.IO.FileInfo(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map");
        ulong mapFileSize = (ulong)fileInfo.Length;
        mapFileSize = mapFileSize / 1024;
        fileSize = mapFileSize;
        MapMetadata.Byte_Size = mapFileSize;

        string metadataJson = JsonUtility.ToJson(MapMetadata);            

        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Missions"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Missions");

        System.IO.File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MapMetadata.Guid + ".json.metadata", metadataJson);

        Map map = new Map();
        map.Guid = guid;
        mat.m00 = mat.m11 = mat.m22 = 1000;
        map.unityToAnchors = mat;
        invMatrix = Matrix4x4.Inverse(mat);
        string mapJson = JsonUtility.ToJson(map);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + map.Guid + ".json.mission", mapJson);
    }
    /*
    public void BuildImportedSceneAndReturn()
    {
        BuildImportedScene();
        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/PersistentData/Thumbnails"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/PersistentData/Thumbnails");
        //calibrationPanel.SetActive(false);  

        //SceneManager.LoadScene("PlanSelection");
        //canvas.GetComponent<Canvas>().enabled = false;

    }

    public void BuildAlignedScene()
    {
        if (importedModel)
            BuildImportedSceneAndReturn();
        else {
            BuildEmptyScene();

            //calibrationPanel.SetActive(false);
            //
            //planSelection.SetActive(true);
            //planSelection.transform.parent.GetComponent<PlanSelectionManager>().enabled = true;
        }
    }
    */
    //I think this function is no longer used
    public void goToCalibrateBox()
    {
        importedModel = false;

        if (!float.TryParse(widthInput.text, out width))
        {
            UnityEngine.Debug.Log("Width is not a numeric value");
        }
        if (!float.TryParse(heightInput.text, out height))
        {
            UnityEngine.Debug.Log("Height is not a numeric value");
        }
        if (!float.TryParse(depthInput.text, out depth))
        {
            UnityEngine.Debug.Log("Depth is not a numeric value");
        }
        MissionManager.modelBoundingBox = new Vector3(width, height, depth);
        MissionManager.fileSize = 2;

        UnityEngine.Debug.Log("Creating empty scene");

        if (mapName == "")
        {

            inputMapName.text = mapName = "DefaultCube";

        }
        if (mapLocation == "")
        {
            mapLocation = "Default Location";
        }

        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        createEmptyScene.SetActive(false);
        previewScene.SetActive(false);

        modificationTextGui.text = modificationText;
        boundingBoxGui.text = modelBoundingBox.x.ToString("0.00") + "x" + modelBoundingBox.y.ToString("0.00") + "x" + modelBoundingBox.z.ToString("0.00");
        mapSizeGui.text = fileSize + "KB";
        inputMapName.text = mapName;
        inputMapLocation.text = mapLocation;
        //planSelection.SetActive(true);
        //mapInfo.transform.parent.GetComponent<MissionManager>().enabled = false;
        //mapInfo.transform.parent.GetComponent<MissionManager>().enabled = true;
        mapInfo.SetActive(true);
        //planSelection.transform.parent.GetComponent<PlanSelectionManager>().enabled = true;
        //GameObject.Find("Background").GetComponent<BackgroundClicks>().enabled = false;
        //GameObject.Find("Background").GetComponent<BackgroundClicks>().enabled = true;
            
        
    }
    //This three function control when the user edited the x, y or z axis on the create an empty box screen.
    public void WidthInputChanged()
    {
        if (float.TryParse(widthInput.text, out width))
        {
            widthText.text = widthInput.text + "m";
        }
        else
            widthText.text = "NaN";
    }

    public void DepthInputChanged()
    {
        if (float.TryParse(depthInput.text, out depth))
        {
            depthText.text = depthInput.text + "m";
        }
        else
            depthText.text = "NaN";
    }

    public void HeightInputChanged()
    {
        if (float.TryParse(heightInput.text, out height))
        {
            heightText.text = heightInput.text + "m";
        }
        else
            heightText.text = "NaN";
    }

    public static void ChangeToRecording()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Recording");
    }
}
