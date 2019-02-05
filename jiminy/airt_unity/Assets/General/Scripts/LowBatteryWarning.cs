using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// This class manages the low battery warning buttons and UI 
/// </summary>
public class LowBatteryWarning : MonoBehaviour {
    public RectTransform warningPanel,firstText, cancelButton, okButton, throbber, OkLandedButton, secondText,background ;
    ClientUnity clientUnity;
    public static bool sendLandingCommand = false;
    public static bool landed = false;

	public void Cancel()
    {
        warningPanel.gameObject.SetActive(false);
        //FCSModule.debug = true;
    }
	public void Ok()
    {
        cancelButton.gameObject.SetActive(false);
        okButton.gameObject.SetActive(false);
        throbber.gameObject.SetActive(true);
        firstText.GetComponent<Text>().text = "Warning, you have started \n an emergency landing";
        if (GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.Recording)
        {
            clientUnity.client.SendCommand((byte)Modules.PLAN_EXECUTOR_MODULE, (byte)PlanExecutorCommandType.PLAN_EXEC_EXIT);
        }
        sendLandingCommand = true;
    }
    public void OkLanded()
    {
        FCSModule.debug = true;

        UnityEngine.SceneManagement.SceneManager.LoadScene("General");
    }
    private void Start()
    {

        float width = background.sizeDelta.x;
        clientUnity = UnityEngine.GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        
        warningPanel.sizeDelta = new Vector2(Screen.width / 3, Screen.height / 3);
        //Position of the warning panel o nthe general scene where there is a canvas scaler
        okButton.anchoredPosition = new Vector2(-warningPanel.sizeDelta.x / 6, warningPanel.sizeDelta.y / 5);
        throbber.anchoredPosition = new Vector2(warningPanel.sizeDelta.x / 2, warningPanel.sizeDelta.y / 5);
        warningPanel.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
        
        OkLandedButton.anchoredPosition = new Vector2(-warningPanel.sizeDelta.x / 6, warningPanel.sizeDelta.y / 5);
        cancelButton.anchoredPosition = new Vector2(warningPanel.sizeDelta.x / 6, warningPanel.sizeDelta.y / 5);
        firstText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, warningPanel.sizeDelta.y / 8);
        secondText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, warningPanel.sizeDelta.y / 8);

        if (GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.General)
        {
            return;
        }
        //Else
        okButton.anchoredPosition = new Vector2(-warningPanel.sizeDelta.x / 2, warningPanel.sizeDelta.y / 6);
        throbber.anchoredPosition = new Vector2(warningPanel.sizeDelta.x / 2, warningPanel.sizeDelta.y / 5);
        warningPanel.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);

        OkLandedButton.anchoredPosition = new Vector2(-warningPanel.sizeDelta.x / 6, warningPanel.sizeDelta.y / 5);
        cancelButton.anchoredPosition = new Vector2(warningPanel.sizeDelta.x / 2, warningPanel.sizeDelta.y / 6);
        firstText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, warningPanel.sizeDelta.y / 3);
        secondText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, warningPanel.sizeDelta.y / 3);


        okButton.sizeDelta = new Vector2(warningPanel.sizeDelta.x / 2, warningPanel.sizeDelta.y / 4);
        OkLandedButton.sizeDelta = new Vector2(warningPanel.sizeDelta.x / 2, warningPanel.sizeDelta.y / 4);
        cancelButton.sizeDelta = new Vector2(warningPanel.sizeDelta.x / 2, warningPanel.sizeDelta.y / 4);
        okButton.GetChild(0).GetComponent<Text>().fontSize = (int)(warningPanel.sizeDelta.x / 15);
        OkLandedButton.GetChild(0).GetComponent<Text>().fontSize = (int)(warningPanel.sizeDelta.x / 15);
        cancelButton.GetChild(0).GetComponent<Text>().fontSize = (int)(warningPanel.sizeDelta.x / 15);
        throbber.sizeDelta = new Vector2(warningPanel.sizeDelta.x / 6, warningPanel.sizeDelta.y / 6);


        firstText.GetComponent<RectTransform>().sizeDelta = new Vector2(warningPanel.sizeDelta.x, warningPanel.sizeDelta.y / 3);
        firstText.GetComponent<Text>().fontSize = (int)(warningPanel.sizeDelta.x / 10);
        secondText.GetComponent<Text>().fontSize = (int)(warningPanel.sizeDelta.x / 10);
        secondText.GetComponent<RectTransform>().sizeDelta = new Vector2(warningPanel.sizeDelta.x, warningPanel.sizeDelta.y / 3);
        //warningPanel.sizeDelta = new Vector2(Screen.width, Screen.height);


    }
    // Update is called once per frame
    void Update () {
        if (throbber.gameObject.activeSelf)
        {
            throbber.transform.Rotate(new Vector3(0, 0, -180 * Time.deltaTime));

        }
        if (sendLandingCommand && landed)
        {
            throbber.gameObject.SetActive(false);
            firstText.gameObject.SetActive(false);
            warningPanel.GetComponent<Image>().color = new Vector4(1, 0, 0, 0);
            OkLandedButton.gameObject.SetActive(true);
            secondText.gameObject.SetActive(true);

        }
    }
}
