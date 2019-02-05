using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using UnityEngine.UI;
using System;
//This loads a map in planning
public class MapLoader : MonoBehaviour {
    //Shader to apply to the box or model
    public Shader shader, cullBackShader;
    //The waypoint object to instantiate and the parent
    public GameObject pointGO, sphereParentGO;
    //The two cameras to adjust
    public Camera topCam, frontCam;
    //Material for the line
    public Material lineMat;
    Path ownPath;
    //Poi panel
    public GameObject poiEditorPanel;
    //Point of interest object to instantiate
    public GameObject poi;
    List<GameObject> poisCreated;
    //UI
    public Button editPointButton, editPOIButton, previewButton, CurveButton;
    //materials to assign the waypoints
    // Materiales para indicar la esfera seleccionada
    public Material waypointNotSelectedMaterial;
    public Material landingNotSelectedMaterial;
    public Material homeNotSelectedMaterial;

    ClientUnity clientUnity;


    public static bool mapDownloaded = false;


    // Compute shader reference
    public ComputeShader cs_create_triangles;

    // Uniforms
    private ComputeBuffer points;
    private ComputeBuffer normals;
    private ComputeBuffer triangles;
    private ComputeBuffer base_triangle;
    private BaseTriangle baseTriValues;
    public GameObject pcPrefab;
    public GameObject prefab_cloud_go;
    void Start()
    {
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        //At the beggining we load the map
        if (MissionManager.loadMap) {
            LoadMap(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.metadata");
        }
       
        ownPath = Path.Instance;
    }
    private void Update()
    {
        //This doesn't happen here anymore
        if (mapDownloaded)
        {
            mapDownloaded = false;
            LoadMap(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.metadata");

        }
    }
    /// <summary>
    /// This is the main fucntion that loads the map and the plan waypoints
    /// </summary>
    /// <param name="path"></param>
    void LoadMap(string path)
    {
        //We load the main path
        if (ownPath == null)
            ownPath = Path.Instance;
        //laod the metadata
        string json = System.IO.File.ReadAllText(path);

        MapMetadata metadata = JsonUtility.FromJson<MapMetadata>(json);
        UnityEngine.Debug.Log("Loading map");

        BinaryFormatter bFormatter = new BinaryFormatter();
        //if (!AskForMap())
        //    return;
        FileStream file;
        
        //If it is a pointcloud we load a pointcloud map, and if it isn't a collada
        if (metadata.Map_type == (byte)MapMetadata.MapType.PointCloud)
        {
            file = File.OpenRead(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dpl.map");
        }
        else
        {

            file = File.OpenRead(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map");

        }
        //var xmlWriter = new XmlTextWriter(file, Encoding.UTF8);
        //xmlWriter.
        XmlDocument doc = new XmlDocument();
        string aux;
        
        GameObject auxGameObject = new GameObject();
        UnityEngine.Debug.Log(MissionManager.invMatrix);
        //An empty box and a 3d model are the same
        if (metadata.Map_type ==(byte) MapMetadata.MapType.EmptyBox)
        {
            using (StreamReader reader = new StreamReader(file))
            {
                aux = reader.ReadToEnd();
            }
            doc.LoadXml(aux);
            ColladaImporter importer = new ColladaImporter(ref auxGameObject);
            StartCoroutine(importer.getMesh(doc, cullBackShader));
            

        }
        else if(metadata.Map_type == (byte)MapMetadata.MapType.Model3D)
        {
            using (StreamReader reader = new StreamReader(file))
            {
                aux = reader.ReadToEnd();
            }
            doc.LoadXml(aux);
            ColladaImporter importer = new ColladaImporter(ref auxGameObject);
            StartCoroutine(importer.getMesh(doc, shader));

        }//But pointclouds are loaded different
        else
        {
            //MapLoader used in mapalignment uses this exact function and they are already explained there
            createBaseTriangle();
            byte[] pclBytes = File.ReadAllBytes(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dpl.map");
            GameObject daeModel = new GameObject();
            daeModel.name = "DaeModel";
            daeModel.transform.parent = auxGameObject.transform;
            LoadPointCloud(pclBytes, ref daeModel);
            //SavedPointCloud pointCloud = JsonUtility.FromJson<SavedPointCloud>(aux);
            //
            //for (int i = 0; i < pointCloud.PointCloud.Count; i++)
            //{
            //    
            //    GameObject gameObject = Instantiate(pcPrefab, auxGameObject.transform);
            //    gameObject.GetComponent<MeshFilter>().sharedMesh = new Mesh();
            //    gameObject.GetComponent<MeshFilter>().sharedMesh.name = "cmesh" + i.ToString();
            //    gameObject.GetComponent<MeshFilter>().mesh.vertices = pointCloud.PointCloud[i].vertex;
            //    int[] indices = new int[pointCloud.PointCloud[i].vertex.Length];
            //    Color[] colors = new Color[indices.Length];
            //    for (int j = 0; j < pointCloud.PointCloud[i].vertex.Length; j++)
            //    {
            //        indices[j] = j;
            //        colors[j] = new Color(pointCloud.PointCloud[i].colors[j].x, pointCloud.PointCloud[i].colors[j].y, pointCloud.PointCloud[i].colors[j].z, 1);
            //    }
            //    gameObject.GetComponent<MeshFilter>().mesh.colors = colors;
            //    gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            //    gameObject.GetComponent<MeshFilter>().mesh.SetIndices(indices, MeshTopology.Triangles, 0, true);
            //    gameObject.GetComponent<MeshFilter>().mesh.RecalculateBounds();
            //    Matrix4x4 auxMat = pointCloud.PointCloud[i].matrix;
            //    gameObject.transform.rotation = auxMat.rotation;
            //    gameObject.transform.position = new Vector3(auxMat.m03, auxMat.m13, auxMat.m23);
            //
            //
            //}

        }
        //Once the maps are loaded, we start with the waypoints. First a mission is loaded
        auxGameObject.name = "DaemodelChild";
       

        string mapJson = File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.mission");
        Map map = JsonUtility.FromJson<Map>(mapJson);
        Path waypointPath = Path.Instance;
        //sphereParentGO.transform.position = auxGameObject.transform.position;
        //If this isn't a new plan, we load the plan selected previously
        if (MissionManager.planIndex != -1)
        {
            //Here we get a plan from all the paths saved in missionmanager
            waypointPath = map.Paths[MissionManager.planIndex];
            ownPath.setPath(waypointPath);
            //And give them the default parameters previously selected
            MissionManager.planDefaultHeight = waypointPath.FlightParams.height;
            MissionManager.planDefaultSpeed = waypointPath.FlightParams.speed;
            MissionManager.planDefaultDuration = waypointPath.FlightParams.duration;
            
            //Here we instantiate the waypoints
            for (int i = 0; i < waypointPath.Count(); i++)
            {
                //waypointPath.GetPoint(i).PointPosition = new Vector3(waypointPath.GetPoint(i).PointPosition.x, waypointPath.GetPoint(i).PointPosition.z, waypointPath.GetPoint(i).PointPosition.y);

                GameObject auxPoint = Instantiate(pointGO, waypointPath.GetPoint(i).PointPosition, Quaternion.Euler(0, 0, 0), sphereParentGO.transform);
                //assign them the transform to the script
                waypointPath.GetPoint(i).PointTransf = auxPoint.transform;
                auxPoint.SetActive(true);
                //Scale it
                TransformSphere(topCam, auxPoint);
                //And if it is the first one, we assign it the yellow colour, and to the last one, the green colour. The rest use the normal waypoint colour
                if (i == 0)
                {
                    auxPoint.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeNotSelectedMaterial;
                    //auxPoint.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.color = Color.white;
                }
                else if (i != waypointPath.Count() - 1)
                {
                    auxPoint.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = waypointNotSelectedMaterial;
                   // auxPoint.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.color = Color.white;
                    //auxPoint.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = whiteOutlineMaterial;
                    //auxPoint.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial = whiteOutlineMaterial;
                }
                else
                {
                    //auxPoint.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingMaterial;
                    //auxPoint.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial = landingMaterial;
                    auxPoint.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingNotSelectedMaterial;
                    //auxPoint.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.color = Color.white;
                }
                //We assign the monobehaviour that contains the point parameters
                auxPoint.AddComponent<PathPoint>().createPathPointWithPoint(waypointPath.GetPoint(i), 0,0);                

            }
            
            //here we create the middlepoints that the curve contains
            for (int i = 0; i < waypointPath.Count() - 1; i++)
            {
                lineMat.SetPass(0);
                if (topCam != null)

                    CatmullRomSpline.DisplayCatmullRomSpline2(waypointPath, i, ref waypointPath.middlePointsTop, topCam.orthographicSize / 2.0f, true, topCam);
                if(frontCam != null)
                CatmullRomSpline.DisplayCatmullRomSpline(waypointPath, i, ref waypointPath.middlePointsRight, frontCam.orthographicSize / 2.0f, true, frontCam);
            }
            //This script is also used on recording, where we don't want points of interests. We adjust the path to the adjustmnets doen in model alignment and exit
            if (GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.Recording)
            {
                sphereParentGO.transform.position = new Vector3(map.unityToAnchors.m03 * 0.001f, map.unityToAnchors.m23 * 0.001f, map.unityToAnchors.m13 * 0.001f);
                sphereParentGO.transform.localScale = new Vector3(map.unityToAnchors.m00 * 0.001f, map.unityToAnchors.m11 * 0.001f, map.unityToAnchors.m22 * 0.001f);
                return;

            }


            //If not we create the points of interest
            poisCreated = new List<GameObject>();

            for (int i = 0; i < waypointPath.wpParametersCount(); i++)
            {


                //for each waypoint of interest that the path contains, we assign it to the respective waypoint
                GimballParameters auxGimballUpdateParameters = waypointPath.getGbUpdateParameter(i);
                Point auxPoint = waypointPath.GetPoint(auxGimballUpdateParameters.id_pointer);
                auxPoint.PointTransf.gameObject.GetComponent<PathPoint>().Wp.gimbal_parameters = auxGimballUpdateParameters;
                //And now depending on the type of point of interest, one behaviour is assigned. It can be a look at point, look at the curve, 
                //locked in yaw only while looking in a direction or looking in a direction without locks
                switch (auxGimballUpdateParameters.mode)
                {
                    case 0:
                        GameObject newpoi;
                        //First we search for a POI
                        System.Predicate<GameObject> predicate = (GameObject p) => { return p.transform.position == auxGimballUpdateParameters.poi_or_angles; };
                        //If one POI has been placed at least
                        if (poisCreated.Count > 0)
                        {
                            //we find if the POI was already there, just in case multiple waypoints share the same POI
                            newpoi = poisCreated.Find(predicate);
                            if (newpoi == null)
                            {
                                newpoi = Instantiate(poi, auxGimballUpdateParameters.poi_or_angles, Quaternion.Euler(new Vector3(0, 0, 0)));
                                poisCreated.Add(newpoi);
                                newpoi.AddComponent<POI>();
                            }
                        }
                        else
                        {
                            newpoi = Instantiate(poi, auxGimballUpdateParameters.poi_or_angles, Quaternion.Euler(new Vector3(0, 0, 0)));
                            poisCreated.Add(newpoi);
                            newpoi.AddComponent<POI>();

                        }

                       

                        //We create a new line that points from the POI to the waypoint. The poi object already has one, but if there is more than one waypoint
                        //related to the POI, we need to create more lines
                        GameObject newLine = newpoi.transform.GetChild(2).gameObject;

                        if (i > 0)
                        {
                            newLine = Instantiate(poi.transform.GetChild(2).gameObject, newpoi.transform.position, Quaternion.Euler(new Vector3(0, 0, 0)), newpoi.transform);

                        }
                        //All of this calculate the rotation, position and size of the line
                        Vector3 centerPos = new Vector3(auxPoint.PointTransf.position.x + newpoi.transform.position.x, auxPoint.PointTransf.position.y + newpoi.transform.position.y, auxPoint.PointTransf.position.z + newpoi.transform.position.z) / 2f;
                        float scaleX = Mathf.Abs((auxPoint.PointTransf.position - newpoi.transform.position).magnitude);

                        newLine.transform.localScale = new Vector3(scaleX, 3f, 3f);
                        //Esto es para rotar las imagenes de las camaras que tienen cada Waypoint
                        Transform cube = auxPoint.PointTransf.Find("CubeTop");
                        cube.gameObject.SetActive(true);

                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from = new Vector2(-1, 0);
                        Vector3 aux2 = newpoi.transform.position - auxPoint.PointTransf.position;
                        Vector2 to = new Vector2(aux2.x, aux2.z).normalized;
                        float angle = Vector2.SignedAngle(from, to);
                        Transform cube2 = auxPoint.PointTransf.Find("CubeFront");
                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from2 = new Vector2(-1, 0);
                        Vector3 aux3 = newpoi.transform.position - auxPoint.PointTransf.position;
                        Vector2 to2 = new Vector2(aux3.x, aux3.y).normalized;
                        float angle2 = Vector2.SignedAngle(from2, to2);

                        //float angle = Mathf.Acos(distance2 / distance);
                        //item.transform.Rotate(new Vector3(0, 1, 0), Vector2.Angle(from, to));
                        cube.transform.rotation = Quaternion.Euler(new Vector3(0, -angle, 0));
                        cube2.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle2));
                        cube.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
                        cube2.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
                        auxPoint.PointTransf.GetComponent<PathPoint>().Poi = newpoi.transform;
                        newpoi.GetComponent<POI>().addPoint(auxPoint.PointTransf.GetComponent<PathPoint>(), newLine);
                        //UnityEngine.Debug.Log(newpoi.GetComponent<POI>().Referenced.Count);

                        float sineC = (auxPoint.PointTransf.position.y - newpoi.transform.position.y) / scaleX;

                        newLine.transform.rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg);
                        newLine.transform.position = newpoi.transform.position;
                        var rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg).eulerAngles;
                        auxPoint.PointTransf.GetComponent<PathPoint>().GimbalRotation = new Vector3(rotation.z, rotation.y, rotation.x + 90);
                        auxPoint.PointTransf.GetComponent<PathPoint>().getGimballMode = 0;
                        TransformSphere(topCam, newpoi);

                        break;
                    case 1:
                        //This is the case where the direction will always point to the curve
                        float angle3 = 0;
                        float angle4 = 0;

                        int middlePointsIndexTop = 0;
                        int middlePointsIndexRight = 0;
                        //We iterate through the array to see the closest point from the curve to the waypoint. The signal that indicates this mode is activated is a green camera looking at that point on the waypoint.
                        for (i = 0; i < waypointPath.Count(); i++)
                        {
                            if (waypointPath.GetPoint(i) == auxPoint)
                            {
                                break;
                            }
                            middlePointsIndexTop += waypointPath.GetPoint(i).SegmentsTop;
                            middlePointsIndexRight += waypointPath.GetPoint(i).SegmentsRight;

                        }
                        //If this is the last point, there is no curve to look at anymore
                        if (middlePointsIndexRight >= waypointPath.middlePointsRight.Count)
                        {
                            break;
                        }
                        //Now from here we calculate the rotation of the camera mentioned above
                        Vector3 pointRight = waypointPath.middlePointsRight[middlePointsIndexRight];
                        Vector3 pointTop = waypointPath.middlePointsTop[middlePointsIndexTop];


                        //Esto es para rotar las imagenes de las camaras que tienen cada Waypoint
                        Transform cube3 = auxPoint.PointTransf.Find("CubeTop");
                        cube3.gameObject.SetActive(true);
                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from3 = new Vector2(-1, 0);
                        Vector3 aux4 = pointTop - auxPoint.PointTransf.position;
                        Vector2 to3 = new Vector2(aux4.x, aux4.z).normalized;

                        angle3 = Vector2.SignedAngle(from3, to3);

                        Transform cube4 = auxPoint.PointTransf.Find("CubeFront");
                        cube4.gameObject.SetActive(true);

                        cube3.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.green;
                        cube4.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.green;

                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from4 = new Vector2(-1, 0);
                        Vector3 aux5 = pointRight - auxPoint.PointTransf.position;
                        Vector2 to4 = new Vector2(aux5.x, aux5.y).normalized;
                        angle4 = Vector2.SignedAngle(from4, to4);

                        //float angle = Mathf.Acos(distance2 / distance);
                        //item.transform.Rotate(new Vector3(0, 1, 0), Vector2.Angle(from, to));
                        cube3.transform.rotation = Quaternion.Euler(new Vector3(0, -angle3, 0));
                        cube4.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle4));
                        //UnityEngine.Debug.Log(newpoi.GetComponent<POI>().Referenced.Count);
                        break;
                    case 2:

                        //This mode is for a direction with the yaw? locked
                        //We only show a line in this mode, so even if we instantiate a poi, is not shown
                        GameObject newpoi2 = Instantiate(poi, auxGimballUpdateParameters.poi_or_angles + auxPoint.PointTransf.position, Quaternion.Euler(new Vector3(0, 0, 0)));
                        //UnityEngine.Debug.Log(" POI POS: " + auxGimballUpdateParameters.poi_or_angles + " ITEM POS: " + auxPoint.PointTransf.position);


                        /*
                        if (Input.mousePosition.x < Camera.main.pixelWidth / 2)
                        {
                            newpoi.transform.position = new Vector3(newpoi.transform.position.x, 0, newpoi.transform.position.z);

                        }
                        else
                        {
                            newpoi.transform.position = new Vector3(newpoi.transform.position.x, newpoi.transform.position.y, 1);

                        }*/
                        newpoi2.SetActive(true);
                        newpoi2.transform.GetChild(0).gameObject.SetActive(false);
                        newpoi2.transform.GetChild(1).gameObject.SetActive(false);
                        newpoi2.AddComponent<POI>();
                        newpoi2.GetComponent<POI>().Direction = true;

                          
                        float angle5 = 0;
                        float angle6 = 0;
                        float sineC5 = 0;
                            
                               

                        //from now on, we isntantiate a line and calculate position and direction
                        GameObject newLine2 = newpoi2.transform.GetChild(2).gameObject;
                        if (i > 0)
                        {
                            newLine2 = Instantiate(poi.transform.GetChild(2).gameObject, newpoi2.transform.position, Quaternion.Euler(new Vector3(0, 0, 90)), newpoi2.transform);

                        }
                        newLine2.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.grey;
                        newLine2.transform.GetChild(0).gameObject.layer = 10;
                        //Que tamaño tendra
                        float scaleX5 = Mathf.Abs((auxPoint.PointTransf.position - newpoi2.transform.position).magnitude);

                        newLine2.transform.localScale = new Vector3(scaleX5, 3f, 3f);
                        //Esto es para rotar las imagenes de las camaras que tienen cada Waypoint
                        Transform cube5 = auxPoint.PointTransf.Find("CubeTop");
                        cube5.gameObject.SetActive(false);
                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from5 = new Vector2(-1, 0);
                        Vector3 aux6 = newpoi2.transform.position - auxPoint.PointTransf.position;
                        Vector2 to5 = new Vector2(aux6.x, aux6.z).normalized;
                        if (i == 0)
                        {
                            angle5 = Vector2.SignedAngle(from5, to5);
                        }
                        Transform cube6 = auxPoint.PointTransf.Find("CubeFront");
                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from6 = new Vector2(-1, 0);
                        Vector3 aux7 = newpoi2.transform.position - auxPoint.PointTransf.position;
                        Vector2 to6 = new Vector2(aux7.x, aux7.y).normalized;
                        if (i == 0)
                            angle6 = Vector2.SignedAngle(from6, to6);

                        //float angle = Mathf.Acos(distance2 / distance);
                        //item.transform.Rotate(new Vector3(0, 1, 0), Vector2.Angle(from, to));
                        cube5.transform.rotation = Quaternion.Euler(new Vector3(0, -angle5, 0));
                        cube6.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle6));
                        cube5.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
                        cube6.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
                        auxPoint.PointTransf.GetComponent<PathPoint>().Poi = newpoi2.transform;
                        newpoi2.GetComponent<POI>().addPoint(auxPoint.PointTransf.GetComponent<PathPoint>(), newLine2);
                        //UnityEngine.Debug.Log(newpoi.GetComponent<POI>().Referenced.Count);
                       
                            sineC5 = (auxPoint.PointTransf.transform.position.y - newpoi2.transform.position.y) / scaleX5;

                        newLine2.transform.rotation = Quaternion.Euler(0, -angle5, Mathf.Asin(sineC5) * Mathf.Rad2Deg);

                        newLine2.transform.position = auxPoint.PointTransf.transform.position;
                        var rotation2 = Quaternion.Euler(0, -angle5, Mathf.Asin(sineC5) * Mathf.Rad2Deg).eulerAngles;
                       

                                
                            
                            
                        
                        break;
                    case 3:

                        //Same as above without locking yaw
                        GameObject newpoi3 = Instantiate(poi, auxGimballUpdateParameters.poi_or_angles + auxPoint.PointTransf.position, Quaternion.Euler(new Vector3(0, 0, 0)));

                    
                        newpoi3.transform.GetChild(0).gameObject.SetActive(false);
                        newpoi3.transform.GetChild(1).gameObject.SetActive(false);
                        newpoi3.AddComponent<POI>();
                        newpoi3.GetComponent<POI>().Direction = true;

                        float angle7 = 0;
                        float angle8 = 0;
                        float sineC7 = 0;

                        //Creamos una nueva linea que apunta del waypoint al POI
                        GameObject newLine3 = newpoi3.transform.GetChild(2).gameObject;
                       
                            //newLine3 = Instantiate(poi.transform.GetChild(2).gameObject, newpoi3.transform.position, Quaternion.Euler(new Vector3(0, 0, 0)), newpoi3.transform);


                        newLine3.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.gray;
                        //Que tamaño tendra
                        float scaleX7 = Mathf.Abs((auxPoint.PointTransf.position - newpoi3.transform.position).magnitude);

                        newLine3.transform.localScale = new Vector3(scaleX7, 3f, 3f);
                        //Esto es para rotar las imagenes de las camaras que tienen cada Waypoint
                        Transform cube7 = auxPoint.PointTransf.Find("CubeTop");
                        cube7.gameObject.SetActive(true);

                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from7 = new Vector2(-1, 0);
                        Vector3 aux8 = newpoi3.transform.position - auxPoint.PointTransf.position;
                        Vector2 to7 = new Vector2(aux8.x, aux8.z).normalized;
                        if (i == 0)
                        {
                            angle7 = Vector2.SignedAngle(from7, to7);
                        }
                        Transform cube8 = auxPoint.PointTransf.Find("CubeFront");
                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from8 = new Vector2(-1, 0);
                        Vector3 aux9 = newpoi3.transform.position - auxPoint.PointTransf.position;
                        Vector2 to8 = new Vector2(aux8.x, aux8.y).normalized;
                        if (i == 0)
                            angle2 = Vector2.SignedAngle(from8, to8);

                        //float angle = Mathf.Acos(distance2 / distance);
                        //item.transform.Rotate(new Vector3(0, 1, 0), Vector2.Angle(from, to));
                        cube7.transform.rotation = Quaternion.Euler(new Vector3(0, -angle7, 0));
                        cube8.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle8));
                        cube7.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
                        cube8.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
                        auxPoint.PointTransf.GetComponent<PathPoint>().Poi = newpoi3.transform;
                        newpoi3.GetComponent<POI>().addPoint(auxPoint.PointTransf.GetComponent<PathPoint>(), newLine3);
                        //UnityEngine.Debug.Log(newpoi.GetComponent<POI>().Referenced.Count);
                            sineC7 = (auxPoint.PointTransf.position.y - newpoi3.transform.position.y) / scaleX7;

                        newLine3.transform.rotation = Quaternion.Euler(0, -angle7, Mathf.Asin(sineC7) * Mathf.Rad2Deg);
                        newLine3.transform.position = auxPoint.PointTransf.transform.position;


                            

                        


                        break;
                    default:
                        break;
                }
            }

            //This assigns reccam parameters to the waypoints that had them
            for (int i = 0; i < waypointPath.rcParametersCount(); i++)
            {
                RecCamParameters auxRcUpdateParameters = waypointPath.getRcUpdateParameter(i);

                reSetRecCamGuis(auxRcUpdateParameters, waypointPath.GetPoint(auxRcUpdateParameters.id_pointer).PointTransf.gameObject.GetComponent<PathPoint>());

            }

            if (poiEditorPanel != null)
                poiEditorPanel.GetComponent<POIEditor>().Deactivate();

            //activate every panel. If we saved a path, there has to be enough points to make sure this can be done
            if (editPointButton != null)
            {
                editPointButton.interactable = true;
                editPOIButton.interactable = true;
                previewButton.interactable = true;
                CurveButton.interactable = true;
            }


        }
        else {//We assign an index to the new path
            if (waypointPath != null) {
                MissionManager.planIndex = map.Paths.Count;
            }
        }
        //importedGO.name = "DaeModel";


    }
    /// <summary>
    /// The function that loads a pointcloud. Same as mentioned in PlaceModel.cs
    /// </summary>
    /// <param name="pcl"></param>
    /// <param name="daeModel"></param>
    void LoadPointCloud(byte[] pcl, ref GameObject daeModel)
    {
        int numDrawn = 0;
        //PCLMsg pclmsg = queue.Dequeue();

           

        PCLMsgHeader cm = new PCLMsgHeader();
        string cabezeraBasura = BitConverter.ToString(pcl,0,4);
        UInt64 numberOfPCL = BitConverter.ToUInt64(pcl, 28);
        PCLMesh[] mesh;
        mesh = new PCLMesh[numberOfPCL];
        int tamañoPointCloudActual = 0;
        int tamañoPointCloudPasado = 0;
        for (UInt64 i = 0; i < numberOfPCL; i++)
        {
            string firma = BitConverter.ToString(pcl,36 + tamañoPointCloudPasado  ,4);
            tamañoPointCloudActual += 4;
            cm.x = BitConverter.ToSingle(pcl, 36 + tamañoPointCloudPasado + 4);
            tamañoPointCloudActual += 4;

            cm.y = BitConverter.ToSingle(pcl, 36 + tamañoPointCloudPasado + 8);
            tamañoPointCloudActual += 4;

            cm.z = BitConverter.ToSingle(pcl, 36 + tamañoPointCloudPasado + 12);
            tamañoPointCloudActual += 4;

            cm.pitch = BitConverter.ToSingle(pcl, 36 + tamañoPointCloudPasado + 16);
            tamañoPointCloudActual += 4;

            cm.roll =  BitConverter.ToSingle(pcl, 36 + tamañoPointCloudPasado + 20);
            tamañoPointCloudActual += 4;

            cm.yaw =   BitConverter.ToSingle(pcl, 36 + tamañoPointCloudPasado + 24);
            tamañoPointCloudActual += 4;

            UInt64 numberOfPoints = BitConverter.ToUInt32(pcl, 36 + tamañoPointCloudPasado + 28);
            tamañoPointCloudActual += 8;
 
            
                mesh[i] = new PCLMesh();
                fillMesh(ref mesh[i], pcl, cm, numberOfPoints,tamañoPointCloudPasado, ref tamañoPointCloudActual);
                // Instanciate pointclouds in gameobjects
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
            new Vector3(1,1,1));

        instance.transform.rotation = toUnityCoordsMat.rotation;
        instance.transform.position = new Vector3(toUnityCoordsMat.m03, toUnityCoordsMat.m13,toUnityCoordsMat.m23);
            
        //instance.transform.RotateAround(drone.transform.position, new Vector3(1, 0, 0), toUnityCoordsMat.rotation.eulerAngles.x);
        //instance.transform.RotateAround(drone.transform.position, new Vector3(0, 1, 0), -toUnityCoordsMat.rotation.eulerAngles.y);
        //instance.transform.RotateAround(drone.transform.position, new Vector3(0, 0, 1), toUnityCoordsMat.rotation.eulerAngles.z);
        //UnityEngine.Debug.Log(toUnityCoordsMat);

        //instance.transform.localScale = new Vector3(-1 * instance.transform.localScale.x, instance.transform.localScale.y, instance.transform.localScale.z);

                //instance.name = "cloudgo" + pclmsg.cm.pointCloudID.i + "" + pclmsg.cm.pointCloudID.j + "" + pclmsg.cm.pointCloudID.k + "" + pclmsg.cm.pointCloudID.heading;


        instance.transform.parent = daeModel.transform;
            tamañoPointCloudPasado = tamañoPointCloudActual;
            
		}
        


        
        

       
        

    }

    
    public void fillMesh(ref PCLMesh mesh, byte[] data_bytes, PCLMsgHeader header, UInt64 num_points, int offset_data, ref int tamañoPointCloudActual)
    {
        //UnityEngine.Debug.Log("offset data " + offset_data);

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

        for (int i = 0, byte_index =  8+ offset_data; i < (int)N; byte_index += PCLMsgOffsets.POINTNORMAL_SIZE, i++)
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


    /// <summary>
    /// This function assigns the reccam parameters saved to a waypoint
    /// </summary>
    /// <param name="rc"> The reccamparameters to assign</param>
    /// <param name="point">Waypoint that will get reccam parameters assigned</param>
    void reSetRecCamGuis(RecCamParameters rc, PathPoint point)
    {
        //This switch is based on  messagecodes RCamCommandType
        for (int i = 0; i < rc.reccam_parameters.Count; i++)
        {
            switch (rc.reccam_parameters[i].array[0])
            {
                case 6:
                    point.Rc.active = true;
                    point.Rc.switchToRec = 1;
                    break;
                case 7:
                    point.Rc.active = true;
                    point.Rc.switchToRec = 0;

                    break;
                
                case 9:
                    point.Rc.active = true;
                    point.Rc.switchToRec = 1;

                    break;
                case 10:
                    point.Rc.active = false;
                    break;
                case 11:
                    point.Rc.active = true;
                    point.Rc.switchToRec = 0;
                    break;
                case 12:
                    point.Rc.active = true;
                    point.Rc.switchToRec = 0;
                    break;
                case 17:
                    //This switch is based on RCamConfigParameter
                    switch (rc.reccam_parameters[i].array[1])
                    {
                        case 0:
                            point.Rc.resolution = (rc.reccam_parameters[i].array[2]);
                            break;
                        case 1:
                            point.Rc.megaPixels = rc.reccam_parameters[i].array[2];
                            break;
                        case 2:
                            point.Rc.autoManualWB = rc.reccam_parameters[i].array[2];
                            break;
                        case 54:
                            point.Rc.WBTint = rc.reccam_parameters[i].array[2];
                            break;
                        case 3:
                            point.Rc.ISO = rc.reccam_parameters[i].array[2];
                            break;
                        case 4:
                            point.Rc.sharpness = rc.reccam_parameters[i].array[2];
                            break;
                        case 5:
                            point.Rc.contrast = new byte[]{ rc.reccam_parameters[i].array[2], rc.reccam_parameters[i].array[3], rc.reccam_parameters[i].array[4], rc.reccam_parameters[i].array[5] };
                            break;
                        case 6:
                            point.Rc.AE = rc.reccam_parameters[i].array[2];
                            break;
                        case 12:
                            point.Rc.AF = 1;
                            break;
                        case 14:
                            point.Rc.saturation = new byte[] { rc.reccam_parameters[i].array[2], rc.reccam_parameters[i].array[3], rc.reccam_parameters[i].array[4], rc.reccam_parameters[i].array[5] };
                            break;
                        case 15:
                            point.Rc.brightness = new byte[] { rc.reccam_parameters[i].array[2], rc.reccam_parameters[i].array[3], rc.reccam_parameters[i].array[4], rc.reccam_parameters[i].array[5] };
                            break;
                        case 17:
                            point.Rc.photoQuality = rc.reccam_parameters[i].array[2];
                            break;
                        case 19:
                            point.Rc.upsideDown = rc.reccam_parameters[i].array[2];
                            break;
                        case 21:
                            point.Rc.irisAperture = rc.reccam_parameters[i].array[2];
                            break;
                        case 36:
                            point.Rc.burstMode = rc.reccam_parameters[i].array[2];
                            break;
                        case 64:
                            point.Rc.burstSpeed = rc.reccam_parameters[i].array[2];
                            break;
                        default:
                            break;
                    }
                    break;
                case 32:
                    point.Rc.active = true;
                    point.Rc.switchToRec = 0;
                    break;
                case 33:
                    point.Rc.active = false;
                    break;

                default:
                    break;
            }
        }



        point.Rc.edited = true;
        

    }

    //No longer used. Maps download on Plan Selector
    bool AskForMap()
    {
        // Open the file using the path
        if (!File.Exists(Application.persistentDataPath + "/PersistentData/Maps/" + MissionManager.guid + ".dae.map"))
        {
            clientUnity.client.sendTwoPartCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_MAP, MissionManager.guid + "\0");
            PlanSelectionManager.askedForMaps = true;

            return false;
        }
        return true;
    }
    //Scales the sphere
    void TransformSphere(Camera cam, GameObject esfera)
    {
        if (frontCam != null)
        {
            if (frontCam.orthographicSize > 15)
            {
                foreach (Transform child in sphereParentGO.transform)
                {
                    child.GetChild(0).localScale = new Vector3(frontCam.orthographicSize / 8.0f * 25, frontCam.orthographicSize / 8.0f * 25, frontCam.orthographicSize / 8.0f * 25);
                   
                }
            }
            else
            {
                foreach (Transform child in sphereParentGO.transform)
                {
                    child.GetChild(0).localScale = new Vector3(frontCam.orthographicSize / 4.0f * 25, frontCam.orthographicSize / 4.0f * 25, frontCam.orthographicSize / 4.0f * 25);
                }
            }
        }
        

    }
    //Scales the POI
    void TransformPOI(Camera cam, GameObject esfera)
    {
        if (frontCam != null)
        {
            if (frontCam.orthographicSize > 15)
            {
                foreach (GameObject child in poisCreated)
                {
                    child.transform.localScale = new Vector3(frontCam.orthographicSize / 15.0f, frontCam.orthographicSize / 15.0f, frontCam.orthographicSize / 15.0f);

                }
            }
            else
            {
                foreach (GameObject child in poisCreated)
                {
                    child.transform.localScale = new Vector3(frontCam.orthographicSize / 8.0f, frontCam.orthographicSize / 8.0f, frontCam.orthographicSize / 8.0f);
                }
            }
        }


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
