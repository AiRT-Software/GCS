using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderExplainer : MonoBehaviour {

    public GameObject anchorGO;
    public GameObject dronGO;

	void DrawCalibratedScene(Vector3[] anchors){

        for (int i = 0; i < anchors.Length; i++)
            Instantiate(anchorGO, anchors[i], Quaternion.identity);

    }

    // Funcion que dibuja el dron segun su posicion y orientacion
    void RenderDrone(Vector3 dronPos, Vector3 dronOri) {

        dronGO.transform.position = dronPos;
        dronGO.transform.rotation = Quaternion.Euler(dronOri);
        
    }
}
