using NetMQ;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecCamModule {

    //public StateMachine FPVimageReceiverSM;
    ClientUnity clientUnity;
    long lastTime = 0, currentTime = 0;
    public static long period = 0;
    bool firstFrame = true;

    public RecCamModule()
    {
        clientUnity = UnityEngine.GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.RCAM_NOTIFICATIONS_MODULE, (byte)RCamNotificationType.RCAM_CONFIG_STATUS_NOTIFICATION, onConfigStatusReceived));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.RCAM_NOTIFICATIONS_MODULE, (byte)RCamNotificationType.RCAM_MODE_NOTIFICATION, onModeReceived));

    }
    public void onModeReceived(NetMQMessage m)
    {
        //We only use 2 modes, capture and record. There is no place to see the playback
        RCamMode mode = (RCamMode)m[0].Buffer[3];
        switch (mode)
        {
            case RCamMode.RCAM_MODE_CAPTURE:
                RecCamControl.photoStateFromModule = true;
                break;
            case RCamMode.RCAM_MODE_RECORD:
                RecCamControl.photoStateFromModule = false;

                break;
            case RCamMode.RCAM_MODE_PLAYBACK:
                break;
            default:
                break;
        }
        RecCamControl.modeReceived = true;
    }
    public void onConfigStatusReceived(NetMQMessage m)
    {
        //This receives the actual config of the camera
        byte type = m[0].Buffer[3];
        RCamConfigParameter parameter = (RCamConfigParameter)m[0].Buffer[4];
        switch (type)
        {
            case 0:
                byte valueByte = m[0].Buffer[5];
                int valueByteToInt = valueByte;
                RecCamControl.paramDict[(byte)parameter] = valueByteToInt;

                break;
            case 1:
                int valueInt = BitConverter.ToInt32(m[0].Buffer, 5);
                RecCamControl.paramDict[(byte)parameter] = valueInt;
                
                break;
            default:
                break;
        }
        RecCamControl.configsReceived++;

    }

}
