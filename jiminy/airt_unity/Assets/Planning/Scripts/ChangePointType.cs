using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// Manages the actions done in the edit point info panel
/// </summary>
public class ChangePointType : MonoBehaviour
{
    private Path path;
    public static GameObject pointSelected;
    //Top camera
    public Camera cam;
    Vector3 lastPosition;
    // Materiales para indicar la esfera seleccionada
    public Material waypointNotSelectedMaterial, waypointSelectedMaterial;
    public Material landingSelectedMaterial, landingNotSelectedMaterial;
    public Material homeSelectedMaterial, homeNotSelectedMaterial;
    // Objeto contenedor de todas las esferas
    public UnityEngine.UI.Button point, gimball, recCam;
    //If the waypoint is a normal waypoint or a stop
    public Text isWaypoint, isStop;
    public GameObject sphereParent;
    public GameObject esfera;
    //Front cam and gimbal cam
    public Camera otraCamara, gimballcamera;

    public Material pointCloudMaterial;
    public Material modelMaterial;
    public InputField speedField;
    public InputField stopField;

    public bool activateCamera = true;
    bool changeTheAlphaOnlyOnce = true;
    private float orthoZoomSpeed = 0.02f;
    //Reccam if the parameter is active
    public Toggle activeCameraToggle;
    public Button configParametersButton;
    public GameObject parametersPanel;

    public Button changeToVideoButton, changeToPhotoButton;
    public GameObject videoParametersPanel, photoParametersPanel;
    public Sprite videoSprite, photoSprite;
    public Image imageStateRecording;
    public Toggle recording, whiteBalance, upsideDownCamera, burstMode, AF;
    public Slider brightness, tintWB, contrast, saturation;
    public Dropdown resolutionDropDown, megapixels, sharpness, AE, photoQuality, burstSpeed;
    public Button irisaperture, Iso;
    public GameObject irisPanel, isoPanel;
    public GameObject otherCanvas;
    public enum resolution
    {
        RCAM_MOVIE_FORMAT_4KP25 = 0,
        RCAM_MOVIE_FORMAT_1080P50 = 1,
        RCAM_MOVIE_FORMAT_1080P25 = 2,
        RCAM_MOVIE_FORMAT_720P50 = 3,
        RCAM_MOVIE_FORMAT_WVGAP25 = 4,
        RCAM_MOVIE_FORMAT_2160P25 = 5,
        RCAM_MOVIE_FORMAT_1440P25 = 6,
        RCAM_MOVIE_FORMAT_S1920P25 = 7,
    }
    //Changes the rotation of the camera gimbal
    public void changeGimballRotation()
    {
        if (pointSelected && pointSelected.tag == "Waypoint")
        {
            if (pointSelected.GetComponent<PathPoint>().GimbalRotation == null)
            {
                pointSelected.GetComponent<PathPoint>().GimbalRotation = new Vector3(0, 0, 0);
            }
            gimballcamera.transform.rotation = Quaternion.Euler(pointSelected.GetComponent<PathPoint>().GimbalRotation);
        }
        
    }
    //Same as always
    private void FindShader(string shaderName, int discard)
    {
        int count = 0;

        List<Material> armat = new List<Material>();

        Renderer[] arrend = (Renderer[])Resources.FindObjectsOfTypeAll(typeof(Renderer));
        foreach (Renderer rend in arrend)
        {
            foreach (Material mat in rend.sharedMaterials)
            {
                if (!armat.Contains(mat))
                {
                    armat.Add(mat);
                }
            }
        }

        foreach (Material mat in armat)
        {
            if (mat != null && mat.shader != null && mat.shader.name != null && mat.shader.name == shaderName)
            {
                mat.SetFloat("_Discard", discard);

            }
        }

        
    }
    public void ActivateRecCam()
    {
        if (activeCameraToggle.isOn)
        {
            configParametersButton.gameObject.SetActive(true);
            pointSelected.GetComponent<PathPoint>().Rc.active = true;
            pointSelected.GetComponent<PathPoint>().Rc.edited = true;

        }
        else
        {
            configParametersButton.gameObject.SetActive(false);
            pointSelected.GetComponent<PathPoint>().Rc.active = false;
            pointSelected.GetComponent<PathPoint>().Rc.edited = true;


        }
    }
    public void ShowRecCamButtons()
    {


        parametersPanel.SetActive(true);
        otherCanvas.SetActive(true);
    }
    public void HideRecCamButtons()
    {
        parametersPanel.SetActive(false);
        otherCanvas.SetActive(false);

    }

    //Same as always. Discard is 0 if the shader doesn't discard or 1 if the shader discards
    public void Activate(GameObject sphere, int discard)
    {
        //DEPENDE DE SI USAMOS UN MODELO O POINTCLOUD, ACTIVAR AQUI UN MATERIAL O OTRO
        pointCloudMaterial.SetFloat("_Discard",0);
        FindShader("Custom/ObjectsShader", discard);
        if (sphere == null)
        {
            return;
        }
        pointSelected = sphere;
        if (changeTheAlphaOnlyOnce)
        {
            point.targetGraphic.color = new Vector4(255, 255, 255, 1f);
            gimball.targetGraphic.color = new Vector4(255, 255, 255, 0.5f);
            recCam.targetGraphic.color = new Vector4(255, 255, 255, 0.5f);
        }
        changeTheAlphaOnlyOnce = false;

    }
    // Use this for initialization
    void Awake () {
        if(path == null)
            path = Path.Instance;
    }
    public void onSpeedValueChanged()
    {
        if (speedField.text.Length > 0)
        {
            int speed = 0;
            
            if (int.TryParse(speedField.text, out speed))
            {
                if (speed >= 0 && pointSelected)
                {
                    pointSelected.GetComponent<PathPoint>().Speed = speed;
                }
                else if(pointSelected)
                {
                    speedField.text = "0";
                }
            }
            
        }
    }
    public void onStopValueChanged()
    {
        if (stopField.text.Length > 0)
        {
            uint stopTime = 0;

            if (uint.TryParse(stopField.text, out stopTime))
            {
                if (stopTime >= 0 && pointSelected)
                {
                    pointSelected.GetComponent<PathPoint>().StopTime = stopTime;
                }
                else if(pointSelected)
                {
                    stopField.text = "0";
                }
            }
        }
    }
    /// <summary>
    /// If a waypoint is clicked and the user clicks on the waypoint button, it will assign them that it is a waypoint
    /// </summary>
    public void waypointClicked()
    {
        isWaypoint.color = new Color(1, 1, 1, 1);
        isStop.color = new Color(1, 1, 1, 0.5f);

        if (pointSelected)
        {
            pointSelected.GetComponent<PathPoint>().setPointType(Point.PointType.WayPoint);
            //int pos = 0;
            //pos = path.NeedsToUpdateGbParameters(pointSelected.GetComponent<PathPoint>().Id);
            //if (pos == -1)
            //{
            //    path.addNewParamGb(pointSelected.GetComponent<PathPoint>().Wp.gimbal_parameters);
            //}
            //else
            //{
            //    path.updateParamGB(pos, pointSelected.GetComponent<PathPoint>().Wp.gimbal_parameters);
            //}
        }
    }
    /// If a waypoint is clicked and the user clicks on the stop button, it will assign them that it is a stop, and also the time if the user changes the time that it will stop

    public void stopClicked()
    {
        isWaypoint.color = new Color(1, 1, 1, 0.5f);
        isStop.color = new Color(1, 1, 1, 1f);
        if (pointSelected)
        {
            pointSelected.GetComponent<PathPoint>().setPointType(Point.PointType.Stop);
            //int pos = 0;
            //pos = path.NeedsToUpdateGbParameters(pointSelected.GetComponent<PathPoint>().Id);
            //if (pos == -1)
            //{
            //    path.addNewParamGb(pointSelected.GetComponent<PathPoint>().Wp.gimbal_parameters);
            //}
            //else
            //{
            //    path.updateParamGB(pos, pointSelected.GetComponent<PathPoint>().Wp.gimbal_parameters);
            //}
        }
    }
   
    private void Update()
    {
        Vector3 point = Camera.main.ViewportToScreenPoint(new Vector3(cam.rect.x, cam.rect.y,0));
        Vector3 point2 = Camera.main.ViewportToScreenPoint(new Vector3(cam.rect.xMax, cam.rect.yMax, 0));

        var rect = new Rect(point.x,point.y, point2.x - point.x, point2.y - point.y);
        //If we click inside the area of the camera
        if (rect.Contains(Input.mousePosition) && Input.GetMouseButtonDown(0))
        {
            GameObject aux = pointSelected;
            //For the previous clicked waypoint
            if (pointSelected && pointSelected.tag == "Waypoint")
            {
                //We color it as deselected
                if (pointSelected.GetComponent<PathPoint>().Id == 0)
                {
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeNotSelectedMaterial;

                }
                else if ( pointSelected.GetComponent<PathPoint>().Id == path.Count() - 1)
                {
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingNotSelectedMaterial;

                }
                else
                {
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().material = waypointNotSelectedMaterial;
                }
            }
            Ray ray = cam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f));
            pointSelected = RayToSphere(ray);
            //If we click a waypoint
            if (pointSelected && pointSelected.tag == "Waypoint")
            {
                //We put the gimbal camera in this waypoint
                gimballcamera.gameObject.transform.position = pointSelected.transform.position;
                gimballcamera.gameObject.transform.rotation = Quaternion.Euler(pointSelected.GetComponent<PathPoint>().GimbalRotation);
                //Restart every option value
                if (pointSelected.GetComponent<PathPoint>().Speed == 0)
                {
                    speedField.text = "" + MissionManager.planDefaultSpeed;
                }
                else
                {
                    speedField.text = "" + pointSelected.GetComponent<PathPoint>().Speed;
                }
                stopField.text = "" + pointSelected.GetComponent<PathPoint>().StopTime;

                reSetRecCamGuis(pointSelected.GetComponent<PathPoint>().Rc);


            }
            if (!pointSelected || !pointSelected.tag.Equals("Waypoint"))
            {
                //If we didn't click any point, we restore the previous point as the one clicked and recolor it again
                pointSelected = aux;
                if (pointSelected.GetComponent<PathPoint>().Id == 0)
                {
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeSelectedMaterial;

                }
                else if (pointSelected.GetComponent<PathPoint>().Id == path.Count() - 1)
                {
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingSelectedMaterial;

                }
                else
                {
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().material = waypointSelectedMaterial;
                }



            }


        }
        


        //This is to check if we clicked inside the gimbal camera 
        Vector3 gimbalCamPoint = Camera.main.ViewportToScreenPoint(new Vector3(gimballcamera.rect.x, gimballcamera.rect.y, 0));
        Vector3 gimbalCamPoint2 = Camera.main.ViewportToScreenPoint(new Vector3(gimballcamera.rect.xMax, gimballcamera.rect.yMax, 0));

        var gimbalCamrect = new Rect(gimbalCamPoint.x, gimbalCamPoint.y, gimbalCamPoint2.x - gimbalCamPoint.x, gimbalCamPoint2.y - gimbalCamPoint.y);
#if UNITY_EDITOR || UNITY_STANDALONE_WIN


        // Comprobamos si se pulsa el clic derecho
        if (Input.GetMouseButtonDown(1))
        {
            lastPosition = Input.mousePosition;
        }
        
        //moves the top camera
        // Comprobamos si se pulsa el clic derecho
        if (Input.GetMouseButton(1) )
        {
            MoveCamera(Input.mousePosition - lastPosition, cam);
            
            lastPosition = Input.mousePosition;
        }

#else
        
        //Same for android
        // Comprobamos si hay dos toques en pantalla para hacer zoom o pan
        if (Input.touchCount == 2)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began ||  Input.GetTouch(1).phase == TouchPhase.Began)
	        {
		  
                lastPosition = Input.mousePosition;
            
	        }
            
            MoveCamera(Input.mousePosition - lastPosition, cam);
            lastPosition = Input.mousePosition;
            // Almacenamos los dos touches
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Calculamos el desplazamiento con respecto al frame anterior de cada touch
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Transformamos la distancia entre los touch a un valor en coma flotante
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Calculamos la diferencia entre estos valores
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
            
                // Hacemos zoom en funcion a la distancia entre los dedos
                cam.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;

                // Aseguramos que el Orthographic size no es menor que 0.1
                cam.orthographicSize = Mathf.Max(cam.orthographicSize, 1f);


                //Scales spheres
                TransformSphere(cam, esfera);


            


        }
#endif
    }
    /// <summary>
    /// Set camera parameters to the ones the waypoint clicked contains
    /// </summary>
    /// <param name="rc"></param>
    void reSetRecCamGuis(RecCamParameters rc)
    {

        if(!rc.active)
        {
            configParametersButton.gameObject.SetActive(false);
            activeCameraToggle.isOn = false;
            return;
        }
        else
        {
            configParametersButton.gameObject.SetActive(true);
            activeCameraToggle.isOn = true;

        }
        if (rc.switchToRec == 0)
        {

            changeToPhotoButton.gameObject.SetActive(false);
            changeToVideoButton.gameObject.SetActive(true);
            videoParametersPanel.SetActive(false);
            photoParametersPanel.SetActive(true);
            activeCameraToggle.transform.GetChild(1).GetComponent<Text>().text = "Take Photo";

            imageStateRecording.sprite = photoSprite;



        }
        else
        {

            changeToVideoButton.gameObject.SetActive(false);
            changeToPhotoButton.gameObject.SetActive(true);
            photoParametersPanel.SetActive(false);
            videoParametersPanel.SetActive(true);
            activeCameraToggle.transform.GetChild(1).GetComponent<Text>().text = "Recording";

            imageStateRecording.sprite = videoSprite;


        }

        switch (rc.resolution)
        {
            case 22:
                resolutionDropDown.value = 0;
                break;
            case 23:
                resolutionDropDown.value = 1;
                break;
            case 24:
                resolutionDropDown.value = 2;
                break;
            case 25:
                resolutionDropDown.value = 3;
                break;
            case 26:
                resolutionDropDown.value = 4;
                break;
            case 35:
                resolutionDropDown.value = 5;
                break;
            case 41:
                resolutionDropDown.value = 6;
                break;
            case 47:
                resolutionDropDown.value = 7;
                break;
            default:
                break;
        }

        megapixels.value = rc.megaPixels;

        brightness.value = (float) BitConverter.ToInt32(rc.brightness, 0);

        if (rc.autoManualWB == 0)
        {
            whiteBalance.isOn = true;

        }
        else
        {
            whiteBalance.isOn = false;

        }
        tintWB.value = rc.WBTint;

        RCamConfigISO isoPointParam = (RCamConfigISO)rc.ISO;
        switch (isoPointParam)
        {
            case RCamConfigISO.RCAM_ISO_AUTO:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "Auto";
                break;
            case RCamConfigISO.RCAM_ISO_100:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "100";
                break;
            case RCamConfigISO.RCAM_ISO_125:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "125";
                break;
            case RCamConfigISO.RCAM_ISO_160:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "160";
                break;
            case RCamConfigISO.RCAM_ISO_200:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "200";
                break;
            case RCamConfigISO.RCAM_ISO_250:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "250";
                break;
            case RCamConfigISO.RCAM_ISO_320:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "320";
                break;
            case RCamConfigISO.RCAM_ISO_400:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "400";
                break;
            case RCamConfigISO.RCAM_ISO_500:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "500";
                break;
            case RCamConfigISO.RCAM_ISO_640:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "640";
                break;
            case RCamConfigISO.RCAM_ISO_800:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "800";
                break;
            case RCamConfigISO.RCAM_ISO_1000:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "1000";
                break;
            case RCamConfigISO.RCAM_ISO_1250:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "1250";
                break;
            case RCamConfigISO.RCAM_ISO_1600:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "1600";
                break;
            case RCamConfigISO.RCAM_ISO_2000:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "2000";
                break;
            case RCamConfigISO.RCAM_ISO_2500:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "2500";
                break;
            case RCamConfigISO.RCAM_ISO_3200:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "3200";
                break;
            case RCamConfigISO.RCAM_ISO_4000:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "4000";
                break;
            case RCamConfigISO.RCAM_ISO_5000:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "5000";
                break;
            case RCamConfigISO.RCAM_ISO_6400:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "6400";
                break;
            case RCamConfigISO.RCAM_ISO_8000:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "8000";
                break;
            case RCamConfigISO.RCAM_ISO_10000:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "100000";
                break;
            case RCamConfigISO.RCAM_ISO_12800:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "12800";
                break;
            case RCamConfigISO.RCAM_ISO_16000:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "16000";
                break;
            case RCamConfigISO.RCAM_ISO_20000:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "20000";
                break;
            case RCamConfigISO.RCAM_ISO_25600:
                Iso.transform.GetChild(0).GetComponent<Text>().text = "25600";
                break;
            default:
                break;
        }

        sharpness.value = rc.sharpness;

        contrast.value = (float)BitConverter.ToInt32(rc.contrast, 0);

        AE.value = rc.AE;

        saturation.value = (float)BitConverter.ToInt32(rc.saturation, 0);

        photoQuality.value = rc.photoQuality;

        if (rc.upsideDown == 3)
        {
            upsideDownCamera.isOn = true;

        }
        else
        {
            upsideDownCamera.isOn = false;

        }
        RCamConfigIris irisValueFromButton = (RCamConfigIris) rc.irisAperture;
        switch (irisValueFromButton)
        {
            case RCamConfigIris.RCAM_IRIS_F0_7:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F0.7";
                break;
            case RCamConfigIris.RCAM_IRIS_F0_8:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F0.8";

                break;
            case RCamConfigIris.RCAM_IRIS_F0_9:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F0.9";

                break;
            case RCamConfigIris.RCAM_IRIS_F1:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F1";

                break;
            case RCamConfigIris.RCAM_IRIS_F1_1:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F1.1";
                break;
            case RCamConfigIris.RCAM_IRIS_F1_2:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F1.2";
                break;
            case RCamConfigIris.RCAM_IRIS_F1_4:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F1.4";
                break;
            case RCamConfigIris.RCAM_IRIS_F1_6:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F1.6";
                break;
            case RCamConfigIris.RCAM_IRIS_F1_8:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F1.8";
                break;
            case RCamConfigIris.RCAM_IRIS_F2:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F2";
                break;
            case RCamConfigIris.RCAM_IRIS_F2_2:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F2.2";
                break;
            case RCamConfigIris.RCAM_IRIS_F2_5:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F2.5";
                break;
            case RCamConfigIris.RCAM_IRIS_F2_8:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F2.8";
                break;
            case RCamConfigIris.RCAM_IRIS_F3_2:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F3.2";
                break;
            case RCamConfigIris.RCAM_IRIS_F3_5:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F3.5";
                break;
            case RCamConfigIris.RCAM_IRIS_F4:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F4";
                break;
            case RCamConfigIris.RCAM_IRIS_F4_5:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F4.5";
                break;
            case RCamConfigIris.RCAM_IRIS_F5:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F5";
                break;
            case RCamConfigIris.RCAM_IRIS_F5_6:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F5.6";
                break;
            case RCamConfigIris.RCAM_IRIS_F6_3:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F6.3";
                break;
            case RCamConfigIris.RCAM_IRIS_F7_1:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F7.1";
                break;
            case RCamConfigIris.RCAM_IRIS_F8:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F8";
                break;
            case RCamConfigIris.RCAM_IRIS_F9:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F9";
                break;
            case RCamConfigIris.RCAM_IRIS_F10:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F10";
                break;
            case RCamConfigIris.RCAM_IRIS_F11:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F11";
                break;
            case RCamConfigIris.RCAM_IRIS_F13:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F13";
                break;
            case RCamConfigIris.RCAM_IRIS_F14:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F14";
                break;
            case RCamConfigIris.RCAM_IRIS_F16:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F16";
                break;
            case RCamConfigIris.RCAM_IRIS_F18:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F18";
                break;
            case RCamConfigIris.RCAM_IRIS_F20:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F20";
                break;
            case RCamConfigIris.RCAM_IRIS_F22:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F22";
                break;
            case RCamConfigIris.RCAM_IRIS_F25:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F25";
                break;
            case RCamConfigIris.RCAM_IRIS_F29:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F29";
                break;
            case RCamConfigIris.RCAM_IRIS_F32:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F32";
                break;
            case RCamConfigIris.RCAM_IRIS_F36:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F36";
                break;
            case RCamConfigIris.RCAM_IRIS_F40:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F40";
                break;
            case RCamConfigIris.RCAM_IRIS_F45:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F45";
                break;
            case RCamConfigIris.RCAM_IRIS_F51:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F51";
                break;
            case RCamConfigIris.RCAM_IRIS_F57:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F57";
                break;
            case RCamConfigIris.RCAM_IRIS_F64:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F64";
                break;
            case RCamConfigIris.RCAM_IRIS_F72:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F72";
                break;
            case RCamConfigIris.RCAM_IRIS_F80:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F80";
                break;
            case RCamConfigIris.RCAM_IRIS_F90:
                irisaperture.transform.GetChild(0).GetComponent<Text>().text = "F90";
                break;
            default:
                break;
        }
        if (rc.burstMode == 1)
        {
            burstMode.isOn = true;

        }
        else
        {
            burstMode.isOn = false;

        }

        burstSpeed.value = rc.burstSpeed;
        if (rc.AF == 0)
        {
            AF.isOn = false;
        }
        else
        {
            AF.isOn = true;
        }
    }



    //Scales spheres
    void TransformSphere(Camera cam, GameObject esfera)
    {
        if (cam.orthographicSize > 15)
        {
            foreach (Transform child in sphereParent.transform)
            {
                child.localScale = new Vector3(cam.orthographicSize / 15.0f, cam.orthographicSize / 15.0f, cam.orthographicSize / 15.0f);
            }
        }
        else
        {
            foreach (Transform child in sphereParent.transform)
            {
                child.localScale = new Vector3(cam.orthographicSize / 8.0f, cam.orthographicSize / 8.0f, cam.orthographicSize / 8.0f);
            }
        }

    }
    //Rotates a camera
    public void RotateCamera(Vector2 pos, Camera cam)
    {
        cam.transform.Rotate(pos.y, pos.x, 0);
    }
    //moves a camera
    public void MoveCamera(Vector2 pos, Camera cam)
    {

        //Debug.Log("CamViewportToWorldPoint: " + cam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, 0));
        //Vector3 aux = cam.transform.position - cam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, 0));

        //cam.transform.position += new Vector3(aux.x, 0.0f, aux.z) * Time.deltaTime;
       
        Vector2 move = new Vector2((-pos.x / cam.pixelWidth) * cam.aspect, -pos.y / cam.pixelHeight);
        cam.transform.Translate(move.x * cam.orthographicSize * 2, move.y * cam.orthographicSize * 2, 0);

        
        


    }
    //As always, throws a raycast from the clicked position on screen
    GameObject RayToSphere(Ray ray)
    {

        RaycastHit[] hit = Physics.RaycastAll(ray);
        return DistanceToLine(ray, hit);

    }
    //And finds if a waypoint was clicked
    public GameObject DistanceToLine(Ray ray, RaycastHit[] point)
    {
        float min = Mathf.Infinity;
        float distance = 0.0f;
        GameObject sphereSelected = null;

        for (int i = 0; i < point.Length; i++)
        {
            distance = Vector3.Cross(ray.direction, point[i].transform.position - ray.origin).magnitude;
            if (min > distance)
            {
                min = distance;
                sphereSelected = point[i].transform.gameObject;
            }
        }

        if (sphereSelected != null && sphereSelected.transform.parent.tag == "Waypoint")
        {
            sphereSelected = sphereSelected.transform.parent.gameObject;

            if (sphereSelected.GetComponent<PathPoint>().Id == 0)
            {
                sphereSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeSelectedMaterial;

            }
            else if (sphereSelected.GetComponent<PathPoint>().Id == path.Count() - 1)
            {
                sphereSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingSelectedMaterial;

            }
            else
            {
                sphereSelected.transform.GetChild(0).GetComponent<Renderer>().material = waypointSelectedMaterial;
            }

        }

        return sphereSelected;
    }
    //Deactivates the panel
    public void Deactivate()
    {
        if (pointSelected)

        {
            if (path == null)
            {
                path = Path.Instance;
            }
            //Deselects any button selected
            pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = waypointNotSelectedMaterial;
            //UnityEngine.Debug.Log(path);

            if (pointSelected.GetComponent<PathPoint>().Id == path.Count() -1)
            {
                pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingSelectedMaterial;

            }
            else if (pointSelected.GetComponent<PathPoint>().Id == 0)
            {
                pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeSelectedMaterial;

            }

            pointSelected = null;
        }
        //Assigns to the front cam the zoom if the user zoomed the top cam
        otraCamara.orthographicSize = cam.orthographicSize;

    }
    //All of this is recording parameters that are asigned to a waypoint when the respective buttons are clicked
    public void changeToRecording()
    {

        if (pointSelected.GetComponent<PathPoint>().Rc.switchToRec == 1)
        {
            pointSelected.GetComponent<PathPoint>().Rc.switchToRec = 0;

            changeToPhotoButton.gameObject.SetActive(false);
            changeToVideoButton.gameObject.SetActive(true);
            videoParametersPanel.SetActive(false);
            photoParametersPanel.SetActive(true);

            activeCameraToggle.transform.GetChild(1).GetComponent<Text>().text = "Take Photo";

            imageStateRecording.sprite = photoSprite;



        }
        else
        {
            pointSelected.GetComponent<PathPoint>().Rc.switchToRec = 1;

            changeToVideoButton.gameObject.SetActive(false);
            changeToPhotoButton.gameObject.SetActive(true);
            photoParametersPanel.SetActive(false);
            videoParametersPanel.SetActive(true);
            activeCameraToggle.transform.GetChild(1).GetComponent<Text>().text = "Recording";

            imageStateRecording.sprite = videoSprite;


        }


    }
   
    public void onResolutionChanged()
    {
        pointSelected.GetComponent<PathPoint>().Rc.edited = true;

        resolution index = (resolution)resolutionDropDown.value;
        switch (index)
        {
            case resolution.RCAM_MOVIE_FORMAT_4KP25:
                pointSelected.GetComponent<PathPoint>().Rc.resolution = (byte)RCamMovieFormat.RCAM_MOVIE_FORMAT_4KP25;
                break;
            case resolution.RCAM_MOVIE_FORMAT_1080P50:
                pointSelected.GetComponent<PathPoint>().Rc.resolution = (byte)RCamMovieFormat.RCAM_MOVIE_FORMAT_1080P50;
                break;
            case resolution.RCAM_MOVIE_FORMAT_1080P25:
                pointSelected.GetComponent<PathPoint>().Rc.resolution = (byte)RCamMovieFormat.RCAM_MOVIE_FORMAT_1080P25;
                break;
            case resolution.RCAM_MOVIE_FORMAT_720P50:
                pointSelected.GetComponent<PathPoint>().Rc.resolution = (byte)RCamMovieFormat.RCAM_MOVIE_FORMAT_720P50;
                break;
            case resolution.RCAM_MOVIE_FORMAT_WVGAP25:
                pointSelected.GetComponent<PathPoint>().Rc.resolution = (byte)RCamMovieFormat.RCAM_MOVIE_FORMAT_WVGAP25;
                break;
            case resolution.RCAM_MOVIE_FORMAT_2160P25:
                pointSelected.GetComponent<PathPoint>().Rc.resolution = (byte)RCamMovieFormat.RCAM_MOVIE_FORMAT_2160P25;
                break;
            case resolution.RCAM_MOVIE_FORMAT_1440P25:
                pointSelected.GetComponent<PathPoint>().Rc.resolution = (byte)RCamMovieFormat.RCAM_MOVIE_FORMAT_1440P25;
                break;
            case resolution.RCAM_MOVIE_FORMAT_S1920P25:
                pointSelected.GetComponent<PathPoint>().Rc.resolution = (byte)RCamMovieFormat.RCAM_MOVIE_FORMAT_S1920P25;
                break;
            default:
                break;
        }
    }
    public void onMPChanged()
    {
        pointSelected.GetComponent<PathPoint>().Rc.edited = true;

        pointSelected.GetComponent<PathPoint>().Rc.megaPixels = (byte)megapixels.value;
    }
    public void brightnessChangedVideo()
    {
        pointSelected.GetComponent<PathPoint>().Rc.edited = true;

        pointSelected.GetComponent<PathPoint>().Rc.brightness = BitConverter.GetBytes((int)brightness.value);

    }
    public void onAfChanged()
    {
        pointSelected.GetComponent<PathPoint>().Rc.edited = true;
        if (AF.isOn)
        {
            pointSelected.GetComponent<PathPoint>().Rc.AF = 1;

        }
        else
        {
            pointSelected.GetComponent<PathPoint>().Rc.AF = 0;

        }



    }

    public void toggleWBVideo()
    {
        pointSelected.GetComponent<PathPoint>().Rc.edited = true;

        if (!whiteBalance.isOn)
        {
            pointSelected.GetComponent<PathPoint>().Rc.autoManualWB = 254;
            tintWB.gameObject.SetActive(true);

        }
        else
        {
            pointSelected.GetComponent<PathPoint>().Rc.autoManualWB = 0;
            tintWB.gameObject.SetActive(false);

        }
    }
    public void tintWBVideo()
    {
        pointSelected.GetComponent<PathPoint>().Rc.edited = true;

        pointSelected.GetComponent<PathPoint>().Rc.WBTint = (byte)tintWB.value;
          

    }
    
    public void onISOVIdeoChanged()
    {
        pointSelected.GetComponent<PathPoint>().Rc.edited = true;

        isoPanel.SetActive(true);
        otherCanvas.SetActive(true);
        //pointSelected.GetComponent<PathPoint>().Rc.ISO = (byte)inputParametersVideos[4].GetComponent<Dropdown>().value;
        //inputParametersPhotos[4].GetComponent<Dropdown>().value = inputParametersVideos[4].GetComponent<Dropdown>().value;
    }
    public void onIsoClose()
    {
        isoPanel.SetActive(false);
        otherCanvas.SetActive(false);
    }
    public void onSharpnessVideoChanged()
    {
        pointSelected.GetComponent<PathPoint>().Rc.edited = true;

        pointSelected.GetComponent<PathPoint>().Rc.sharpness = (byte)sharpness.value;

    }

   
    public void onContrastVideoChanged()
    {
        pointSelected.GetComponent<PathPoint>().Rc.edited = true;

        pointSelected.GetComponent<PathPoint>().Rc.contrast = BitConverter.GetBytes((int)contrast.value);
        UnityEngine.Debug.Log(pointSelected.GetComponent<PathPoint>().Rc.contrast.Length);   
    }
    public void onAEVideoChanged()
    {
        pointSelected.GetComponent<PathPoint>().Rc.edited = true;

        pointSelected.GetComponent<PathPoint>().Rc.AE = (byte)AE.value;

    }
   
    public void onSaturationVideoChanged()
    {

        pointSelected.GetComponent<PathPoint>().Rc.edited = true;

        pointSelected.GetComponent<PathPoint>().Rc.saturation = BitConverter.GetBytes((int)saturation.value);
            
    }
    public void onPhotoQualityChanged()
    {
        pointSelected.GetComponent<PathPoint>().Rc.edited = true;

        pointSelected.GetComponent<PathPoint>().Rc.photoQuality = (byte)photoQuality.value;
    }
   
    public void onRotationChangedVideo()
    {
        pointSelected.GetComponent<PathPoint>().Rc.edited = true;

        if (upsideDownCamera.isOn)
        {
            pointSelected.GetComponent<PathPoint>().Rc.upsideDown = 3;
        }
        else
        {
            pointSelected.GetComponent<PathPoint>().Rc.upsideDown = 0;
        }
    }
    public void onIrisApertureVideoChanged()
    {
        pointSelected.GetComponent<PathPoint>().Rc.edited = true;

        irisPanel.SetActive(true);
        otherCanvas.SetActive(true);



        //pointSelected.GetComponent<PathPoint>().Rc.irisAperture = (byte)inputParametersVideos[10].GetComponent<Dropdown>().value;
        //inputParametersPhotos[11].GetComponent<Dropdown>().value = inputParametersVideos[10].GetComponent<Dropdown>().value;

    }
    //The iris and iso are different panels to the main reccam one, so we need to ope nand close them
    public void onIrisClose()
    {
        irisPanel.SetActive(false);
        otherCanvas.SetActive(false);
    }
    public void onPhotoBurstMode()
    {
        pointSelected.GetComponent<PathPoint>().Rc.edited = true;

        if (burstMode.isOn)
        {
            pointSelected.GetComponent<PathPoint>().Rc.burstMode = 1;
        }
        else
        {
            pointSelected.GetComponent<PathPoint>().Rc.burstMode = 0;
        }
    }
    public void PhotoBurstSpeed()
    {
        pointSelected.GetComponent<PathPoint>().Rc.edited = true;

        pointSelected.GetComponent<PathPoint>().Rc.burstSpeed = (byte)burstSpeed.value;

    }
}
