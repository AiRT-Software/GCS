using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IndividualIsoParameters : MonoBehaviour {

    public GameObject isoPanel;
    public GameObject isoButtonToAssign;
    public bool isInPlanning = false;
    private void Start()
    {
        //There are at least more than 20 ISO buttons. This is the fastest way to assign them a function
        this.GetComponent<Button>().onClick.AddListener(buttonPressed);
    }

    public void buttonPressed()
    {
        ClientUnity clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();

        byte i = 0;
        //If the button clicked is the same as the one that has the script, the iso parameter is sent to the drone

        foreach (Transform item in isoPanel.transform)
        {

            if (this.name.Equals(item.name))
            {
                isoButtonToAssign.transform.GetChild(0).GetComponent<Text>().text = this.transform.GetChild(0).GetComponent<Text>().text;
                if (!isInPlanning)
                {
                    clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_ISO, i);
                }
                else
                {
                    //This is used in planning, which assigns the waypoint this iris aperture

                    ChangePointType.pointSelected.GetComponent<PathPoint>().Rc.ISO = i;

                }
                isoPanel.SetActive(false);
            }
            i++;
            if (item.name.Equals("1600"))
            {
                //In order to fit them on the screen, I had to remove one
                i++;
            }
        }
    }
}
