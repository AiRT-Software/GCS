using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
//This file is used to check if a box has been clicked to assign an id from an anchor to a box.
public class AnchorsClick : MonoBehaviour, IPointerDownHandler {

    public GameObject anchorList;
    public Camera cubeCam;

    RectTransform rect;
    /*
	void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount == 1))
        {
            UnityEngine.Debug.Log("Anchor clicked: " + name);
            CalibrationSettings.anchorClicked = name;
            anchorList.SetActive(true);

            for (int i = 0; i < 8; i++)
            {
                if (transform.parent.GetChild(i).name == name)
                    transform.parent.GetChild(i).GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
                else
                    transform.parent.GetChild(i).GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");
            }
        }        
    }
    */

    public void OnPointerDown(PointerEventData eventData)
    {
        Ray ray = cubeCam.ScreenPointToRay(eventData.position);
        RaycastHit cube;
        if (Physics.Raycast(ray, out cube, LayerMask.NameToLayer("Anchor")))
        {
            CalibrationSettings.anchorClicked = cube.transform.name;
            for (int i = 0; i < 8; i++)
            {
                //Selected boxes are brighter
                if (cube.transform.parent.GetChild(i).name == cube.transform.name)
                    cube.transform.parent.GetChild(i).GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
                else
                    cube.transform.parent.GetChild(i).GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");
            }
        }
    }
}
