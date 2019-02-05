using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This class manages the ortographics cameras
/// </summary>
public class PostRenderFront : MonoBehaviour
{
    public Material mat;

    public GameObject sphereParent;
    public GameObject esfera;

    private Path path;

    public Material pointMaterial;



    Matrix4x4 m = Matrix4x4.identity;
    Camera cam;
    Vector3 lastPanPosition;

    List<Vector3> auxVert = new List<Vector3>();
    bool first = true;
    //If the array list with the points of the curve needs to be updated, this will be true
    bool update = false;
    bool right = false;
    void Awake()
    {

        cam = GetComponent<Camera>();
        //We check if the camera is the front camera or the top camera
        if (cam.pixelRect.position.x >= 0.5)
            right = true;
        if (GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.Recording)
        {
            right = true;
        }


        // Singleton del Path (array de posicion de esferas)
        path = Path.Instance;

       

    }
    //Actiavte the culling shader
    private void FindShader(string shaderName)
    {
        int count = 0;

        List<Material> armat = new List<Material>();

        Renderer[] arrend = (Renderer[])Resources.FindObjectsOfTypeAll(typeof(Renderer));
        foreach (Renderer rend in arrend)
        {
            foreach (Material mats in rend.sharedMaterials)
            {
                if (!armat.Contains(mats))
                {
                    armat.Add(mats);
                }
            }
        }

        foreach (Material mats in armat)
        {
            if (mats != null && mats.shader != null && mats.shader.name != null && mats.shader.name == shaderName)
            {
                mats.SetFloat("_Discard", 0);
                //if (mats.GetTexture("_MainTex") == null)
                //{
                //    mats.SetTexture("_MainTex", mat.GetTexture("_Albedo"));
                //}
            }
        }


    }

    void OnPreRender()
    {
        //Activates the culling shader only on the front camera, not top
        if (right)
        {
            pointMaterial.SetFloat("_Discard", 1);

        }
        else
        {
            pointMaterial.SetFloat("_Discard", 0);

        }




    }
    /// <summary>
    /// Sets the boolean necessary to update the points of the curve
    /// </summary>
    /// <param name="activate"></param>
    public void setUpdate(bool activate)
    {
        update = activate;
    }

   /// <summary>
   /// Deletes a waypoint a clears the curve array
   /// </summary>
   /// <param name="sphere"></param>
    public void DeleteSphere(GameObject sphere)
    {
        path.DeletePoint(sphere.GetComponent<PathPoint>().Id);
        Destroy(sphere);

        update = true;
        path.middlePointsTop.Clear();
        path.middlePointsRight.Clear();

    }
    /// <summary>
    /// Only for pc, to move the camera and scale the waypoints
    /// </summary>
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
                    child.localScale = new Vector3(cam.orthographicSize / 8.0f, cam.orthographicSize / 8.0f, cam.orthographicSize / 8.0f);
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
                    child.localScale = new Vector3(cam.orthographicSize / 8.0f, cam.orthographicSize / 8.0f, cam.orthographicSize / 8.0f);
                }
            }
        }
    }
    
    void OnPostRender()
    {
        //This draws the curve
        if (path.Count() > 1)
        {
            GL.Clear(true, false, Color.white, 1);
            //GL.PushMatrix();
            //A material needs to be assigned before drawing with GL
            mat.SetPass(0);


            //GL.LoadIdentity();

            //m = Matrix4x4.Translate(new Vector3(0, 0, 0) ) ;

            //GL.MultMatrix(m * cam.worldToCameraMatrix  );            

            GL.Color(Color.red);

            for (int i = 0; i < path.Count() - 1; i++)
            {
                //Depending on the camera we call one function or another
                if (right)
                {
                    CatmullRomSpline.DisplayCatmullRomSpline(path, i, ref path.middlePointsRight, cam.orthographicSize / 2.0f, update, cam);


                }
                else
                {
                    CatmullRomSpline.DisplayCatmullRomSpline2(path, i, ref path.middlePointsTop, cam.orthographicSize / 2.0f, update, cam);
                }
            }
            //cam.Render();
            update = false;
            //GL.PopMatrix();
        }
    }
    /// <summary>
    /// Returns the path
    /// </summary>
    /// <returns></returns>
    public Path GetPath()
    {
        return path;
    }
}