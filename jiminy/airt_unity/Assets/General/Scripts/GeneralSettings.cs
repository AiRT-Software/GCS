using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GeneralSettings : MonoBehaviour {

    ClientUnity clientUnity;

    // About and settings panels
    public GameObject aboutPanel, settingsPanel1, settingsPanel2;

    //Settings elements
    public Button powerOffOCSButton; 
    public Text OCSState;
    public GameObject recCamControl;
    // About elements
    public Text versionTextAbout, versionTextSettings1, versionTextSettings2;
    public GameObject gimbalControl;
    public GameObject goToDifferentConfigsPanel;
    public GameObject fcControl;

    void Start()
    {
        GameObject finder = GameObject.Find("ClientUnity");
        if (finder)
            clientUnity = finder.GetComponent<ClientUnity>();
    }

    void Update()
    {

        if (settingsPanel1.activeSelf && StandardModule.OCSAlive)
        {
            OCSState.text = "OCS Alive";
            powerOffOCSButton.interactable = true;
        }
        //This writes the server version if the client is connected
        if (AtreyuModule.versionUpdated)
        {
            Vector3 version = GetVersion();

            versionTextAbout.text = "Client version: " + version.x + "." + version.y + "." + version.z + "\n" + "Server Version " + AtreyuModule.majorV + "." + AtreyuModule.minorV + "." + AtreyuModule.patchV;

        }

            
    }
    //This is not used anymore
    public void SettingsPanel1()
    {
        UnityEngine.Debug.Log("SettingsPanel1");
        if (transform.parent.GetComponent<Canvas>())
            transform.parent.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        settingsPanel1.SetActive(true);
        settingsPanel2.SetActive(false);
        aboutPanel.SetActive(false);
        
    }
    //This is not used anymore

    public void SettingsPanel2()
    {
        settingsPanel1.SetActive(false);
        settingsPanel2.SetActive(true);
        aboutPanel.SetActive(false);
    }
    //This shows the credits
    public void AboutPanel()
    {
        //UnityEngine.Debug.Log("AboutPanel");
        if (transform.parent.GetComponent<Canvas>())
            transform.parent.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        settingsPanel1.SetActive(false);
        settingsPanel2.SetActive(false);
        aboutPanel.SetActive(true);
        Vector3 version = GetVersion();
        versionTextAbout.text = "Client version: " + version.x + "." + version.y + "." + version.z +"\n" + "Server Version " + AtreyuModule.majorV + "." + AtreyuModule.minorV + "." + AtreyuModule.patchV;
        versionTextSettings1.text = "Version: " + version.x + "." + version.y + "." + version.z;
        versionTextSettings2.text = "Version: " + version.x + "." + version.y + "." + version.z;

    }
    //This is in the cross on the about panel 
    public void ClosePanels()
    {
        if (transform.parent.GetComponent<Canvas>())
            transform.parent.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        settingsPanel1.SetActive(false);
        settingsPanel2.SetActive(false);
        aboutPanel.SetActive(false);
    }
    //Powers off the ocs. Called from the button
    public void PowerOffOCS()
    {
        clientUnity.client.SendCommand((byte)Modules.STD_COMMANDS_MODULE, (byte)CommandType.POWEROFF);
        powerOffOCSButton.interactable = false;
        OCSState.text = "OCS Off";
    }

    public void OCSAlive()
    {

    }

    public Vector3 GetVersion()
    {
        return new Vector3((byte)AIRTVersion.MAJOR, (byte)AIRTVersion.MINOR, (byte)AIRTVersion.PATCH);
    }

    public void StopQuit()
    {
        StartCoroutine(StopAndQuit());
    }

    
    IEnumerator StopAndQuit()
    {
        //This is not used, and if it was used, be careful when calling stop and quit
        clientUnity.client.SendCommand((byte)Modules.MAPPER_MODULE, (byte)MapperCommandType.MAPPER_PAUSE_MAPPING);
        yield return new WaitForSeconds(0.5f);
        clientUnity.client.SendCommand((byte)Modules.STD_COMMANDS_MODULE, (byte)CommandType.STOP);
        yield return new WaitForSeconds(1.0f);
        clientUnity.client.SendCommand((byte)Modules.STD_COMMANDS_MODULE, (byte)CommandType.QUIT);
        clientUnity.client.unSuscribeAll();
        Application.Quit();
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
#endif
    }
    //Open the gimball config screen
    public void OpenGimbalConfig()
    {
        aboutPanel.SetActive(false);
        gimbalControl.SetActive(true);
    }
    //Open the reccam config screen

    public void enterRecCamState()
    {
        recCamControl.SetActive(true);
        aboutPanel.SetActive(false);
    }
    //Open the gotodifferentconfigs window
    public void enterButtonsConfigurations()
    {
        goToDifferentConfigsPanel.SetActive(true);
        Vector3 version = GetVersion();
        versionTextAbout.text = "Client version: " + version.x + "." + version.y + "." + version.z + "\n" + "Server Version " + AtreyuModule.majorV + "." + AtreyuModule.minorV + "." + AtreyuModule.patchV;
        versionTextSettings1.text = "Version: " + version.x + "." + version.y + "." + version.z;
        versionTextSettings2.text = "Version: " + version.x + "." + version.y + "." + version.z;
    }
    public void exitButtonsConfigurations()
    {
        goToDifferentConfigsPanel.SetActive(false);
    }
    //Opens RC window
    public void enterFcControlState()
    {
        fcControl.SetActive(true);
    }
}
