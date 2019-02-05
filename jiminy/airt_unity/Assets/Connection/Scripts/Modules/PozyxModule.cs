using NetMQ;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PozyxModule  {

    public static string errorMsg = "";
    public static string textMsg = "";
    public static string decAnchors = "";

    public static bool anchorsReceived = false;

    static QueueSync<ServerMessages.IPSFrameAnchorData> ips_anchor_frames = new QueueSync<ServerMessages.IPSFrameAnchorData>(2);
    static ServerMessages.IPSDroneTag ips_tag_frames;
    static List<int> anchorsList = new List<int>();
    public static bool positiningIsValid;
    public static Int32 movementFreedom = -1;
    public static float updatePeriod = -1f;
    ClientUnity clientUnity;

    int index = 0;

    public PozyxModule()
    {
        positiningIsValid = false;
        clientUnity = UnityEngine.GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        ips_tag_frames = new ServerMessages.IPSDroneTag();
        // Notifications from pozyxsource
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_POSITIONING_STARTED, onIPSStarted));

        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_POSITIONING_STOPPED, onIPSStopped));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)NotificationType.QUITTING, onIPSQuitting));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_LOCALTAG_CONNECTED, onIPSConnected));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_DISCOVERED, onIPSDiscovered));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_AUTOCALIBRATED, onIPSAutocalibrated));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_UPDATED_SETTINGS, onIPSUpdated));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_DATA_ANCHOR, onIPSAnchorData));  // anchor data
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_DATA_ANCHORS_LIST, onIPSAnchorList));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_NOTENOUGH_TAGSFOUND, onIPSNotenoughTags));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_NOTENOUGH_ANCHORSFOUND, onIPSNotenoughAnchors));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_LAST_REQUEST_HAS_FAILED, onIPSLastRequestFailed));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_ANCHORS_MANUAL_CONFIG_ACCEPTED, onIPSManualConfigAccepted));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_ANCHORS_TOBE_AUTOCALIBRATED_ACCEPTED, onIPSAnchorsToBeAutocalibratedAccepted));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_DRONETAGS_LIST, onDroneTagsList));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_DRONETAGS, onDroneLastTagData));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_DRONETAGS_MANUAL_CONFIG_ACCEPTED, onConfirmedManualConfig));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)PositioningNotificationType.IPS_GET_DRONEFILTER_NOTIFICATION, onGetDroneFilter));

    }
    // positioning
    public void onIPSStarted(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onIPSStarted");
        positiningIsValid = true;
        //SendLoadPlan(MissionManager.guid, (byte)MissionManager.planIndex);

    }
    public void onIPSStopped(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onIPStopped");
    }
    public void onIPSQuitting(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onIPSQuitting");
    }
    public void onIPSConnected(NetMQMessage m)
    {
        index = 0;
        CalibrationSettings.ClearAnchorData();
        //UnityEngine.Debug.Log("onIPSConnected");
        //Only start receiving IPS data if the the user is in the calibration (normal claibration of anchors) or in the tags configuration scene(use last configuration)
        if (GeneralSceneManager.sceneState != GeneralSceneManager.SceneState.Calibration && GeneralSceneManager.sceneState != GeneralSceneManager.SceneState.TagsConfiguration)
        {
            return;
        }
        if (!clientUnity.client.SendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_DISCOVER))
            UnityEngine.Debug.Log("Error. Could not send IPS_UPDATE_SETTINGS command to the server. Check the log for more information");
    }
    public void onIPSDiscovered(NetMQMessage m)
    {
        //clientUnity.client.sendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_UPDATE_SETTINGS);
       // UnityEngine.Debug.Log("onIPSDiscovered");
    }
    public void onIPSAutocalibrated(NetMQMessage m)
    {
        index = 0;
        //UnityEngine.Debug.Log("onIPSAutocalibrated");
        AnchorsCalibration.state = AnchorsCalibration.CalibrationState.AUTOCALIBRATION_ACCEPTED;
    }
    public void onIPSUpdated(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onIPSUpdated");
        // Mensaje para empezar a recibir el posicionamiento en el cliente
        if (!clientUnity.client.SendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_START_POSITIONING))
            UnityEngine.Debug.LogWarning("Warning. Could not send IPS_START_POSITIONING command to the server. Check the log for more information");
        //if (!clientUnity.client.sendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_STOP_POSITIONING))
            //UnityEngine.Debug.LogWarning("Warning. Could not send IPS_STOP_POSITIONING command to the server. Check the log for more information");

        //if (!clientUnity.client.sendCommand((byte)Modules.STD_COMMANDS_MODULE, (byte)CommandType.START))
            //UnityEngine.Debug.Log("Error. Could not send IPS_START command to the server. Check the log for more information");

        clientUnity.ipsupdated = true;
    }
    
    public void onIPSNotenoughTags(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onIPSNotenoughTags");
    }
    public void onIPSNotenoughAnchors(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onIPSNotenoughAnchors");
    }
    public void onIPSAnchorData(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onIPSAnchorData");
        //Here we get the anchor data, right now we only accept 8, but Pozyx works with 1?
        int numAnchors = BitConverter.ToInt32(m[0].Buffer, 4);

        UnityEngine.Debug.Log("NumAnchors: " + numAnchors);
        if (numAnchors != 8)
        {
            AnchorsCalibration.state = AnchorsCalibration.CalibrationState.LAST_REQ_FAILED;
            errorMsg = "Not enough anchors received!";
            return;
        }
        CalibrationSettings.ClearAnchorData();

        for (int i = 0; i < numAnchors; i++)
        {
            ServerMessages.IPSFrameAnchorData anchorData =
            new ServerMessages.IPSFrameAnchorData(
                BitConverter.ToInt32(m[i + 1].Buffer, 0),
                new Vector3(
                    BitConverter.ToInt32(m[i + 1].Buffer, 4),
                    BitConverter.ToInt32(m[i + 1].Buffer, 8),
                    BitConverter.ToInt32(m[i + 1].Buffer, 12)
                    ),
                    m[i + 1].Buffer[16]
                );

            UnityEngine.Debug.Log("EnqueuingData");
            //ips_anchor_frames.Enqueue(anchorData);
            CalibrationSettings.AddAnchorData(i, anchorData);
        }

        //UnityEngine.Debug.Log(m.FrameCount);
        //UnityEngine.Debug.Log(m[1].Buffer.Length);

        // changing radians to degrees, reverse yaw angle, and position millimeters to meters

        //We enter this function twice, once when we ask for the anchors, and again on autocalibration
        if (GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.Calibration) { 
            UnityEngine.Debug.Log(AnchorsCalibration.state);
            if (AnchorsCalibration.state == AnchorsCalibration.CalibrationState.RECEIVED_DISCOVER_ANCHORS)
                AnchorsCalibration.state = AnchorsCalibration.CalibrationState.DISCOVERED;
            else if (AnchorsCalibration.state == AnchorsCalibration.CalibrationState.AUTOCALIBRATION_ACCEPTED)
                AnchorsCalibration.state = AnchorsCalibration.CalibrationState.AUTOCALIBRATION_RECEIVED;
        }

        anchorsReceived = true;
        UnityEngine.Debug.Log(AnchorsCalibration.state);
        
        //UnityEngine.Debug.Log("DataEnqueued");
        clientUnity.currentlyreceiving = true;
    }

    public void onIPSAnchorList(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onIpsAnchorList");
        anchorsList.Clear();
        //if (BitConverter.ToInt32(m[0].Buffer, 3) <= 8)
        //    clientUnity.client.sendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_DISCOVER);
        //else
         UnityEngine.Debug.Log("FullAnchorsListReceived");
        //This contains the id of the anchors, but not the position 
        int numAnchors = BitConverter.ToInt32(m[0].Buffer, 3);

        for (int i = 0; i < numAnchors; i++)
        {
            anchorsList.Add(BitConverter.ToInt32(m[1].Buffer, i * 4));
           //UnityEngine.Debug.Log(anchorsList[i]);
        }

        if (GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.Calibration) { 
            if (numAnchors < 8)
            {
                UnityEngine.Debug.LogWarning("Not enough anchors found!");
                AnchorsCalibration.state = AnchorsCalibration.CalibrationState.LAST_REQ_FAILED;
                errorMsg = "Not enough anchors found: " + numAnchors + "\nRetrying...";
                clientUnity.client.SendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_DISCOVER);
            }
            else
                AnchorsCalibration.state = AnchorsCalibration.CalibrationState.RECEIVING_DISCOVER_ANCHORS;
        }

    }
    void onDroneTagsList(NetMQMessage m)
    {

        //here we receive the tags from the drone. The project was done to use 4 tags, but more can be found ( when 2 drones are near?)
        decAnchors = "";
        textMsg = "";
        //UnityEngine.Debug.Log("onDroneTagsList");
        byte number = m[0].Buffer[3];
        if (number >= 4) {
            for (int i = 0; i < number; i++)
            {
                textMsg += "0x" + BitConverter.ToInt32(m[1].Buffer, i * 4).ToString("x") + "\n";
                decAnchors += BitConverter.ToInt32(m[1].Buffer, i * 4).ToString() + "\n";
            }
            UITagsConfigurationManager.tagInfoReceived = true;

        }
        if (number < 4)
        {
            UnityEngine.Debug.Log("Not enough tags found: " + number);
            errorMsg = "Not enough tags found: " + number + "\nRetrying...";
            clientUnity.client.SendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_DISCOVER_DRONETAGS);
            UITagsConfigurationManager.state = UITagsConfigurationManager.TagsConfigState.ErrorReceived;
        }
    }
    void onDroneLastTagData(NetMQMessage m)
    {
        //The last tag data that the drone had
        UnityEngine.Debug.Log("onDroneLastTagData");
        ips_tag_frames.width = BitConverter.ToInt32(m[0].Buffer, 3);
        ips_tag_frames.height = BitConverter.ToInt32(m[0].Buffer, 7);
        ips_tag_frames.idSW = BitConverter.ToInt32(m[0].Buffer, 11);
        ips_tag_frames.idNW = BitConverter.ToInt32(m[0].Buffer, 15);
        ips_tag_frames.idNE = BitConverter.ToInt32(m[0].Buffer, 19);
        ips_tag_frames.idSE = BitConverter.ToInt32(m[0].Buffer, 23);
        //ips_tag_frames.camDist = BitConverter.ToInt32(m[0].Buffer, 27);
        //ips_tag_frames.camDist = 0;

        //UITagsConfigurationManager.state = UITagsConfigurationManager.TagsConfigState.TagsFound;
        
    }

    void onConfirmedManualConfig(NetMQMessage m)
    {

        //UnityEngine.Debug.Log("onConfirmedManualConfig");
        UITagsConfigurationManager.tagInfoConfirmed = true;
        //UITagsConfigurationManager.state = UITagsConfigurationManager.TagsConfigState.ConfigurationAccepted;
        
    }    

    public void onIPSLastRequestFailed(NetMQMessage m)
    {
        UnityEngine.Debug.Log("onIPSLastRequestFailed");
        if (GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.Calibration) {
            UnityEngine.Debug.Log("onIPSLastRequestFailed: Calibration");
            AnchorsCalibration.state = AnchorsCalibration.CalibrationState.LAST_REQ_FAILED;
            errorMsg = "Last Request Failed";
        }
        if (GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.TagsConfiguration)
        {
            UnityEngine.Debug.Log("onIPSLastRequestFailed: Calibration");
            UITagsConfigurationManager.state = UITagsConfigurationManager.TagsConfigState.ErrorReceived;
            errorMsg = "Last Request Failed";
        }
    }

    public void onIPSManualConfigAccepted(NetMQMessage m)
    {
       // UnityEngine.Debug.Log("onIPSManualConfigAccepted");
        AnchorsCalibration.state = AnchorsCalibration.CalibrationState.MANUAL_ACCEPTED;        
    }

    public void onIPSAnchorsToBeAutocalibratedAccepted(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onIPSAnchorsToBeAutocalibratedAccepted");
        //only do this on calibration
        if (GeneralSceneManager.sceneState != GeneralSceneManager.SceneState.Calibration && GeneralSceneManager.sceneState != GeneralSceneManager.SceneState.TagsConfiguration)
        {
            return;
        }
        clientUnity.client.SendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_AUTOCALIBRATE);
    }
    public void onGetDroneFilter(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Got drone filter");
        //this is an option that only pozyx wanted
        updatePeriod = BitConverter.ToSingle(m[0].Buffer, 4);

        movementFreedom = BitConverter.ToInt32(m[0].Buffer, 8);
        UITagsConfigurationManager.droneFilterReceived = true;
    }
    public static ServerMessages.IPSFrameAnchorData DequeueIPSAnchorFrame()
    {
        if (ips_anchor_frames.GetSize() > 0)
            return ips_anchor_frames.Dequeue();
        else
            return null;
    }

    public static ServerMessages.IPSDroneTag GetTagFrame()
    {
        return ips_tag_frames;
    }

    public static List<int> GetAnchorList()
    {
        if (anchorsList.Count == 8)
            return anchorsList;
        else
            return null;
    }
   
}
