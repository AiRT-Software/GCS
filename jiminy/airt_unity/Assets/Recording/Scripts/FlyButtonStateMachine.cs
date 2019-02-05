using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
/// <summary>
/// This manages the states of the take off button in recording.
/// </summary>
public class FlyButtonStateMachine : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{

    ClientUnity clientUnity;

    public enum buttonState
    {
        DEACTIVATED = 0,
        ACTIVATED,
        BEINGPRESSED,
        FIVE_SEC_PRESSED
    }
    GameObject launchButtonGO;
    public static buttonState state = buttonState.DEACTIVATED;
    public GameObject recordingPanel;
    public GameObject topCam;
    public GameObject warningRecStart;
    float pressTime = 0.0f, buttonWidth = 0.0f;
    
    //This is the take off button
    RectTransform fillButton;

    // Use this for initialization
    void Start () {
        launchButtonGO = this.gameObject;
        buttonWidth = launchButtonGO.GetComponent<RectTransform>().sizeDelta.x;
        launchButtonGO.GetComponent<Image>().color = new Vector4(1.0f, 1.0f, 1.0f, 0.4f);
        launchButtonGO.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 0.4f);
        fillButton = transform.GetChild(1).GetComponent<RectTransform>();
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();

        //launchButtonGO.GetComponent<Button>()
    }

    // Update is called once per frame
    void Update () {

        switch (state)
        {
            case buttonState.DEACTIVATED:
                pressTime = 0.0f;
                break;
            //If a plan was loaded, we activate the button
            case buttonState.ACTIVATED:
                if (launchButtonGO.GetComponent<Image>().color.a < 0.5f)
                {
                    launchButtonGO.GetComponent<Image>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    launchButtonGO.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    //UnityEngine.Debug.Log("Activated");
                }
                fillButton.sizeDelta = new Vector2(0.0f, 0.0f);
                pressTime = 0.0f;
                break;
                //If the button is being pressed, it has to wait for 5 seconds for security reasons, and then we send a takeoff
            case buttonState.BEINGPRESSED:
                pressTime += Time.deltaTime;
                fillButton.sizeDelta = new Vector2((pressTime * buttonWidth) / 5.0f, 0.0f);
                if (pressTime >= 5.0f)
                {
                    clientUnity.client.SendCommand((byte)Modules.PLAN_EXECUTOR_MODULE, (byte)PlanExecutorCommandType.PLAN_EXEC_START);

                    state = buttonState.FIVE_SEC_PRESSED;
                }
                break;
            case buttonState.FIVE_SEC_PRESSED:
                //UnityEngine.Debug.Log("5 sec pressed");
                // A take off here will be necessary. The fc didn't work so we can't do it right now
                recordingPanel.SetActive(true);
                topCam.gameObject.GetComponent<PostRenderFront>().enabled = true;
                warningRecStart.SetActive(false);
                break;
            default:
                break;
        }

    }
    //these two functions determine if the button is being pressed or not. If the button stops being pressed before 5 seconds had passed, the button state resets
    public void OnPointerDown(PointerEventData eventData)
    {
        if(state == buttonState.ACTIVATED)
            state = buttonState.BEINGPRESSED;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (state == buttonState.BEINGPRESSED)
            state = buttonState.ACTIVATED;
    }
    
}
