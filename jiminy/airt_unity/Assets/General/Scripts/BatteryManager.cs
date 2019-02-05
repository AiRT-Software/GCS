using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BatteryManager : MonoBehaviour {
    public Image batteryButton, warningButton;
    public Sprite fullBatterySprite, halfBatterySprite, lowBatterySprite;
    public static batteryState state = batteryState.FULL;
    public static bool graphicChanged = true;
    public GameObject lowBatteryPanel;
    public enum batteryState
    {
        FULL = 0,
        WARNING,
        CRITICAL,
        SHUTDOWN
    }

    void Awake()
    {
        graphicChanged = false;
    }

    private void Update()
    {
        switch (state)
        {
            //This state machine changes the battery sprite. If the variable graphicChanged is true, the graphic changes. 
            case batteryState.FULL:
                if (!graphicChanged)
                {

                    batteryButton.sprite = fullBatterySprite;
                    graphicChanged = true;
                    warningButton.color = new Vector4(1, 1, 1, 1);

                }
                break;
            case batteryState.WARNING:
                if (!graphicChanged)
                {
                    batteryButton.sprite = halfBatterySprite;
                    graphicChanged = true;
                    warningButton.color = new Vector4(1, 1, 1, 1);

                }
                break;
            case batteryState.CRITICAL:
                if (!graphicChanged)
                {
                    batteryButton.sprite = lowBatterySprite;
                    warningButton.color = new Vector4(1, 0, 0, 1);
                    graphicChanged = true;
                    //lowBatteryPanel.SetActive(true);
                }
                break;
            case batteryState.SHUTDOWN:
                if (!graphicChanged)
                {
                    //For shutdown, we activate a panel that urges the user to land the drone
                    batteryButton.sprite = lowBatterySprite;
                    warningButton.color = new Vector4(1, 0, 0, 1);
                    graphicChanged = true;
                    lowBatteryPanel.SetActive(true);
                }
                break;
            default:
                break;
        }
    }

}
