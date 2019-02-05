using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
//This is the class that contains every mission of a plan. Is saved as the guid.json.mission
[Serializable]
public class Map {
    MapMetadata metadata;
    
    [SerializeField]
    private string guid = "-1";
    //This are all the missions
    [SerializeField]
    List<Path> paths = new List<Path>();
    //The matrix to convert the model from unnity to pozyx
    public Matrix4x4 unityToAnchors = new Matrix4x4();
    [SerializeField]
    private bool calibration_applied = false;

    public string Guid
    {
        get { return guid; }
        set { guid = value; }
    }

    public List<Path> Paths
    {
        get { return paths; }
        set { paths = value; }
    }
    public bool Alignment_applied
    {
        get { return calibration_applied; }
        set { calibration_applied = value; }
    }

    //private int type = 
    // Internal info (User CAN NOT view this information)
    //private int numParts = 0;
    //private int numPoints = 0;
    //private List<Path> paths = new List<Path>();

    /*public Map(string guid, string name, string location, MapType type)
    {
        this.guid = guid;
        this.name = name;
        this.location = location;
        this.date = DateTime.Now;
        this.byte_Size = 0;
        this.boundingBox = new Vector3(0, 0, 0);
        this.type = (byte)type;

    }*/

    public void AddPath(Path path)
    {
        paths.Add(path);
    }

}
