using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NetMQ;
using System;

public class MissionExecutorModule {

    ClientUnity clientUnity;

    public MissionExecutorModule()
    {
        clientUnity = UnityEngine.GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)PlanExecutorNotificationType.PLAN_EXEC_REGISTRATION_MATRIX_CHANGED_NOTIFICATION , onRegistrationMatrixChanged));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)PlanExecutorNotificationType.PLAN_EXEC_PLAN_LOADING_NOTIFICATION, onPlanExecPlanLoading));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)PlanExecutorNotificationType.PLAN_EXEC_PLAN_LOADED_NOTIFICATION, onPlanExecPlanLoaded));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)PlanExecutorNotificationType.PLAN_EXEC_PREFLIGHT_TESTING_NOTIFICATION, onPreflightTestStart));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)PlanExecutorNotificationType.PLAN_EXEC_FCS_NOT_RESPONDING_NOTIFICATION, onPlanExecNotResponding));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)PlanExecutorNotificationType.PLAN_EXEC_PREFLIGHT_TESTS_FAILED_NOTIFICATION, onPreflightFailed));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)PlanExecutorNotificationType.PLAN_EXEC_ERROR_LOADING_PLAN_NOTIFICATION, onErrorLoadingPlan));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)PlanExecutorNotificationType.PLAN_EXEC_READY_TO_FLIGHT_NOTIFICATION, onReadyToFlight));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)PlanExecutorNotificationType.PLAN_EXEC_FLIGHT_PLAN_COMPLETED_NOTIFICATION, onPlanCompleted));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)PlanExecutorNotificationType.PLAN_EXEC_FIRST_WAYPOINT_IS_TOO_FAR_NOTIFICATION, onFirstWaypointIsTooFar));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)PlanExecutorNotificationType.PLAN_EXEC_FLYING_TO_NEXT_WP_NOTIFICATION, onFlyingToNextWp));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)PlanExecutorNotificationType.PLAN_EXEC_CURRENT_FLIGHT_PLAN_NOTIFICATION, onCurrentPlanReceived));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)PlanExecutorNotificationType.PLAN_EXEC_LAUNCHED_NOTIFICATION, onPlanLaunched));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)PlanExecutorNotificationType.PLAN_EXEC_REACHED_WAYPOINT_NOTIFICATION, onReachedWp));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)PlanExecutorNotificationType.PLAN_EXEC_LANDING_NOTIFICATION, onLanding));

        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)NotificationType.INVALID_STATUS, onInvalidStatus));
        
    }

    public void onErrorLoadingPlan(NetMQMessage m)
    {

        //UnityEngine.Debug.Log("Loading plan failed" + System.Text.Encoding.ASCII.GetString(m[1].Buffer));
        SendJSONStateMachine.state = SendJSONStateMachine.SendJSonStates.IDLE;

    }
    public void onPlanExecNotResponding(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Preflight tests failed:" + System.Text.Encoding.ASCII.GetString(m[1].Buffer));

    }
    public void onPreflightFailed(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Plan execution not responding");

    }
    public void onRegistrationMatrixChanged(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Registration Matrix Changed Notification");

    }
    public void onPreflightTestStart(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Preflight tests start...");

    }
    public void onPlanExecPlanLoading(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onPlanExecPlanLoading");
        //byte planIndex = m[1].Buffer[0];
        //clientUnity.client.sendTwoPartCommand(UIRecordingManager.registrationMatrix);
        
    }
    public void onPlanExecPlanLoaded(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Plan loaded ");
        //If the plan is loaded, we activate the take off button
        FlyButtonStateMachine.state = FlyButtonStateMachine.buttonState.ACTIVATED;

        //MonoBehaviour.StartCoroutine(sendStartExec());  start 


    }
    public void onReadyToFlight(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Starting Flight");
        //If the drone starts flying, we change to this state
        UIRecordingManager.states = UIRecordingManager.FlyingStates.STARTFLYING;

    }
    public void onPlanCompleted(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Plan Completed");
        //If the drone reached its destination, we activate the button that comes back to general
        UIRecordingManager.flightEnded = true;

    }
    public void onFirstWaypointIsTooFar(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("First Waypoint Is Too Far");

    }
    public void onInvalidStatus(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Invalid Status");

    }

    public void onFlyingToNextWp(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("OnFlyingToNextWP");
    }
    public void onCurrentPlanReceived(NetMQMessage m)
    {
        //This message is received if a tablet is not in recording but the drone is in recording
        if (GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.Recording)
        {
            return;
        }
        MissionManager.planIndex = BitConverter.ToUInt16(m[0].Buffer, 4);
        MissionManager.guid = System.Text.Encoding.ASCII.GetString(m[1].Buffer);
        MissionManager.guid = MissionManager.guid.Substring(0, MissionManager.guid.Length - 1);
        //UnityEngine.Debug.Log("Plan index: " + MissionManager.planIndex + " Guid: " + MissionManager.guid);
        clientUnity.goToRecording = true;
    }

    public void onPlanLaunched(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Mission launched!");
    }

    public void onReachedWp(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Waypoint reached, moving to the next!");
    }

    public void onLanding(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("Landing started!");
    }
}

public class LoadPlan 
{
    public AIRT_Message_Header PlanHeader = new AIRT_Message_Header((byte)Modules.PLAN_EXECUTOR_MODULE, (byte)PlanExecutorCommandType.PLAN_EXEC_LOAD_PLAN);

    public byte indexPath; 
}

public class RegistrationMatrix
{
    public AIRT_Message_Header MatrixHeader = new AIRT_Message_Header((byte)Modules.PLAN_EXECUTOR_MODULE, (byte)PlanExecutorCommandType.PLAN_EXEC_SET_REGISTRATION_MATRIX);
    public byte rowMajor;
    public float[] elems = new float[16];
}