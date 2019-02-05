using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// the class that contains the waypoint info to serialize
/// </summary>
[System.Serializable]
public class Point {

    // Enums
    public enum PointType
    {
        TakeOff = 0,
        WayPoint,
        Stop,
        Land,
    };

    public enum RecMode
    {
        Photo = 0,
        Video
    };

    // Variables
    public int id;
    [SerializeField]
    float speed = 0;
    [SerializeField]
    Vector3 coordinates;
    [SerializeField]
    PointType point_type;
    [SerializeField]
    uint stopTime = 0; //ms
    Transform pointTransf;
    Transform poi;

    int segmentsTop;
    int segmentsRight;
    /// <summary>
    /// Number of points between waypoints in the curve drawn on the top camera
    /// </summary>
    public int SegmentsTop
    {
        get { return segmentsTop; }
        set { segmentsTop = value; }
    }
    /// <summary>
    /// Number of points between waypoints in the curve drawn on the front camera
    /// </summary>
    public int SegmentsRight
    {
        get { return segmentsRight; }
        set { segmentsRight = value; }
    }
    public Transform Poi
    {
        get { return Poi1; }
        set {
            Poi1 = value;
        }
    }


    // Getters & Setters
    public int Id
    {
        get { return id; }
        set { id = value; }
    }

    public Transform PointTransf
    {
        get { return PointTransf1; }
        set { PointTransf1 = value; }
    }

    public float Speed
    {
        get
        {
            return speed;
        }

        set
        {
            speed = value;
        }
    }

    

    public Transform PointTransf1
    {
        get
        {
            return pointTransf;
        }

        set
        {
            pointTransf = value;
        }
    }


    public Transform Poi1
    {
        get
        {
            return poi;
        }

        set
        {
            poi = value;
        }
    }

   

    public Vector3 PointPosition
    {
        get
        {
            return coordinates;
        }

        set
        {
            coordinates = value;
        }
    }

    public uint StopTime
    {
        get
        {
            return stopTime;
        }

        set
        {
            stopTime = value;
        }
    }

    public PointType getPointType()
    {
        return point_type;
    }
    public void setPointType(PointType type)
    {
        point_type = type;
    }
    public Point(Transform transf)
    {
        PointTransf1 = transf;
        PointPosition = transf.position;
        Speed = 0;

    }

    // Custom functions

}
