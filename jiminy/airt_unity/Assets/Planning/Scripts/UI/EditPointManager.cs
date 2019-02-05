using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// This class manages the change in edit point mode, between editing the type of point, previewing the gimbal and editing the rec camera
/// </summary>
public class EditPointManager : MonoBehaviour
{
    public GameObject selectPointType;
    public GameObject pointTypePanel;
    public GameObject estimatedDronePose;
    public GameObject drone;
    public GameObject cam;
    public UnityEngine.UI.Image point, gimball, recCam;
    public GameObject speedText, speedInputField;
    public GameObject RecCameraPanel;

    //RecCameraPanelsOptions



    public void changePointType()
    {
        point.color = new Vector4(255, 255, 255, 1f);
        gimball.color = new Vector4(255, 255, 255, 0.4f);
        recCam.color = new Vector4(255, 255, 255, 0.4f);

        RecCameraPanel.SetActive(false);
        cam.SetActive(false);
        pointTypePanel.SetActive(true);
        //estimatedDronePose.SetActive(true);
        //drone.SetActive(true);
        selectPointType.SetActive(true);
        speedText.SetActive(true);
        speedInputField.SetActive(true);

    }
    public void changeGimballRotation()
    {
        point.color = new Vector4(255, 255, 255, 0.4f);
        gimball.color = new Vector4(255, 255, 255, 1f);
        recCam.color = new Vector4(255, 255, 255, 0.4f);
        RecCameraPanel.SetActive(false);
        pointTypePanel.SetActive(false);
        estimatedDronePose.SetActive(false);
        drone.SetActive(false);
        selectPointType.SetActive(false);
        cam.SetActive(true);
        speedText.SetActive(false);
        speedInputField.SetActive(false);
        cam.gameObject.transform.position = ChangePointType.pointSelected.transform.position;
        cam.gameObject.transform.rotation = Quaternion.Euler(ChangePointType.pointSelected.GetComponent<PathPoint>().GimbalRotation);
        GetComponent<ChangePointType>().changeGimballRotation();
    }
    public void changeToRecCam()
    {
        point.color = new Vector4(255, 255, 255, 0.4f);
        gimball.color = new Vector4(255, 255, 255, 0.4f);
        recCam.color = new Vector4(255, 255, 255, 1f);
        pointTypePanel.SetActive(false);
        estimatedDronePose.SetActive(false);
        drone.SetActive(false);
        selectPointType.SetActive(false);
        cam.SetActive(false);
        RecCameraPanel.SetActive(true);
        speedText.SetActive(false);
        speedInputField.SetActive(false);

    }
    

}
