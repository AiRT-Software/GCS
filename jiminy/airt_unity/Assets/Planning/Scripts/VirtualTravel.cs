using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// This functions manages the preview panel on planning. This includes some UI and initialzie the list of waypoints that the camera has to go through
/// </summary>
public class VirtualTravel : MonoBehaviour {
    List<GameObject> waypoints = new List<GameObject>();
    public Camera camToTravel;
    public Material modelMaterial;
    public GameObject playButton;
    public RectTransform doneButton, textDone;
    public Sprite playImage, pauseImage;
    bool active = false;
    int indexWaypoints = 0;
    int indexMiddlePoints = 0;
    float time = 0;
    Vector3 actualWaypoint;
    Vector3 nextWaypoint;
    int TotalindexMiddlePoints = 0;
    Path path;
    bool blockedDIrection = false;
    RectTransform throbberImage;

    ClientUnity clientUnity;

    /// <summary>
    /// Deactivates the shader that hides parts from a model in order to show the camera traveling perfectly fine through the scene. Because there are multiple objects with
    /// the shader, we need to find each that contains it.
    /// </summary>
    /// <param name="shaderName"></param>
    private void FindShader(string shaderName)
    {
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

    void Awake()
    {
        path = Path.Instance;
        doneButton.sizeDelta = new Vector2(Screen.width / 8, Screen.height / 20);
        //doneButton.anchoredPosition = new Vector2(0, Screen.height / 40);
        textDone.sizeDelta = new Vector2(Screen.width / 8, Screen.height / 10);
        textDone.anchoredPosition = new Vector2(-doneButton.sizeDelta.x, -Screen.height / 40);

        throbberImage = doneButton.GetChild(3).GetComponent<RectTransform>();
        throbberImage.sizeDelta = new Vector2(Screen.width / 30, Screen.width / 30);
        throbberImage.anchoredPosition = new Vector2(0, -Screen.height / 15);

        //doneButton.anchoredPosition = 
    }
    /// <summary>
    /// Function called when the panel gets active
    /// </summary>
    public void Activate () {
        //First we deactivate the shader and get every waypoint
        GameObject[] aux = GameObject.FindGameObjectsWithTag("Waypoint");
        modelMaterial.SetFloat("_Discard", 0);
        FindShader("Custom/ObjectsShader");
        //And add the waypoints to an array, that we will later use to know at wich waypoint are we and how the gimbal should behave
        foreach (var item in aux)
        {
            waypoints.Add(item);
        }
        //Sort the waypoints by id
        waypoints.Sort((IComparer<GameObject>)new SortPointsByID());
        indexWaypoints = 0;
        indexMiddlePoints = 0;
        TotalindexMiddlePoints = 0;
        //The camera starts at the first waypoint
        camToTravel.transform.position = waypoints[0].transform.position;
        //The current waypoint
        actualWaypoint = waypoints[0].transform.position;
        //The next waypoint to travel to
        nextWaypoint = path.middlePointsTop[0];
        //If the camera has a POI, it will look at it
        if (waypoints[0].GetComponent<PathPoint>().Poi != null)
        {
            camToTravel.transform.LookAt(waypoints[0].GetComponent<PathPoint>().Poi);
        }
        else
        {
            camToTravel.transform.rotation = Quaternion.Euler(0, 0, 0) ;

        }
    }
    /// <summary>
    /// Function called when the panel is deactivated
    /// </summary>
    public void DeActivate()
    {
        active = false;
        waypoints.Clear();
    }
    /// <summary>
    /// Function to sort points by ID. Called by array.Sort
    /// </summary>
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
    /// <summary>
    /// The play button on preview calls this function. Pauses or plays the camera travelling.
    /// </summary>
    public void StartOrStop()
    {

        if (active)
            playButton.GetComponent<Image>().sprite = playImage;
        else
            playButton.GetComponent<Image>().sprite = pauseImage;

        active = !active;
    }
    // Update is called once per frame
    void FixedUpdate () {
        //If the camera reached the ending, stop
        if (indexWaypoints >= waypoints.Count || TotalindexMiddlePoints >= path.middlePointsTop.Count)
        {
            active = false;

        }
        //IF the camera is travelling and we haven't reach the next waypoint
        if (active && time < 1)
        {
            //Interpolate the actual waypoint with the next, by a factor of time passed between updates
            camToTravel.transform.position = Vector3.Lerp(actualWaypoint, nextWaypoint, time);
            //camToTravel.transform.position = nextWaypoint * Time.deltaTime * 0.001f;
            //This is for the camera rotation
            switch (waypoints[indexWaypoints].GetComponent<PathPoint>().getGimballMode)
            {
                case 0:
                    //Look at poi
                    camToTravel.transform.LookAt(waypoints[indexWaypoints].GetComponent<PathPoint>().Poi);
                    break;
                case 1:
                    //If the camera is at the last waypoint, there is not a point to look at anymore
                    if (TotalindexMiddlePoints + 1 >= path.middlePointsTop.Count)
                    {
                        //camToTravel.transform.LookAt(waypoints[indexWaypoints].GetComponent<PathPoint>().Poi);
                    }
                    else
                    {
                        camToTravel.transform.LookAt(path.middlePointsTop[TotalindexMiddlePoints + 1]);

                    }
                    break;
                case 2:
                    //Blocked direction
                    camToTravel.transform.forward = waypoints[indexWaypoints].GetComponent<PathPoint>().GimbalRotation;
                    //camToTravel.transform.localRotation = Quaternion.Euler(waypoints[indexWaypoints].GetComponent<PathPoint>().GimbalRotation.x, waypoints[indexWaypoints].GetComponent<PathPoint>().GimbalRotation.y - 90, 0);
                    Quaternion qAux3 = Quaternion.Euler(waypoints[indexWaypoints].GetComponent<PathPoint>().GimbalRotation);
                    Vector3 vAux3 = waypoints[indexWaypoints].GetComponent<PathPoint>().GimbalRotation;
                    if (TotalindexMiddlePoints + 1 >= path.middlePointsTop.Count)
                    {
                        //camToTravel.transform.LookAt(waypoints[indexWaypoints].GetComponent<PathPoint>().Poi);
                    }
                    else
                    {

                        Vector3 lockedDegreesToLookCamera = camToTravel.transform.rotation.eulerAngles;
                        camToTravel.transform.LookAt(path.middlePointsTop[TotalindexMiddlePoints + 1]);
                        camToTravel.transform.rotation = Quaternion.Euler(lockedDegreesToLookCamera.x, camToTravel.transform.rotation.eulerAngles.y, lockedDegreesToLookCamera.z);
                        camToTravel.transform.forward = new Vector3(waypoints[indexWaypoints].GetComponent<PathPoint>().GimbalRotation.x, camToTravel.transform.forward.y, waypoints[indexWaypoints].GetComponent<PathPoint>().GimbalRotation.z) ;

                        //camToTravel.transform.rotation = Quaternion.Euler(new Vector3(camToTravel.transform.rotation.eulerAngles.x, camToTravel.transform.rotation.eulerAngles.y +45, camToTravel.transform.rotation.eulerAngles.z));

                    }


                    //UnityEngine.Debug.Log("Quaternion: " + qAux2 + " Vector: " + vAux2);

                    break;
                case 3:
                    //Blocked direction
                    camToTravel.transform.forward = waypoints[indexWaypoints].GetComponent<PathPoint>().GimbalRotation;
                    //camToTravel.transform.localRotation = Quaternion.Euler(waypoints[indexWaypoints].GetComponent<PathPoint>().GimbalRotation.x, waypoints[indexWaypoints].GetComponent<PathPoint>().GimbalRotation.y - 90, 0);
                    Quaternion qAux2 = Quaternion.Euler(waypoints[indexWaypoints].GetComponent<PathPoint>().GimbalRotation);
                    Vector3 vAux2 = waypoints[indexWaypoints].GetComponent<PathPoint>().GimbalRotation;
                    //UnityEngine.Debug.Log("Quaternion: " + qAux2 + " Vector: " + vAux2);
                    break;
                default:
                    break;
            }
            //This rotates the sprite to be able to see it always from above
            camToTravel.transform.GetChild(0).rotation = Quaternion.Euler(90, 0, 0);


            time += Time.deltaTime;
        }
        //If the camera reached the next waypoint
        if (active && time >= 1)
        {
            actualWaypoint = nextWaypoint;
            //UnityEngine.Debug.Log(waypoints[indexWaypoints].GetComponent<PathPoint>().Segments);
            //If a curve doesn't have more than 3 points, the drone camera doesn't follow the curve right, so it will go from waypoint to waypoint instead of following the curve.
            //Technically, if it only has 2 points, it will be a straight line, so there is no problem
            if (waypoints[indexWaypoints].GetComponent<PathPoint>().Segments <= 2)
            {
                //As long as we haven't reach the ending
                if(waypoints.Count-1 > indexWaypoints)
                {
                    //Get the next point in the curve that is next to the next waypoint
                    indexWaypoints++;
                    TotalindexMiddlePoints += waypoints[indexWaypoints].GetComponent<PathPoint>().Segments;
                    //Get the next waypoint
                    nextWaypoint = waypoints[indexWaypoints].transform.position;
                    indexMiddlePoints = 0;
                }
                blockedDIrection = false;

            }
            else
            {
                ////If we have reached a waypoint, we enter here
                if (waypoints[indexWaypoints].GetComponent<PathPoint>().Segments == indexMiddlePoints + 1)
                {
                    indexWaypoints++;
                    TotalindexMiddlePoints++;
                    //If we haven't reach the ending
                    if (TotalindexMiddlePoints < path.middlePointsTop.Count)
                        nextWaypoint = path.middlePointsTop[TotalindexMiddlePoints];
                    indexMiddlePoints = 0;
                    blockedDIrection = false;

                }
                //If the drone is still between two waypoints, but reached another point of the curve, and we haven't reached the ending, we enter here
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
        
    }
}
