using NetMQ;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapperModule {

    //QueueSync<PCLMsg> pcl_frames = new QueueSync<PCLMsg>(10);
    static QueueSync<PCLMsg> pcl_frames = new QueueSync<PCLMsg>(5);


    ClientUnity clientUnity;
    public enum MapperState
    {
        IDLE,
        READY,
        START,
        PAUSED,
        DONE
    }

    public static MapperState state = MapperState.IDLE;
    public static bool allPointCloudsDeletedBool = false;
    public static bool lastPointCloudDeletedBool = false;

    public MapperModule()
    {
        clientUnity = UnityEngine.GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        //clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)MapperNotificationType.MAPPER_STARTED_MAPPING_NOTIFICATION, pointCloudStarted));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)MapperNotificationType.MAPPER_MAP_READY_NOTIFICATION, onMappingReady));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)MapperNotificationType.MAPPER_ERROR_SAVING_DDBB_NOTIFICATION, errorSavingDDBB));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)MapperNotificationType.MAPPER_IDLE_NOTIFICATION, idle));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)MapperNotificationType.MAPPER_ALL_POINT_CLOUDS_DELETED_NOTIFICATION, allPointCloudsDeleted));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)MapperNotificationType.MAPPER_DELETED_POINTCLOUD_NOTIFICATION, PointCloudDeleted));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)MapperNotificationType.MAPPER_CURRENT_GUID_NOTIFICATION, currentGUID));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)MapperNotificationType.MAPPER_NUMBER_POINTCLOUDS_IN_MAP_NOTIFICATION, numberOfPointClouds));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)MapperNotificationType.MAPPER_LIST_POINTCLOUD_IDS_NOTIFICATION, listOfPointCloudsGUID));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)MapperNotificationType.MAPPER_MAP_READY_NOTIFICATION, mapReadyNotification));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)MapperNotificationType.MAPPER_STARTED_MAPPING_NOTIFICATION, mapStartedNotification));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)MapperNotificationType.MAPPER_PAUSE_MAPPING_NOTIFICATION, mapPausedNotification));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)MapperNotificationType.MAPPER_IDLE_NOTIFICATION, mapIdleNotification));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)MapperNotificationType.MAPPER_DONE_MAPPING_NOTIFICATION, mapDoneNotification));

        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)MapperNotificationType.MAPPER_DONE_MAPPING_NOTIFICATION, pointCloudStopped));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)MapperNotificationType.MAPPER_POINTCLOUD_NOTIFICATION, onPclData));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)NotificationType.INVALID_STATUS, onInvalidStatus));

    }
    //Every state corresponds to the drone state when it's on mapping
    public void mapReadyNotification(NetMQMessage m)
    {
        state = MapperState.READY;
        TakeOffButton.changedState = true;
    }
    public void mapStartedNotification(NetMQMessage m)
    {
        //This is to put on scanning the tablet that didn't press the button
        TakeOffButton.state = TakeOffButton.TakeOffButtonEnum.Scanning;

        state = MapperState.START;
        TakeOffButton.changedState = true;

    }
    public void mapPausedNotification(NetMQMessage m)
    {
        //Same but with paused
        TakeOffButton.state = TakeOffButton.TakeOffButtonEnum.StopScan;
        state = MapperState.PAUSED;
        TakeOffButton.changedState = true;


    }
    public void mapIdleNotification(NetMQMessage m)
    {
        state = MapperState.IDLE;
        TakeOffButton.changedState = true;


    }
    public void mapDoneNotification(NetMQMessage m)
    {
        //When a tablet sents a done mapping, one will enter the idle and the other ones will go
        //to DONE to save the metadata and mission
        if (PointCloud.done == true)
        {
            PointCloud.done = false;
            state = MapperState.IDLE;

        }
        else
        {
            state = MapperState.DONE;
        }
        PlanSelectionManager.askedForMaps = true;
        PlaceModel.pointCloudAskedForMetadata = false;
        TakeOffButton.changedState = true;

    }

    public void allPointCloudsDeleted(NetMQMessage m)
    {
        //To delete all pointclouds in all the tablets
        allPointCloudsDeletedBool = true;
    }
    public void PointCloudDeleted(NetMQMessage m)
    {
        //To delete last pointcloud in all the tablets
        lastPointCloudDeletedBool = true;
    }
    public void numberOfPointClouds(NetMQMessage m)
    {
        //uint numberOfPointClouds = BitConverter.ToUInt32(m[0].Buffer, 3);
        //for (int i = 0; i < numberOfPointClouds; i++)
        //{
        //}
    }
    public void listOfPointCloudsGUID(NetMQMessage m)
    {

    }
    public void currentGUID(NetMQMessage m)
    {
        //The tablets that aren't the first to enter on mapping will get the guid like this.
        string guid = System.Text.Encoding.ASCII.GetString(m[1].Buffer);
        //As always, this is to remove the \0
        MissionManager.guid = guid.Substring(0, guid.Length - 1);
    }

    public void pointCloudStarted(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Started pointCloud");
    }
    public void errorSavingDDBB(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Error saving in database");
    }
    public void onMappingReady(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("PointCloud Ready");
        TakeOffButton.mapReady = true;
        

    }
    public void idle(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("idle");


    }
    public void pointCloudStopped(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Stopped pointCloud");
    }
    public void onPclData(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onPclData" + m[0].BufferSize);
        //This contains the pointcloud data
        PCLMsg pcl = new PCLMsg(m[0].Buffer);
        pcl.fillMesh(m[1].Buffer);
        //pcl_frames.Enqueue(pcl);
        pcl_frames.Enqueue(pcl);

        clientUnity.currentlyreceiving = true;
    }
    public void onInvalidStatus(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Invalid status");
        //MissionManager.guid = Guid.NewGuid().ToString();
        //clientUnity.client.sendTwoPartCommand((byte)Modules.MAPPER_MODULE, (byte)MapperCommandType.MAPPER_CREATE_MAP, MissionManager.guid);
    }
    public static PCLMsg DequeuePCLFrame()
    {
        if (pcl_frames.GetSize() > 0)
            return pcl_frames.Dequeue();
        else
            return null;
    }
}
