using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    protected Transform t_cam;

    protected Vector3 local_rot;
    protected float cam_distance = 475;  //starting distance

    public float mouse_factor = 20f;
    public float orbit_dampening = 16f;

    public float scroll_factor = 16f;
    public float pan_factor = 160f;
    public float scroll_dampening = 7f;

    public float touch_factor = 0.05f;
    public float touch_orbit_dampening = 2f;

    public float pinch_factor = 0.05f;
    public float pinch_dampening = 2f;

    public float maxRot = 85.0f;
    public float minRot = -85.0f;

    public float movementFactor = 50;

    public bool allowOrbit = true;
    public bool allowPanning = true;
    public bool allowZoom = true;
    public bool allowMovement = false;

    private bool orbiting = false;
    private bool panning = false;
    private bool zooming = false;
    private bool moving = false;

    public RectTransform avoidRect;
    Vector2 localCursor;
    //Do not use orbitcamera v1 o v2, they are the previous versions. This one is the most recent one
	// Use this for initialization
	void Start () {
        this.t_cam = this.transform;
        local_rot = new Vector3(t_cam.eulerAngles.y, t_cam.eulerAngles.x, 0.0f);
        //local_rot = t_cam.transform.rotation;
	}
	
	// Update is called once per frame
	void LateUpdate () {

        Vector3 gimbalCamPoint = Camera.main.ViewportToScreenPoint(new Vector3(GetComponent<Camera>().rect.x, GetComponent<Camera>().rect.y, 0));
        Vector3 gimbalCamPoint2 = Camera.main.ViewportToScreenPoint(new Vector3(GetComponent<Camera>().rect.xMax, GetComponent<Camera>().rect.yMax, 0));

        var gimbalCamrect = new Rect(gimbalCamPoint.x, gimbalCamPoint.y, gimbalCamPoint2.x - gimbalCamPoint.x, gimbalCamPoint2.y - gimbalCamPoint.y);
        if (!gimbalCamrect.Contains(Input.mousePosition))
        {
            orbiting = false;
            zooming = false;
            panning = false;
            moving = false;
            return;
        }
        //First we check if orbit, zoom, panning, or zoom moving the camera are enabled, and if the mouse is inside the camera that contains the script
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0) && allowOrbit)
        {
            if (avoidRect && RectTransformUtility.RectangleContainsScreenPoint(avoidRect, Input.mousePosition))
            {
                UnityEngine.Debug.Log("Scroll Panel");
                return;
            }
            orbiting = true;
        }
        if (orbiting && Input.GetMouseButtonUp(0) && allowOrbit)
        {
            orbiting = false;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (avoidRect && RectTransformUtility.RectangleContainsScreenPoint(avoidRect, Input.mousePosition))
            {
                return;
            }
            if (allowMovement)                
                moving = true;
            else
            {
                moving = false;
                if (allowZoom)
                    zooming = true;
                else
                    zooming = false;
            }
        }
        if (moving && scroll == 0 && allowMovement)
        {
            moving = false;
        }
        if (zooming && scroll == 0 && allowZoom)
        {
            zooming = false;
        }

        if (Input.GetMouseButtonDown(2) && allowPanning)
        {
            if (avoidRect && RectTransformUtility.RectangleContainsScreenPoint(avoidRect, Input.mousePosition))
            {
                UnityEngine.Debug.Log("Scroll Panel");
                return;
            }
            orbiting = false;
            zooming = false;
            panning = true;
        }
        if (panning && Input.GetMouseButtonUp(2) && allowPanning)
        {
            panning = false;
        }

        if (orbiting)
        {
            //This rotates de camera
            float x_axis = Input.GetAxis("Mouse X");
            float y_axis = Input.GetAxis("Mouse Y");
            if (x_axis != 0 || y_axis != 0)
            {
                local_rot.x += x_axis * mouse_factor;
                local_rot.y -= y_axis * mouse_factor;
                local_rot.y = Mathf.Clamp(local_rot.y, minRot, maxRot);
            }
        }
        if (zooming)
        {
            //If the camera needs a zoom modifying the fov, this is done here


            //float scroll_amount = scroll * scroll_factor;
            //this.t_cam.position += this.t_cam.forward * scroll_amount;
            //faster when further away
            //scroll_amount *= this.cam_distance * 0.3f;
            //this.cam_distance += scroll_amount * -0.1f;
            //this.cam_distance = Mathf.Clamp(this.cam_distance, 10f, 130f);
            float fov =this.GetComponent<Camera>().fieldOfView;
            fov -= Input.GetAxis("Mouse ScrollWheel") * scroll_factor;
            fov = Mathf.Clamp(fov, 1, 90);
            this.GetComponent<Camera>().fieldOfView = fov;


        }
        if (panning)
        {
            //This moves the camera in x and y
            float x_axis = Input.GetAxis("Mouse X");
            float y_axis = Input.GetAxis("Mouse Y");

            this.t_cam.position -= this.t_cam.right * x_axis * pan_factor * 0.5f * Time.deltaTime;
            this.t_cam.position -= this.t_cam.up * y_axis * pan_factor * 0.5f * Time.deltaTime;
        }
        if (moving)
        {
            //Zooms moving the camera in z
            this.t_cam.position += this.t_cam.forward * movementFactor * Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime;
        }
        //assigns rotation to the camera interpolating it with the actual rotation to make it smooth
        //update parent (pivot) rotation
        Quaternion qt = Quaternion.Euler(local_rot.y, local_rot.x, 0f);
        this.t_cam.rotation = Quaternion.Lerp(this.t_cam.rotation, qt, Time.deltaTime * orbit_dampening);
        //update position
        //if (this.t_cam.localPosition.z != this.cam_distance * -0.1f)
        //{
        //    this.t_cam.localPosition = new Vector3(
        //        this.t_cam.localPosition.x,
        //        this.t_cam.localPosition.y,
        //        Mathf.Lerp(this.t_cam.localPosition.z, this.cam_distance, Time.deltaTime * scroll_dampening));
        //}
        //And now for android
#elif UNITY_ANDROID

        if(allowOrbit && Input.touchCount == 1)
        {
            if (avoidRect && RectTransformUtility.RectangleContainsScreenPoint(avoidRect, Input.GetTouch(0).position))
            {
                UnityEngine.Debug.Log("Scroll Panel");
                return;
            }
            orbiting = true;
        }
        if(orbiting && Input.touchCount != 1)
        {
            orbiting = false;
        }

        if(allowZoom && Input.touchCount == 2)
        {
            if (avoidRect && (RectTransformUtility.RectangleContainsScreenPoint(avoidRect, Input.GetTouch(0).position) || RectTransformUtility.RectangleContainsScreenPoint(avoidRect, Input.GetTouch(1).position)))
            {
                UnityEngine.Debug.Log("Scroll Panel");
                return;
            }
            zooming = true;
        }
        if(zooming && Input.touchCount != 2)
        {
            zooming = false;
        }

        if (allowPanning && Input.touchCount == 3)
        {
            if (avoidRect && (RectTransformUtility.RectangleContainsScreenPoint(avoidRect, Input.GetTouch(0).position) || RectTransformUtility.RectangleContainsScreenPoint(avoidRect, Input.GetTouch(1).position) || RectTransformUtility.RectangleContainsScreenPoint(avoidRect, Input.GetTouch(2).position)))
            {
                UnityEngine.Debug.Log("Scroll Panel");
                return;
            }
            panning = true;
        }
        if (panning && Input.touchCount != 3)
        {
            orbiting = false;
            zooming = false;
            panning = false;
        }
        if (allowMovement && Input.touchCount == 2)
        {
            if (avoidRect && (RectTransformUtility.RectangleContainsScreenPoint(avoidRect, Input.GetTouch(0).position) || RectTransformUtility.RectangleContainsScreenPoint(avoidRect, Input.GetTouch(1).position)))
            {
                UnityEngine.Debug.Log("Scroll Panel");
                return;
            }
            moving = true;
        }
        if (moving && Input.touchCount != 2)
        {
            moving = false;
        }

        if (orbiting)
        {
            Touch t0 = Input.touches[0];
            Vector2 t0_prev_pos = t0.position - t0.deltaPosition;

            float x_axis = t0.position.x - t0_prev_pos.x;
            float y_axis = t0.position.y - t0_prev_pos.y;

            if (x_axis != 0 || y_axis != 0)
            {
                local_rot.x += x_axis * touch_factor;
                local_rot.y -= y_axis * touch_factor;
                local_rot.y = Mathf.Clamp(local_rot.y, minRot, maxRot);
            }
        }

        if (zooming)
        {
            Touch t0 = Input.touches[0];
            Touch t1 = Input.touches[1];

            //positions in the previous frame
            Vector2 t0_prev_pos = t0.position - t0.deltaPosition;
            Vector2 t1_prev_pos = t1.position - t1.deltaPosition;

            //vector-magnitude (the distance between the touches) in the previous frame
            float magnitude_prev = (t0_prev_pos - t1_prev_pos).magnitude;

            //current vector-magnitude
            float magnitude_curr = (t0.position - t1.position).magnitude;

            //difference
            float pinch_amount = magnitude_prev - magnitude_curr;
            pinch_amount *= pinch_factor;

            //this.t_cam.position += this.t_cam.forward * pinch_amount;
            
            //faster when further away
            //pinch_amount *= this.cam_distance * 0.3f;
            //this.cam_distance += pinch_amount * 0.1f;
            //this.cam_distance = Mathf.Clamp(this.cam_distance, 10f, 130f);


            float fov =this.GetComponent<Camera>().fieldOfView;
            fov += pinch_amount * scroll_factor;
            fov = Mathf.Clamp(fov, 1, 90);
            this.GetComponent<Camera>().fieldOfView = fov;
        }

        if (panning)
        {
            Touch t2 = Input.touches[2];
            Vector2 t2_prev_pos = t2.position - t2.deltaPosition;

            float x_axis = t2.position.x - t2_prev_pos.x;
            float y_axis = t2.position.y - t2_prev_pos.y;

            if (x_axis != 0 || y_axis != 0)
            {
                this.t_cam.position -= this.t_cam.right * x_axis * 0.5f * Time.deltaTime;
                this.t_cam.position -= this.t_cam.up * y_axis * 0.5f * Time.deltaTime;
            }
        }
        if (moving)
        {
            Touch t0 = Input.touches[0];
            Touch t1 = Input.touches[1];

            //positions in the previous frame
            Vector2 t0_prev_pos = t0.position - t0.deltaPosition;
            Vector2 t1_prev_pos = t1.position - t1.deltaPosition;

            //vector-magnitude (the distance between the touches) in the previous frame
            float magnitude_prev = (t0_prev_pos - t1_prev_pos).magnitude;

            //current vector-magnitude
            float magnitude_curr = (t0.position - t1.position).magnitude;

            //difference
            float pinch_amount = magnitude_curr - magnitude_prev ;
            pinch_amount *= pinch_factor;

            this.t_cam.position += this.t_cam.forward * movementFactor * pinch_amount * Time.deltaTime;
        }

        //update parent (pivot) rotation
        Quaternion qt = Quaternion.Euler(local_rot.y, local_rot.x, 0f);
        this.t_cam.rotation = Quaternion.Lerp(this.t_cam.rotation, qt, Time.deltaTime * touch_orbit_dampening);

        //update position
        //if (this.t_cam.localPosition.z != this.cam_distance * -0.1f)
        //{
        //    this.t_cam.localPosition = new Vector3(
        //        0f,
        //        0f,
        //        Mathf.Lerp(this.t_cam.localPosition.z, this.cam_distance * -0.1f, Time.deltaTime * pinch_dampening));
        //}
#endif
    }

}
