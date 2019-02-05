using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using NetMQ;

public class ManageFPV : MonoBehaviour {

    public Image fpvRender, airtBackground;
    public Sprite alphaBackground, opaqueBackground;
    public Sprite openFPVImage, closeFPVImage;
    public Button toggleFPV;

    private ClientUnity clientUnity;
    public static bool start = false;
    public static bool stop = false;
    bool retry = true;

    bool cameraOpen = false;

    // ## TODO: QUITAR!!! Solo es para debug
    public Text FPVPeriod;

    public static int resX = 0, resY = 0;

    static QueueSync<byte[]> image_msg = new QueueSync<byte[]>(2);

    enum currentState
    {
        IDLE = 0,
        INIT,
        GETRESOLUTION,
        STARTED,
        STOPPED
    }
    currentState state = currentState.IDLE;
    void Awake()
    {
        // Logos size
        //airtLogo.sizeDelta = new Vector2(Screen.width/3.0f, Screen.height/6.0f);
        //partnerLogos.sizeDelta = new Vector2(Screen.width - 50.0f, Screen.height / 8.0f);
        Application.runInBackground = true;
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        if (fpvRender)
        {
            //we create a sprite on the first time always to have the resolution that the camera has and to have the texture clean
            fpvRender.sprite = Sprite.Create(new Texture2D(640, 480), new Rect(0, 0, 640, 480), new Vector2(0.5f, 0.5f));
        }
    }

    private void Update()
    {

        switch (state)
        {
            case currentState.IDLE:
                // Idle state

                break;

            case currentState.GETRESOLUTION:
                //No one is getting the reolution right now. It goes from idle to init
                if (retry)
                {
                    GetResolution();
                    StartCoroutine(WaitForResponse());
                    if (resX != 0)
                    {
                        state = currentState.INIT;
                    }
                    if (stop)
                        state = currentState.IDLE;
                }
                break;

            case currentState.INIT:

                if (retry)
                {
                    //The fpv is started, and if the drone receives the message, the texture begins to be displayed
                    startFPV();

                    StartCoroutine(WaitForResponse());
                    if (start) { 
                        state = currentState.STARTED;
                    }
                    if (stop)
                        state = currentState.IDLE;
                }
                break;

            case currentState.STARTED:
                if (FPVPeriod)
                    FPVPeriod.text = FPVModule.period.ToString();

                putTexture();

                if (stop)
                    state = currentState.IDLE;
                break;

            default:
                Application.Quit();
                break;

        };
    }
    private IEnumerator WaitForResponse()
    {
        retry = false;
        //string hola = WWW.EscapeURL("G:/tmp/out.jpeg");
        //hola = "File:///" + hola;
        //WWW aux = new WWW(hola);
        //yield return aux;
        //tex.LoadImage(aux.bytes);
        //tex.Apply();

        yield return new WaitForSeconds(1.0f);
        retry = true;
    }
    void startFPV()
    {
        if (clientUnity != null && clientUnity.client != null && clientUnity.client.isConnected)
        {
            UnityEngine.Debug.Log("Starting FPV");
            clientUnity.client.SendCommand((byte)Modules.FPV_MODULE, (byte)FPVCommandType.FPV_START_STREAMING);
            state = currentState.STARTED;
            //UnityEngine.Debug.Log("Version Get: " + AtreyuManager.majorV + "." + AtreyuManager.minorV + "." + AtreyuManager.patchV);
        }
    }

    void stopFPV()
    {
        if (clientUnity != null && clientUnity.client != null && clientUnity.client.isConnected)
        {
            UnityEngine.Debug.Log("Stopped FPV");
            clientUnity.client.SendCommand((byte)Modules.FPV_MODULE, (byte)FPVCommandType.FPV_STOP_STREAMING);
            state = currentState.IDLE;
            //UnityEngine.Debug.Log("Version Get: " + AtreyuManager.majorV + "." + AtreyuManager.minorV + "." + AtreyuManager.patchV);
        }
    }

    void GetResolution()
    {
        if (clientUnity != null && clientUnity.client != null && clientUnity.client.isConnected)
        {
            UnityEngine.Debug.Log("Getting resolution");
            clientUnity.client.SendCommand((byte)Modules.FPV_MODULE, (byte)FPVCommandType.FPV_GET_RESOLUTION);
            //UnityEngine.Debug.Log("Version Get: " + AtreyuManager.majorV + "." + AtreyuManager.minorV + "." + AtreyuManager.patchV);
        }
    }

    public void putTexture()
    {
        if (image_msg.GetSize() > 0)
        {
            byte[] array = image_msg.Dequeue();
            //if (!tex.mainTexture)
            //{
            //    tex.mainTexture = new Texture2D(resX, resY);
            //}
            //if (!materialForTexture.mainTexture)
            //{
            //    materialForTexture.mainTexture = new Texture2D(resX, resY);
            //}
            //Texture2D texAux = (Texture2D)tex.mainTexture;
            //Texture2D newtex = fpvRender.sprite.texture;

            //From the byte array that the fpv message returns, we create a image using unity loadimage method and then we always have to apply the changes made to the sprite
            fpvRender.sprite.texture.LoadImage(array);
            fpvRender.sprite.texture.Apply();
            //newtex.LoadImage(array);
            //newtex.Apply();
            //materialForTexture.mainTexture = newtex;


        }
    }

    public void getHeader(byte[] array)
    {

    }

    public void ToggleCamera()
    {
        //activates and deactivates the camera
        if (cameraOpen)
        {
            if (clientUnity != null && clientUnity.client != null && clientUnity.client.isConnected)
            {
                start = false;
                clientUnity.client.SendCommand((byte)Modules.FPV_MODULE, (byte)FPVCommandType.FPV_STOP_STREAMING);
                //UnityEngine.Debug.Log("Version Get: " + AtreyuManager.majorV + "." + AtreyuManager.minorV + "." + AtreyuManager.patchV);

                if (airtBackground != null)
                {
                    //If there is a background, we hide it and put the fpv texture. Recording is the only one that doesn't have one
                    //airtBackground.sprite = opaqueBackground;
                    airtBackground.gameObject.SetActive(true);
                }
                fpvRender.gameObject.SetActive(false);
                //toggleFPV.interactable = true;
                //closeFPV.interactable = false;
                toggleFPV.gameObject.GetComponent<Image>().sprite = openFPVImage;
                cameraOpen = false;
                //We are never in mapping anymore here
                if (GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.Mapping)
                {
                    GameObject.Find("Scan").GetComponent<Button>().interactable = true;
                    GameObject.Find("Scan").transform.GetChild(0).GetComponent<Text>().color = new Vector4(1, 1, 1, 1);
                }
            }
        }
        else
        {
            stop = false;
            state = currentState.INIT;
            if (airtBackground != null)
            {
                //airtBackground.sprite = alphaBackground;
                airtBackground.gameObject.SetActive(false);
            }
            fpvRender.gameObject.SetActive(true);
            //openFPV.interactable = false;
            //closeFPV.interactable = true;
            toggleFPV.gameObject.GetComponent<Image>().sprite = closeFPVImage;
            cameraOpen = true;

            if (GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.Mapping)
            {
                GameObject.Find("Scan").GetComponent<Button>().interactable = false;
                GameObject.Find("Scan").transform.GetChild(0).GetComponent<Text>().color = new Vector4(1, 1, 1, 0.4f);
            }
        }
    }
    //These 2 functions were substituted for the one above
    public void StartReceiving()
    {
        stop = false;
        state = currentState.INIT;
        if (airtBackground != null)
        {
            //airtBackground.sprite = alphaBackground;
            airtBackground.gameObject.SetActive(false);
        }
        fpvRender.gameObject.SetActive(true);
        //toggleFPV.interactable = false;
        //closeFPV.interactable = true;


    }

    public void StopReceiving()
    {
        if (clientUnity != null && clientUnity.client != null && clientUnity.client.isConnected)
        {
            start = false;
            clientUnity.client.SendCommand((byte)Modules.FPV_MODULE, (byte)FPVCommandType.FPV_STOP_STREAMING);
            //UnityEngine.Debug.Log("Version Get: " + AtreyuManager.majorV + "." + AtreyuManager.minorV + "." + AtreyuManager.patchV);

            if (airtBackground != null)
            {
                //airtBackground.sprite = opaqueBackground;
                airtBackground.gameObject.SetActive(true);
            }
            fpvRender.gameObject.SetActive(false);
            //toggleFPV.interactable = true;
            //closeFPV.interactable = false;
        }
    }

    //static int debug = 0;
    //The function to add the byte arrays of images from the fpv. Needs to be a public static because the message is received in a not monobehavour, while this scripts needs to be one
    public static void ImageDataReceived(NetMQMessage m)
    {
        //debug++;
        //UnityEngine.Debug.Log("FPV Message received! " + debug);
        image_msg.Enqueue(m[1].Buffer);
        //UnityEngine.Debug.Log("Img msg size: " + image_msg.GetSize());
    }
}
