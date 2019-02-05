using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FCExecutor : MonoBehaviour {

    public InputField inputTakeOff;
    public GameObject overlay;
    ClientUnity clientUnity;
    private void Start()
    {
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
    }
    /// <summary>
    /// Closes the RC Panel
    /// </summary>
    public void close()
    {
        this.gameObject.SetActive(false);
    }
    /// <summary>
    /// Arms the drone
    /// </summary>
    public void Arm()
    {
        if (clientUnity.client.isConnected)
        {
            clientUnity.client.SendCommand((byte)Modules.FCS_MULTIPLEXER_MODULE, (byte)FCSMultiplexerCommandType.FCS_ARM);
        }
    }

    void Update()
    {
        //To check if the app is still connected to the drone
        if (overlay != null && clientUnity != null && clientUnity.client != null && clientUnity.client.isConnected)
            overlay.SetActive(false);
        else if (overlay)
            overlay.SetActive(true);
    }
    /// <summary>
    /// Sends a takeoff with the value of the input field
    /// </summary>
    public void TakeOff()
    {

        int height;
        if (!int.TryParse(inputTakeOff.text, out height))
        {
            return;
        }

        clientUnity.client.sendTakeOffCommand((byte)Modules.FCS_MULTIPLEXER_MODULE, (byte)FCSMultiplexerCommandType.FCS_TAKEOFF, height);

    }
    /// <summary>
    /// Sends a loiter
    /// </summary>
    public void Loiter()
    {
        clientUnity.client.sendCommand((byte)Modules.FCS_MULTIPLEXER_MODULE, (byte)FCSMultiplexerCommandType.FCS_SETMODE, (byte)FCSFlightModes.MODE_LOITER);

    }
    /// <summary>
    /// Puts the drone on auto
    /// </summary>
    public void Auto()
    {
        clientUnity.client.sendCommand((byte)Modules.FCS_MULTIPLEXER_MODULE, (byte)FCSMultiplexerCommandType.FCS_SETMODE, (byte)FCSFlightModes.MODE_AUTO);

    }
    /// <summary>
    /// Sends a land command
    /// </summary>
    public void Land()
    {
        clientUnity.client.SendCommand((byte)Modules.FCS_MULTIPLEXER_MODULE, (byte)FCSMultiplexerCommandType.FCS_LAND);

    }
    /// <summary>
    /// Clears any uploaded mission
    /// </summary>
    public void ClearMission()
    {
        clientUnity.client.SendCommand((byte)Modules.FCS_MULTIPLEXER_MODULE, (byte)FCSMultiplexerCommandType.FCS_CLEARALL);

    }
    /// <summary>
    /// Disarms the drone
    /// </summary>
    public void Disarm()
    {
        clientUnity.client.SendCommand((byte)Modules.FCS_MULTIPLEXER_MODULE, (byte)FCSMultiplexerCommandType.FCS_DISARM);

    }
}
