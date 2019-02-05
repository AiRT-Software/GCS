using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using NetMQ;

public class DroneModule {

	ClientUnity clientUnity;

    static QueueSync<ServerMessages.IPSFrameData> ips_frames = new QueueSync<ServerMessages.IPSFrameData>(2);
    static Vector3 cameraOffset;

    public DroneModule()
    {
        cameraOffset = new Vector3(-1, -1, -1);
        clientUnity = UnityEngine.GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        // Notifications from pozyxsource
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.DRONE_NOTIFICATIONS_MODULE, (byte)DroneNotificationType.DRONE_GIMBAL_POSE_NOTIFICATION, onGimbalPose));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.DRONE_NOTIFICATIONS_MODULE, (byte)DroneNotificationType.DRONE_ZCAMERA_OFFSET_NOTIFICATION, onDroneCamOffset));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.DRONE_NOTIFICATIONS_MODULE, (byte)DroneNotificationType.DRONE_POSITION_POSE_NOTIFICATION, onIPSData));

        //clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_DATA, onIPSData));  // dron data

    }

    public void onDroneCamOffset(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Drone cam offset");
        cameraOffset.x = BitConverter.ToSingle(m[0].Buffer, 4);
        cameraOffset.y = BitConverter.ToSingle(m[0].Buffer, 8);
        cameraOffset.z = BitConverter.ToSingle(m[0].Buffer, 12);
        //UnityEngine.Debug.Log("CamOffset values: " + cameraOffset);
        //UnityEngine.Debug.Log("Drone cam offset received");
        // ?????
        if(UITagsConfigurationManager.state == UITagsConfigurationManager.TagsConfigState.ConfigurationSent)
            UITagsConfigurationManager.camOffsetConfirmed = true;
        else
            UITagsConfigurationManager.camOffsetReceived = true;
    }

    public void onGimbalPose(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onGimbalPose");
    }

    public void onIPSData(NetMQMessage m)
    {

        //Here the positioning data is received
        //The rotation comes in radians, and is not in the same system as Unity. The position comes in milimeters and y and z are interchanged

        //UnityEngine.Debug.Log("Pitch: " + BitConverter.ToSingle(m[0].Buffer, 12));
        //UnityEngine.Debug.Log("Roll: " + BitConverter.ToSingle(m[0].Buffer, 20));
        //UnityEngine.Debug.Log("Yaw: " + BitConverter.ToSingle(m[0].Buffer, 16));
        // changing radians to degrees, reverse yaw angle, and position millimeters to meters
        ServerMessages.IPSFrameData pos =
            new ServerMessages.IPSFrameData(
            //BitConverter.ToDouble(m[0].Buffer, 3),  //space
            //BitConverter.ToDouble(m[0].Buffer, 4),  //timestamp
                0.0,
                new Vector3(
                    BitConverter.ToSingle(m[0].Buffer, 12) * -180f / 3.141592f,  //pitch
                    BitConverter.ToSingle(m[0].Buffer, 20) * -180f / 3.141592f, // roll
                    BitConverter.ToSingle(m[0].Buffer, 16) * -180f / 3.141592f  //yaw
            //BitConverter.ToSingle(m[0].Buffer, 12),  //pitch
            //BitConverter.ToSingle(m[0].Buffer, 20), //yaw
            //BitConverter.ToSingle(m[0].Buffer, 16)  //roll 
                    ),
                new Vector3(
                    BitConverter.ToSingle(m[0].Buffer, 24) * 0.001f,  //x
                    BitConverter.ToSingle(m[0].Buffer, 32) * 0.001f,  //y
                    BitConverter.ToSingle(m[0].Buffer, 28) * 0.001f  //z
                    )
                );
        ips_frames.Enqueue(pos);
        clientUnity.currentlyreceiving = true;
    }
    public static int NumberOfIPSMessages()
    {
        //this function is not needed because we check the size of the array before getting the data
        return ips_frames.GetSize();
    }
    public static ServerMessages.IPSFrameData DequeueIPSDronFrame()
    {
        //This function is to get a positioning frame to the class that positions the drone. A lot of classes position the drone (Mapping and recording), so it needs to be public
        //and static to keep receiving frames, as the drone can keep flying once mapping finishes
        if (ips_frames.GetSize() > 0)
            return ips_frames.Dequeue();
        else
            return null;
    }
    public static Vector3 GetCamOffset()
    {
        return cameraOffset;
    }
}
