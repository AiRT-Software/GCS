using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 
//This class manages the creation of waypoints, editing of waypoints and deleting them
public class PointClicker : MonoBehaviour, IPointerUpHandler, IPointerDownHandler {

    // PROTOTIPO: Enum para identificar el tipo de click o touch
    private enum PointType
    {
        FastTap = 0,
        LongTap,
        Drag
    };

    PointType pT;
    //Botones para no hacerlos interactuables si no hay puntos
    public Button editWaypointButton, POIButton, editCurvesButton, previewButton;


    // Camaras encargadas de los renderizados a textura
    public Camera topCam;
    public Camera frontCam;

    // Materiales para indicar la esfera seleccionada
    public Material waypointNotSelectedMaterial, waypointSelectedMaterial;
    public Material landingSelectedMaterial, landingNotSelectedMaterial;
    public Material homeSelectedMaterial, homeNotSelectedMaterial;

    // Esfera a ser movida
    private GameObject pointSelected;
    private GameObject prevSelected;
    //Esfera a ser eliminada
    public GameObject eraseButton;

    // Scripts para el renderizado de texturas
    private PostRenderFront topRender;
    private PostRenderFront frontRender;

    // Variables para el posicionamiento local del cursor
    Vector2 localCursor;
    RectTransform rect;

    // Variables para el manejo de eventos del raton
    private float orthoZoomSpeed = 0.02f;
    private float deltaTime;
    private bool dragging = false;
    private bool dontRender = false;
    private Vector3 lastPosition;

    // Objeto contenedor de todas las esferas
    public GameObject sphereParent;
    public GameObject esfera;



    bool front = true;
    //We'll save the model bounding box in order not to let the user draw sphere outside of the map
    Vector3 boundingBox = new Vector3();
    //path
    Path path;

    void Awake()
    {
        // Obtencion de componentes
        topRender = topCam.GetComponent<PostRenderFront>();
        frontRender = frontCam.GetComponent<PostRenderFront>();
        rect = GetComponent<RectTransform>();

        // Add and configurate the delegete to drag the waypoints
        EventTrigger trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drag;
        entry.callback.AddListener((data) => { OnDragDelegate((PointerEventData)data); });
        trigger.triggers.Add(entry);
        path = Path.Instance;

        string metadata = File.ReadAllText(Application.persistentDataPath + "/PersistentData/Missions/" + MissionManager.guid + ".json.metadata");
        MapMetadata metadataFile = JsonUtility.FromJson<MapMetadata>(metadata);
        boundingBox = metadataFile.BoundingBox;

    }

    // Delegate to treat the drag event
    public void OnDragDelegate(PointerEventData data)
    {
        dragging = true;
        // Transform local screen coordinates to local rectangle coordinates and make sure we clicked inside the rectangle. The rectangle is a gameobject that encompasses the two cameras
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, null, out localCursor))
            return;
        //If we are in windows
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        // We check if a waypoint is selected and if we should move it
        if (data.button == PointerEventData.InputButton.Left && pointSelected && pointSelected.transform.tag == "Waypoint")
        {
            // In which camera are we going to move it
            if (Input.mousePosition.x < Camera.main.pixelWidth / 2)
            {
                localCursor = new Vector2((localCursor.x) / (rect.sizeDelta.x / 2), (localCursor.y + (rect.sizeDelta.y / 2)) / rect.sizeDelta.y);

            }
            else
            {
                frontCam.GetComponent<CameraMovement>().enabled = false;

                localCursor = new Vector2((localCursor.x * 2) / rect.sizeDelta.x - 1, (localCursor.y + (rect.sizeDelta.y / 2)) / rect.sizeDelta.y);
                //MoveCamera(Input.mousePosition - lastPosition, frontCam);

            }
            // Move the waypoint
            MoveSphere(localCursor, pointSelected);
                
           
        }

        // We check if the right mouse click is being pressed
        if (Input.GetMouseButton(1)) {
            Camera aux = topCam;
            // Identificar camara que vamos a tratar. Solo interactuar en la izquierda
            if (Input.mousePosition.x >= Camera.main.pixelWidth / 2)
            {

                aux = frontCam;

            }
            if (Input.mousePosition.x < Camera.main.pixelWidth / 2)
            {
                MoveCamera(Input.mousePosition - lastPosition, aux);

            }
            lastPosition = Input.mousePosition;
        }

#else
        // Check for a single finger to move the sphere
        if (Input.touchCount == 1) {
            // Identify the camera
             if (Input.GetTouch(0).position.x < (Camera.main.pixelWidth / 2))
            {
                // Move the sphere
                localCursor = new Vector2((localCursor.x) / (rect.sizeDelta.x / 2), (localCursor.y + (rect.sizeDelta.y / 2)) / rect.sizeDelta.y);
            }
            else
            {
                // Movemos la esfera
                localCursor = new Vector2((localCursor.x * 2) / rect.sizeDelta.x - 1, (localCursor.y + (rect.sizeDelta.y / 2)) / rect.sizeDelta.y);
            }
            MoveSphere(localCursor, pointSelected);
        }

        // With two finger we do zoom or pan
        if(Input.touchCount == 2) {
            Camera aux = topCam;

            // Identificar camara que vamos a tratar
            if (Input.GetTouch(0).position.x < Camera.main.pixelWidth / 2) 
            {
                // Movemos la camara
                localCursor = new Vector2((localCursor.x) / (rect.sizeDelta.x / 2), (localCursor.y + (rect.sizeDelta.y / 2)) / rect.sizeDelta.y);
                MoveCamera(Input.mousePosition - lastPosition, aux);


            }
            else
            {
                aux = frontCam;

                // Movemos la camara
                localCursor = new Vector2((localCursor.x * 2) / rect.sizeDelta.x - 1, (localCursor.y + (rect.sizeDelta.y / 2)) / rect.sizeDelta.y);           
            }
            lastPosition = Input.mousePosition;
            // Save the two touches
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Calculate the position diference between frames
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Transform the distance to a float
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Calculate teh difference between these two values
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // Identify the camera
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, null, out localCursor))
            {
                // We zoom the camera depending on the distance between the fingers
                topCam.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;

                // Ortographic size can't be lower than zero
                topCam.orthographicSize = Mathf.Max(topCam.orthographicSize, 1f);

                // Same as above for the other camera
                //frontCam.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;

                // Aseguramos que el Orthographic size no es menor que 0.1
                //frontCam.orthographicSize = Mathf.Max(frontCam.orthographicSize, 1);
                //Scale the spheres
                TransformSphere(frontCam, esfera);
            }
        }
#endif

    }
    /// <summary>
    /// This function is called when somebdy presses with a finger or mouse click the screen
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        //Save the mouse position and the loocal coordenades in a rectangle of the last click
        lastPosition = Input.mousePosition;
        deltaTime = Time.time;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, null, out localCursor))
            return;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        // Check if a static left click is being pressed
        if (eventData.button == PointerEventData.InputButton.Left && Input.GetAxisRaw("Mouse X") == 0 && Input.GetAxisRaw("Mouse Y") == 0) {
            // Identify the camera
            Camera aux = topCam;
            //Transform coordinates to cam coordinates

            if (Input.mousePosition.x > Camera.main.pixelWidth / 2)
            {
                aux = frontCam;
                localCursor = new Vector2((localCursor.x * 2) / rect.sizeDelta.x - 1, (localCursor.y + (rect.sizeDelta.y / 2)) / rect.sizeDelta.y);
            }
            else
            {
                localCursor = new Vector2((localCursor.x) / (rect.sizeDelta.x / 2), (localCursor.y + (rect.sizeDelta.y / 2)) / rect.sizeDelta.y);
            }
            // Desselect a previously selected point
            if (pointSelected != null && pointSelected.transform.tag == "Waypoint")
            {
                //If the first point is deselected, we just change its color. The first point material contains a texture with a house and its different to the rest
                if (pointSelected.GetComponent<PathPoint>().Id == 0)
                {
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeNotSelectedMaterial;
                    //pointSelected.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.color = Color.white;
                }
                else if(pointSelected.GetComponent<PathPoint>().Id != path.Count()-1)
                {
                    //For the points that aren't landing or home, we change the material, and just in case it was previously a landing, the color
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = waypointNotSelectedMaterial;

                }
                else
                {
                    //As for the last point, the material contains a texture with a landing icon
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingNotSelectedMaterial;

                }

            }
            // Save the last selected point
            prevSelected = pointSelected;
            pointSelected = null;
            // Ray to detect collision between waypoints
            Ray ray = aux.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z));
            pointSelected = RayToSphere(ray);
            if (Input.mousePosition.x > Camera.main.pixelWidth / 2)
            {
                //On the right screen we only want to know if something was selected;
                return;
            }
            // If there wasn't a collision, pointselected will be null, which means that the user clicked on an empty space
            if (pointSelected == null)
            {
                // We check if the user clicked on the curve
                GameObject finalPoint = null;
                int index = 0;
                float minMagnitude = Mathf.Infinity;
                int segments = 0;
                int j = 0;

                for (int i = 0; i < path.Count() - 1; i++)
                {
                    //First we get how many points there are between waypoints
                    if (aux == topCam)
                    {
                        segments += path.GetPoint(i).SegmentsTop;
                    }
                    else
                    {
                        segments += path.GetPoint(i).SegmentsRight;

                    }
                    //For each point of the segment of the curve, we check for the closest point to the place the user clicked
                    while (j < segments)
                    {
                        float magnitude;
                        if (Input.mousePosition.x > Camera.main.pixelWidth / 2)
                        {
                            magnitude = NearestPointOnFiniteLineFront(aux.WorldToViewportPoint(path.middlePointsRight[j]) * 10, aux.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z)) * 10);
                        }
                        else
                        {
                            magnitude = NearestPointOnFiniteLineTop(aux.WorldToViewportPoint(path.middlePointsTop[j]) * 10, aux.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z)) * 10);

                        }
                        // Calculate the distance between middlepoints

                        // We check if this is the closest point between every point checked and we save it if is inside a threshold
                        if (magnitude < 0.5f && magnitude < minMagnitude)
                        {
                            minMagnitude = magnitude;
                            finalPoint = path.GetPoint(i).PointTransf.transform.gameObject;
                            index = j;
                        }
                        j++;
                    }
                        
                }

                // If we found a point close enough to the curve we paint it 
                if (finalPoint) {
                    if (Input.mousePosition.x > Camera.main.pixelWidth / 2)
                    {
                        pointSelected = DrawSphereInTheMiddle(path.middlePointsRight[index], finalPoint.GetComponent<PathPoint>().Id);
                    }
                    else
                    {
                        pointSelected = DrawSphereInTheMiddle(path.middlePointsTop[index], finalPoint.GetComponent<PathPoint>().Id);

                    }
                    //Scale it
                    TransformSphere(aux, esfera);
                    //Assign a material. I think it can never be a home here?
                    if (pointSelected.GetComponent<PathPoint>().Id == 0)
                    {
                        pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeSelectedMaterial;

                    }
                    else if (pointSelected.GetComponent<PathPoint>().Id != path.Count()-1)
                    {
                        pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = waypointSelectedMaterial;

                    }
                    else
                    {

                        if (path.Count() > 2)
                        {

                            path.GetPoint(path.Count() - 2).PointTransf.GetChild(0).GetComponent<Renderer>().sharedMaterial = waypointNotSelectedMaterial;

                        }


                        pointSelected.GetComponent<PathPoint>().setPointType(Point.PointType.Land);

                        pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingSelectedMaterial;

                    }
                }

            }
            //else // Si cambio siempre que selecciono una esfera, dejo muchas fuera
            //{
            //    frontCam.nearClipPlane = -frontCam.transform.position.z + pointSelected.transform.position.z + 0.3f;
            //}
        }

#else
        //The same for android
        if (Input.touchCount == 1) {
            // Identificar la camara a tratar
            Camera aux = topCam;
            //Transformamos las coordenadas de click a coordenadas locales de la cámara clicada

            if (Input.mousePosition.x > Camera.main.pixelWidth / 2)
            {
                aux = frontCam;
                localCursor = new Vector2((localCursor.x * 2) / rect.sizeDelta.x - 1, (localCursor.y + (rect.sizeDelta.y / 2)) / rect.sizeDelta.y);
            }
            else
            {
                localCursor = new Vector2((localCursor.x) / (rect.sizeDelta.x / 2), (localCursor.y + (rect.sizeDelta.y / 2)) / rect.sizeDelta.y);
            }
            if (pointSelected != null && pointSelected.transform.tag == "Waypoint"){
                if (pointSelected.GetComponent<PathPoint>().Id == 0)
                {
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeNotSelectedMaterial;

                }
                else if(pointSelected.GetComponent<PathPoint>().Id != path.Count()-1)
                {
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = waypointNotSelectedMaterial;

                }
                else
                {
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingNotSelectedMaterial;

                }
            }
            prevSelected = pointSelected;

            Ray ray = aux.ViewportPointToRay(new Vector3(localCursor.x, localCursor.y, 0.0f));
            pointSelected = RayToSphere(ray);
            
            if (Input.mousePosition.x > Camera.main.pixelWidth / 2)
            {
                return;
            }


            if (pointSelected == null)
            {
                GameObject lastWithIndex = null;
                GameObject finalPoint = null;
                int index = 0;
                float minMagnitude = Mathf.Infinity;
                int segments = 0;
                int j = 0;
                for (int i = 0; i < path.Count() - 1; i++)
                {
                    if (aux == topCam)
                    {
                        segments += path.GetPoint(i).SegmentsTop;
                    }
                    else
                    {
                        segments += path.GetPoint(i).SegmentsRight;

                    }                    
                    while (j < segments)
                    {

                        // !! Calculamos la distancia del punto de click a cada uno de los midlepoints 
                        float magnitude;
                        if (Input.mousePosition.x > Camera.main.pixelWidth / 2)
                        {
                            magnitude = NearestPointOnFiniteLineFront(aux.WorldToViewportPoint(path.middlePointsRight[j]) * 10, aux.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z)) * 10);
                        }
                        else
                        {
                            magnitude = NearestPointOnFiniteLineTop(aux.WorldToViewportPoint(path.middlePointsTop[j]) * 10, aux.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z)) * 10);

                        }
                        // Comprobamos si es el punto más cercano de los encontrados y lo guardamos
                        if (magnitude < 0.5f && magnitude < minMagnitude)
                        {
                            minMagnitude = magnitude;
                            finalPoint = path.GetPoint(i).PointTransf.gameObject;
                            index = j;
                        }
                        j++;
                    }
                        
                }
                if (finalPoint)
                {
                    if (Input.mousePosition.x > Camera.main.pixelWidth / 2)
                    {
                        pointSelected = DrawSphereInTheMiddle(path.middlePointsRight[index], finalPoint.GetComponent<PathPoint>().Id);
                    }
                    else
                    {
                        pointSelected = DrawSphereInTheMiddle(path.middlePointsTop[index], finalPoint.GetComponent<PathPoint>().Id);

                    }
                    TransformSphere(topCam, esfera);
                    if (pointSelected.GetComponent<PathPoint>().Id == 0)
                    {
                        pointSelected.GetComponent<PathPoint>().setPointType(Point.PointType.TakeOff);
                        pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeSelectedMaterial;

                    }
                    else if (pointSelected.GetComponent<PathPoint>().Id != path.Count()-1)
                    {
                        pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = waypointSelectedMaterial;

                    }
                    else
                    {
                       
                        if (path.Count() > 2)
                        {
                            path.GetPoint(path.Count() - 2).PointTransf.GetChild(0).GetComponent<Renderer>().sharedMaterial = waypointNotSelectedMaterial;

                        }
                        pointSelected.GetComponent<PathPoint>().setPointType(Point.PointType.Land);

                        pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingSelectedMaterial;

                    }
                }

            }
        }
#endif
    }


    /// <summary>
    /// This function is called when the user stops pressing on the screen
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(PointerEventData eventData)
    {
        frontCam.GetComponent<CameraMovement>().enabled = true;

        // A click outside of the cameras exits the fucntion
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, null, out localCursor))
            return;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        //If a left click was on an empty space
        if (pointSelected == null && eventData.button == PointerEventData.InputButton.Left && !dontRender) {
            Camera aux = topCam;

            // Clic en el area de visualizacion izquierda 
            if (eventData.position.x < Camera.main.pixelWidth / 2)
            {
                localCursor = new Vector2((localCursor.x) / (rect.sizeDelta.x / 2), (localCursor.y + (rect.sizeDelta.y / 2)) / rect.sizeDelta.y);
            }
            // Clic en el area de visualizacion derecha
            else {
                return;
                localCursor = new Vector2((localCursor.x * 2) / rect.sizeDelta.x - 1, (localCursor.y + (rect.sizeDelta.y / 2)) / rect.sizeDelta.y);
                aux = frontCam;

            }
            //And it was a short click
            if (Time.time - deltaTime < 0.2f)
            {
                //We create a new sphere
                pointSelected = DrawSphere(localCursor, aux);
                //We scale the new sphere
                TransformSphere(aux, esfera);
                //And we assign it a material and a property depending on if it is the first waypoint (home), last(landing) or other
                if (pointSelected.GetComponent<PathPoint>().Id == 0)
                {
                    pointSelected.GetComponent<PathPoint>().setPointType(Point.PointType.TakeOff);

                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeSelectedMaterial;
                    //pointSelected.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.color = new Color32(254, 161, 0, 255);
                }
                else if (pointSelected.GetComponent<PathPoint>().Id != path.Count() - 1)
                {
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = waypointSelectedMaterial;
                    //pointSelected.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial = redOutlineMaterial;
                }
                else
                {
                    //path.GetPoint(path.Count() - 2).PointTransf.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.color = Color.white;
                    if (path.Count() > 2)
                    {
                        path.GetPoint(path.Count() - 2).PointTransf.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = waypointNotSelectedMaterial;
                        //path.GetPoint(path.Count() - 2).PointTransf.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial = whiteOutlineMaterial;
                    }
                    //The other panels should only be activated if there are at least two points
                    if (editWaypointButton.interactable == false)
                    {
                        editWaypointButton.interactable = true;
                        POIButton.interactable = true;
                        editCurvesButton.interactable = false;
                        previewButton.interactable = true;
                    }
                    pointSelected.GetComponent<PathPoint>().setPointType(Point.PointType.Land);

                    // pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingMaterial;
                    //pointSelected.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial = landingMaterial;
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingSelectedMaterial;
                    //pointSelected.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.color = new Color32(254, 161, 0, 255);
                }
            }
        }

#else
        //Same for android
        if (pointSelected == null && Input.touchCount == 1) {
            // Clic en el area de visualizacion izquierda 
            Camera aux = topCam;

            // Clic en el area de visualizacion izquierda 
            if (eventData.position.x < Camera.main.pixelWidth / 2)
            {
                localCursor = new Vector2((localCursor.x) / (rect.sizeDelta.x / 2), (localCursor.y + (rect.sizeDelta.y / 2)) / rect.sizeDelta.y);
            }
            // Clic en el area de visualizacion derecha
            else
            {
                return;
                localCursor = new Vector2((localCursor.x * 2) / rect.sizeDelta.x - 1, (localCursor.y + (rect.sizeDelta.y / 2)) / rect.sizeDelta.y);
                aux = frontCam;

            }
            if (Time.time - deltaTime < 0.2f)
            {

                pointSelected = DrawSphere(localCursor, aux);
                TransformSphere(aux, esfera);
                if (pointSelected.GetComponent<PathPoint>().Id == 0)
                {
                    pointSelected.GetComponent<PathPoint>().setPointType(Point.PointType.TakeOff);

                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeSelectedMaterial;
                }
                else if (pointSelected.GetComponent<PathPoint>().Id != path.Count()-1)
                {
                    
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = waypointSelectedMaterial;
                }
                else
                {

                    if (path.Count() > 2)
                    {
                        pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = waypointNotSelectedMaterial;

                    }

                    if (editWaypointButton.interactable == false)
                        {
                            editWaypointButton.interactable = true;
                            POIButton.interactable = true;
                            editCurvesButton.interactable = false;
                            previewButton.interactable = true;
                        }
                    pointSelected.GetComponent<PathPoint>().setPointType(Point.PointType.Land);

                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingSelectedMaterial;

                }
            }
        }
#endif
        dontRender = false;
        //If a waypoint was selected
        if (pointSelected && pointSelected.transform.tag == "Waypoint")
        {
            //We activate the erasebutton to be able to erase the button
            eraseButton.SetActive(true);
            //If the waypoint was already selected
            if (pointSelected == prevSelected && prevSelected != null && !dragging)
            {
                //we change the material to the deselected material
                if (pointSelected.GetComponent<PathPoint>().Id == 0)
                {
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeNotSelectedMaterial;
                }
                else if(pointSelected.GetComponent<PathPoint>().Id != path.Count()-1)
                {
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = waypointNotSelectedMaterial;

                }
                else
                {
                    pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = landingNotSelectedMaterial;

                }


                pointSelected = null;
                dontRender = true;
                eraseButton.SetActive(false);
            } 
        }
        else
            eraseButton.SetActive(false);

        //Disabel dragging
        dragging = false;

    }
    /// <summary>
    /// Function which throws a ray from the click point to detect if a waypoint has been selected, and if it finds any, it returns the waypoint
    /// </summary>
    /// <param name="ray"></param>
    /// <returns></returns>
    GameObject RayToSphere(Ray ray) {

        RaycastHit[] hit = Physics.RaycastAll(ray);
        return DistanceToLine(ray, hit);

    }
    /// <summary>
    /// Move the camera
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="cam"></param>
    public void MoveCamera(Vector2 pos, Camera cam)
    {

        //Debug.Log("CamViewportToWorldPoint: " + cam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, 0));
        //Vector3 aux = cam.transform.position - cam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, 0));

        //cam.transform.position += new Vector3(aux.x, 0.0f, aux.z) * Time.deltaTime;
        if (cam == topCam)
        {
           
            Vector2 move = new Vector2((-pos.x / cam.pixelWidth) * cam.aspect, -pos.y / cam.pixelHeight);
            cam.transform.Translate(move.x * cam.orthographicSize * 2, move.y * cam.orthographicSize * 2, 0);

        }
        else
        {
            //Vector2 move = new Vector2((-pos.x / cam.pixelWidth) * cam.aspect * 2, (-pos.y / cam.pixelHeight) * 2);
            //cam.transform.Translate(move.x * cam.orthographicSize, move.y * cam.orthographicSize, 0);
        }
        path.middlePointsTop.Clear();
        path.middlePointsRight.Clear();

        frontRender.setUpdate(true);
        topRender.setUpdate(true);

    }
    /// <summary>
    /// Creates a sphere
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="cam"></param>
    /// <returns></returns>
    public GameObject DrawSphere(Vector2 pos, Camera cam)
    {
        esfera.SetActive(true);
        GameObject aux;
        //We instantiate with the coordinates that depend on the view(top cam is from above, frontcam from the front)
        if (cam == topCam)
        {
            
            aux = Instantiate(esfera, cam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, cam.transform.position.y - MissionManager.planDefaultHeight)), Quaternion.identity, sphereParent.transform);

        }
        else
        {
            aux = Instantiate(esfera, cam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, 1.0f - cam.transform.position.z)), Quaternion.identity, sphereParent.transform);


        }
        esfera.SetActive(false);
        //We add a component
        aux.AddComponent<PathPoint>();

        aux.GetComponent<PathPoint>().createPathPoint(aux.transform);
        aux.GetComponent<PathPoint>().Speed = MissionManager.planDefaultSpeed;

        path.AddPointWithGimball(aux.GetComponent<PathPoint>().getPoint(), aux.GetComponent<PathPoint>());
        //Clear the points from the curve which will be redone on the next postrender event with the new waypoint
        path.middlePointsTop.Clear();
        path.middlePointsRight.Clear();

        frontRender.setUpdate(true);
        topRender.setUpdate(true);

        //frontCam.transform.position = new Vector3(aux.transform.position.x, aux.transform.position.y, aux.transform.position.z - 10);
        //frontCam.transform.LookAt(aux.transform.position);
        return aux;
    }
    /// <summary>
    /// Debug
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            foreach (var item in path.middlePointsTop)
            {
                GameObject basura = Instantiate(esfera, item, Quaternion.identity);
                basura.SetActive(true);
            }
        }
       
    }
    /// <summary>
    /// Moves sphere
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="sphere"></param>
    public void MoveSphere(Vector2 pos, GameObject sphere)
    {
        Vector3 aux;
        //The position to move depends on the cameras
        if (Input.mousePosition.x < Camera.main.pixelWidth / 2)
        {
            aux = topCam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, 1 + topCam.transform.position.y));
            sphere.transform.position = new Vector3(aux.x, sphere.transform.position.y, aux.z);

        }
        else
        {
            aux = frontCam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, 1 + frontCam.transform.position.y));
            if (boundingBox.y + 2 < aux.y)
            {
                return;
            }
            if (aux.y < -1)
            {
                return;
            }
            sphere.transform.position = new Vector3(sphere.transform.position.x, aux.y, sphere.transform.position.z);

        }
        //We update the parameters
        sphere.GetComponent<PathPoint>().getPoint().PointPosition = sphere.transform.position;
        frontRender.setUpdate(true);
        topRender.setUpdate(true);
        //Clear the curve points, which will be redone on the post render event
        path.middlePointsTop.Clear();
        path.middlePointsRight.Clear();

        //frontCam.transform.position = new Vector3(sphere.transform.position.x, sphere.transform.position.y, sphere.transform.position.z - 5);
        //frontCam.transform.LookAt(sphere.transform.position);

    }
    /// <summary>
    /// This creates a sphere on the middle of a curve
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public GameObject DrawSphereInTheMiddle(Vector3 pos, int id)
    {
        esfera.SetActive(true);
        GameObject aux = Instantiate(esfera, new Vector3(pos.x, pos.y, pos.z), Quaternion.identity, sphereParent.transform);
        esfera.SetActive(false);
        aux.AddComponent<PathPoint>();
        aux.GetComponent<PathPoint>().createPathPoint(aux.transform);

        path.AddPointatId(aux.GetComponent<PathPoint>().getPoint(), id);
        path.middlePointsTop.Clear();
        path.middlePointsRight.Clear();

        frontRender.setUpdate(true);
        topRender.setUpdate(true);
        //frontCam.transform.position = new Vector3(frontCam.transform.position.x, frontCam.transform.position.y, aux.transform.position.z - 5);

        return aux;
    }
    /// <summary>
    /// Deletes a sphere
    /// </summary>
    public void DeleteSphere()
    {
        //Depending on the waypoint deleted, the previous or next may need to become another type of waypoint
        if (path.Count() > 1)
        {
            if (pointSelected.GetComponent<PathPoint>().Id == 0)
            {
                path.GetPoint(1).PointTransf.gameObject.GetComponent<PathPoint>().setPointType(Point.PointType.TakeOff);
                path.GetPoint(1).PointTransf.Find("low poly sphere").gameObject.GetComponent<MeshRenderer>().material = homeNotSelectedMaterial;

            }
            else if (pointSelected.GetComponent<PathPoint>().Id == path.Count() - 1)
            {
                path.GetPoint(path.Count() - 2).PointTransf.gameObject.GetComponent<PathPoint>().setPointType(Point.PointType.Land);

                path.GetPoint(path.Count() - 2).PointTransf.Find("low poly sphere").gameObject.GetComponent<MeshRenderer>().material = landingNotSelectedMaterial;

            }
        }
        //Some fields from the cameras need to be updated
        frontRender.DeleteSphere(pointSelected);
        frontRender.setUpdate(true);
        topRender.setUpdate(true);
        pointSelected = null;
        eraseButton.SetActive(false);
        //If there is 1 waypoint or none, the panels need to be deactivated
        if (path.Count() < 2)
        {
            editWaypointButton.interactable = false;
            POIButton.interactable = false;
            editCurvesButton.interactable = false;
            previewButton.interactable = false;

        }
        //If there is only 1 waypoint, it will be a takeoff
        if (path.Count() == 1)
        {
            path.GetPoint(0).PointTransf.gameObject.GetComponent<PathPoint>().setPointType(Point.PointType.TakeOff);

            path.GetPoint(0).PointTransf.Find("low poly sphere").gameObject.GetComponent<MeshRenderer>().material = homeNotSelectedMaterial;
        }
    }
    //Scales the waypoint. This is because people kept telling us that the waypoints were too big when close and also when the camera was far
    void TransformSphere(Camera cam, GameObject esfera) {
        if (frontCam.orthographicSize > 15)
        {
            foreach (Transform child in sphereParent.transform)
            {
                child.GetChild(0).localScale = new Vector3(frontCam.orthographicSize / 8.0f * 25, frontCam.orthographicSize / 8.0f * 25, frontCam.orthographicSize / 8.0f * 25);
            }
        }
        else
        {
            foreach (Transform child in sphereParent.transform)
            {
                child.GetChild(0).localScale = new Vector3(frontCam.orthographicSize / 4.0f * 25, frontCam.orthographicSize / 4.0f * 25, frontCam.orthographicSize / 4.0f * 25);
            }
        }
    
    }
    /// <summary>
    /// Continuation from the function that throws rays. Calculates distance from a ray to a waypoint and determines if a waypoint is close enough to be hit
    /// </summary>
    /// <param name="ray"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public GameObject DistanceToLine(Ray ray,RaycastHit[] point)
    {
        float min = Mathf.Infinity;
        float distance = 0.0f;
        GameObject sphereSelected = null;

        for (int i = 0; i < point.Length; i++) { 
            distance = Vector3.Cross(ray.direction, point[i].transform.position - ray.origin).magnitude;
            if(min > distance){
                min = distance;
                sphereSelected = point[i].transform.gameObject;
            }
        }
        //Also only do this to waypoints
        if (sphereSelected != null && sphereSelected.transform.parent.tag == "Waypoint") { 
            sphereSelected = sphereSelected.transform.parent.gameObject;
            //We put the waypoint the material that determines that it has been selected or change its colour
            if (sphereSelected.GetComponent<PathPoint>().Id == 0)
            {
                sphereSelected.transform.GetChild(0).GetComponent<Renderer>().material = homeSelectedMaterial;
                sphereSelected.GetComponent<PathPoint>().setPointType(Point.PointType.TakeOff);

            }
            else if (sphereSelected.GetComponent<PathPoint>().Id != path.Count() - 1)
            {
               

                sphereSelected.transform.GetChild(0).GetComponent<Renderer>().material = waypointSelectedMaterial;

            }
            else
            {

                if (path.Count() > 2)
                {
                    path.GetPoint(path.Count() - 2).PointTransf.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = waypointNotSelectedMaterial;
                }
                sphereSelected.GetComponent<PathPoint>().setPointType(Point.PointType.Land);
                sphereSelected.transform.GetChild(0).GetComponent<Renderer>().material = landingSelectedMaterial;

            }
            //We change camera position to make the shader take effect. The shader that culls the objects that are too near in order to see the waypoints better.
            //frontCam.transform.position = new Vector3(sphereSelected.transform.position.x, sphereSelected.transform.position.y, sphereSelected.transform.position.z - 5);
            //frontCam.transform.LookAt(sphereSelected.transform.position);
        }

        return sphereSelected;
    }
    public static float NearestPointOnFiniteLineTop(Vector3 start,  Vector3 pnt)
    {

        return Vector2.Distance(new Vector2(start.x, start.y), new Vector2(pnt.x, pnt.y));
    }
    public static float NearestPointOnFiniteLineFront(Vector3 start, Vector3 pnt)
    {
        return Vector2.Distance(new Vector2(start.x, start.y), new Vector2(pnt.x, pnt.y));
    }
    //Function called when changing panels to edit POI or preview or any other
    /// <summary>
    ///Selects point 0 if there are none selected and returns the selected point
    /// </summary>
    /// <returns>Selected point</returns>
    public GameObject Deactivate()
    {
        if(!pointSelected)
            {
            
            pointSelected = path.GetPoint(0).PointTransf.gameObject;

            pointSelected.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = homeSelectedMaterial;
        }
        GameObject aux = pointSelected;
        pointSelected = null;
        return aux;
    }
}
