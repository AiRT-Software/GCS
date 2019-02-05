using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This class got fused with renderfront
/// </summary>
public class PostRenderTop : MonoBehaviour {

    public Material mat;

    public GameObject sphereParent;
    public GameObject esfera;

    //private List<Transform> path = new List<Transform>();
    private Path path;

    private bool update = false;

    Matrix4x4 m = Matrix4x4.identity;
    Camera cam;
    Vector3 lastPanPosition;

    List<Vector3> auxVert = new List<Vector3>();
    bool first = true;

    void Awake()
    {

        cam = GetComponent<Camera>();

        // Singleton del Path (array de posicion de esferas)
        path = Path.Instance;

    }

    /*public GameObject DrawSphere(Vector2 pos)
    {
        esfera.SetActive(true);
        GameObject aux = Instantiate(esfera, cam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, cam.transform.position.y - 1)), Quaternion.identity, sphereParent.transform);
        esfera.SetActive(false);
        aux.AddComponent<PathPoint>();
        aux.GetComponent<PathPoint>().createPathPoint(aux.transform);

        path.AddPoint(aux.GetComponent<PathPoint>());

        update = true;
        path.middlePoints.Clear();

        return aux;
    }

    public GameObject DrawSphereInTheMiddle(Vector3 pos, int id)
    {
        esfera.SetActive(true);
        GameObject aux = Instantiate(esfera, new Vector3(pos.x, pos.y, pos.z), Quaternion.identity, sphereParent.transform);
        esfera.SetActive(false);
        aux.AddComponent<PathPoint>();
        aux.GetComponent<PathPoint>().createPathPoint(aux.transform);

        path.AddPointatId(aux.GetComponent<PathPoint>(), id);

        update = true;
        path.middlePoints.Clear();

        return aux;
    }

    public void MoveSphere(Vector2 pos, GameObject sphere)
    {
        Vector3 aux = cam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, 1 + cam.transform.position.y));
        sphere.transform.position = new Vector3(aux.x, sphere.transform.position.y, aux.z);

        update = true;
        path.middlePoints.Clear();
    }

    public void MoveCamera( Vector2 pos)
    {
        //Debug.Log("CamViewportToWorldPoint: " + cam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, 0));
        //Vector3 aux = cam.transform.position - cam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, 0));

        //cam.transform.position += new Vector3(aux.x, 0.0f, aux.z) * Time.deltaTime;

        Vector2 move = new Vector2((-pos.x / cam.pixelWidth) * cam.aspect, -pos.y / cam.pixelHeight);
        cam.transform.Translate(move.x * cam.orthographicSize * 2, move.y * cam.orthographicSize * 2, 0);

    }

    void Update()
    {

        // ## Debug para mover la camara
        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.position -= transform.right;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            transform.position += transform.right;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            transform.position += transform.up;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            transform.position -= transform.up;
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            cam.orthographicSize++;
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
                    child.localScale = new Vector3(cam.orthographicSize / 5.0f, cam.orthographicSize / 5.0f, cam.orthographicSize / 5.0f);
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.E) && cam.orthographicSize > 1)
        {
            cam.orthographicSize--;
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
                    child.localScale = new Vector3(cam.orthographicSize / 5.0f, cam.orthographicSize / 5.0f, cam.orthographicSize / 5.0f);
                }
            }
        }


    }
    
    void OnPostRender()
    {
        if (!mat)
        {
            UnityEngine.Debug.LogError("Please Assign a material on the inspector");
            return;
        }

        if (path.Count() > 1)
        {
            //GL.PushMatrix();
            mat.SetPass(0);
            //GL.LoadOrtho();

            GL.Color(new Color32(254, 161, 0, 255););
            for (int i = 0; i < path.Count() - 1; i++)
            {
                CatmullRomSpline.DisplayCatmullRomSpline2(path, i, ref path.middlePoints, cam.orthographicSize, update);
            }
            //GL.PopMatrix();

            update = false;


        }
    }

    public Path GetPath()
    {
        return path;
    }*/
}
