using NetMQ;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SendJSONStateMachine : MonoBehaviour {

    //public GameObject camFront;
    //public GameObject Canvas;

    //public RectTransform doneButton, throbberImage;
    //public Text doneText, doneButtonText;

    ClientUnity clientUnity;
    LibraryModule lm;

    public static SendJSonStates state = SendJSonStates.IDLE;

    bool retry = true;

    public static bool pathsSend = false;

    public static bool allFilesSent = false;

    public static string library_path = "";

    public enum SendJSonStates
    {
        IDLE = 0,
        SENDPATHREQUEST,
        RECEIVEPATHREQUEST,
        RECEIVEDATAFILESRESPONSE,
        SENDRECORDINGSIGNAL
    } 

	// Use this for initialization
	void Start () {

        Application.runInBackground = true;
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();

        //doneText.fontSize = Screen.width / 80;
        //doneButtonText.fontSize = Screen.width / 65;
        //doneText.text = "Press done\nto proceed";

    }
    /// <summary>
    /// This was called by a done button in planning. Is not used anymore 
    /// </summary>
	public void BeginSending()
    {
        //doneButton.GetComponent<Button>().interactable = false;
        //doneButton.GetChild(1).GetComponent<Text>().color = new Vector4(1, 1, 1, 0.4f);

        MainPlanningUI.SavePath();
        if(clientUnity.client.isConnected)
            state = SendJSonStates.SENDPATHREQUEST;

    }

	// Update is called once per frame
    // In model alignment, first we will consult if the server doesn't have a metadata that we have, and if it doesn't have it, this script state will change to SENDPATHREQUEST and start uploading everything
	void Update () {
        switch (state)
        {
            case SendJSonStates.IDLE:
                //throbberImage.gameObject.SetActive(false);
                //doneButton.GetComponent<Button>().interactable = true;
                //doneButton.GetChild(1).GetComponent<Text>().color = new Vector4(1, 1, 1, 1);
                //doneText.text = "Press done\nto proceed";
                break;
            case SendJSonStates.SENDPATHREQUEST:
                if (retry)
                {
                    //doneText.text = "Sending map\nto server";
                    //throbberImage.gameObject.SetActive(true);
                    SendPathRequest();
                    StartCoroutine(WaitForResponse());
                }
                //throbberImage.Rotate(0, 0, -180 * Time.deltaTime);
                break;
            case SendJSonStates.RECEIVEPATHREQUEST:
                if (pathsSend)
                {
                    state = SendJSonStates.RECEIVEDATAFILESRESPONSE;
                }
                //throbberImage.Rotate(0, 0, -180 * Time.deltaTime);
                break;
            case SendJSonStates.RECEIVEDATAFILESRESPONSE:
                if (allFilesSent)
                {
                    SendRecordingCommand();
                }
                //throbberImage.Rotate(0, 0, -180 * Time.deltaTime);
                break;
            default:
                break;
        }
    }
    private IEnumerator WaitForResponse()
    {
        retry = false;
        

        yield return new WaitForSeconds(1.0f);
        retry = true;
    }
    /// <summary>
    /// Requests the paths on the server before uploading the files
    /// </summary>
    void SendPathRequest()
    {
        if (clientUnity != null && clientUnity.client != null && clientUnity.client.isConnected)
        {
            UnityEngine.Debug.Log("Sending map metadata");

            clientUnity.client.SendCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_LIBRARY_PATH);
            //UnityEngine.Debug.Log("Version Get: " + AtreyuManager.majorV + "." + AtreyuManager.minorV + "." + AtreyuManager.patchV);
            state = SendJSonStates.RECEIVEPATHREQUEST;
        }
    }
    
    public static void pathReceived(NetMQMessage m)
    {
        //path = BitConverter.ToString(m[1].Buffer);
    }
    /// <summary>
    /// JUmps to recording
    /// </summary>
    public void SendRecordingCommand()
    {
        state = SendJSonStates.IDLE;
        UnityEngine.Debug.Log("Load recording scene");
        //DontDestroyOnLoad(GameObject.Find("Daemodel"));
        UnityEngine.SceneManagement.SceneManager.LoadScene("Recording"); // ## TODO: 
    }
}
