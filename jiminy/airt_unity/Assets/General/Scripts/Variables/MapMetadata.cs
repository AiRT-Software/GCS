using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//This is the metadata class from a plan. Saved as the guid.json.metadata
[Serializable]
public class MapMetadata {
    public enum MapType
    {
        PointCloud = 0,
        Model3D,
        EmptyBox
    }
    //We created our own time structure to save it and compare it as we want
    [Serializable]
    public struct AirtDateTime : IComparable
    {
        public int year;
        public byte month;
        public byte day;
        public byte hours;
        public byte minutes;
        public byte seconds;

        public AirtDateTime(byte Day, byte Month, int Year, byte Seconds, byte Minutes, byte Hours )
        {
            day = Day;
            month = Month;
            year = Year;
            seconds = Seconds;
            minutes = Minutes;
            hours = Hours;
        }
        public int CompareTo(object obj2)
        {
            if (obj2 == null) return 1;

            AirtDateTime obj = (AirtDateTime)obj2;

            return year == obj.year && month == obj.month && day == obj.day && hours == obj.hours && minutes == obj.minutes && seconds == obj.seconds?0:1;
           
        }
        public override string ToString()
        {

            DateTime aux = new DateTime(year, month, day, hours, minutes, seconds);
            
            return (int) day + "/" + (int) month + "/" + year + " at " + (int)hours + ":" + (int)minutes + ":" + (int)seconds ;
        }

    }

    // User info (User can view this informartion)
    [SerializeField]
    private string guid = "-1";
    [SerializeField]
    private string name = "Map_";
    [SerializeField]
    private string location = "Location_";
    [SerializeField]
    private AirtDateTime modification_date;
    [SerializeField]
    private ulong bytes_size;
    [SerializeField]
    private Vector3 boundingBox;
    [SerializeField]
    byte map_type;
   // [SerializeField]
   // private string file_path = "Path";
    //Useless
    private Vector3 box_scale = new Vector3(0, 0, 0);

    public string Guid
    {
        get { return guid; }
    }

    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    public string Location
    {
        get { return location; }
        set { location = value; }
    }    

    public Vector3 BoundingBox
    {
        get { return boundingBox; }
        set { boundingBox = value; }
    }

    public byte Map_type
    {
        get { return map_type; }
        set { map_type = value; }
    }

    //public string MapPath
    //{
    //    get { return file_path; }
    //    set { file_path = value; }
    //}

    public Vector3 BoxScale
    {
        get { return box_scale; }
        set { box_scale = value; }
    }

    public AirtDateTime Date
    {
        get
        {
            return modification_date;
        }

        set
        {
            modification_date = value;
        }
    }

    public ulong Byte_Size
    {
        get
        {
            return bytes_size;
        }

        set
        {
            bytes_size = value;
        }
    }

    public MapMetadata(string guid, string name, string location, MapType type)
    {
        this.guid = guid;
        this.name = name;
        this.location = location;
        this.Date = new AirtDateTime((byte)DateTime.Now.Day, (byte)DateTime.Now.Month, DateTime.Now.Year, (byte)DateTime.Now.Second, (byte)DateTime.Now.Minute, (byte)DateTime.Now.Hour) ;
        this.Byte_Size = 0;
        this.boundingBox = new Vector3(0, 0, 0);
        this.map_type = (byte)type;

    }

}
