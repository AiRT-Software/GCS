using NetMQ;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LibraryModule {
    ClientUnity clientUnity;
    public static bool serverMapExists = false;
    public static bool substituteMap = false;
    public LibraryModule() { 
        clientUnity = UnityEngine.GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_LIBRARIAN_NOTIFICATIONS_MODULE, (byte)PlanLibrarianNotificationType.PLAN_LIB_LIBRARY_PATH_NOTIFICATION, onPathReceivedMessage));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_LIBRARIAN_NOTIFICATIONS_MODULE, (byte)PlanLibrarianNotificationType.PLAN_LIB_NUMBER_PLANS_IN_DDBB_NOTIFICATION, onNumberOfPlansReceived));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_LIBRARIAN_NOTIFICATIONS_MODULE, (byte)PlanLibrarianNotificationType.PLAN_LIB_BASE_NAME_NOTIFICATION, onPlanNameReceived));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_LIBRARIAN_NOTIFICATIONS_MODULE, (byte)PlanLibrarianNotificationType.PLAN_LIB_METADATA_NOTIFICATION, onMetadataReceived));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_LIBRARIAN_NOTIFICATIONS_MODULE, (byte)PlanLibrarianNotificationType.PLAN_LIB_THUMBNAIL_NOTIFICATION, onThumbnailReceived));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_LIBRARIAN_NOTIFICATIONS_MODULE, (byte)PlanLibrarianNotificationType.PLAN_LIB_MISSION_NOTIFICATION, onMissionReceived));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_LIBRARIAN_NOTIFICATIONS_MODULE, (byte)PlanLibrarianNotificationType.PLAN_LIB_MAP_NOTIFICATION, onMapReceived));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_LIBRARIAN_NOTIFICATIONS_MODULE, (byte)PlanLibrarianNotificationType.PLAN_LIB_FILE_NOT_FOUND_NOTIFICATION, onFileNotFoundException));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_LIBRARIAN_NOTIFICATIONS_MODULE, (byte)PlanLibrarianNotificationType.PLAN_LIB_MISSION_INDEX_NOT_FOUND_NOTIFICATION, onMissionIndexNotFoundException));
        
    }
    public void onPathReceivedMessage(NetMQMessage m)
    {

        string library_path_str = System.Text.Encoding.ASCII.GetString(m[1].Buffer);
        //To remove \0 use this
        //We get the path from the server
        library_path_str = library_path_str.Substring(0, library_path_str.Length - 1);
        SendJSONStateMachine.library_path = library_path_str;

        string pathMetadata = library_path_str + "/" + MissionManager.guid + ".json.metadata\0";
        string missionMetadata = library_path_str + "/" + MissionManager.guid + ".json.mission\0";
        string model_filepath = "";

        //If the map is a pointcloud send the first, if not the second
        if (PlaceModel.pointCloudAskedForMetadata == true)
        {
            PlaceModel.pointCloudAskedForMetadata = false;
            model_filepath = library_path_str + "/" + MissionManager.guid + ".dpl.map\0";

        }
        else
        {
            model_filepath = library_path_str + "/" + MissionManager.guid + ".dae.map\0";

        }
        string thumbnail_filepath = library_path_str + "/" + MissionManager.guid + ".jpeg.thumbnail\0";
        //If we are in plan selection, we asked for a delete file, if not, a create file, to the upload it, which means we are in recording
        if (GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.PlanSelection && !substituteMap) {

            clientUnity.client.sendTwoPartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_DELETE_FILE, pathMetadata);
            clientUnity.client.sendTwoPartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_DELETE_FILE, missionMetadata);
            clientUnity.client.sendTwoPartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_DELETE_FILE, model_filepath);
            clientUnity.client.sendTwoPartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_DELETE_FILE, thumbnail_filepath);
        }
        else {
            
            //UnityEngine.Debug.Log("PathMetadata: " + pathMetadata);
            clientUnity.client.sendTwoPartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_CREATE_FILE, pathMetadata);
            clientUnity.client.sendTwoPartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_CREATE_FILE, missionMetadata);

            if (!PlanSelectionManager.uploadMapMetadata)
            {
                clientUnity.client.sendTwoPartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_CREATE_FILE, model_filepath);
            }
            clientUnity.client.sendTwoPartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_CREATE_FILE, thumbnail_filepath);
            substituteMap = false;
            //StateMachine.resY = BitConverter.ToInt32(m[0].Buffer, 7);
            return;
        } 
    }

    public void onLastNotification(NetMQMessage m)
    {

    }
   
    public void onNumberOfPlansReceived(NetMQMessage m)
    {
        uint numberOfPlans = BitConverter.ToUInt32(m[0].Buffer, 3);
        //We assign this variable to know how many map buttons we have to create, and later when we delete one, to know how many map buttons we have to create again,
        //in order for plan selection not to get desaligned
        PlanSelectionManager.metadatasDownloaded = numberOfPlans;
        //If we are not in plan selection, we aren't asking for maps
        if (GeneralSceneManager.sceneState != GeneralSceneManager.SceneState.PlanSelection)
        {
            return;
        }
        //UnityEngine.Debug.Log("NUMBER OF PLANS RECEIVED : " + numberOfPlans);
        for (int i = 0; i < numberOfPlans; i++)
        {
            //we ask for every metadata on the server, asking first for each guid of the maps
            clientUnity.client.sendCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_BASE_NAME,i);

        }
    }

    public void onPlanNameReceived(NetMQMessage m)
    {
        //When we receive the plan name from PLAN_LIB_REQUEST_BASE_NAME, we request the map or the metadata, depending from where we come from
        string planGUID = System.Text.Encoding.ASCII.GetString(m[1].Buffer);
        //UnityEngine.Debug.Log("Plane name received " + planGUID);
        if (PlaceModel.pointCloudAskedForMetadata)
        {
            PlaceModel.pointCloudAskedForMetadata = false;

            clientUnity.client.sendTwoPartCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_MAP, MissionManager.guid + "\0");

        }
        else
        {
            //UnityEngine.Debug.Log("Hola");
            clientUnity.client.sendTwoPartCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_METADATA, m[1].Buffer);

        }
    }
    public void onMetadataReceived(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Metadata Received");
        //If we receive a metadtata and we asked from mapalignment, it means that the metadata exist in the server, so we don't upload it
        //Los mensajes tienen 3 partes, no 2 y el 2 tiene el path, el 1 es el guid
        if (PlaceModel.pointCloudAskedForMetadata)
        {
            PlaceModel.pointCloudAskedForMetadata = false;
            LibraryModule.serverMapExists = true;
        }
        else
        {
            //if we came from other place, we want to download the metadata, so we start downloading
            string pathToMetadata = System.Text.Encoding.ASCII.GetString(m[2].Buffer);
            clientUnity.client.sendTwoPartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_REQUEST_FILE_SIZE, m[2].Buffer);
        }
       
    }
    public void onThumbnailReceived(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Thumbnail Received");

        //Los mensajes tienen 3 partes, no 2 y el 2 tiene el path, el 1 es el guid
        string pathToThumbnail = System.Text.Encoding.ASCII.GetString(m[2].Buffer);
        clientUnity.client.sendTwoPartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_REQUEST_FILE_SIZE, m[2].Buffer);
    }
    public void onMissionReceived(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Mission Received");

        //Los mensajes tienen 3 partes, no 2 y el 2 tiene el path, el 1 es el guid
        string pathToMission = System.Text.Encoding.ASCII.GetString(m[2].Buffer);
        clientUnity.client.sendTwoPartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_REQUEST_FILE_SIZE, m[2].Buffer);
    }
    public void onMapReceived(NetMQMessage m)
    {

        //UnityEngine.Debug.Log("Map Received");
        //If the drone has the map, we check if it has the metadata. The poointclouds save the map but not the metadatas
        if (PlaceModel.pointCloudAskedForMetadata)
        {
            PlanSelectionManager.uploadMapMetadata = false;
            //UnityEngine.Debug.Log("Hola2");
            PlanSelectionManager.uploadMapMetadata = true;
            //UnityEngine.Debug.Log("Hola2");
            SendJSONStateMachine.state = SendJSONStateMachine.SendJSonStates.SENDPATHREQUEST;
        }
        else
        {
            //Los mensajes tienen 3 partes, no 2 y el 2 tiene el path, el 1 es el guid
            //If we were not in modelAlignment, we download the map
            if (GeneralSceneManager.sceneState != GeneralSceneManager.SceneState.ModelAlignment)
            {
                string pathToMap = System.Text.Encoding.ASCII.GetString(m[2].Buffer);
                PlanSelectionManager.askedForMaps = true;

                clientUnity.client.sendTwoPartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_REQUEST_FILE_SIZE, m[2].Buffer);
            }
            else
            {
                //else the map exists. Isn't this the same as up there???
                UnityEngine.Debug.Log("ServerMapExists");
                serverMapExists = true;
                PlanSelectionManager.uploadMapMetadata = true;
                //UnityEngine.Debug.Log("Hola2");
                SendJSONStateMachine.state = SendJSONStateMachine.SendJSonStates.SENDPATHREQUEST;
            }
        }
      
    }
    public void onFileNotFoundException(NetMQMessage m)
    {
        UnityEngine.Debug.LogWarning("File Not Found Exception!!!");
        //If we asked for a file that we have and the drone doesn't have it, and we are in this scenes, we want it to upload it
        // ## TODO: Revisar donde va esta orden!!!
        if(GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.PlanSelection || GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.ModelAlignment)
            SendJSONStateMachine.state = SendJSONStateMachine.SendJSonStates.SENDPATHREQUEST;
    }

    public void onMissionIndexNotFoundException(NetMQMessage m)
    {
        UnityEngine.Debug.LogWarning("Mission index not found Exception!!!");
    }
}
