using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using NetMQ;

public class StateMachine : MonoBehaviour {
    //This was a test, this class is not used
    public Image tex;
    public Material materialForTexture;
    public GameObject clientUnityGO;
    private ClientUnity clientUnity;
    public static bool start = false;
    public static bool stop = false;
    bool retry = true;

    public static int resX = 0, resY = 0;

    static QueueSync<byte[]> image_msg = new QueueSync<byte[]>(60);

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
        clientUnity = clientUnityGO.GetComponent<ClientUnity>();

        tex.sprite = Sprite.Create(new Texture2D(640, 480), new Rect(0, 0, 640, 480), new Vector2(0.5f, 0.5f));
    }

    private void Update()
    { 

        switch (state)
        {
            case currentState.IDLE:
                // Idle state

                break;

            case currentState.GETRESOLUTION:

                if (retry) {
                    GetResolution();
                    StartCoroutine(WaitForResponse());
                    if (resX != 0) { 
                        state = currentState.INIT;
                    }
                    if (stop)
                        state = currentState.IDLE;
                }
                break;

            case currentState.INIT:

                if (retry) { 
                    startFPV();

                    StartCoroutine(WaitForResponse());
                    if (start)
                        state = currentState.STARTED;
                    if (stop)
                        state = currentState.IDLE;
                }
                break;

            case currentState.STARTED:
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
            clientUnity.client.SendCommand((byte)Modules.FPV_MODULE, (byte)CommandType.START);
            state = currentState.STARTED;
            //UnityEngine.Debug.Log("Version Get: " + AtreyuManager.majorV + "." + AtreyuManager.minorV + "." + AtreyuManager.patchV);
        }
    }

    void stopFPV()
    {
        if (clientUnity != null && clientUnity.client != null && clientUnity.client.isConnected)
        {
            UnityEngine.Debug.Log("Stopped FPV");
            clientUnity.client.SendCommand((byte)Modules.FPV_MODULE, (byte)CommandType.STOP);
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
            Texture2D newtex = tex.sprite.texture;
            newtex.LoadImage(array);
            newtex.Apply();
            //materialForTexture.mainTexture = newtex;
             

        }
    }

    public void getHeader(byte[] array)
    {

    }

    public void StartReceiving()
    {
        stop = false;
        state = currentState.INIT;
    }

    public void StopReceiving()
    {
        if (clientUnity != null && clientUnity.client != null && clientUnity.client.isConnected)
        {
            start = false;
            clientUnity.client.SendCommand((byte)Modules.FPV_MODULE, (byte)CommandType.STOP);
            //UnityEngine.Debug.Log("Version Get: " + AtreyuManager.majorV + "." + AtreyuManager.minorV + "." + AtreyuManager.patchV);
        }
    }

    public static void ImageDataReceived(NetMQMessage m)
    {
        image_msg.Enqueue(m[1].Buffer);

    }
}
