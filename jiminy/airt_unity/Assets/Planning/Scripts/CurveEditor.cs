using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//This class was suposed to activate the curve editor panel. We eliminated it because the drone can't adjust speed? If in the future variable speed can be adjusted along a curve
//use this class and finish planning with the addition of the panel to adjust the speed.
public class CurveEditor : MonoBehaviour {

    public GameObject TimePoint;
    Path path;
    public Camera upCam;
    public Camera downCam;
    // Use this for initialization
    void Start () {
        if (path == null)
        {
            path = Path.Instance;

        }
    }
    //The deactivate function makes the necesary changes to deactivate a panel. In this case, destroys every waypoint related to this panel
    public void DeActivate()
    {
        upCam.gameObject.SetActive(false);
        downCam.gameObject.SetActive(false);
        GameObject[] points = GameObject.FindGameObjectsWithTag("Time");
        foreach (var item in points)
        {
            Destroy(item);
        }
    }
    /// <summary>
    /// Creates a waypoint list that can be adjusted to modify speed and height without breaking the original waypoints
    /// </summary>
    public void Activate()
    {
        if (path == null)
        {
            path = Path.Instance;

        }
        upCam.gameObject.SetActive(true);
        downCam.gameObject.SetActive(true);
        float zPosition = 0;
        for (int i = 0; i < path.Count(); i++)
        {
            GameObject timeObject = Instantiate(TimePoint);
            timeObject.tag = "Time";
            timeObject.layer = 9;
            timeObject.transform.GetChild(0).gameObject.layer = 9;
            timeObject.transform.GetChild(1).gameObject.layer = 9;

            var point = path.GetPoint(i);

            timeObject.AddComponent<PathPoint>();
            timeObject.GetComponent<PathPoint>().copyPoint(point,null, null, timeObject.transform);
            timeObject.SetActive(true);
            if (i == 0)
            {
                zPosition = 0;
            }
            else if (path.GetPoint(i-1).Speed == 0)
            {
                zPosition = path.GetPoint(i - 1).SegmentsTop;
            }
            else
            {
                zPosition = path.GetPoint(i - 1).Speed;
            }
            timeObject.transform.position = new Vector3(i * 5, point.PointTransf.position.y, zPosition);
        }

    }
    // Update is called once per frame
    void Update () {
		
	}
}
