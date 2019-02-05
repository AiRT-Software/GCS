using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This is the monobehaviour assigned to every waypoint and handles assigning variables to the classes that contain the points that will be sent to the drone
/// </summary>

public class PathPoint : MonoBehaviour
{
    Point point;
    WaypointUpdateParameters wp;

    public Point getPoint()
    {
        return point;
    }
    public byte getGimballMode
    {
        get { return wp.gimbal_parameters.mode; }
        set { wp.gimbal_parameters.mode = value; }
    }
    /// <summary>
    /// Number of points in the curve between waypoint
    /// </summary>
    public int Segments
    {
        get { return SegmentsTop; }
        set { SegmentsTop = value; }
    }
    /// <summary>
    /// The point of interest transform
    /// </summary>
    public Transform Poi
    {
        get { return point.Poi; }
        set { point.Poi = value; }
    }


    // Getters & Setters
    /// <summary>
    /// Position in the array
    /// </summary>
    public int Id
    {
        get { return point.Id; }
        set { point.Id = value; }
    }
    /// <summary>
    /// Transform of the point
    /// </summary>
    public Transform PointTransf
    {
        get { return point.PointTransf; }
        set { point.PointTransf = value; }
    }
    /// <summary>
    /// Time it will stop if it is a stopTime waypoint
    /// </summary>
    public uint StopTime
    {
        get
        {
            return point.StopTime;
        }

        set
        {
            point.StopTime = value;
        }
    }
    public float Speed
    {
        get
        {
            return point.Speed;
        }

        set
        {
            point.Speed = value;
        }
    }

    public Vector3 GimbalRotation
    {
        get
        {
            return Gb.poi_or_angles;
        }

        set
        {
            Gb.poi_or_angles = value;
        }
    }

    
    
    

    public int SegmentsTop
    {
        get
        {
            return point.SegmentsTop;
        }

        set
        {
            point.SegmentsTop = value;
        }
    }
    public int SegmentsRight
    {
        get
        {
            return point.SegmentsRight;
        }

        set
        {
            point.SegmentsRight = value;
        }
    }
    

   

    public GimballParameters Gb
    {
        get
        {
            return wp.gimbal_parameters;
        }

        set
        {
            wp.gimbal_parameters = value;
        }
    }

    public WaypointUpdateParameters Wp
    {
        get
        {
            return wp;
        }

        set
        {
            wp = value;
        }
    }

    public RecCamParameters Rc
    {
        get
        {
            return wp.reccam_parameters;
        }

        set
        {
            wp.reccam_parameters = value;
        }
    }

    public Point.PointType getPointType()
    {
        return point.getPointType();
    }
    public void setPointType(Point.PointType type)
    {
        point.setPointType(type);
    }

    public void createPathPoint(Transform pos)
    {
        point = new Point(pos);
        wp = new WaypointUpdateParameters();

        Gb = new GimballParameters();
        Rc = new RecCamParameters();
        wp.gimbal_parameters = Gb;
        wp.reccam_parameters = Rc;
        wp.gimbal_parameters.id_pointer = Id;
        wp.reccam_parameters.id_pointer = Id;
    }
    public void createPathPointWithPoint(Point pos, int gbId, int rcId)
    {
        point = pos;
        gbId = 0;
        rcId = 0;
        wp = new WaypointUpdateParameters();
        Gb = new GimballParameters();
        Rc = new RecCamParameters();
        wp.gimbal_parameters = Gb;
        wp.reccam_parameters = Rc;
        Gb.id_pointer = gbId;
        Rc.id_pointer = rcId;
    }
    public void FirstWaypoint()
    {

        Gb.poi_or_angles = new Vector3(0, 0, 0);

        wp = new WaypointUpdateParameters();
        wp.gimbal_parameters = new GimballParameters() ;
        wp.reccam_parameters = new RecCamParameters();
        wp.gimbal_parameters.id_pointer = 0;
        wp.reccam_parameters.id_pointer = 0;

    }
    public void copyPoint(Point secondPoint, GimballParameters gbParam, RecCamParameters rcParam, Transform transf)
    {
        point.PointTransf1 = transf;
        point.Speed = secondPoint.Speed;

       
        if (gbParam != null)
        {
            Gb.poi_or_angles = gbParam.poi_or_angles;
            Gb.mode = gbParam.mode;
            Rc.resolution = rcParam.resolution; // ## Necesitamos saber resoluciones mínimas y/o máximas
            Rc.brightness = rcParam.brightness;
        }


        point.setPointType(secondPoint.getPointType());

        point.id = secondPoint.Id;
        point.SegmentsTop = secondPoint.SegmentsTop;
        point.SegmentsRight = secondPoint.SegmentsRight;

        point.Poi1 = secondPoint.Poi;
        
        
    }
}



