using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class handles some global variables fro mthe app
/// </summary>
public class ApplicationVariables : MonoBehaviour
{

    public static string persistentDataPath;
    public enum RemoveState{
        None = 0,
        LocalMapRemove,
        ServerMapRemove,
        PlanRemove
    }

    public static byte appRemoveState = (byte)RemoveState.None;
    public static GameObject removeGO;

    bool firstLoad = true;

    void Awake()
    {
        if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        persistentDataPath = Application.persistentDataPath + "/PersistentData/";
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

    }
    //Subscribes to the topics related to the scene
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentScene =scene.name;
        StateChangeSubscription(currentScene, true);
    }
    //Unsubscribes to the topics that the app subscribed before
    void OnSceneUnloaded(Scene scene)
    {
        string currentScene = scene.name;
        StateChangeSubscription(currentScene, false);
    }
    /// <summary>
    /// When the app is paused, we unsubcribe in order no to receive any petition from an inactive app
    /// </summary>
    /// <param name="pauseStatus"></param>
    void OnApplicationPause(bool pauseStatus)
    {
        if (firstLoad) {
            firstLoad = false;
            return;
        } 
        ClientUnity clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        //UnityEngine.Debug.Log(pauseStatus);

        string currentScene = SceneManager.GetActiveScene().name;
        if (pauseStatus) // Application is paused, unsuscribe depending on the scene;
        {
            StateChangeSubscription(currentScene, false);
        }
        else // Application resume, suscribe depending on the scene
        {
            StateChangeSubscription(currentScene, true);
        }
    }
    /// <summary>
    /// Subscribes or unsubscribes to a notification
    /// </summary>
    /// <param name="state">The scene</param>
    /// <param name="subscribe">If should subscribe or not</param>
    void StateChangeSubscription(string state, bool subscribe)
    {
        ClientUnity clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        switch (state)
        {
            //We don't unsubscribe from the topics subscribed at general
            case "General":
                byte[] generalTopics = { (byte)Modules.OS_NOTIFICATIONS_MODULE, (byte)Modules.STD_NOTIFICATIONS_MODULE, (byte)Modules.ATREYU_NOTIFICATIONS_MODULE, (byte)Modules.FCS_MULTIPLEXER_NOTIFICATIONS_MODULE, (byte)Modules.FPV_NOTIFICATIONS_MODULE, (byte)Modules.POSITIONING_NOTIFICATIONS_MODULE };
                if (subscribe){
                    UnityEngine.Debug.Log("Subscribing to " + state + " topics");
                    clientUnity.client.subscribeTo(generalTopics);
                }
                //else {
                //    UnityEngine.Debug.Log("Unsubscribing to " + state + " topics");
                //    clientUnity.client.unsubscribeTo(generalTopics);
                //}
                break;
            case "TagsConfiguration":
                byte[] tagsConfigTopics = { (byte)Modules.DRONE_NOTIFICATIONS_MODULE, (byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)Modules.FCS_MULTIPLEXER_NOTIFICATIONS_MODULE };
                if (subscribe){
                    UnityEngine.Debug.Log("Subscribing to " + state + " topics");
                    clientUnity.client.subscribeTo(tagsConfigTopics);
                }
                else {
                    UnityEngine.Debug.Log("Unsubscribing to " + state + " topics");
                    clientUnity.client.unsubscribeTo(tagsConfigTopics);
                }
                break;
            case "Calibration":
                byte[] calibrationTopics = { (byte)Modules.DRONE_NOTIFICATIONS_MODULE, (byte)Modules.POSITIONING_NOTIFICATIONS_MODULE, (byte)Modules.FCS_MULTIPLEXER_NOTIFICATIONS_MODULE };
                if (subscribe){
                    UnityEngine.Debug.Log("Subscribing to " + state + " topics");
                    clientUnity.client.subscribeTo(calibrationTopics);
                }
                else {
                    UnityEngine.Debug.Log("Unsubscribing to " + state + " topics");
                    clientUnity.client.unsubscribeTo(calibrationTopics);
                }
                break;
            case "PlanSelection":
                byte[] planSelectionTopics = {(byte)Modules.PLAN_LIBRARIAN_NOTIFICATIONS_MODULE, (byte)Modules.OS_NOTIFICATIONS_MODULE };
                if (subscribe){
                    UnityEngine.Debug.Log("Subscribing to " + state + " topics");
                    clientUnity.client.subscribeTo(planSelectionTopics);
                }
                else {
                    UnityEngine.Debug.Log("Unsubscribing to " + state + " topics");
                    clientUnity.client.unsubscribeTo(planSelectionTopics);
                }
                break;
            case "ModelAlignment":
                byte[] librarianTopics = {(byte)Modules.DRONE_NOTIFICATIONS_MODULE, (byte)Modules.PLAN_LIBRARIAN_NOTIFICATIONS_MODULE, (byte)Modules.OS_NOTIFICATIONS_MODULE };
                if (subscribe){
                    UnityEngine.Debug.Log("Subscribing to " + state + " topics");
                    clientUnity.client.subscribeTo(librarianTopics);
                }
                else {
                    UnityEngine.Debug.Log("Unsubscribing to " + state + " topics");
                    clientUnity.client.unsubscribeTo(librarianTopics);
                }
                break;
            case "Planning":
                break;
            case "Mapping":
                byte[] mappingTopics = { (byte)Modules.MAPPER_NOTIFICATIONS_MODULE, (byte)Modules.DRONE_NOTIFICATIONS_MODULE, (byte)Modules.PLAN_LIBRARIAN_NOTIFICATIONS_MODULE };
                if (subscribe){
                    UnityEngine.Debug.Log("Subscribing to " + state + " topics");
                    clientUnity.client.subscribeTo(mappingTopics);
                }
                else {
                    UnityEngine.Debug.Log("Unsubscribing to " + state + " topics");
                    clientUnity.client.unsubscribeTo(mappingTopics);
                }
                break;
            case "Recording":
                byte[] recordingTopics = {(byte)Modules.RCAM_NOTIFICATIONS_MODULE, (byte)Modules.PLAN_EXECUTOR_NOTIFICATIONS_MODULE, (byte)Modules.FPV_NOTIFICATIONS_MODULE, (byte)Modules.GIMBAL_MULTIPLEXER_NOTIFICATIONS_MODULE, (byte)Modules.DRONE_NOTIFICATIONS_MODULE};
                if (subscribe){
                    UnityEngine.Debug.Log("Subscribing to " + state + " topics");
                    clientUnity.client.subscribeTo(recordingTopics);
                }
                else {
                    UnityEngine.Debug.Log("Unsubscribing to " + state + " topics");
                    clientUnity.client.unsubscribeTo(recordingTopics);
                }
                break;
            default: 
                UnityEngine.Debug.LogWarning("Scene not identified! " + state);
                break;
        }
    }
    
}
