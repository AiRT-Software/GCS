using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// This class holds the waypoints and parameters from a plan
/// </summary>
[Serializable]
public class Path {

    // Variables
    private static Path _instance;
    [SerializeField]
    PathMetadata path_metadata;
    [SerializeField]
    DefaultFlightParams flightParams = new DefaultFlightParams();
    [SerializeField]
    List<Point> waypoints;
    [SerializeField]
    List<GimballParameters> gimbal_update_parameters = new List<GimballParameters>();
    [SerializeField]
    List<RecCamParameters> rcam_update_parameters = new List<RecCamParameters>();
    [NonSerialized]
    public List<Vector3> middlePointsTop = new List<Vector3>();
    [NonSerialized]
    public List<Vector3> middlePointsRight = new List<Vector3>();
    bool bezier = false;

    // Singleton
    public static Path Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Path();
            }
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }

    public DefaultFlightParams FlightParams
    {
        get { return flightParams; }
        set { flightParams = value; }
    }


    public int wpParametersCount()
    {
        return gimbal_update_parameters.Count;
    }
    public GimballParameters getGbUpdateParameter(int id)
    {
        return gimbal_update_parameters[id];
    }
    /// <summary>
    /// This class finds if a point already has a gimbal parameter assigned. If it has it return the id, if it doesn't returns a -1
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public int NeedsToUpdateGbParameters(int id)
    {
        int i = 0;
        foreach (var item in gimbal_update_parameters)
        {
            if(item.id_pointer == id)
            {
                return i;
            }
            i++;
        }
        return -1;
    }
    public void deleteParamGb(GimballParameters param)
    {
        gimbal_update_parameters.Remove(param);
    }
    public void addNewParamGb(GimballParameters param)
    {
        gimbal_update_parameters.Add(param);
    }
    public void addNewParamGbAtBeggining(GimballParameters param)
    {
        gimbal_update_parameters.Insert(0,param);
    }
    public void updateParamGB(int pos, GimballParameters param)
    {
        gimbal_update_parameters[pos] = param;
    }
    /// <summary>
    /// This class finds if a point already has a rec cam parameter assigned. If it has it return the id, if it doesn't returns a -1
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public int NeedsToUpdateRcParameters(int id)
    {
        int i = 0;
        foreach (var item in rcam_update_parameters)
        {
            if (item.id_pointer == id)
            {
                return i;
            }
            i++;
        }
        return -1;
    }
    public void clearRc()
    {
        rcam_update_parameters.Clear();
    }
    public int rcParametersCount()
    {
        return rcam_update_parameters.Count;
    }
    public RecCamParameters getRcUpdateParameter(int id)
    {
        return rcam_update_parameters[id];
    }
    public void addNewParamRc(RecCamParameters param)
    {
        rcam_update_parameters.Add(param);
    }
    public void addNewParamRcAtBeggining(RecCamParameters param)
    {
        rcam_update_parameters.Insert(0, param);
    }
    public void updateParamRc(int pos, RecCamParameters param)
    {
        rcam_update_parameters[pos] = param;
    }
    public PathMetadata Path_metadata
    {
        get { return path_metadata; }
    }
    /// <summary>
    /// Erases the path singleton
    /// </summary>
    public void delete()
    {
        waypoints = null;
        middlePointsTop = null;
        middlePointsRight = null;

        _instance = null;

    }
    private Path()
    {
        path_metadata = new PathMetadata("Plan_", "Author_", DateTime.Now, new Vector3(0, 0, 0), "");
        waypoints = new List<Point>();

    }

    // Getters & Setters
    public int Count()
    {
        return waypoints.Count;
    }

    /// <summary>
    /// Adds a point
    /// </summary>
    /// <param name="point"></param>
    /// <param name="pathpoint"></param>
    public void AddPointWithGimball(Point point, PathPoint pathpoint){
        point.Id = waypoints.Count;
        waypoints.Add(point);
        if (waypoints.Count == 1)
        {
            pathpoint.FirstWaypoint();
            gimbal_update_parameters.Add(pathpoint.Wp.gimbal_parameters);
            
        }

    }
    /// <summary>
    /// Adds a point at a specific position inn the array. Used when a user creates a waypoint from the curve
    /// </summary>
    /// <param name="point"></param>
    /// <param name="id"></param>
    public void AddPointatId(Point point, int id)
    {
        point.Id = id+1;
        waypoints.Insert(id+1, point);
        for (int i = point.Id + 1; i < waypoints.Count; i++)
        {
            waypoints[i].Id++ ;
        }

    }

    public void DeletePoint(int id)
    {
        if (waypoints[id].Poi != null && waypoints[id].Poi.GetComponent<POI>().Referenced.Count > 1)
        {
            waypoints[id].Poi.GetComponent<POI>().removePoint(waypoints[id].PointTransf.gameObject.GetComponent<PathPoint>());
        }
        else if(waypoints[id].Poi != null)
        {
            waypoints[id].Poi.GetComponent<POI>().removePoint(waypoints[id].PointTransf.gameObject.GetComponent<PathPoint>());
            GameObject.Destroy(waypoints[id].Poi.gameObject);
        }
        waypoints.RemoveAt(id);

        for (int i = id; i < waypoints.Count; i++) { 
            waypoints[i].Id--;
        }
        
    }
    /// <summary>
    /// Sets the path to the newPath passed to the function
    /// </summary>
    /// <param name="newPath"></param>
    public void setPath(Path newPath)
    {
        waypoints = newPath.waypoints;
        gimbal_update_parameters = newPath.gimbal_update_parameters;
        rcam_update_parameters = newPath.rcam_update_parameters;
        middlePointsTop = newPath.middlePointsTop;
        middlePointsRight = newPath.middlePointsRight;

    }
    public Point GetPoint(int id)
    {
        return waypoints[id];
    }

    
}
