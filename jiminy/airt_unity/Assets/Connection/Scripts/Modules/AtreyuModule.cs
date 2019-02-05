using System.Collections;
using System.Collections.Generic;
using System;  //Func, Action

using NetMQ;

public class AtreyuModule {

    public static bool versionUpdated = false;
    public static bool jumpToMappingDirectly = false;
    // ## Quitar public, solo es para debug
    public static byte majorV, minorV, patchV;

    ClientUnity clientUnity;

    public AtreyuModule()
    {
        clientUnity = UnityEngine.GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.ATREYU_NOTIFICATIONS_MODULE, (byte)AtreyuNotificationType.ATREYU_VERSION_NOTIFICATION, onVersionRcv));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.ATREYU_NOTIFICATIONS_MODULE, (byte)AtreyuNotificationType.ATREYU_UPDATE_PACKAGE_PATHNAME_NOTIFICATION, onUpdatePath));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.ATREYU_NOTIFICATIONS_MODULE, (byte)AtreyuNotificationType.ATREYU_SYSTEM_STATE_CHANGE_NOTIFICATION, onSystemStateChange));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.ATREYU_NOTIFICATIONS_MODULE, (byte)AtreyuNotificationType.ATREYU_CANNOT_CHANGE_STATE_NOTIFICATION, onSystemStateCannotChange));

    }

    public void onVersionRcv(NetMQMessage m)
    {
        //When the software version in the drone is not received
        byte major = m[0].Buffer[3];
        byte minor = m[0].Buffer[4];
        byte patch = m[0].Buffer[5];
        UnityEngine.Debug.Log("Version: " + major + "." + minor + "." + patch);
        majorV = major;
        minorV = minor;
        patchV = patch;
        versionUpdated = true;
        UnityEngine.Debug.Log("Version Updated");
    }

    public void onUpdatePath(NetMQMessage m)
    {
        //updatePathName = BitConverter.ToString(m[1].Buffer, 0);

        clientUnity.client.sendTwoPartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_CREATE_FILE, m[1].Buffer);

    }

    public static bool EqualVersions()
    {
        if (majorV == (byte)AIRTVersion.MAJOR && minorV == (byte)AIRTVersion.MINOR && patchV == (byte)AIRTVersion.PATCH)
            return true;
        else
            return false;
    }

    public void onSystemStateChange(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("OnSystemStateChange");
        byte oldState = m[0].Buffer[3];
        byte newState = m[0].Buffer[4];

        string oldStateStr = "";
        if (oldState == (byte)AtreyuSystemState.IDLE_STATE) { oldStateStr = "IDLE_STATE"; }
        else if (oldState == (byte)AtreyuSystemState.MAPPING_STATE) { oldStateStr = "MAPPING_STATE"; }
        else if (oldState == (byte)AtreyuSystemState.RECORDING_STATE) { oldStateStr = "RECORDING_STATE"; }

        string newStateStr = "";
        if (newState == (byte)AtreyuSystemState.IDLE_STATE) { newStateStr = "IDLE_STATE"; }
        else if (newState == (byte)AtreyuSystemState.MAPPING_STATE) {
            //If the new state is mapping state, and the app is not in the mapping scene, we get the anchors configured from another app and start to prepare 
            //to change to mapping scenee
            newStateStr = "MAPPING_STATE";
            if (GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.General || GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.Calibration || GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.TagsConfiguration)
            {
                clientUnity.client.SendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_GET_ANCHORS_FROM_FILE);

                jumpToMappingDirectly = true;
            }


        }

        else if (newState == (byte)AtreyuSystemState.RECORDING_STATE) { 
            //If it's recording, and the app is in the recording scene, we send the transform matrix and load the plan on the plan executor
            newStateStr = "RECORDING_STATE";
            //UnityEngine.Debug.Log("Recording state");
            UIRecordingManager.isRecState = true;
            if (UIRecordingManager.registrationMatrix != null)
            {
                clientUnity.client.sendCommand(UIRecordingManager.registrationMatrix);
                //clientUnity.client.sendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_SYSTEM);

                if (!SendLoadPlan(MissionManager.guid, (byte)MissionManager.planIndex))
                {
                    UnityEngine.Debug.Log("Error. Could not connect to the server. Check the log for more information");
                    return;
                }
            }
            else
            {
                //If not, we ask for positioning and the current flight plan
                clientUnity.client.SendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_SYSTEM);
                clientUnity.client.SendCommand((byte)Modules.PLAN_EXECUTOR_MODULE, (byte)PlanExecutorCommandType.PLAN_EXEC_REQUEST_CURRENT_FLIGHT_PLAN);

            }
        }

        //UnityEngine.Debug.Log("Atreyu state. Old: " + oldStateStr + " New: " + newStateStr);
    }

    public void onSystemStateCannotChange(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("System State Cannot Change");
        // ## TODO ESTO NO VA AQUI
        //clientUnity.client.sendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_SYSTEM);

    }
    bool SendLoadPlan(string guid, byte pathNumber)
    {
        LoadPlan loadPlan = new LoadPlan();
        loadPlan.indexPath = pathNumber;
        guid = guid + "\0";
        return clientUnity.client.sendTwoPartCommand(loadPlan, guid);
    }
}
