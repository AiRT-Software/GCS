using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NetMQ;

public class StandardModule {

    ClientUnity clientUnity;

    public static bool OCSAlive = true;
    bool systemSemaphore;

    public StandardModule()
    {
        clientUnity = UnityEngine.GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.STD_NOTIFICATIONS_MODULE, (byte)NotificationType.ACK, onStdAck));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.STD_NOTIFICATIONS_MODULE, (byte)NotificationType.HEART_BEAT, onStdHearbeat));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.STD_NOTIFICATIONS_MODULE, (byte)NotificationType.STARTED, onStdStarted));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.STD_NOTIFICATIONS_MODULE, (byte)NotificationType.STOPPED, onStdStopped));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.STD_NOTIFICATIONS_MODULE, (byte)NotificationType.QUITTING, onStdQuitting));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.STD_NOTIFICATIONS_MODULE, (byte)NotificationType.POWERING_OFF, onStdPoweringoff));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.STD_NOTIFICATIONS_MODULE, (byte)NotificationType.UNKNOWN_COMMAND, onStdUnknowncommand));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.STD_NOTIFICATIONS_MODULE, (byte)NotificationType.UNDEFINED_MODULE, onStdUndefinedmodule));
    }

    // std
    public void onStdAck(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onStdAck");
    }
    public void onStdHearbeat(NetMQMessage m)
    {

        //UnityEngine.Debug.Log("onStdHeartbeat");
        //This regulates the alpha of the connected tick on the top panel of the app. If it's true, the tick will be bright with an alpha of 1, if not the alpha is 0.5 which means it's disconnected.
        OCSAlive = true;
        //CheckState();
    }
    public void onStdStarted(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onStdStarted");
        OCSAlive = true;
    }
    public void onStdStopped(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onStdStopped");

        clientUnity.currentlyreceiving = false;
    }
    public void onStdQuitting(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onStdQuitting");
    }
    public void onStdPoweringoff(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onStdPoweringoff");
        OCSAlive = false;
    }
    public void onStdUnknowncommand(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onStdUnknowncommand");
    }
    public void onStdUndefinedmodule(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onStdUndefinedmodule");
    }

    void CheckState()
    {
        //We receive always this info
        if (systemSemaphore) { 
            clientUnity.client.SendCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_QUERY_SYSTEM_INFO);
            clientUnity.client.SendCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_QUERY_AVAILABLE_DISKSPACE);
        }
        systemSemaphore = !systemSemaphore;
    }
}
