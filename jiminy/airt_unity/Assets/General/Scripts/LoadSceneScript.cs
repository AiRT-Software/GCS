using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneScript : MonoBehaviour {
	
	// Update is called once per frame
	void FixedUpdate () {
        transform.GetChild(1).Rotate(0, 0, -200 * Time.deltaTime);
	}
}
