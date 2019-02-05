using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// Contains the metadata of the path
/// </summary>
[Serializable]
public class PathMetadata {

    [SerializeField]
    private string path_name = "plan_";
    [SerializeField]
    private string author_name = "Bob_";
    [SerializeField]
    MapMetadata.AirtDateTime modification_date;
    [SerializeField]
    int version_major = 0;
    [SerializeField]
    int version_minor = 0;
    [SerializeField]
    int version_patch = 0;
    [SerializeField]
    private string user_notes = "";

    public string Name
    {
        get { return path_name; }
        set { path_name = value; }
    }

    public string Author
    {
        get { return author_name; }
        set { author_name = value; }
    }

    public MapMetadata.AirtDateTime ModificationDate
    {
        get { return modification_date; }
        set { modification_date = value; }
    }

    public Vector3 Version
    {
        get { return new Vector3(version_major, version_minor, version_patch); }
        set { version_major = (int)value.x;
            version_minor = (int)value.y;
            version_patch = (int)value.z;
        }
    }

    public string Notas
    {
        get { return user_notes; }
        set { user_notes = value; }
    }

    public PathMetadata(string name, string author, DateTime date, Vector3 version, string notas)
    {
        this.path_name = name;
        this.author_name = author;
        this.modification_date = new MapMetadata.AirtDateTime((byte)DateTime.Now.Day, (byte)DateTime.Now.Month, DateTime.Now.Year, (byte)DateTime.Now.Second, (byte)DateTime.Now.Minute, (byte)DateTime.Now.Hour);
        version_major = (int)version.x;
        version_minor = (int)version.y;
        version_patch = (int)version.z;
        this.user_notes = notas;
    }	
}
