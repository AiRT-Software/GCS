using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GimbalControl : MonoBehaviour {

    ClientUnity clientUnity;
    int i = 0;

    public GameObject pitchContainer, rollContainer, yawContainer, speedContainer, sendButton;
    public GameObject aboutPanel;
    public Toggle continuousToggle;
    public GameObject overlay;

    float lastTime = 0.0f, actualTime = 0.0f;
    float clampTime = 0.3f;

    float pitchValue = 0.0f, rollValue = 0.0f, yawValue = 0.0f, speedValue = 1.0f;

    void Awake(){
        //Adds listener on the start, just in case a change is produced because any change on the slider, even if it is by code will call inmediately the listener.
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        pitchContainer.transform.GetChild(1).GetComponent<Slider>().onValueChanged.AddListener(PitchValueChanged);
        rollContainer.transform.GetChild(1).GetComponent<Slider>().onValueChanged.AddListener(RollValueChanged);
        yawContainer.transform.GetChild(1).GetComponent<Slider>().onValueChanged.AddListener(YawValueChanged);
        speedContainer.transform.GetChild(1).GetComponent<Slider>().onValueChanged.AddListener(SpeedValueChanged);
    }
    void Update()
    {
        if (overlay != null && clientUnity != null && clientUnity.client != null && clientUnity.client.isConnected)
            overlay.SetActive(false);
        else if (overlay)
            overlay.SetActive(true);
    }        
    //Sends the angle to the gimball on the pitch axis
    public void PitchValueChanged(float pitch)
    {
        pitchValue = (pitch * Mathf.PI / 180.0f);
        if (continuousToggle.isOn && ((Time.time - lastTime) > clampTime))
        {
            lastTime = Time.time;
            GimbalAngle ga = new GimbalAngle(pitchValue, rollValue, yawValue, speedValue);
            clientUnity.client.sendCommand((byte)Modules.GIMBAL_MULTIPLEXER_MODULE, (byte)GimbalMultiplexerCommandType.GIMBAL_GOTO_ANGLE, ga);
            //UnityEngine.Debug.Log("Changed! " + i++);
        }

    }
    //Sends the angle to the gimball on the roll axis

    public void RollValueChanged(float roll)
    {
        rollValue = (roll * Mathf.PI / 180.0f);
        if (continuousToggle.isOn && ((Time.time - lastTime) > clampTime))
        {
            lastTime = Time.time;
            GimbalAngle ga = new GimbalAngle(pitchValue, rollValue, yawValue, speedValue);
            clientUnity.client.sendCommand((byte)Modules.GIMBAL_MULTIPLEXER_MODULE, (byte)GimbalMultiplexerCommandType.GIMBAL_GOTO_ANGLE, ga);
            //UnityEngine.Debug.Log("Changed! " + i++);
        }
    }
    //Sends the angle to the gimball on the yaw axis

    public void YawValueChanged(float yaw)
    {
        yawValue = (yaw * Mathf.PI / 180.0f);
        if (continuousToggle.isOn && ((Time.time - lastTime) > clampTime))
        {
            lastTime = Time.time;
            GimbalAngle ga = new GimbalAngle(pitchValue, rollValue, yawValue, speedValue);
            clientUnity.client.sendCommand((byte)Modules.GIMBAL_MULTIPLEXER_MODULE, (byte)GimbalMultiplexerCommandType.GIMBAL_GOTO_ANGLE, ga);
            //UnityEngine.Debug.Log("Changed! " + i++);
        }
    }
    //Changes the speed at which the gimball rotates

    public void SpeedValueChanged(float speed)
    {
        speedValue = (speed * Mathf.PI / 180.0f);
        if (continuousToggle.isOn && ((Time.time - lastTime) > clampTime))
        {
            lastTime = Time.time;
            GimbalAngle ga = new GimbalAngle(pitchValue, rollValue, yawValue, speedValue);
            clientUnity.client.sendCommand((byte)Modules.GIMBAL_MULTIPLEXER_MODULE, (byte)GimbalMultiplexerCommandType.GIMBAL_GOTO_ANGLE, ga);
            //UnityEngine.Debug.Log("Changed! " + i++);
        }
    }
    //Rotates the gimball at a speed and with a roll, yaw and pitch

    public void SendGimbalInfo()
    {
        UnityEngine.Debug.Log("Pitch: " + pitchValue);
        UnityEngine.Debug.Log("Roll: " + rollValue);
        UnityEngine.Debug.Log("Yaw: " + yawValue);
        UnityEngine.Debug.Log("Speed: " + speedValue);
        GimbalAngle ga = new GimbalAngle(pitchValue, rollValue, yawValue, speedValue);
        //UnityEngine.Debug.Log("Send");
        clientUnity.client.sendCommand((byte)Modules.GIMBAL_MULTIPLEXER_MODULE, (byte)GimbalMultiplexerCommandType.GIMBAL_GOTO_ANGLE, ga);
    }

    public void CloseGimbalConfig()
    {
        this.gameObject.SetActive(false);
    }

    public void ResetValues()
    {
        pitchContainer.transform.GetChild(1).GetComponent<Slider>().value = 0.0f;
        rollContainer.transform.GetChild(1).GetComponent<Slider>().value = 0.0f;
        yawContainer.transform.GetChild(1).GetComponent<Slider>().value = 0.0f;
        speedContainer.transform.GetChild(1).GetComponent<Slider>().value = 1.0f;
    }

}

public class GimbalAngle
{
    public float gimbalPitch, gimbalRoll, gimbalYaw, gimbalSpeed;

    public GimbalAngle()
    {
        gimbalPitch = 0;
        gimbalRoll = 0;
        gimbalYaw = 0;
        gimbalSpeed = 1;

    }

    public GimbalAngle(float gimbalPitch_, float gimbalRoll_, float gimbalYaw_, float gimbalSpeed_)
    {
        gimbalPitch = gimbalPitch_;
        gimbalRoll = gimbalRoll_;
        gimbalYaw = gimbalYaw_;
        gimbalSpeed = gimbalSpeed_;

    }
}