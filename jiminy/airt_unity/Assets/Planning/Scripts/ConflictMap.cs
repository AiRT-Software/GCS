using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// This class goes on every map button inside a conflict gameobject. Only activated when a map with the same name has different dates locally and the server
/// </summary>
public class ConflictMap : MonoBehaviour {
    
    MapMetadata server;
    MapMetadata local;
    GameObject panel;
    ClientUnity clientUnity;
    public void setConflictingMaps(MapMetadata Server, MapMetadata Local, GameObject Panel, ClientUnity client)
    {
        server = Server;
        local = Local;
        panel = Panel;
        clientUnity = client;

    }
   public void ButtonClicked()
    {
        panel.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "The server file was modified on: " + server.Date.ToString() + "\n The local file was modified on: " + local.Date.ToString() + "\n Which do you want to keep?";
        panel.transform.GetChild(0).GetChild(1).GetComponent<Button>().onClick.AddListener(keepLocal);
        panel.transform.GetChild(0).GetChild(2).GetComponent<Button>().onClick.AddListener(keepServer);
        panel.SetActive(true);
   }
    //We ask the app to upload the metadata, mission and thumbnail to the server
    public void keepLocal()
    {
        if ((clientUnity != null) && (clientUnity.client != null) && clientUnity.client.isConnected)
        {
            PlanSelectionManager.uploadMapMetadata = true;
            MissionManager.guid = local.Guid;
            LibraryModule.substituteMap = true;
            clientUnity.client.SendCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_LIBRARY_PATH);

            panel.SetActive(false);
            transform.parent.GetChild(1).GetComponent<Image>().color = Color.yellow;
            StartCoroutine(waitForUpload());
        }

    }


    public IEnumerator waitForUpload()
    {
        while (SendJSONStateMachine.allFilesSent == false && PlanSelectionManager.uploadMapMetadata == true)
        {
            yield return new WaitForEndOfFrame();
        }
        transform.parent.GetChild(1).GetComponent<Image>().color = Color.green;
        PlanSelectionManager.uploadMapMetadata = false;
        SendJSONStateMachine.allFilesSent = false;
        gameObject.SetActive(false);


    }
    public IEnumerator waitForDownload()
    {
        while (MapLoader.mapDownloaded == false && PlanSelectionManager.askedForMaps == true)
        {
            yield return new WaitForEndOfFrame();
        }
        transform.parent.GetChild(1).GetComponent<Image>().color = Color.green;
        MapLoader.mapDownloaded = false;
        PlanSelectionManager.askedForMaps = true;
        gameObject.SetActive(false);


    }
    //We save the previously downloaded metadata and ask for the mission and thumbnail (mission will be asked once the thumbnail is downloaded)
    public void keepServer()
    {
        if ((clientUnity != null) && (clientUnity.client != null) && clientUnity.client.isConnected)
        {
            string json = JsonUtility.ToJson(server);
            File.WriteAllText(Application.persistentDataPath + "/PersistentData/Missions/" + server.Guid + ".json.metadata", json);
            clientUnity.client.sendTwoPartCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_THUMBNAIL, server.Guid + "\0");
            PlanSelectionManager.askedForMaps = true;
            panel.SetActive(false);
            transform.parent.GetChild(1).GetComponent<Image>().color = Color.yellow;
            gameObject.SetActive(false);
            StartCoroutine(waitForDownload());


        }
    }
}
