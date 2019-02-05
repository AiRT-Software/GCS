using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IndividualIrisButton : MonoBehaviour {

    public GameObject irisScreen;
    public Button irisButtonToGiveValues;
    public bool isInPlanning = false;
    private void Start()
    {
        //There are at least more than 40 iris buttons. This is the fastest way to assign them a function
        this.GetComponent<Button>().onClick.AddListener(buttonPressed);
    }
    public void buttonPressed()
    {
        ClientUnity clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();

        byte i = 0;
        //If the button clicked is the same as the one that has the script, the iris parameter is sent to the drone
        foreach (Transform item in irisScreen.transform)
        {
            
            if (this.name.Equals(item.name))
            {
                irisButtonToGiveValues.transform.GetChild(0).GetComponent<Text>().text = this.transform.GetChild(0).GetComponent<Text>().text;
                if (!isInPlanning)
                {
                    clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_IRIS, i);
                }
                else
                {
                    //This is in planning, which assigns the waypoint this iris aperture
                    ChangePointType.pointSelected.GetComponent<PathPoint>().Rc.irisAperture = i;

                }
                irisScreen.SetActive(false);
            }
            i++;
            if (item.name.Equals( "F20"))
            {
                //In order to fit them on the screen, I had to remove one
                i++;
            }
        }
    }
}
