using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    protected Transform t_cam;
    protected Transform t_parent;

    protected Vector3 local_rot;
    protected float cam_distance = 20f;  //starting distance

    public float mouse_factor = 8f;
    public float orbit_dampening = 7f;

    public float scroll_factor = 16f;
    public float scroll_dampening = 7f;

    public float touch_factor = 0.05f;
    public float touch_orbit_dampening = 2f;

    public float pinch_factor = 0.05f;
    public float pinch_dampening = 2f;

    private bool orbiting = false;
    private bool panning = false;
    private bool zooming = false;

    void Start()
    {
        this.t_cam = this.transform;
        this.t_parent = this.transform.parent;
    }

    void LateUpdate()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            orbiting = true;
        }
        if (orbiting && Input.GetMouseButtonUp(0))
        {
            orbiting = false;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            zooming = true;
        }
        if (zooming && scroll == 0)
        {
            zooming = false;
        }

        if (Input.GetMouseButtonDown(2))
        {
            orbiting = false;
            zooming = false;
            panning = true;
        }
        if (panning && Input.GetMouseButtonUp(2))
        {
            panning = false;
        }

        if (orbiting)
        {
            float x_axis = Input.GetAxis("Mouse X");
            float y_axis = Input.GetAxis("Mouse Y");
            if (x_axis != 0 || y_axis != 0)
            {
                local_rot.x += x_axis * mouse_factor;
                local_rot.y -= y_axis * mouse_factor;
                local_rot.y = Mathf.Clamp(local_rot.y, 0f, 90f);
            }
        }
        if (zooming)
        {
            float scroll_amount = scroll * scroll_factor;

            //faster when further away
            scroll_amount *= this.cam_distance * 0.3f;
            this.cam_distance += scroll_amount * -0.1f;
            this.cam_distance = Mathf.Clamp(this.cam_distance, 10f, 130f);
        }
        if (panning)
        {
            float x_axis = Input.GetAxis("Mouse X");
            float y_axis = Input.GetAxis("Mouse Y");

            this.t_parent.position -= this.t_parent.right * x_axis * scroll_factor * 0.5f * Time.deltaTime;
            this.t_parent.position -= this.t_parent.up * y_axis * scroll_factor * 0.5f * Time.deltaTime;
        }

        //update parent (pivot) rotation
        Quaternion qt = Quaternion.Euler(local_rot.y, local_rot.x, 0f);
        this.t_parent.rotation = Quaternion.Lerp(this.t_parent.rotation, qt, Time.deltaTime * orbit_dampening);

        //update position
        if (this.t_cam.localPosition.z != this.cam_distance * -0.1f)
        {
            this.t_cam.localPosition = new Vector3(
                0f,
                0f,
                Mathf.Lerp(this.t_cam.localPosition.z, this.cam_distance * -0.1f, Time.deltaTime * scroll_dampening));
        }

#elif UNITY_ANDROID
        if(Input.touchCount == 1)
        {
            orbiting = true;
        }
        if(orbiting && Input.touchCount != 1)
        {
            orbiting = false;
        }

        if(Input.touchCount == 2)
        {
            zooming = true;
        }
        if(zooming && Input.touchCount != 2)
        {
            zooming = false;
        }

        if (Input.touchCount == 3)
        {
            panning = true;
        }
        if (panning && Input.touchCount != 3)
        {
            orbiting = false;
            zooming = false;
            panning = false;
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
                local_rot.y = Mathf.Clamp(local_rot.y, 0f, 90f);
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

            //faster when further away
            pinch_amount *= this.cam_distance * 0.3f;
            this.cam_distance += pinch_amount * 0.1f;
            this.cam_distance = Mathf.Clamp(this.cam_distance, 10f, 130f);
        }

        if(panning)
        {
            Touch t2 = Input.touches[2];
            Vector2 t2_prev_pos = t2.position - t2.deltaPosition;

            float x_axis = t2.position.x - t2_prev_pos.x;
            float y_axis = t2.position.y - t2_prev_pos.y;

            if (x_axis != 0 || y_axis != 0)
            {
                this.t_parent.position -= this.t_parent.right * x_axis * 0.5f * Time.deltaTime;
                this.t_parent.position -= this.t_parent.up * y_axis * 0.5f * Time.deltaTime;
            }
        }

        //update parent (pivot) rotation
        Quaternion qt = Quaternion.Euler(local_rot.y, local_rot.x, 0f);
        this.t_parent.rotation = Quaternion.Lerp(this.t_parent.rotation, qt, Time.deltaTime * touch_orbit_dampening);

        //update position
        if (this.t_cam.localPosition.z != this.cam_distance * -0.1f)
        {
            this.t_cam.localPosition = new Vector3(
                0f,
                0f,
                Mathf.Lerp(this.t_cam.localPosition.z, this.cam_distance * -0.1f, Time.deltaTime * pinch_dampening));
        }
#endif
    }
}
