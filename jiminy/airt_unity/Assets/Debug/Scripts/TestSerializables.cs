using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TestSerializables : MonoBehaviour {

	// Use this for initialization
	void Start () {

        Path path = Path.Instance;
        this.gameObject.AddComponent<PathPoint>();
        //PathPoint point = this.gameObject.GetComponent<PathPoint>();
        this.gameObject.GetComponent<PathPoint>().createPathPoint(this.transform);
        path.AddPointWithGimball(this.GetComponent<PathPoint>().getPoint(), this.GetComponent<PathPoint>());
        Map m = new Map();
        MapMetadata mapMetadata = new MapMetadata(System.Guid.NewGuid().ToString(), "mapa1", "1L08", MapMetadata.MapType.EmptyBox);

        m.AddPath(path);
        string json = JsonUtility.ToJson(m);
        File.WriteAllText(mapMetadata.Name + ".json", json);

        json = File.ReadAllText(mapMetadata.Name + ".json");
        MapMetadata m2 = JsonUtility.FromJson<MapMetadata>(json);
        m2.Name = "mapa2";
        json = JsonUtility.ToJson(m2);
        File.WriteAllText(m2.Name + ".json", json);
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
