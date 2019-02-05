using System;
using System.Diagnostics;

using NetMQ;

public class FPVModule {

    //public StateMachine FPVimageReceiverSM;
	ClientUnity clientUnity;
    long lastTime = 0, currentTime = 0;
    public static long period = 0;
    bool firstFrame = true;
    Stopwatch stopWatch;

    public FPVModule()
    {
        stopWatch = new Stopwatch();
        clientUnity = UnityEngine.GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.FPV_NOTIFICATIONS_MODULE, (byte)FPVNotificationType.FPV_RESOLUTION_NOTIFICATION, onResolutionMessage));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.FPV_NOTIFICATIONS_MODULE, (byte)FPVNotificationType.FPV_IMAGE_JPEG_NOTIFICATION, onImageMessage));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.FPV_NOTIFICATIONS_MODULE, (byte)FPVNotificationType.FPV_LAST_NOTIFICATION, onLastNotification));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.FPV_NOTIFICATIONS_MODULE, (byte)FPVNotificationType.FPV_STREAMING_STARTED, onFPVStart));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.FPV_NOTIFICATIONS_MODULE, (byte)FPVNotificationType.FPV_STREAMING_STOPPED, onFPVStop));
    }

    public void onResolutionMessage(NetMQMessage m)
    {
        //The resolution is not really needed as the fpv will always come in 640x480p. In case the resolution is needed if the fpv image processing improves, take a look at this
        //message and send a  FPVCommandType.FPV_GET_RESOLUTION before getting the image
        ManageFPV.resX = BitConverter.ToInt32(m[0].Buffer, 3);
        ManageFPV.resY = BitConverter.ToInt32(m[0].Buffer, 7);
        return;
    }

    public void onImageMessage(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("ImageMessage");
        //if (firstFrame){
        //    stopWatch.Start();
        //    currentTime = stopWatch.ElapsedMilliseconds;
        //    lastTime = stopWatch.ElapsedMilliseconds;
        //    firstFrame = false;
        //}
        //else {
        //
        //    currentTime = stopWatch.ElapsedMilliseconds;
        //    period = currentTime - lastTime;
        //    lastTime = currentTime;
        //}
        ManageFPV.ImageDataReceived(m);
    }

    public void onLastNotification(NetMQMessage m)
    {

    }

    public void onFPVStart(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Start notification");
        ManageFPV.start = true;
    }

    public void onFPVStop(NetMQMessage m)
    {
        ManageFPV.stop = true;
    }
}
