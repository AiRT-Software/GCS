using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// The monobehaviour that every point of interest has to manage the realtion with the waypoints
/// </summary>
public class POI : MonoBehaviour {

    Dictionary<PathPoint, GameObject> referenced = new Dictionary<PathPoint, GameObject>();
    bool direction = false;
    public Dictionary<PathPoint, GameObject> Referenced
    {
        get { return referenced; }
        set { referenced = value; }
    }

    public bool Direction
    {
        get
        {
            return direction;
        }

        set
        {
            direction = value;
        }
    }

    public void addPoint(PathPoint point, GameObject box)
    {
        referenced.Add(point, box);
    }
    public void removePoint(PathPoint point)
    {
        referenced.Remove(point);
    }
}
