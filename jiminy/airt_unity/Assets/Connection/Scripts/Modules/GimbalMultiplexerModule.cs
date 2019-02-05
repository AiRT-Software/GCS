using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetMQ;

public class GimbalMultiplexerModule {

    ClientUnity clientUnity;

    public GimbalMultiplexerModule()
    {
        clientUnity = UnityEngine.GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        // Notifications from pozyxsource
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.GIMBAL_MULTIPLEXER_NOTIFICATIONS_MODULE, (byte)GimbalMultiplexerNotificationType.GIMBAL_GOTO_ANGLE_NOTIFICATION, onGimbalGotoAngle));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.GIMBAL_MULTIPLEXER_NOTIFICATIONS_MODULE, (byte)GimbalMultiplexerNotificationType.GIMBAL_ANGLE_CHANGED_NOTIFICATION, onGimbalAngleChanged));

        //clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_DATA, onIPSData));  // dron data

    }

    public void onGimbalGotoAngle(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onGimbalGotoAngle");
    }

    public void onGimbalAngleChanged(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onGimbalAngleChanged");
    }
}
