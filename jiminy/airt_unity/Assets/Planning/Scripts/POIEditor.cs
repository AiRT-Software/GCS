using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.UI;
//This class adds and edits points of interest
public class POIEditor : MonoBehaviour {

    private Path path;
    private List<GameObject> pointSelected = new List<GameObject>();
    List<GameObject> poiToDraw = new List<GameObject>();
    public Camera topCam;
    public Camera frontCam;
    Vector3 lastPosition;
    public GameObject poi;
    // Materiales para indicar el POI
    public Material  greenMaterial, whitePOIOutlineMaterial;
    // Materiales para indicar la esfera seleccionada
    public Material waypointNotSelectedMaterial, waypointSelectedMaterial;
    public Material landingSelectedMaterial, landingNotSelectedMaterial;
    public Material homeSelectedMaterial, homeNotSelectedMaterial;
    // Objeto contenedor de todas las esferas
    public GameObject sphereParent;
    public GameObject esfera;
    private float orthoZoomSpeed = 0.02f;
    GameObject auxPointSelected;
    GameObject lastPoi;
    bool modeDirection = false;
    //Change state
    GimballParameters.GimbalMode POIEditorState = GimballParameters.GimbalMode.LOOK_AT_POI;
    public Button changeStateButton;
    public Sprite lookAtPOI, lookAhead, lookAheadFixPitch, lockDirection;
    // Use this for initialization
    void Awake()
    {
        path = Path.Instance;
    }
    private void Update()
    {
        //We check which side was clicked(left or right) Ont the left we have the top cam and on the left the front cam (it may change to fully 3d)
        Camera aux = frontCam;
        // Identificar camara que vamos a tratar
        if (Input.mousePosition.x < Camera.main.pixelWidth / 2)
        {
            aux = topCam;
        }


        Vector3 point = Camera.main.ViewportToScreenPoint(new Vector3(aux.rect.x, aux.rect.y, 0));
        Vector3 point2 = Camera.main.ViewportToScreenPoint(new Vector3(aux.rect.xMax, aux.rect.yMax, 0));

        var rect = new Rect(point.x, point.y, point2.x - point.x, point2.y - point.y);
        if (!rect.Contains(Input.mousePosition))
            return;
        //Depending on if we are using windows or android, we check if a mouse was clicked or if we touched the the screen with a finger. 
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (Input.GetMouseButtonDown(0))
#else
        if (Input.touchCount == 1 &&  Input.GetMouseButtonDown(0) )
#endif
        {
            //We throw a ray from the main camera. This will touch an element from the screen and if that element is a waypoint of point of interest, we get it
            Ray ray = aux.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z));
            auxPointSelected = RayToSphere(ray);
            //This state machine has 4 states. Make the camera look at a point of interest, make the camera always look at the curve, lock the camera in a direction with a locked yaw and lock the camera in a direction 

            switch (POIEditorState)
            {
                case GimballParameters.GimbalMode.LOOK_AT_POI:
                    //if the user clicked an empty space, and there are waypoints selected, this condition is true
                    if (!auxPointSelected && pointSelected.Count > 0)
                    {


                        //If the user clicked in a point of interest previously, we deselect the previous POI
                        if (lastPoi != null)
                        {
                            lastPoi.transform.GetChild(0).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                            lastPoi.transform.GetChild(1).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                        }
                        if (Input.mousePosition.x > Camera.main.pixelWidth / 2)
                        {
                            return;
                        }
                        //Instantiate a new point of interest
                        GameObject newpoi = Instantiate(poi, aux.ScreenToWorldPoint(Input.mousePosition), Quaternion.Euler(new Vector3(0, 0, 0)));
                        //Depending of the camera clicked, the y or the z is fixed
                        if (Input.mousePosition.x < Camera.main.pixelWidth / 2)
                        {
                            newpoi.transform.position = new Vector3(newpoi.transform.position.x, MissionManager.planDefaultHeight, newpoi.transform.position.z);

                        }
                        else
                        {
                            newpoi.transform.position = new Vector3(newpoi.transform.position.x, newpoi.transform.position.y, 1);

                        }
                        //We add the script to this POI
                        newpoi.SetActive(true);
                        newpoi.AddComponent<POI>();
                        //And add it to the array that will scale it
                        poiToDraw.Add(newpoi);
                        newpoi.transform.GetChild(0).GetComponent<Renderer>().material = greenMaterial;
                        newpoi.transform.GetChild(1).GetComponent<Renderer>().material = greenMaterial;
                        int i = 0;
                        foreach (var item in pointSelected)
                        {
                            //If the waypoint that have the new POI had any previous POI, stop referencing the old POI
                            if (item.gameObject.GetComponent<PathPoint>().Poi != null)
                            {
                                Transform poiToDelete = item.gameObject.GetComponent<PathPoint>().Poi;
                                poiToDelete.GetComponent<POI>().removePoint(item.gameObject.GetComponent<PathPoint>());
                                //If the old POI isn't referenced anymore, destroy the POI
                                if (poiToDelete.GetComponent<POI>().Referenced.Count == 0)
                                {
                                    Destroy(poiToDelete.gameObject);
                                }
                                else
                                {
                                    //If there is still a waypoint referencing, at least remove the lines that connected the waypoint to the old POI
                                    foreach (Transform item2 in poiToDelete)
                                    {
                                        Vector3 centerPosToDelete = new Vector3(item.transform.position.x + poiToDelete.transform.position.x, item.transform.position.y + poiToDelete.transform.position.y, item.transform.position.z + poiToDelete.transform.position.z) / 2f;

                                        if (item.transform.position == item2.position)
                                        {
                                            poiToDraw.Remove(item2.gameObject);
                                            Destroy(item2.gameObject);

                                        }
                                    }
                                }
                            }
                            

                            //We create a new line that points from the waypoint to the POI
                            GameObject newLine = newpoi.transform.GetChild(2).gameObject;
                            if (i > 0)
                            {
                                newLine = Instantiate(poi.transform.GetChild(2).gameObject, newpoi.transform.position, Quaternion.Euler(new Vector3(0, 0, 0)), newpoi.transform);

                            }
                            //We determine the length, position and rotation of the line here
                            Vector3 centerPos = new Vector3(item.transform.position.x + newpoi.transform.position.x, item.transform.position.y + newpoi.transform.position.y, item.transform.position.z + newpoi.transform.position.z) / 2f;
                            float scaleX = Mathf.Abs((item.transform.position - newpoi.transform.position).magnitude);

                            newLine.transform.localScale = new Vector3(scaleX, 3f, 3f);
                            //Esto es para rotar las imagenes de las camaras que tienen cada Waypoint
                            Transform cube = item.transform.Find("CubeTop");
                            cube.gameObject.SetActive(true);

                            //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                            //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                            Vector2 from = new Vector2(-1, 0);
                            Vector3 aux2 = newpoi.transform.position - item.transform.position;
                            Vector2 to = new Vector2(aux2.x, aux2.z).normalized;
                            float angle = Vector2.SignedAngle(from, to);
                            Transform cube2 = item.transform.Find("CubeFront");
                            //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                            //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                            Vector2 from2 = new Vector2(-1, 0);
                            Vector3 aux3 = newpoi.transform.position - item.transform.position;
                            Vector2 to2 = new Vector2(aux3.x, aux3.y).normalized;
                            float angle2 = Vector2.SignedAngle(from2, to2);

                            //float angle = Mathf.Acos(distance2 / distance);
                            //item.transform.Rotate(new Vector3(0, 1, 0), Vector2.Angle(from, to));
                            cube.transform.rotation = Quaternion.Euler(new Vector3(0, -angle, 0));
                            cube2.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle2));
                            cube.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
                            cube2.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
                            item.GetComponent<PathPoint>().Poi = newpoi.transform;
                            newpoi.GetComponent<POI>().addPoint(item.GetComponent<PathPoint>(), newLine);
                            //UnityEngine.Debug.Log(newpoi.GetComponent<POI>().Referenced.Count);

                            float sineC = (item.transform.position.y - newpoi.transform.position.y) / scaleX;

                            newLine.transform.rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg);
                            newLine.transform.position = newpoi.transform.position;
                            var rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg).eulerAngles;
                            item.GetComponent<PathPoint>().GimbalRotation = newpoi.transform.position;
                            item.GetComponent<PathPoint>().getGimballMode = 0;
                            lastPoi = newpoi;
                            i++;
                            int pos = 0;
                            //Here we add the gimbal parameter to add send it to the server later.
                            pos = path.NeedsToUpdateGbParameters(item.GetComponent<PathPoint>().Id);
                            if (pos == -1)
                            {
                                item.GetComponent<PathPoint>().Wp.gimbal_parameters.id_pointer = item.GetComponent<PathPoint>().Id;
                                path.addNewParamGb(item.GetComponent<PathPoint>().Wp.gimbal_parameters);
                            }
                            else
                            {
                                item.GetComponent<PathPoint>().Wp.gimbal_parameters.id_pointer = item.GetComponent<PathPoint>().Id;
                                path.updateParamGB(pos, item.GetComponent<PathPoint>().Wp.gimbal_parameters);
                            }
                        }
                        auxPointSelected = newpoi;
                    }
                    //If we clicked a waypoint instead of an empty space, we enter here
                    else if (auxPointSelected && auxPointSelected.tag == "Waypoint")
                    {
                        //If the waypoint was not selected, we add it to the array of selected points that will reference a POI if an empty space is clicked with them selected
                        if (!pointSelected.Contains(auxPointSelected))
                        {
                            pointSelected.Add(auxPointSelected);
                        }
                        else
                        {
                            //If it was already selected, we deselect it
                            if (auxPointSelected.GetComponent<PathPoint>().Id == 0 )
                            {
                                auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeNotSelectedMaterial;

                            }
                            else if (auxPointSelected.GetComponent<PathPoint>().Id == path.Count() - 1)
                            {
                                auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingNotSelectedMaterial;

                            }
                            else
                            {
                                auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().material = waypointNotSelectedMaterial;
                            }
                            pointSelected.Remove(auxPointSelected);
                        }

                    }


                    break;
                case GimballParameters.GimbalMode.LOOK_AHEAD:
                    //This case makes the camera look all the time to the curve
                    if (auxPointSelected && auxPointSelected.tag == "Waypoint")
                    {
                        if (Input.mousePosition.x > Camera.main.pixelWidth / 2)
                        {
                            return;
                        }
                        int i = 0;
                        float angle = 0;
                        float angle2 = 0;

                        auxPointSelected.GetComponent<PathPoint>().getGimballMode = 1;
                        int middlePointsIndexTop = 0;
                        int middlePointsIndexRight = 0;
                        //If the waypoint had a POI before, stop referencing it
                        if (auxPointSelected.gameObject.GetComponent<PathPoint>().Poi != null)
                        {
                            Transform poiToDelete = auxPointSelected.gameObject.GetComponent<PathPoint>().Poi;
                            poiToDelete.GetComponent<POI>().removePoint(auxPointSelected.gameObject.GetComponent<PathPoint>());
                            if (poiToDelete.GetComponent<POI>().Referenced.Count == 0)
                            {
                                Destroy(poiToDelete.gameObject);
                            }
                            else
                            {
                                //Remove the cubes that joined the waypoint and POI
                                foreach (Transform item2 in poiToDelete)
                                {
                                    Vector3 centerPosToDelete = new Vector3(auxPointSelected.transform.position.x + poiToDelete.transform.position.x, auxPointSelected.transform.position.y + poiToDelete.transform.position.y, auxPointSelected.transform.position.z + poiToDelete.transform.position.z) / 2f;

                                    if (auxPointSelected.transform.position == item2.position)
                                    {
                                        Destroy(item2.gameObject);

                                    }
                                }
                            }
                        }
                        //We find which point was clicked and get the index of points from the catmull rom that reference the point just in front of the waypoint. 
                        for (i = 0; i < path.Count(); i++)
                        {
                            if (path.GetPoint(i).PointTransf.gameObject == auxPointSelected)
                            {
                                break;
                            }
                            middlePointsIndexTop += path.GetPoint(i).SegmentsTop;
                            middlePointsIndexRight += path.GetPoint(i).SegmentsRight;
                            
                        }
                        //If we are on the last waypoint, exit
                        if (middlePointsIndexRight >= path.middlePointsRight.Count)
                        {
                            break ;
                        }
                        //Get the point for each curve
                        Vector3 pointRight = path.middlePointsRight[middlePointsIndexRight];
                        Vector3 pointTop = path.middlePointsTop[middlePointsIndexTop];


                        //We rotate the camera image that is on the waypoint and color it green
                        Transform cube = auxPointSelected.transform.Find("CubeTop");
                        cube.gameObject.SetActive(true);
                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from = new Vector2(-1, 0);
                        Vector3 aux2 = pointTop - auxPointSelected.transform.position;
                        Vector2 to = new Vector2(aux2.x, aux2.z).normalized;
                       
                        angle = Vector2.SignedAngle(from, to);
                       
                        Transform cube2 = auxPointSelected.transform.Find("CubeFront");
                        cube2.gameObject.SetActive(true);

                        cube.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.green;
                        cube2.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.green;

                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from2 = new Vector2(-1, 0);
                        Vector3 aux3 = pointRight - auxPointSelected.transform.position;
                        Vector2 to2 = new Vector2(aux3.x, aux3.y).normalized;
                        angle2 = Vector2.SignedAngle(from2, to2);

                        //float angle = Mathf.Acos(distance2 / distance);
                        //item.transform.Rotate(new Vector3(0, 1, 0), Vector2.Angle(from, to));
                        cube.transform.rotation = Quaternion.Euler(new Vector3(0, -angle, 0));
                        cube2.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle2));
                        UnityEngine.Debug.Log("Point Top: " + pointRight);

                        //UnityEngine.Debug.Log(newpoi.GetComponent<POI>().Referenced.Count);
                        //As always we add or update the parameter of the gimbal to send it to the server
                        int pos = 0;
                        pos = path.NeedsToUpdateGbParameters(auxPointSelected.GetComponent<PathPoint>().Id);
                        if (pos == -1)
                        {
                            auxPointSelected.GetComponent<PathPoint>().Wp.gimbal_parameters.id_pointer = auxPointSelected.GetComponent<PathPoint>().Id;

                            path.addNewParamGb(auxPointSelected.GetComponent<PathPoint>().Wp.gimbal_parameters);

                        }
                        else
                        {
                            auxPointSelected.GetComponent<PathPoint>().Wp.gimbal_parameters.id_pointer = auxPointSelected.GetComponent<PathPoint>().Id;
                            path.updateParamGB(pos, auxPointSelected.GetComponent<PathPoint>().Wp.gimbal_parameters);

                        }











                        //If it was already selected, we deselect it
                        if (auxPointSelected.GetComponent<PathPoint>().Id == 0)
                        {
                            auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeNotSelectedMaterial;

                        }
                        else if (auxPointSelected.GetComponent<PathPoint>().Id == path.Count() - 1)
                        {
                            auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingNotSelectedMaterial;

                        }
                        else
                        {
                            auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().material = waypointNotSelectedMaterial;
                        }

                    }
                    break;
                case GimballParameters.GimbalMode.LOOK_AHEAD_FIX_PITCH:

                    
                    //If the user clicked on a space
                    if (!auxPointSelected && pointSelected.Count > 0 && aux == topCam)
                        {

                            
                            if (lastPoi != null)
                            {
                                lastPoi.transform.GetChild(0).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                                lastPoi.transform.GetChild(1).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                            }
                            if (Input.mousePosition.x > Camera.main.pixelWidth / 2)
                            {
                                return;
                            }

                        GameObject newpoi = Instantiate(poi,new Vector3( aux.ScreenToWorldPoint(Input.mousePosition).x, 1, aux.ScreenToWorldPoint(Input.mousePosition).z), Quaternion.Euler(new Vector3(0, 0, 0)));
                            
                            if (Input.mousePosition.x < Camera.main.pixelWidth / 2)
                            {
                                newpoi.transform.position = new Vector3(newpoi.transform.position.x, 0, newpoi.transform.position.z);

                            }
                            else
                            {
                                newpoi.transform.position = new Vector3(newpoi.transform.position.x, newpoi.transform.position.y, 1);

                            }
                            newpoi.SetActive(true);
                            newpoi.transform.GetChild(0).gameObject.SetActive(false);
                            newpoi.transform.GetChild(1).gameObject.SetActive(false);
                            newpoi.AddComponent<POI>();
                            newpoi.GetComponent<POI>().Direction = true;

                            newpoi.transform.GetChild(0).GetComponent<Renderer>().material = greenMaterial;
                            newpoi.transform.GetChild(1).GetComponent<Renderer>().material = greenMaterial;
                            int i = 0;
                            float angle = 0;
                            float angle2 = 0;
                            float sineC = 0;

                            //Crear un poi
                            //We sort the Waypoints by id to give the direction obtained on the first waypoint to all the rest
                            pointSelected.Sort((IComparer<GameObject>)new SortPointsByID());
                            foreach (var item in pointSelected)
                            {
                                int middlePointsIndexTop = 0;
                                int middlePointsIndexRight = 0;
                                //We find which point was clicked and get the index of points from the catmull rom that reference the point just in front of the waypoint. 
                                for (int i2 = 0; i2 < path.Count(); i2++)
                                {
                                    if (path.GetPoint(i2).PointTransf.gameObject == item)
                                    {
                                        break;
                                    }
                                    middlePointsIndexTop += path.GetPoint(i2).SegmentsTop;
                                    middlePointsIndexRight += path.GetPoint(i2).SegmentsRight;

                                }
                                //If we are on the last waypoint, exit
                                if (middlePointsIndexRight >= path.middlePointsRight.Count)
                                {
                                    continue;
                                }
                                //Get the point for each curve
                                Vector3 pointRight = path.middlePointsRight[middlePointsIndexRight];
                                Vector3 pointTop = path.middlePointsTop[middlePointsIndexTop];
                                
                                //Just like before
                                if (item.gameObject.GetComponent<PathPoint>().Poi != null)
                                    {
                                        Transform poiToDelete = item.gameObject.GetComponent<PathPoint>().Poi;
                                        poiToDelete.GetComponent<POI>().removePoint(item.gameObject.GetComponent<PathPoint>());
                                        if (poiToDelete.GetComponent<POI>().Referenced.Count == 0)
                                        {
                                            Destroy(poiToDelete.gameObject);
                                        }
                                        else
                                        {
                                            //Esto quita los cubos que apuntan del POI al waypoint
                                            foreach (Transform item2 in poiToDelete)
                                            {
                                                Vector3 centerPosToDelete = new Vector3(item.transform.position.x + poiToDelete.transform.position.x, item.transform.position.y + poiToDelete.transform.position.y, item.transform.position.z + poiToDelete.transform.position.z) / 2f;

                                                if (item.transform.position == item2.position)
                                                {
                                                    Destroy(item2.gameObject);

                                                }
                                            }
                                        }
                                    }
                            newpoi.transform.position = new Vector3(newpoi.transform.position.x, pointRight.y, newpoi.transform.position.z);
                                    //Creamos una nueva linea que apunta del waypoint al POI
                                    GameObject newLine = newpoi.transform.GetChild(2).gameObject;
                                    if (i > 0)
                                    {
                                        newLine = Instantiate(poi.transform.GetChild(2).gameObject, newpoi.transform.position, Quaternion.Euler(new Vector3(0, 0, 90)), newpoi.transform);

                                    }
                                    newLine.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.magenta;
                                    //newLine.transform.GetChild(0).gameObject.layer = 10;
                                    //Que tamaño tendra
                                    Vector3 centerPos = new Vector3(item.transform.position.x + newpoi.transform.position.x, item.transform.position.y + newpoi.transform.position.y, item.transform.position.z + newpoi.transform.position.z) / 2f;
                                    float scaleX = Mathf.Abs((item.transform.position - newpoi.transform.position).magnitude);

                                    newLine.transform.localScale = new Vector3(scaleX, 3f, 3f);
                                    //Esto es para rotar las imagenes de las camaras que tienen cada Waypoint
                                    Transform cube = item.transform.Find("CubeTop");
                                    cube.gameObject.SetActive(false);
                                    //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                                    //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                                    Vector2 from = new Vector2(-1, 0);
                                    Vector3 aux2 = newpoi.transform.position - item.transform.position;
                                    Vector2 to = new Vector2(aux2.x, aux2.z).normalized;
                                    if (i == 0)
                                    {
                                        angle = Vector2.SignedAngle(from, to);
                                    }
                                    Transform cube2 = item.transform.Find("CubeFront");
                                    //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                                    //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                                    Vector2 from2 = new Vector2(-1, 0);
                                    Vector3 aux3 = newpoi.transform.position - item.transform.position;
                                    Vector2 to2 = new Vector2(aux3.x, aux3.y).normalized;
                                    if (i == 0)
                                        angle2 = Vector2.SignedAngle(from2, to2);

                                    //float angle = Mathf.Acos(distance2 / distance);
                                    //item.transform.Rotate(new Vector3(0, 1, 0), Vector2.Angle(from, to));
                                    cube.transform.rotation = Quaternion.Euler(new Vector3(0, -angle, 0));
                                    cube2.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle2));
                                    cube.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
                                    cube2.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
                                    item.GetComponent<PathPoint>().Poi = newpoi.transform;
                                    newpoi.GetComponent<POI>().addPoint(item.GetComponent<PathPoint>(), newLine);
                                    //UnityEngine.Debug.Log(newpoi.GetComponent<POI>().Referenced.Count);
                                    if (i == 0)
                                        sineC = (item.transform.position.y - newpoi.transform.position.y) / scaleX;

                                    newLine.transform.rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg);
                                    newLine.transform.position = item.transform.position;
                                    var rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg).eulerAngles;
                                    item.GetComponent<PathPoint>().GimbalRotation = newpoi.transform.position - item.transform.position;
                                    item.GetComponent<PathPoint>().getGimballMode = 2;

                                    lastPoi = newpoi;
                                    i++;
                            //Little change to before. Because we lock direction, every waypoint clicked will have the same direction as the first one, so we will save the parameters of the first one, and the next 
                            //will copy the last waypoint
                            if (i==0)
                            {
                                int pos = 0;
                                pos = path.NeedsToUpdateGbParameters(pointSelected[0].GetComponent<PathPoint>().Id);
                                if (pos == -1)
                                {
                                    item.GetComponent<PathPoint>().Wp.gimbal_parameters.id_pointer = pointSelected[0].GetComponent<PathPoint>().Id;

                                    path.addNewParamGb(item.GetComponent<PathPoint>().Wp.gimbal_parameters);
                                }
                                else
                                {
                                    item.GetComponent<PathPoint>().Wp.gimbal_parameters.id_pointer = pointSelected[0].GetComponent<PathPoint>().Id;

                                    path.updateParamGB(pos, item.GetComponent<PathPoint>().Wp.gimbal_parameters);
                                }
                            }
                                
                            }
                            auxPointSelected = newpoi;
                        }
                    //If a waypoint was clicked, the same as the first one
                        else if (auxPointSelected && auxPointSelected.tag == "Waypoint")
                        {

                            if (!pointSelected.Contains(auxPointSelected))
                            {
                                pointSelected.Add(auxPointSelected);
                            }
                            else
                            {
                                //If it was already selected, we deselect it
                                if (auxPointSelected.GetComponent<PathPoint>().Id == 0)
                                {
                                    auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeNotSelectedMaterial;

                                }
                                else if (auxPointSelected.GetComponent<PathPoint>().Id == path.Count() - 1)
                                {
                                    auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingNotSelectedMaterial;

                                }
                                else
                                {
                                    auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().material = waypointNotSelectedMaterial;
                                }
                                pointSelected.Remove(auxPointSelected);
                            }

                        }


                  
                    break;
                case GimballParameters.GimbalMode.BLOCK_DIRECTION:
                    //Same premise as the last case, except all the axes are blocked
                    if (!auxPointSelected && pointSelected.Count > 0 )
                    {
                        if (lastPoi != null)
                        {
                            lastPoi.transform.GetChild(0).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                            lastPoi.transform.GetChild(1).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                        }
                        if (Input.mousePosition.x > Camera.main.pixelWidth / 2)
                        {
                            return;
                        }
                        GameObject newpoi = Instantiate(poi, aux.ScreenToWorldPoint(Input.mousePosition), Quaternion.Euler(new Vector3(0, 0, 0)));

                        if (Input.mousePosition.x < Camera.main.pixelWidth / 2)
                        {
                            newpoi.transform.position = new Vector3(newpoi.transform.position.x, 1, newpoi.transform.position.z);

                        }
                        else
                        {
                            newpoi.transform.position = new Vector3(newpoi.transform.position.x, newpoi.transform.position.y, 1);

                        }
                        newpoi.SetActive(true);
                        newpoi.transform.GetChild(0).gameObject.SetActive(false);
                        newpoi.transform.GetChild(1).gameObject.SetActive(false);
                        newpoi.AddComponent<POI>();
                        newpoi.GetComponent<POI>().Direction = true;

                        newpoi.transform.GetChild(0).GetComponent<Renderer>().material = greenMaterial;
                        newpoi.transform.GetChild(1).GetComponent<Renderer>().material = greenMaterial;
                        int i = 0;
                        float angle = 0;
                        float angle2 = 0;
                        float sineC = 0;

                        //Crear un poi
                        pointSelected.Sort((IComparer<GameObject>)new SortPointsByID());
                        foreach (var item in pointSelected)
                        {
                            //Si el waypoint tenia un POI antes, dejar de referenciarlo
                            if (item.gameObject.GetComponent<PathPoint>().Poi != null)
                            {
                                Transform poiToDelete = item.gameObject.GetComponent<PathPoint>().Poi;
                                poiToDelete.GetComponent<POI>().removePoint(item.gameObject.GetComponent<PathPoint>());
                                if (poiToDelete.GetComponent<POI>().Referenced.Count == 0)
                                {
                                    Destroy(poiToDelete.gameObject);
                                }
                                else
                                {
                                    //Esto quita los cubos que apuntan del POI al waypoint
                                    foreach (Transform item2 in poiToDelete)
                                    {
                                        Vector3 centerPosToDelete = new Vector3(item.transform.position.x + poiToDelete.transform.position.x, item.transform.position.y + poiToDelete.transform.position.y, item.transform.position.z + poiToDelete.transform.position.z) / 2f;

                                        if (item.transform.position == item2.position)
                                        {
                                            Destroy(item2.gameObject);

                                        }
                                    }
                                }
                            }
                            
                            //Creamos una nueva linea que apunta del waypoint al POI
                            GameObject newLine = newpoi.transform.GetChild(2).gameObject;
                            if (i > 0)
                            {
                                newLine = Instantiate(poi.transform.GetChild(2).gameObject, newpoi.transform.position, Quaternion.Euler(new Vector3(0, 0, 0)), newpoi.transform);

                            }
                            newLine.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.blue;
                            //Que tamaño tendra
                            Vector3 centerPos = new Vector3(item.transform.position.x + newpoi.transform.position.x, item.transform.position.y + newpoi.transform.position.y, item.transform.position.z + newpoi.transform.position.z) / 2f;
                            float scaleX = Mathf.Abs((item.transform.position - newpoi.transform.position).magnitude);

                            newLine.transform.localScale = new Vector3(scaleX, 3f, 3f);
                            //Esto es para rotar las imagenes de las camaras que tienen cada Waypoint
                            Transform cube = item.transform.Find("CubeTop");
                            cube.gameObject.SetActive(true);

                            //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                            //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                            Vector2 from = new Vector2(-1, 0);
                            Vector3 aux2 = newpoi.transform.position - item.transform.position;
                            Vector2 to = new Vector2(aux2.x, aux2.z).normalized;
                            if (i == 0)
                            {
                                angle = Vector2.SignedAngle(from, to);
                            }
                            Transform cube2 = item.transform.Find("CubeFront");
                            //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                            //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                            Vector2 from2 = new Vector2(-1, 0);
                            Vector3 aux3 = newpoi.transform.position - item.transform.position;
                            Vector2 to2 = new Vector2(aux3.x, aux3.y).normalized;
                            if (i == 0)
                                angle2 = Vector2.SignedAngle(from2, to2);

                            //float angle = Mathf.Acos(distance2 / distance);
                            //item.transform.Rotate(new Vector3(0, 1, 0), Vector2.Angle(from, to));
                            cube.transform.rotation = Quaternion.Euler(new Vector3(0, -angle, 0));
                            cube2.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle2));
                            cube.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
                            cube2.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
                            item.GetComponent<PathPoint>().Poi = newpoi.transform;
                            newpoi.GetComponent<POI>().addPoint(item.GetComponent<PathPoint>(), newLine);
                            //UnityEngine.Debug.Log(newpoi.GetComponent<POI>().Referenced.Count);
                            if (i == 0)
                                sineC = (item.transform.position.y - newpoi.transform.position.y) / scaleX;

                            newLine.transform.rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg);
                            newLine.transform.position = item.transform.position;
                            var rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg).eulerAngles;
                            item.GetComponent<PathPoint>().GimbalRotation = newpoi.transform.position - item.transform.position;
                            item.GetComponent<PathPoint>().getGimballMode = 3;

                            lastPoi = newpoi;
                            if (i == 0)
                            {
                                int pos = 0;
                                pos = path.NeedsToUpdateGbParameters(pointSelected[0].GetComponent<PathPoint>().Id);
                                if (pos == -1)
                                {
                                    item.GetComponent<PathPoint>().Wp.gimbal_parameters.id_pointer = pointSelected[0].GetComponent<PathPoint>().Id;
                                    path.addNewParamGb(item.GetComponent<PathPoint>().Wp.gimbal_parameters);
                                }
                                else
                                {
                                    item.GetComponent<PathPoint>().Wp.gimbal_parameters.id_pointer = pointSelected[0].GetComponent<PathPoint>().Id;

                                    path.updateParamGB(pos, item.GetComponent<PathPoint>().Wp.gimbal_parameters);
                                }
                            }
                            i++;
                           
                            
                        }
                        auxPointSelected = newpoi;
                    }
                    else if (auxPointSelected && auxPointSelected.tag == "Waypoint")
                    {

                        if (!pointSelected.Contains(auxPointSelected))
                        {
                            pointSelected.Add(auxPointSelected);
                        }
                        else
                        {
                            //If it was already selected, we deselect it
                            if (auxPointSelected.GetComponent<PathPoint>().Id == 0)
                            {
                                auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeNotSelectedMaterial;

                            }
                            else if (auxPointSelected.GetComponent<PathPoint>().Id == path.Count() - 1)
                            {
                                auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingNotSelectedMaterial;

                            }
                            else
                            {
                                auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().material = waypointNotSelectedMaterial;
                            }
                            pointSelected.Remove(auxPointSelected);
                        }

                    }
            
                    break;
                default:
                    break;
            }

        }
        
        //Move POI STATE MACHINE
        //This state machine is to move the POI or the lines when blocking direction
        switch (POIEditorState)
        {
            case GimballParameters.GimbalMode.LOOK_AT_POI:
                // We check if a POI is selected, to move it
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if ( auxPointSelected && Input.GetMouseButton(0) && auxPointSelected.tag.Equals("POI"))
#else
                if (Input.touchCount == 1 &&  auxPointSelected && Input.GetMouseButton(0) && auxPointSelected.tag.Equals("POI") )
#endif
                {
                    if (lastPoi != null && lastPoi != auxPointSelected)
                    {
                        lastPoi.transform.GetChild(0).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                        lastPoi.transform.GetChild(1).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                        auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().material = greenMaterial;
                        auxPointSelected.transform.GetChild(1).GetComponent<Renderer>().material = greenMaterial;
                        lastPoi = auxPointSelected;
                    }
                    // Move the POI
                    MoveSphere(aux.ScreenToViewportPoint(Input.mousePosition), auxPointSelected);
                    //every waypoint that relates to this POI is selected to indicate that they belong to this sphere
                    if (auxPointSelected.GetComponent<POI>().Referenced.Count != pointSelected.Count)
                    {
                        foreach (var item in pointSelected)
                        {
                            //If it was already selected, we deselect it
                            if (item.GetComponent<PathPoint>().Id == 0)
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeNotSelectedMaterial;

                            }
                            else if (item.GetComponent<PathPoint>().Id == path.Count() - 1)
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingNotSelectedMaterial;

                            }
                            else
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().material = waypointNotSelectedMaterial;
                            }
                        }
                        pointSelected.Clear();

                    }

                    int i = 2;
                    //for each waypoint that the POI is connected to
                    foreach (var item in auxPointSelected.GetComponent<POI>().Referenced.Keys)
                    {

                        //If it's not in the list of pointsselected, we select it
                        if (!pointSelected.Contains(item.PointTransf.gameObject))
                        {
                            if (item.GetComponent<PathPoint>().Id == 0)
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().material = homeSelectedMaterial;


                            }
                            else if (item.GetComponent<PathPoint>().Id == path.Count() - 1)
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().material = landingSelectedMaterial;

                            }
                            else
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().material = waypointSelectedMaterial;
                            }
                            pointSelected.Add(item.gameObject);
                        }
                        //We find the lines that connect this waypoint with the POI
                        Transform cube = item.transform.Find("CubeTop");
                        Transform cube2 = item.transform.Find("CubeFront");
                        //And calculate the new position, rotation and scale
                        Vector3 centerPos = new Vector3(item.transform.position.x + auxPointSelected.transform.position.x, item.transform.position.y + auxPointSelected.transform.position.y, item.transform.position.z + auxPointSelected.transform.position.z) / 2f;
                        float scaleX = Mathf.Abs((item.transform.position - auxPointSelected.transform.position).magnitude);

                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from = new Vector2(-1, 0);
                        Vector3 aux2 = auxPointSelected.transform.position - item.transform.position;
                        Vector2 to = new Vector2(aux2.x, aux2.z).normalized;
                        float angle = Vector2.SignedAngle(from, to);

                        auxPointSelected.transform.GetChild(i).localScale = new Vector3(scaleX, 1f, 1f);



                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from2 = new Vector2(-1, 0);
                        Vector3 aux3 = auxPointSelected.transform.position - item.transform.position;
                        Vector2 to2 = new Vector2(aux3.x, aux3.y).normalized;
                        float angle2 = Vector2.SignedAngle(from2, to2);

                        //float angle = Mathf.Acos(distance2 / distance);
                        //item.transform.Rotate(new Vector3(0, 1, 0), Vector2.Angle(from, to));

                        float sineC = (item.transform.position.y - auxPointSelected.transform.position.y) / scaleX;

                        cube.transform.rotation = Quaternion.Euler(new Vector3(0, -angle, 0));
                        cube2.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle2));
                        auxPointSelected.transform.GetChild(i).rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg);
                        auxPointSelected.transform.GetChild(i).position = item.transform.position;
                        lastPoi = auxPointSelected;
                        var rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg).eulerAngles;
                        item.GetComponent<PathPoint>().GimbalRotation = auxPointSelected.transform.position;
                        int pos = 0;
                        //We also update the parameter that needs to be sent to the drone
                        pos = path.NeedsToUpdateGbParameters(item.GetComponent<PathPoint>().Id);
                        if (pos == -1)
                        {
                            item.GetComponent<PathPoint>().Wp.gimbal_parameters.id_pointer = item.GetComponent<PathPoint>().Id;

                            path.addNewParamGb(item.GetComponent<PathPoint>().Wp.gimbal_parameters);

                        }
                        else
                        {
                            item.GetComponent<PathPoint>().Wp.gimbal_parameters.id_pointer = item.GetComponent<PathPoint>().Id;
                            path.updateParamGB(pos, item.GetComponent<PathPoint>().Wp.gimbal_parameters);

                        }
                        i++;
                    }
                }
                break;
            case GimballParameters.GimbalMode.LOOK_AHEAD:
                //No movement here
                break;
            case GimballParameters.GimbalMode.LOOK_AHEAD_FIX_PITCH:
                //Because we lock the yaw, if we click on the left camera, we exit from here

                

                //the rest moves the lines just like the first case
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (auxPointSelected && Input.GetMouseButton(0) && auxPointSelected.tag.Equals("POI" ) && aux == topCam)
#else
                if (Input.touchCount == 1 &&  auxPointSelected && Input.GetMouseButton(0) && auxPointSelected.tag.Equals("POI") && aux == topCam)
#endif
                {
                    if (lastPoi != null && lastPoi != auxPointSelected)
                    {
                        lastPoi.transform.GetChild(0).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                        lastPoi.transform.GetChild(1).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                        auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().material = greenMaterial;
                        auxPointSelected.transform.GetChild(1).GetComponent<Renderer>().material = greenMaterial;
                        lastPoi = auxPointSelected;
                    }
                    

                    if (auxPointSelected.GetComponent<POI>().Referenced.Count != pointSelected.Count)
                    {

                        foreach (var item in pointSelected)
                        {
                           
                            //If it was already selected, we deselect it
                            if (item.GetComponent<PathPoint>().Id == 0)
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeNotSelectedMaterial;

                            }
                            else if (item.GetComponent<PathPoint>().Id == path.Count() - 1)
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingNotSelectedMaterial;

                            }
                            else
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().material = waypointNotSelectedMaterial;
                            }
                        }
                        pointSelected.Clear();

                    }
                    int i = 0;
                    float sineC = 0;
                    float angle2 = 0;
                    float angle = 0;
                    foreach (var item in pointSelected)
                    {


                        if (!pointSelected.Contains(item.gameObject) && item.transform.tag.Equals("Waypoint"))
                        {

                           
                            if (item.GetComponent<PathPoint>().Id == 0)
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().material = homeSelectedMaterial;


                            }
                            else if (item.GetComponent<PathPoint>().Id == path.Count() - 1)
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().material = landingSelectedMaterial;

                            }
                            else
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().material = waypointSelectedMaterial;
                            }
                            pointSelected.Add(item.gameObject);
                        }
                        int middlePointsIndexTop = 0;
                        int middlePointsIndexRight = 0;
                        //We find which point was clicked and get the index of points from the catmull rom that reference the point just in front of the waypoint. 
                        for (int i2 = 0; i2 < path.Count(); i2++)
                        {
                            if (path.GetPoint(i2).PointTransf.gameObject == item)
                            {
                                break;
                            }
                            middlePointsIndexTop += path.GetPoint(i2).SegmentsTop;
                            middlePointsIndexRight += path.GetPoint(i2).SegmentsRight;

                        }
                        //If we are on the last waypoint, exit
                        if (middlePointsIndexRight >= path.middlePointsRight.Count)
                        {
                            continue;
                        }
                        //Get the point for each curve
                        Vector3 pointRight = path.middlePointsRight[middlePointsIndexRight];
                        Vector3 pointTop = path.middlePointsTop[middlePointsIndexTop];
                        // Movemos la esfera
                        MoveSphere(aux.ScreenToViewportPoint(Input.mousePosition), auxPointSelected, pointRight);
                        Transform cube = item.transform.Find("CubeTop");
                        Transform cube2 = item.transform.Find("CubeFront");
                        Vector3 centerPos = new Vector3(item.transform.position.x + auxPointSelected.transform.position.x, item.transform.position.y + auxPointSelected.transform.position.y, item.transform.position.z + auxPointSelected.transform.position.z) / 2f;
                        float scaleX = Mathf.Abs((item.transform.position - auxPointSelected.transform.position).magnitude);

                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from = new Vector2(-1, 0);
                        Vector3 aux2 = auxPointSelected.transform.position - item.transform.position;
                        Vector2 to = new Vector2(aux2.x, aux2.z).normalized;

                        auxPointSelected.transform.GetChild(i + 2).localScale = new Vector3(scaleX, 1f, 1f);



                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from2 = new Vector2(-1, 0);
                        Vector3 aux3 = auxPointSelected.transform.position - item.transform.position;
                        Vector2 to2 = new Vector2(aux3.x, aux3.y).normalized;

                        //float angle = Mathf.Acos(distance2 / distance);
                        //item.transform.Rotate(new Vector3(0, 1, 0), Vector2.Angle(from, to));
                        if (i == 0)
                        {
                            angle = Vector2.SignedAngle(from, to);

                            angle2 = Vector2.SignedAngle(from2, to2);
                            sineC = (item.transform.position.y - auxPointSelected.transform.position.y) / scaleX;

                        }

                        cube.transform.rotation = Quaternion.Euler(new Vector3(0, -angle, 0));
                        cube2.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle2));
                        auxPointSelected.transform.GetChild(i + 2).rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg);
                        auxPointSelected.transform.GetChild(i + 2).position = item.transform.position;
                        var rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg).eulerAngles;
                        item.GetComponent<PathPoint>().GimbalRotation = auxPointSelected.transform.position - item.transform.position;
                        //UnityEngine.Debug.Log("GIMBALROTATION: " + item.GetComponent<PathPoint>().GimbalRotation + " POI POS: " + auxPointSelected.transform.position + " ITEM POS: " + item.transform.position);
                        lastPoi = auxPointSelected;
                        if (i == 0)
                        {
                            int pos = 0;
                            pos = path.NeedsToUpdateGbParameters(pointSelected[0].GetComponent<PathPoint>().Id);
                            if (pos == -1)
                            {
                                item.GetComponent<PathPoint>().Wp.gimbal_parameters.id_pointer = pointSelected[0].GetComponent<PathPoint>().Id;
                                path.addNewParamGb(item.GetComponent<PathPoint>().Wp.gimbal_parameters);
                            }
                            else
                            {
                                item.GetComponent<PathPoint>().Wp.gimbal_parameters.id_pointer = pointSelected[0].GetComponent<PathPoint>().Id;

                                path.updateParamGB(pos, item.GetComponent<PathPoint>().Wp.gimbal_parameters);
                            }
                        }
                        
                        i++;
                    }
                }
                break;
            case GimballParameters.GimbalMode.BLOCK_DIRECTION:
                //same as above
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if ( auxPointSelected && Input.GetMouseButton(0) && auxPointSelected.tag.Equals("POI"))
#else
                if (Input.touchCount == 1 &&  auxPointSelected && Input.GetMouseButton(0) && auxPointSelected.tag.Equals("POI") )
#endif
                {
                    if (lastPoi != null && lastPoi != auxPointSelected)
                    {
                        lastPoi.transform.GetChild(0).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                        lastPoi.transform.GetChild(1).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                        auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().material = greenMaterial;
                        auxPointSelected.transform.GetChild(1).GetComponent<Renderer>().material = greenMaterial;
                        lastPoi = auxPointSelected;
                    }
                   
                    // Movemos la esfera
                    MoveSphere(aux.ScreenToViewportPoint(Input.mousePosition), auxPointSelected);

                    if (auxPointSelected.GetComponent<POI>().Referenced.Count != pointSelected.Count)
                    {
                        foreach (var item in pointSelected)
                        {
                            //If it was already selected, we deselect it
                            if (item.GetComponent<PathPoint>().Id == 0)
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeNotSelectedMaterial;

                            }
                            else if (item.GetComponent<PathPoint>().Id == path.Count() - 1)
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingNotSelectedMaterial;

                            }
                            else
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().material = waypointNotSelectedMaterial;
                            }
                        }
                        pointSelected.Clear();

                    }
                    int i = 0;
                    float sineC = 0;
                    float angle2 = 0;
                    float angle = 0;
                    foreach (var item in pointSelected)
                    {


                        if (!pointSelected.Contains(item.gameObject) && item.transform.tag.Equals("Waypoint"))
                        {
                            if (item.GetComponent<PathPoint>().Id == 0 )
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().material = homeSelectedMaterial;


                            }
                            else if (item.GetComponent<PathPoint>().Id == path.Count() - 1)
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().material = landingSelectedMaterial;

                            }
                            else
                            {
                                item.transform.GetChild(0).GetComponent<Renderer>().material = waypointSelectedMaterial;
                            }
                            pointSelected.Add(item.gameObject);
                        }
                        Transform cube = item.transform.Find("CubeTop");
                        Transform cube2 = item.transform.Find("CubeFront");
                        Vector3 centerPos = new Vector3(item.transform.position.x + auxPointSelected.transform.position.x, item.transform.position.y + auxPointSelected.transform.position.y, item.transform.position.z + auxPointSelected.transform.position.z) / 2f;
                        float scaleX = Mathf.Abs((item.transform.position - auxPointSelected.transform.position).magnitude);

                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from = new Vector2(-1, 0);
                        Vector3 aux2 = auxPointSelected.transform.position - item.transform.position;
                        Vector2 to = new Vector2(aux2.x, aux2.z).normalized;

                        auxPointSelected.transform.GetChild(i + 2).localScale = new Vector3(scaleX, 1f, 1f);



                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from2 = new Vector2(-1, 0);
                        Vector3 aux3 = auxPointSelected.transform.position - item.transform.position;
                        Vector2 to2 = new Vector2(aux3.x, aux3.y).normalized;

                        //float angle = Mathf.Acos(distance2 / distance);
                        //item.transform.Rotate(new Vector3(0, 1, 0), Vector2.Angle(from, to));
                        if (i == 0)
                        {
                            angle = Vector2.SignedAngle(from, to);

                            angle2 = Vector2.SignedAngle(from2, to2);
                            sineC = (item.transform.position.y - auxPointSelected.transform.position.y) / scaleX;

                        }

                        cube.transform.rotation = Quaternion.Euler(new Vector3(0, -angle, 0));
                        cube2.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle2));
                        auxPointSelected.transform.GetChild(i + 2).rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg);
                        auxPointSelected.transform.GetChild(i + 2).position = item.transform.position;
                        var rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg).eulerAngles;
                        item.GetComponent<PathPoint>().GimbalRotation = auxPointSelected.transform.position-item.transform.position ;

                        lastPoi = auxPointSelected;
                        if (i == 0)
                        {
                            int pos = 0;
                            pos = path.NeedsToUpdateGbParameters(pointSelected[0].GetComponent<PathPoint>().Id);
                            if (pos == -1)
                            {
                                item.GetComponent<PathPoint>().Wp.gimbal_parameters.id_pointer = pointSelected[0].GetComponent<PathPoint>().Id;
                                path.addNewParamGb(item.GetComponent<PathPoint>().Wp.gimbal_parameters);
                            }
                            else
                            {
                                item.GetComponent<PathPoint>().Wp.gimbal_parameters.id_pointer = pointSelected[0].GetComponent<PathPoint>().Id;

                                path.updateParamGB(pos, item.GetComponent<PathPoint>().Wp.gimbal_parameters);
                            }
                        }
                           
                        i++;
                    }
                }
                break;
            default:
                break;
        }



#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        //This manages the camera movement on windows

        // Comprobamos si se pulsa el clic derecho
        if (Input.GetMouseButtonDown(1))
            {
                lastPosition = Input.mousePosition;
            }
            // Comprobamos si se pulsa el clic derecho
            if (Input.GetMouseButton(1))
            {
                MoveCamera(Input.mousePosition - lastPosition, aux);
                lastPosition = Input.mousePosition;
            }

#else
        //And this manages the camera movement on android(zoom and pan, no orbit)
        // Comprobamos si hay dos toques en pantalla para hacer zoom o pan
        if (Input.touchCount == 2)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began ||  Input.GetTouch(1).phase == TouchPhase.Began)
	        {
		  
                lastPosition = Input.mousePosition;
            
	        }
           
            MoveCamera(Input.mousePosition - lastPosition, aux);
            lastPosition = Input.mousePosition;
            // Almacenamos los dos touches
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Calculamos el desplazamiento con respecto al frame anterior de cada touch
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Transformamos la distancia entre los touch a un valor en coma flotante
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Calculamos la diferencia entre estos valores
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
            
                // Hacemos zoom en funcion a la distancia entre los dedos
                topCam.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;

                // Aseguramos que el Orthographic size no es menor que 0.1
                topCam.orthographicSize = Mathf.Max(topCam.orthographicSize, 1f);

                // Hacemos zoom en funcion a la distancia entre los dedos
                frontCam.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;

                // Aseguramos que el Orthographic size no es menor que 0.1
                frontCam.orthographicSize = Mathf.Max(frontCam.orthographicSize, 1f);

                TransformSphere(aux, esfera);


            


        }
#endif
        
    }
    //Function to sort the PathPoints
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
    //Scales sphere and POI
    void TransformSphere(Camera cam, GameObject esfera)
    {
        if (cam.orthographicSize > 15)
        {
            foreach (Transform child in sphereParent.transform)
            {
                child.GetChild(0).localScale = new Vector3(frontCam.orthographicSize / 8.0f * 25, frontCam.orthographicSize / 8.0f * 25, frontCam.orthographicSize / 8.0f * 25);
            }
            foreach (GameObject poi in poiToDraw)
            {
                poi.transform.localScale = new Vector3(cam.orthographicSize / 15.0f, cam.orthographicSize / 15.0f, cam.orthographicSize / 15.0f);

            }
        }
        else
        {
            foreach (Transform child in sphereParent.transform)
            {
                child.GetChild(0).localScale = new Vector3(frontCam.orthographicSize / 4.0f * 25, frontCam.orthographicSize / 4.0f * 25, frontCam.orthographicSize / 4.0f * 25);
            }
            foreach (GameObject poi in poiToDraw)
            {
                poi.transform.localScale = new Vector3(cam.orthographicSize / 8.0f, cam.orthographicSize / 8.0f, cam.orthographicSize / 8.0f);

            }
        }

    }
    /// <summary>
    /// Moves the camera
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="cam"></param>
    public void MoveCamera(Vector2 pos, Camera cam)
    {

        //Debug.Log("CamViewportToWorldPoint: " + cam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, 0));
        //Vector3 aux = cam.transform.position - cam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, 0));

        //cam.transform.position += new Vector3(aux.x, 0.0f, aux.z) * Time.deltaTime;

        Vector2 move = new Vector2((-pos.x / cam.pixelWidth) * cam.aspect, -pos.y / cam.pixelHeight);
        cam.transform.Translate(move.x * cam.orthographicSize * 2, move.y * cam.orthographicSize * 2, 0);





    }
    /// <summary>
    /// Moves POI
    /// </summary>
    /// <param name="pos">position in viewport coordinates</param>
    /// <param name="sphere">Object to move</param>
    public void MoveSphere(Vector2 pos, GameObject sphere)
    {
        Vector3 aux;
        if (Input.mousePosition.x < Camera.main.pixelWidth / 2)
        {
            aux = topCam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, 1 + topCam.transform.position.y));


            sphere.transform.position = new Vector3(aux.x, sphere.transform.position.y, aux.z);
        
           

        }
        else
        {
            if (POIEditorState == GimballParameters.GimbalMode.LOOK_AHEAD_FIX_PITCH)
            {
                return;
            }
            aux = frontCam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, 1 + frontCam.transform.position.y));
            
            
                sphere.transform.position = new Vector3(sphere.transform.position.x, aux.y, sphere.transform.position.z);

            

        }
        //frontCam.transform.position = new Vector3(frontCam.transform.position.x, frontCam.transform.position.y, sphere.transform.position.z - 5);

        
    }


    /// <summary>
    /// Moves POI while looking at the curve on y axis
    /// </summary>
    /// <param name="pos">position in viewport coordinates</param>
    /// <param name="sphere">Object to move</param>
    public void MoveSphere(Vector2 pos, GameObject sphere, Vector3 point)
    {
        Vector3 aux;
        if (Input.mousePosition.x < Camera.main.pixelWidth / 2)
        {
            aux = topCam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, 1 + topCam.transform.position.y));


            sphere.transform.position = new Vector3(aux.x, point.y, aux.z);



        }
        
        //frontCam.transform.position = new Vector3(frontCam.transform.position.x, frontCam.transform.position.y, sphere.transform.position.z - 5);


    }
    /// <summary>
    /// The function that creates the raycast
    /// </summary>
    /// <param name="ray"></param>
    /// <returns></returns>
    GameObject RayToSphere(Ray ray)
    {

        RaycastHit[] hit = Physics.RaycastAll(ray );
        return DistanceToLine(ray, hit);

    }
    /// <summary>
    /// This function calculates the distance to the rays. If it's close we consider that the ray hit a gameobject
    /// </summary>
    /// <param name="ray"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public GameObject DistanceToLine(Ray ray, RaycastHit[] point)
    {
        float min = Mathf.Infinity;
        float distance = 0.0f;
        GameObject sphereSelected = null;

        for (int i = 0; i < point.Length; i++)
        {
            distance = Vector3.Cross(ray.direction, point[i].transform.position - ray.origin).magnitude;
            if (min > distance)
            {
                min = distance;
                sphereSelected = point[i].transform.gameObject;
            }
        }
        //And here we make sure the function only returns waypoints or POI as the raycast can hit a lot of things such as UI or the map
        if (sphereSelected != null && (sphereSelected.transform.parent.tag == "Waypoint" || sphereSelected.transform.parent.tag.Equals("POI") || sphereSelected.transform.parent.tag.Equals("arrow")))
        {
            if (sphereSelected.transform.parent.tag.Equals("POI"))
            {
                sphereSelected = sphereSelected.transform.parent.gameObject;
                sphereSelected.transform.GetChild(0).GetComponent<Renderer>().material = greenMaterial;
                sphereSelected.transform.GetChild(1).GetComponent<Renderer>().material = greenMaterial;
                return sphereSelected;
            }
            if (sphereSelected.transform.parent.tag.Equals("Box"))
            {
                return null;
            }
            //The arrow that appears on the end of the line fro mthe POI doesn't change color. Everythin else does.
            if (sphereSelected.transform.parent.tag.Equals("arrow"))
            {
                sphereSelected = sphereSelected.transform.parent.parent.parent.gameObject;
                return sphereSelected;
            }
            sphereSelected = sphereSelected.transform.parent.gameObject;

            if (sphereSelected.GetComponent<PathPoint>().Id == 0 )
            {
                sphereSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeSelectedMaterial;

            }
            else if(sphereSelected.GetComponent<PathPoint>().Id == path.Count() - 1)
            {
                sphereSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingSelectedMaterial;
            }
            else
            {
                sphereSelected.transform.GetChild(0).GetComponent<Renderer>().material = waypointSelectedMaterial;
            }
        }
        
        return sphereSelected;
    }
    /// <summary>
    /// Selects every waypoint
    /// </summary>
    public void SelectAlll()
    {
        var gameobjects = GameObject.FindGameObjectsWithTag("Waypoint");
        pointSelected.Clear();
        foreach (var item in gameobjects)
        {
            pointSelected.Add(item);
            if (item.GetComponent<PathPoint>().Id == 0 )
            {
                item.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeSelectedMaterial;

            }
            else if (item.GetComponent<PathPoint>().Id == path.Count() - 1)
            {
                item.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingSelectedMaterial;
            }
            else
            {
                item.transform.GetChild(0).GetComponent<Renderer>().material = waypointSelectedMaterial;
                item.transform.GetChild(1).GetComponent<Renderer>().material = waypointSelectedMaterial;
            }
        }
    }
    /// <summary>
    /// Deletes a selected POI
    /// </summary>
    public void Delete()
    {
        if (auxPointSelected && auxPointSelected.tag.Equals("POI"))
        {
            foreach (var item in auxPointSelected.GetComponent<POI>().Referenced.Keys)
            {
                path.deleteParamGb(item.GetComponent<PathPoint>().Gb);

                item.gameObject.GetComponent<PathPoint>().Poi = null;

            }
            Destroy(auxPointSelected);
        }
    }
    /// <summary>
    /// Changes the state of the class, between look at POI, Look to the points of the curve, lock direction with fixed pitch and lock direction
    /// </summary>
    public void Lock()
    {
        switch (POIEditorState)
        {
            case GimballParameters.GimbalMode.LOOK_AT_POI:
                POIEditorState = GimballParameters.GimbalMode.LOOK_AHEAD;
                changeStateButton.image.sprite = lookAhead;
                if (lastPoi != null)
                {
                    lastPoi.transform.GetChild(0).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                    lastPoi.transform.GetChild(1).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                    lastPoi = null;
                }
                foreach (var points in pointSelected)
                {
                    //If it was already selected, we deselect it
                    if (points.GetComponent<PathPoint>().Id == 0)
                    {
                        points.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeNotSelectedMaterial;

                    }
                    else if (points.GetComponent<PathPoint>().Id == path.Count() - 1)
                    {
                        points.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingNotSelectedMaterial;

                    }
                    else
                    {
                        points.transform.GetChild(0).GetComponent<Renderer>().material = waypointNotSelectedMaterial;
                    }
                }
                pointSelected.Clear();
                break;
            case GimballParameters.GimbalMode.LOOK_AHEAD:
                POIEditorState = GimballParameters.GimbalMode.LOOK_AHEAD_FIX_PITCH;
                changeStateButton.image.sprite = lookAheadFixPitch;
               
                break;
            case GimballParameters.GimbalMode.LOOK_AHEAD_FIX_PITCH:
                POIEditorState = GimballParameters.GimbalMode.BLOCK_DIRECTION;
                changeStateButton.image.sprite = lockDirection;
                
                break;
            case GimballParameters.GimbalMode.BLOCK_DIRECTION:
                POIEditorState = GimballParameters.GimbalMode.LOOK_AT_POI;
                changeStateButton.image.sprite = lookAtPOI;

                break;
            default:
                break;
        }
    }
   /// <summary>
   /// This function is called when planning changes to another mode (Edit waypoint info, preview, ...) Deactivates anything necessary to change to another state
   /// </summary>
    public void Deactivate()
    {
        //We deselect every point
        pointSelected.Clear();
        var gameobjects = GameObject.FindGameObjectsWithTag("Waypoint");
        if (path == null)
        {
            path = Path.Instance;
        }
        foreach (var item in gameobjects)
        {
            //If it was already selected, we deselect it
            if (item.GetComponent<PathPoint>().Id == 0)
            {
                item.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeNotSelectedMaterial;

            }
            else if (item.GetComponent<PathPoint>().Id == path.Count() - 1)
            {
                item.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingNotSelectedMaterial;

            }
            else
            {
                item.transform.GetChild(0).GetComponent<Renderer>().material = waypointNotSelectedMaterial;
            }
            //Hide the POI and make the waypoints the parents of each box, to know better which ones belong to which
            item.transform.Find("CubeFront").gameObject.SetActive(false);
            item.transform.Find("CubeTop").gameObject.SetActive(false);
            if (item.gameObject.GetComponent<PathPoint>().Poi != null)
            {
                if (item.gameObject.GetComponent<PathPoint>().getGimballMode != 1)
                {
                    item.gameObject.GetComponent<PathPoint>().Poi.gameObject.SetActive(false);
                    item.gameObject.GetComponent<PathPoint>().Poi.gameObject.transform.GetChild(0).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                    item.gameObject.GetComponent<PathPoint>().Poi.gameObject.transform.GetChild(1).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                    Transform box = item.gameObject.GetComponent<PathPoint>().Poi.GetChild(2);
                    box.SetParent(item.transform);
                    box.gameObject.SetActive(false);
                }
               
            }
            
        }
        //Clear variables
        auxPointSelected = null;
        lastPoi = null;
    }
    //We activate the panel
    public void Activate()
    {
        //We get every waypoint
        GameObject[] gameobjects = GameObject.FindGameObjectsWithTag("Waypoint");
        float sineC = 0;
        float angle2 = 0;
        float angle = 0;
        int i = 0;

        foreach (var item in gameobjects)
        {
            
            if (item.gameObject.GetComponent<PathPoint>().Poi != null || item.gameObject.GetComponent<PathPoint>().getGimballMode == 1)
            {
                item.transform.Find("CubeFront").gameObject.SetActive(true);
                item.transform.Find("CubeTop").gameObject.SetActive(true);
                //Depending on the state, we assign one behaviour or another
                switch (item.gameObject.GetComponent<PathPoint>().getGimballMode)
                {
                    case 0:
                        //Look at poi
                        //This gets the cubes that represented the lines and, returns them to the POI and fixes the position, rotation, and scale
                        item.transform.Find("CubeFront").gameObject.SetActive(true);
                        item.transform.Find("CubeTop").gameObject.SetActive(true);
                        Transform box = item.transform.GetChild(item.transform.childCount - 1);
                        box.SetParent(item.gameObject.GetComponent<PathPoint>().Poi);
                        GameObject poiaux = item.gameObject.GetComponent<PathPoint>().Poi.gameObject;
                        poiaux.SetActive(true);
                        box.gameObject.SetActive(true);
                        Vector3 centerPos = new Vector3(item.transform.position.x + poiaux.transform.position.x, item.transform.position.y + poiaux.transform.position.y, item.transform.position.z + poiaux.transform.position.z) / 2f;
                        float scaleX = Mathf.Abs((item.transform.position - poiaux.transform.position).magnitude);
                        poiaux.transform.GetChild(0).GetComponent<Renderer>().material = whitePOIOutlineMaterial;
                        poiaux.transform.GetChild(1).GetComponent<Renderer>().material = whitePOIOutlineMaterial;

                        box.position = item.transform.position;
                        box.localScale = new Vector3(scaleX, 1f, 1f);


                        
                        Transform cube = item.transform.Find("CubeTop");
                        cube.gameObject.SetActive(true);
                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from = new Vector2(-1, 0);
                        Vector3 aux2 = poiaux.transform.position - item.transform.position;
                        Vector2 to = new Vector2(aux2.x, aux2.z).normalized;
                        angle = Vector2.SignedAngle(from, to);
                        Transform cube2 = item.transform.Find("CubeFront");
                        cube2.gameObject.SetActive(true);

                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from2 = new Vector2(-1, 0);
                        Vector3 aux3 = poiaux.transform.position - item.transform.position;
                        Vector2 to2 = new Vector2(aux3.x, aux3.y).normalized;
                        angle2 = Vector2.SignedAngle(from2, to2);

                        //float angle = Mathf.Acos(distance2 / distance);
                        //item.transform.Rotate(new Vector3(0, 1, 0), Vector2.Angle(from, to));
                        cube.transform.rotation = Quaternion.Euler(new Vector3(0, -angle, 0));
                        cube2.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle2));

                        sineC = (item.transform.position.y - poiaux.transform.position.y) / scaleX;

                        box.rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg);

                        break;

                    case 1:
                       

                        //Rotates to the new correct position the camera sprite
                        int middlePointsIndexTop = 0;
                        int middlePointsIndexRight = 0;


                        for (i = 0; i < path.Count(); i++)
                        {
                            if (path.GetPoint(i).PointTransf.gameObject == auxPointSelected)
                            {
                                break;
                            }
                            middlePointsIndexTop += path.GetPoint(i).SegmentsTop;
                            middlePointsIndexRight += path.GetPoint(i).SegmentsRight;

                        }
                        if (middlePointsIndexRight >= path.middlePointsRight.Count)
                        {
                            break;
                        }
                        Vector3 pointRight = path.middlePointsRight[middlePointsIndexRight];
                        Vector3 pointTop = path.middlePointsTop[middlePointsIndexTop];


                        //Esto es para rotar las imagenes de las camaras que tienen cada Waypoint
                        Transform cube5 = auxPointSelected.transform.Find("CubeTop");
                        cube5.gameObject.SetActive(true);
                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from7 = new Vector2(-1, 0);
                        Vector3 aux8 = pointTop - auxPointSelected.transform.position;
                        Vector2 to7 = new Vector2(aux8.x, aux8.z).normalized;

                        angle = Vector2.SignedAngle(from7, to7);

                        Transform cube6 = auxPointSelected.transform.Find("CubeFront");
                        cube6.gameObject.SetActive(true);

                        cube5.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.green;
                        cube6.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.green;

                        //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                        //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                        Vector2 from8 = new Vector2(-1, 0);
                        Vector3 aux9 = pointRight - auxPointSelected.transform.position;
                        Vector2 to8 = new Vector2(aux9.x, aux9.y).normalized;
                        angle2 = Vector2.SignedAngle(from8, to8);

                        //float angle = Mathf.Acos(distance2 / distance);
                        //item.transform.Rotate(new Vector3(0, 1, 0), Vector2.Angle(from, to));
                        cube5.transform.rotation = Quaternion.Euler(new Vector3(0, -angle, 0));
                        cube6.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle2));
                        //UnityEngine.Debug.Log("Point Top: " + pointRight);

                        //UnityEngine.Debug.Log(newpoi.GetComponent<POI>().Referenced.Count);

                        int pos = 0;
                        pos = path.NeedsToUpdateGbParameters(auxPointSelected.GetComponent<PathPoint>().Id);
                        if (pos == -1)
                        {
                            path.addNewParamGb(auxPointSelected.GetComponent<PathPoint>().Wp.gimbal_parameters);
                        }
                        else
                        {
                            path.updateParamGB(pos, auxPointSelected.GetComponent<PathPoint>().Wp.gimbal_parameters);
                        }

                        //If it was already selected, we deselect it
                        if (auxPointSelected.GetComponent<PathPoint>().Id == 0)
                        {
                            auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeNotSelectedMaterial;

                        }
                        else if (auxPointSelected.GetComponent<PathPoint>().Id == path.Count() - 1)
                        {
                            auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingNotSelectedMaterial;

                        }
                        else
                        {
                            auxPointSelected.transform.GetChild(0).GetComponent<Renderer>().material = waypointNotSelectedMaterial;
                        }
                        break;
                    case 2:
                        i = 0;
                        item.transform.Find("CubeTop").gameObject.SetActive(false);
                        //We only get the direction of the first waypoint
                        item.gameObject.GetComponent<PathPoint>().Poi.GetComponent<POI>().Referenced.OrderBy(key => item.gameObject.GetComponent<PathPoint>().Id);
                        if (item.gameObject.GetComponent<PathPoint>().Poi.GetComponent<POI>().Referenced.Keys.ToList<PathPoint>()[0] != item.gameObject.GetComponent<PathPoint>())
                        {
                            break;
                        }
                        else
                        {

                            foreach (var waypoint in item.gameObject.GetComponent<PathPoint>().Poi.GetComponent<POI>().Referenced.Keys.ToList<PathPoint>())
                            {


                                int middlePointsIndexTop2 = 0;
                                int middlePointsIndexRight2 = 0;
                                //We find which point was clicked and get the index of points from the catmull rom that reference the point just in front of the waypoint. 
                                for (int i2 = 0; i2 < path.Count(); i2++)
                                {
                                    if (path.GetPoint(i2).PointTransf.gameObject == item)
                                    {
                                        break;
                                    }
                                    middlePointsIndexTop2 += path.GetPoint(i2).SegmentsTop;
                                    middlePointsIndexRight2 += path.GetPoint(i2).SegmentsRight;

                                }
                                //If we are on the last waypoint, exit
                                if (middlePointsIndexRight2 >= path.middlePointsRight.Count)
                                {
                                    continue;
                                }
                                //Get the point for each curve
                                Vector3 pointRight2 = path.middlePointsRight[middlePointsIndexRight2];
                                Vector3 pointTop2 = path.middlePointsTop[middlePointsIndexTop2];

                                //And we reassign the lines, scale, rotate them and position and give them the direction of the first line from the first waypoint that had this parameter
                                Transform box2 = waypoint.gameObject.transform.GetChild(waypoint.transform.childCount - 1);
                                box2.SetParent(waypoint.gameObject.GetComponent<PathPoint>().Poi);
                                GameObject poiaux2 = waypoint.gameObject.GetComponent<PathPoint>().Poi.gameObject;
                                poiaux2.SetActive(true);
                                poiaux2.transform.position = new Vector3(poiaux2.transform.position.x, pointRight2.y, poiaux2.transform.position.z);
                                box2.gameObject.SetActive(true);
                                Vector3 centerPos2 = new Vector3(waypoint.transform.position.x + poiaux2.transform.position.x, waypoint.transform.position.y + poiaux2.transform.position.y, waypoint.transform.position.z + poiaux2.transform.position.z) / 2f;
                                float scaleX2 = Mathf.Abs((waypoint.transform.position - poiaux2.transform.position).magnitude);

                                box2.position = waypoint.gameObject.transform.position;
                                box2.localScale = new Vector3(scaleX2, 1f, 1f);


                                //ALGUIEN VA A MOVER UNA ESFERA TRAS MOVER EL POI????
                                Transform cube3 = waypoint.gameObject.transform.Find("CubeTop");
                                cube3.gameObject.SetActive(true);
                                //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                                //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                                Vector2 from3 = new Vector2(-1, 0);
                                Vector3 aux4 = poiaux2.transform.position - waypoint.gameObject.transform.position;
                                Vector2 to3 = new Vector2(aux4.x, aux4.z).normalized;
                                Transform cube4 = waypoint.gameObject.transform.Find("CubeFront");
                                cube3.gameObject.SetActive(true);

                                //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                                //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                                Vector2 from4 = new Vector2(-1, 0);
                                Vector3 aux5 = poiaux2.transform.position - waypoint.gameObject.transform.position;
                                Vector2 to4 = new Vector2(aux5.x, aux5.y).normalized;

                                //float angle = Mathf.Acos(distance2 / distance);
                                //item.transform.Rotate(new Vector3(0, 1, 0), Vector2.Angle(from, to));


                                if (i == 0)
                                {
                                    angle = Vector2.SignedAngle(from3, to3);

                                    angle2 = Vector2.SignedAngle(from3, to4);

                                    sineC = (item.transform.position.y - poiaux2.transform.position.y) / scaleX2;

                                }
                                cube3.transform.rotation = Quaternion.Euler(new Vector3(0, -angle, 0));
                                cube3.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle2));
                                box2.rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg);

                                i++;
                            }

                        }

                        break;
                    case 3:
                        i = 0;
                        //same as above
                        item.transform.Find("CubeFront").gameObject.SetActive(true);
                        item.transform.Find("CubeTop").gameObject.SetActive(true);
                        item.gameObject.GetComponent<PathPoint>().Poi.GetComponent<POI>().Referenced.OrderBy(key => item.gameObject.GetComponent<PathPoint>().Id);
                        if (item.gameObject.GetComponent<PathPoint>().Poi.GetComponent<POI>().Referenced.Keys.ToList<PathPoint>()[0] != item.gameObject.GetComponent<PathPoint>())
                        {
                            break;
                        }
                        else
                        {

                            foreach (var waypoint in item.gameObject.GetComponent<PathPoint>().Poi.GetComponent<POI>().Referenced.Keys.ToList<PathPoint>())
                            {

                                Transform box2 = waypoint.gameObject.transform.GetChild(waypoint.transform.childCount - 1);
                                box2.SetParent(waypoint.gameObject.GetComponent<PathPoint>().Poi);
                                GameObject poiaux2 = waypoint.gameObject.GetComponent<PathPoint>().Poi.gameObject;
                                poiaux2.SetActive(true);
                                box2.gameObject.SetActive(true);
                                Vector3 centerPos2 = new Vector3(waypoint.transform.position.x + poiaux2.transform.position.x, waypoint.transform.position.y + poiaux2.transform.position.y, waypoint.transform.position.z + poiaux2.transform.position.z) / 2f;
                                float scaleX2 = Mathf.Abs((waypoint.transform.position - poiaux2.transform.position).magnitude);

                                box2.position = waypoint.gameObject.transform.position;
                                box2.localScale = new Vector3(scaleX2, 1f, 1f);


                                //ALGUIEN VA A MOVER UNA ESFERA TRAS MOVER EL POI????
                                Transform cube3 = waypoint.gameObject.transform.Find("CubeTop");
                                cube3.gameObject.SetActive(true);
                                //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                                //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                                Vector2 from3 = new Vector2(-1, 0);
                                Vector3 aux4 = poiaux2.transform.position - waypoint.gameObject.transform.position;
                                Vector2 to3 = new Vector2(aux4.x, aux4.z).normalized;
                                Transform cube4 = waypoint.gameObject.transform.Find("CubeFront");
                                cube3.gameObject.SetActive(true);

                                //float distance = Vector3.Distance(item.transform.position, newpoi.transform.position);
                                //float distance2 = Vector3.Distance(cube.position, item.transform.position) * 2;
                                Vector2 from4 = new Vector2(-1, 0);
                                Vector3 aux5 = poiaux2.transform.position - waypoint.gameObject.transform.position;
                                Vector2 to4 = new Vector2(aux5.x, aux5.y).normalized;

                                //float angle = Mathf.Acos(distance2 / distance);
                                //item.transform.Rotate(new Vector3(0, 1, 0), Vector2.Angle(from, to));


                                if (i == 0)
                                {
                                    angle = Vector2.SignedAngle(from3, to3);

                                    angle2 = Vector2.SignedAngle(from3, to4);

                                    sineC = (item.transform.position.y - poiaux2.transform.position.y) / scaleX2;

                                }
                                cube3.transform.rotation = Quaternion.Euler(new Vector3(0, -angle, 0));
                                cube3.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle2));
                                box2.rotation = Quaternion.Euler(0, -angle, Mathf.Asin(sineC) * Mathf.Rad2Deg);

                                i++;
                            }

                        }

                        break;

                    default:

                        break;
                }
            }
            
            


        }
        
        esfera.SetActive(false);
        poi.SetActive(false);
    }
}
