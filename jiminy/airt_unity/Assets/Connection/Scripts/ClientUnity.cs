using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;  //Func, Action
using System.IO;

using NetMQ;

using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientUnity : MonoBehaviour
{
    // gui
    //public GameObject drone;
    //public GameObject prefab_cloud;
    //public GameObject rootcloud;
    //public ComputeShader ref_compshader;
    //CompShader compshader;


    // Module instances
    AtreyuModule atreyuManager;
    OSModule osManager;
    StandardModule stdManager;
    FPVModule fpvManager;
    LibraryModule libraryManager;
    MissionExecutorModule missionExecutorManager;
    PozyxModule pozyxManager;
    FCSModule FCS_Module;
    MapperModule pointCloudManager;
    DroneModule droneManager;
    RecCamModule recCamModule;
    public bool goToRecording = false;
    public bool ipsupdated = false;
    public bool currentlyreceiving = false;
    // airt
    Thread cli_thread;
    public Client client;

    public List<MsgHandler> mhandlers = new List<MsgHandler>();
    bool tryReconnect = false;

    // Use this for initialization
    void Awake()
    {
        //If clientunity object is already present on the scene, the new one gets destroyed
        if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        //The first ClientUnity is common for every scene
        DontDestroyOnLoad(gameObject);

        //FileInfo fileInfo = new System.IO.FileInfo(Application.dataPath + "/Resources/Update/airt-project-0.1.1-.deb");
        //FileStream fs = File.OpenRead(Application.dataPath + "/Resources/Update/airt-project-0.1.1-.deb");
        //ulong fileSize = (ulong)fileInfo.Length;
        //UnityEngine.Debug.Log("File size: " + fileSize);
        //byte[] fileBytes = new byte[4096];
        //
        //BinaryReader br = new BinaryReader(fs);
        //
        //
        //ulong bytesRead = 0;
        //while (bytesRead < fileSize)
        //{
        //    int readCount = br.Read(fileBytes, (int)0, 4096);
        //    bytesRead += 4096;
        //    UnityEngine.Debug.Log("READING");
        //}
        //UnityEngine.Debug.Log("END");
        //Drone ip
        if (GlobalSettings.Instance.getIP() == "")
            //GlobalSettings.Instance.setIP("10.42.0.101");
            GlobalSettings.Instance.setIP("192.168.8.1");

            //GlobalSettings.Instance.setIP("192.168.8.205");
            //GlobalSettings.Instance.setIP("158.42.154.131");
        //Every module
        client = new Client();

        stdManager = new StandardModule();
        atreyuManager = new AtreyuModule();
        osManager = new OSModule(FileManager.debFilePath, FileManager.debFileName + ".deb");
        fpvManager = new FPVModule();
        FCS_Module = new FCSModule();
        libraryManager = new LibraryModule();
        missionExecutorManager = new MissionExecutorModule();
        pozyxManager = new PozyxModule();
        pointCloudManager = new MapperModule();
        droneManager = new DroneModule();
        recCamModule = new RecCamModule();
        //mhandlers.Add(new MsgHandler((byte)Modules.POINTCLOUD_MODULE, (byte)PointcloudNotificationType.PCL_SINGLE, onPclData));  // data

        

        cli_thread = new Thread(() => client.run(mhandlers));
        cli_thread.Start();
        //We start quering atreyu state to see if the app should jump to mapping or recording, as the drone can't do anything other than mapping or recording if it is
        //is those states
        if (client.isConnected)
        {
            tryReconnect = false;
            client.SendCommand((byte)Modules.ATREYU_MODULE, (byte)AtreyuCommandType.ATREYU_QUERY_SYSTEM_STATE);
        }
        else
            tryReconnect = true;
        //client.sendCommand((byte)Modules.ATREYU_MODULE, (byte)AtreyuCommandType.ATREYU_QUERY_SERVER_VERSION);
            
    }
   
    private void Update()
    {
        //Check if the drone is in recording state
        if (goToRecording)
        {
            goToRecording = false;
            SceneManager.LoadScene("Recording");
        }
        //If at the start the app didn't connect to the drone, it will retry
        if (tryReconnect)
        {
            tryReconnect = false;
            client.SendCommand((byte)Modules.ATREYU_MODULE, (byte)AtreyuCommandType.ATREYU_QUERY_SYSTEM_STATE);
        }
    }

    void OnApplicationQuit()
    {
        // ensure stopping when closing the app
        //client.sendCommand((byte)Modules.STD_COMMANDS_MODULE, (byte)CommandType.STOP);
        //When the app is closed, we unsubscribe from all the toppics and close the thread
        Thread.Sleep(TimeSpan.FromMilliseconds(200));
        client.unSuscribeAll();
        client.stopReceiving();

        if (!cli_thread.Join(TimeSpan.FromMilliseconds(10)))
        {
            UnityEngine.Debug.Log("ClientUnity: cli_thread did not join");
        }
        else
        {
            UnityEngine.Debug.Log("ClientUnity: cli_thread joined");
        }
    }

    // pointcloud, the modules were changed to mapper modules, so these are not used
    public void onPclStarted(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onPclStarted");
    }
    public void onPclStopped(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onPclStopped");
    }
    public void onPclQuitting(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("onPclQuitting");
    }
       

}
