using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectionLine : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Adjust position and scale for the line from a waypoint to the floor (0,0,0) .
	void Update () {
        transform.parent.localScale = new Vector3(0.1f, (transform.parent.parent.GetChild(0).position.y) , 0.1f);
        transform.parent.position = new Vector3(transform.position.x, (transform.parent.parent.GetChild(0).position.y)  , transform.position.z);
	}
}
