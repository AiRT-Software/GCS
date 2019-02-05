using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class CalibrationSettings : MonoBehaviour{

    public GameObject anchorList, anchorsCam, canvasCam, anchorCubeParent;
    public Button confirmManual, editConfig, redoAutocalibration, confirmAuto;

    public static string anchorClicked = "";

    public RectTransform autocalibratingThrobber;
    RectTransform anchor1, anchor2, anchor3, anchor4, anchor5, anchor6, anchor7, anchor8;
    
    Vector2 localCursor;
    float height = 0.0f;
    int fontSize;

    Action<string, Transform> action;

    public static ServerMessages.IPSFrameAnchorData[] anchorConfigData = new ServerMessages.IPSFrameAnchorData[8];
    ClientUnity clientUnity;

    void Awake() {
        //Finding the clientunity as always
        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();

        action = AnchorSelect;

        height = Screen.height / 20;
        fontSize = (int)(Screen.width * 0.018f);

        GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 2, Screen.height / 2);
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0, Screen.height / 4);
        //Resizing anchorlist and its kids

        anchorList.GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.width * 0.6f, -Screen.height / 4);
        anchorList.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width * 0.3f, height * 9 + 48);
        foreach (Transform child in anchorList.transform)
        {
            child.GetComponent<RectTransform>().sizeDelta = new Vector2(0, height);
        }

        for (int i = 1; i < 9; i++)
        {
            int j = i;
            anchorList.transform.GetChild(i).GetChild(1).GetComponent<InputField>().onEndEdit.AddListener(delegate { InputTextModifiedX(anchorList.transform.GetChild(j).GetChild(1).GetComponent<InputField>().text, anchorList.transform.GetChild(j).GetChild(1).parent ); });
            anchorList.transform.GetChild(i).GetChild(2).GetComponent<InputField>().onEndEdit.AddListener(delegate { InputTextModifiedY(anchorList.transform.GetChild(j).GetChild(2).GetComponent<InputField>().text, anchorList.transform.GetChild(j).GetChild(2).parent); });
            anchorList.transform.GetChild(i).GetChild(3).GetComponent<InputField>().onEndEdit.AddListener(delegate { InputTextModifiedZ(anchorList.transform.GetChild(j).GetChild(3).GetComponent<InputField>().text, anchorList.transform.GetChild(j).GetChild(3).parent); });
        }
        //Assigning the recttransform of each child in order to save calls to getcomponent
        anchor1 = anchorList.transform.GetChild(1).GetChild(0).gameObject.GetComponent<RectTransform>();
        anchor2 = anchorList.transform.GetChild(2).GetChild(0).gameObject.GetComponent<RectTransform>();
        anchor3 = anchorList.transform.GetChild(3).GetChild(0).gameObject.GetComponent<RectTransform>();
        anchor4 = anchorList.transform.GetChild(4).GetChild(0).gameObject.GetComponent<RectTransform>();
        anchor5 = anchorList.transform.GetChild(5).GetChild(0).gameObject.GetComponent<RectTransform>();
        anchor6 = anchorList.transform.GetChild(6).GetChild(0).gameObject.GetComponent<RectTransform>();
        anchor7 = anchorList.transform.GetChild(7).GetChild(0).gameObject.GetComponent<RectTransform>();
        anchor8 = anchorList.transform.GetChild(8).GetChild(0).gameObject.GetComponent<RectTransform>();

        //anchor1.sizeDelta = new Vector2(0, height);
        //anchor2.sizeDelta = new Vector2(0, height);
        //anchor3.sizeDelta = new Vector2(0, height);
        //anchor4.sizeDelta = new Vector2(0, height);
        //anchor5.sizeDelta = new Vector2(0, height);
        //anchor6.sizeDelta = new Vector2(0, height);
        //anchor7.sizeDelta = new Vector2(0, height);
        //anchor8.sizeDelta = new Vector2(0, height);
        //Adding the function to each button to assign the id to each anchor box
        anchor1.GetComponent<Button>().onClick.AddListener(() => AnchorSelect(anchor1.name, anchor1.parent));
        anchor2.GetComponent<Button>().onClick.AddListener(() => AnchorSelect(anchor2.name, anchor2.parent));
        anchor3.GetComponent<Button>().onClick.AddListener(() => AnchorSelect(anchor3.name, anchor3.parent));
        anchor4.GetComponent<Button>().onClick.AddListener(() => AnchorSelect(anchor4.name, anchor4.parent));
        anchor5.GetComponent<Button>().onClick.AddListener(() => AnchorSelect(anchor5.name, anchor5.parent));
        anchor6.GetComponent<Button>().onClick.AddListener(() => AnchorSelect(anchor6.name, anchor6.parent));
        anchor7.GetComponent<Button>().onClick.AddListener(() => AnchorSelect(anchor7.name, anchor7.parent));
        anchor8.GetComponent<Button>().onClick.AddListener(() => AnchorSelect(anchor8.name, anchor8.parent));

        anchor1.transform.GetChild(0).GetComponent<Text>().fontSize = fontSize;
        anchor2.transform.GetChild(0).GetComponent<Text>().fontSize = fontSize;
        anchor3.transform.GetChild(0).GetComponent<Text>().fontSize = fontSize;
        anchor4.transform.GetChild(0).GetComponent<Text>().fontSize = fontSize;
        anchor5.transform.GetChild(0).GetComponent<Text>().fontSize = fontSize;
        anchor6.transform.GetChild(0).GetComponent<Text>().fontSize = fontSize;
        anchor7.transform.GetChild(0).GetComponent<Text>().fontSize = fontSize;
        anchor8.transform.GetChild(0).GetComponent<Text>().fontSize = fontSize;

    }

    public void AnchorSelect(string id, Transform parent)
    {
        //When a anchor button with the id is clicked, this function is activated
        bool full = true;
        bool positioned = true;

        GameObject anchor = GameObject.Find(anchorClicked);
        //This function assigns an id to a anchor button
        if (anchor && (("0x" + int.Parse(id).ToString("x")) != anchor.transform.GetChild(0).GetComponent<TextMesh>().text))
        {
            //When assigning an anchor, if autocalib is enabled, it will assign the autos, and pos where they are needed
            if (AnchorsCalibration.autoCalib)
            {
                if ((anchorClicked.Substring(6, 1) == "1") || (anchorClicked.Substring(6, 1) == "5"))
                {
                    parent.GetChild(1).gameObject.GetComponent<InputField>().interactable = true;
                    parent.GetChild(1).gameObject.GetComponent<InputField>().placeholder.GetComponent<Text>().text = "Pos X";
                    parent.GetChild(1).gameObject.GetComponent<InputField>().text = "0";
                    parent.GetChild(2).gameObject.GetComponent<InputField>().interactable = true;
                    parent.GetChild(2).gameObject.GetComponent<InputField>().placeholder.GetComponent<Text>().text = "Pos Y";
                    parent.GetChild(2).gameObject.GetComponent<InputField>().text = "0";
                }

                if ((anchorClicked.Substring(6, 1) == "2") || (anchorClicked.Substring(6, 1) == "6"))
                {
                    parent.GetChild(1).gameObject.GetComponent<InputField>().interactable = false;
                    parent.GetChild(1).gameObject.GetComponent<InputField>().placeholder.GetComponent<Text>().text = "AUTO";
                    parent.GetChild(1).gameObject.GetComponent<InputField>().text = "";
                    parent.GetChild(2).gameObject.GetComponent<InputField>().interactable = true;
                    parent.GetChild(2).gameObject.GetComponent<InputField>().placeholder.GetComponent<Text>().text = "Pos Y";
                    parent.GetChild(2).gameObject.GetComponent<InputField>().text = "0";
                }

                if ((anchorClicked.Substring(6, 1) == "3") || (anchorClicked.Substring(6, 1) == "7"))
                {
                    parent.GetChild(1).gameObject.GetComponent<InputField>().interactable = false;
                    parent.GetChild(1).gameObject.GetComponent<InputField>().placeholder.GetComponent<Text>().text = "AUTO";
                    parent.GetChild(1).gameObject.GetComponent<InputField>().text = "";
                    parent.GetChild(2).gameObject.GetComponent<InputField>().interactable = false;
                    parent.GetChild(2).gameObject.GetComponent<InputField>().placeholder.GetComponent<Text>().text = "AUTO";
                    parent.GetChild(2).gameObject.GetComponent<InputField>().text = "";
                }

                if ((anchorClicked.Substring(6, 1) == "4") || (anchorClicked.Substring(6, 1) == "8"))
                {
                    parent.GetChild(1).gameObject.GetComponent<InputField>().interactable = true;
                    parent.GetChild(1).gameObject.GetComponent<InputField>().placeholder.GetComponent<Text>().text = "Pos X";
                    parent.GetChild(1).gameObject.GetComponent<InputField>().text = "0";
                    parent.GetChild(2).gameObject.GetComponent<InputField>().interactable = false;
                    parent.GetChild(2).gameObject.GetComponent<InputField>().placeholder.GetComponent<Text>().text = "AUTO";
                    parent.GetChild(2).gameObject.GetComponent<InputField>().text = "";
                }
            }
            //Gets the x, y, z of the anchor button pressed
            int x, y, z;
            if (!int.TryParse(parent.GetChild(1).GetComponent<InputField>().text, out x) && parent.GetChild(1).GetComponent<InputField>().interactable) { 
                UnityEngine.Debug.Log("Unable to parse X value");
                x = 0;
                positioned = false;
            }
            if (!int.TryParse(parent.GetChild(2).GetComponent<InputField>().text, out y) && parent.GetChild(2).GetComponent<InputField>().interactable)
            { 
                UnityEngine.Debug.Log("Unable to parse Y value");
                y = 0;
                positioned = false;
            }
            if (!int.TryParse(parent.GetChild(3).GetComponent<InputField>().text, out z) && parent.GetChild(3).GetComponent<InputField>().interactable)
            { 
                UnityEngine.Debug.Log("Unable to parse Z value");
                z = 0;
                positioned = false;
            }
            //Assigns the 3d box to the position selected
            anchor.transform.GetChild(1).localPosition = new Vector3(x, y, z);

            UnityEngine.Debug.Log(anchorClicked.Substring(0, anchorClicked.Length - 4));
            if (positioned)
                GameObject.Find(id).GetComponent<Image>().color = new Vector4(0, 255, 0, 150);
            else
                GameObject.Find(id).GetComponent<Image>().color = new Vector4(255, 255, 0, 150);
            //iterate through the anchor list
            for (int i = 1; i < 9; i++)
            {
                //checks if the box id == button id
                if (("0x" + int.Parse(anchorList.transform.GetChild(i).GetChild(0).name).ToString("x")) == anchor.transform.GetChild(0).GetComponent<TextMesh>().text) { 
                    anchorList.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new Vector4(255, 255, 255, 150);
                    if (AnchorsCalibration.autoCalib) { 
                        anchorList.transform.GetChild(i).GetChild(1).GetComponent<InputField>().interactable = true;
                        anchorList.transform.GetChild(i).GetChild(1).GetComponent<InputField>().placeholder.GetComponent<Text>().text = "Pos X";
                        anchorList.transform.GetChild(i).GetChild(1).GetComponent<InputField>().text = "0";
                        anchorList.transform.GetChild(i).GetChild(2).GetComponent<InputField>().interactable = true;
                        anchorList.transform.GetChild(i).GetChild(2).GetComponent<InputField>().placeholder.GetComponent<Text>().text = "Pos Y";
                        anchorList.transform.GetChild(i).GetChild(2).GetComponent<InputField>().text = "0";
                    }
                }

                if ((Vector4)anchorList.transform.GetChild(i).GetChild(0).GetComponent<Image>().color != new Vector4(0, 255, 0, 150))
                    full = false;

                if (full)
                {
                    UnityEngine.Debug.Log("All IDs are asigned (button)");
                    confirmManual.interactable = true;
                    confirmManual.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                }
                else
                {
                    UnityEngine.Debug.Log("All IDs are not asigned (button)");
                    confirmManual.interactable = false;
                    confirmManual.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 0.4f);
                }
            }

            for (int i = 0; i < 8; i++)
            {
                if (("0x" + int.Parse(id).ToString("x")) == anchorCubeParent.transform.GetChild(i).GetChild(0).GetComponent<TextMesh>().text)
                    anchorCubeParent.transform.GetChild(i).GetChild(0).GetComponent<TextMesh>().text = "";

            }

            anchor.transform.GetChild(0).GetComponent<TextMesh>().text = "0x" + int.Parse(id).ToString("x");
        }
        //anchorList.SetActive(false);
    }

    public void InputTextModifiedX(string xValue, Transform parent)
    {
        //function called each time an anchor textox is modified
        //UnityEngine.Debug.Log(xValue);
        int xParsed = 0, yParsed = 0, zParsed = 0;
        bool yCheck = true, zCheck = true, xCheck = true;
        bool anchorIdentified = false;
        bool full = true;

        if (!int.TryParse(xValue, out xParsed))
        {
            UnityEngine.Debug.Log("Unable to parse X Value");
            xCheck = false;
        }

        if(!int.TryParse(parent.GetChild(2).GetComponent<InputField>().text, out yParsed))
            yCheck = false;

        if(!int.TryParse(parent.GetChild(3).GetComponent<InputField>().text, out zParsed))
            zCheck = false;

        for (int i = 0; i < 8; i++)
        {
            if (anchorCubeParent.transform.GetChild(i).GetChild(0).GetComponent<TextMesh>().text == ("0x" + int.Parse(parent.GetChild(0).name).ToString("x"))) { 
                anchorCubeParent.transform.GetChild(i).GetChild(1).localPosition = new Vector3(xParsed, anchorCubeParent.transform.GetChild(i).GetChild(1).localPosition.y, anchorCubeParent.transform.GetChild(i).GetChild(1).localPosition.z);
                anchorIdentified = true;
            }
        }

        //UnityEngine.Debug.Log(parent.GetChild(0).name);
        //UnityEngine.Debug.Log(anchorCubeParent.transform.GetChild(1).GetChild(0).GetComponent<TextMesh>().text);

        if (AnchorsCalibration.autoCalib && (("0x"+(int.Parse(parent.GetChild(0).name).ToString("x")) == anchorCubeParent.transform.GetChild(3).GetChild(0).GetComponent<TextMesh>().text) || ("0x"+(int.Parse(parent.GetChild(0).name).ToString("x")) == anchorCubeParent.transform.GetChild(7).GetChild(0).GetComponent<TextMesh>().text)))
            yCheck = true;
        //Checks if the x, y , z values are assigned to an anchor, and if they are, it makes the anchor button green, if none are assigned of the button is not identified it's grey and 
        // if not orange
        if (xCheck && yCheck && zCheck && anchorIdentified)
            parent.GetChild(0).GetComponent<Image>().color = new Vector4(0, 255, 0, 150);
        else if (!xCheck && !yCheck && !zCheck && !anchorIdentified)
            parent.GetChild(0).GetComponent<Image>().color = new Vector4(255, 255, 255, 150);
        else
            parent.GetChild(0).GetComponent<Image>().color = new Vector4(255, 255, 0, 150);

        for (int i = 1; i < 9; i++)
        {
            //If anchor button is not green
            if ((Vector4)anchorList.transform.GetChild(i).GetChild(0).GetComponent<Image>().color != new Vector4(0, 255, 0, 150))
                full = false;

            if (full)
            {
                //All green, so we can send the configuration
                confirmManual.interactable = true;
                confirmManual.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            }
            else
            {
                confirmManual.interactable = false;
                confirmManual.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 0.4f);
            }
        }

        //parent.GetChild(0).GetChild(0).name
        //GameObject anchor = GameObject.Find(anchorClicked);
        
        //anchor.transform.GetChild(1).position = new Vector3(xParsed, anchor.transform.GetChild(1).position.y, anchor.transform.GetChild(1).position.z);
    }

    public void InputTextModifiedY(string yValue, Transform parent)
    {
        //Same as above but for inputfields on Y
        int yParsed = 0, xParsed = 0, zParsed = 0;
        bool xCheck = true, yCheck = true, zCheck = true;
        bool anchorIdentified = false;
        bool full = true;
        //UnityEngine.Debug.Log(yValue);
        //GameObject anchor = GameObject.Find(anchorClicked);
        if (!int.TryParse(yValue, out yParsed))
        {
            UnityEngine.Debug.Log("Unable to parse Y Value");
            yCheck = false;
        }

        if (!int.TryParse(parent.GetChild(1).GetComponent<InputField>().text, out xParsed))
            xCheck = false;

        if (!int.TryParse(parent.GetChild(3).GetComponent<InputField>().text, out zParsed))
            zCheck = false;

        for (int i = 0; i < 8; i++)
        {
            if (anchorCubeParent.transform.GetChild(i).GetChild(0).GetComponent<TextMesh>().text == ("0x" + int.Parse(parent.GetChild(0).name).ToString("x"))) { 
                anchorCubeParent.transform.GetChild(i).GetChild(1).localPosition = new Vector3(anchorCubeParent.transform.GetChild(i).GetChild(1).localPosition.x, yParsed, anchorCubeParent.transform.GetChild(i).GetChild(1).localPosition.z);
                anchorIdentified = true;
            }
        }

        if (AnchorsCalibration.autoCalib && (("0x" + (int.Parse(parent.GetChild(0).name).ToString("x")) == anchorCubeParent.transform.GetChild(1).GetChild(0).GetComponent<TextMesh>().text) || (("0x" + int.Parse(parent.GetChild(0).name).ToString("x")) == anchorCubeParent.transform.GetChild(5).GetChild(0).GetComponent<TextMesh>().text)))
            xCheck = true;

        if (xCheck && yCheck && zCheck && anchorIdentified)
            parent.GetChild(0).GetComponent<Image>().color = new Vector4(0, 255, 0, 150);
        else if (!xCheck && !yCheck && !zCheck && !anchorIdentified)
            parent.GetChild(0).GetComponent<Image>().color = new Vector4(255, 255, 255, 150);
        else
            parent.GetChild(0).GetComponent<Image>().color = new Vector4(255, 255, 0, 150);

        for (int i = 1; i < 9; i++)
        {
            if ((Vector4)anchorList.transform.GetChild(i).GetChild(0).GetComponent<Image>().color != new Vector4(0, 255, 0, 150))
                full = false;

            if (full)
            {
                confirmManual.interactable = true;
                confirmManual.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            }
            else
            {
                confirmManual.interactable = false;
                confirmManual.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 0.4f);
            }
        }
        //anchor.transform.GetChild(1).position = new Vector3(anchor.transform.GetChild(1).position.x, yParsed, anchor.transform.GetChild(1).position.z);
    }

    public void InputTextModifiedZ(string zValue, Transform parent)
    {
        //And on Z
        int zParsed = 0, xParsed = 0, yParsed = 0;
        bool xCheck = true, yCheck = true, zCheck = true;
        bool anchorIdentified = false;
        bool full = true;
        //GameObject anchor = GameObject.Find(anchorClicked);
        if (!int.TryParse(zValue, out zParsed))
        {
            UnityEngine.Debug.Log("Unable to parse Z Value");
            zCheck = false;
        }

        if (!int.TryParse(parent.GetChild(1).GetComponent<InputField>().text, out xParsed))
            xCheck = false;

        if (!int.TryParse(parent.GetChild(2).GetComponent<InputField>().text, out yParsed))
            yCheck = false;

        for (int i = 0; i < 8; i++)
        {
            if (anchorCubeParent.transform.GetChild(i).GetChild(0).GetComponent<TextMesh>().text == ("0x" + int.Parse(parent.GetChild(0).name).ToString("x"))) { 
                anchorCubeParent.transform.GetChild(i).GetChild(1).localPosition = new Vector3(anchorCubeParent.transform.GetChild(i).GetChild(1).localPosition.x, anchorCubeParent.transform.GetChild(i).GetChild(1).localPosition.y, zParsed);
                anchorIdentified = true;
            }
        }
        if (AnchorsCalibration.autoCalib && (("0x" + (int.Parse(parent.GetChild(0).name).ToString("x")) == anchorCubeParent.transform.GetChild(1).GetChild(0).GetComponent<TextMesh>().text) || ("0x" + (int.Parse(parent.GetChild(0).name).ToString("x")) == anchorCubeParent.transform.GetChild(5).GetChild(0).GetComponent<TextMesh>().text)))
            xCheck = true;

        if (AnchorsCalibration.autoCalib && (("0x" + (int.Parse(parent.GetChild(0).name).ToString("x")) == anchorCubeParent.transform.GetChild(3).GetChild(0).GetComponent<TextMesh>().text) || ("0x" + (int.Parse(parent.GetChild(0).name).ToString("x")) == anchorCubeParent.transform.GetChild(7).GetChild(0).GetComponent<TextMesh>().text)))
            yCheck = true;

        if (AnchorsCalibration.autoCalib && (("0x" + (int.Parse(parent.GetChild(0).name).ToString("x")) == anchorCubeParent.transform.GetChild(2).GetChild(0).GetComponent<TextMesh>().text) || ("0x" + (int.Parse(parent.GetChild(0).name).ToString("x")) == anchorCubeParent.transform.GetChild(6).GetChild(0).GetComponent<TextMesh>().text))) {
            xCheck = true;
            yCheck = true;
        }

        if (xCheck && yCheck && zCheck && anchorIdentified)
            parent.GetChild(0).GetComponent<Image>().color = new Vector4(0, 255, 0, 150);
        else if (!xCheck && !yCheck && !zCheck && !anchorIdentified)
            parent.GetChild(0).GetComponent<Image>().color = new Vector4(255, 255, 255, 150);
        else
            parent.GetChild(0).GetComponent<Image>().color = new Vector4(255, 255, 0, 150);

        for (int i = 1; i < 9; i++)
        {
            if ((Vector4)anchorList.transform.GetChild(i).GetChild(0).GetComponent<Image>().color != new Vector4(0, 255, 0, 150))
                full = false;

            if (full)
            {
                confirmManual.interactable = true;
                confirmManual.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            }
            else
            {
                confirmManual.interactable = false;
                confirmManual.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 0.4f);
            }
        }

        //anchor.transform.GetChild(1).position = new Vector3(anchor.transform.GetChild(1).position.x, anchor.transform.GetChild(1).position.y, zParsed);
    }

    public static void AddAnchorData(int index, ServerMessages.IPSFrameAnchorData data)
    {
        //Adds the anchordata from the client that contacts with the server. The List needs to be public static to be consulted between an outside function and
        //this one. The other function is not a MonoBehaviour, but this one is.
        anchorConfigData[index] = data;
    }

    public static ServerMessages.IPSFrameAnchorData GetAnchorData(int index)
    {
        //Same as above but to get it
        return anchorConfigData[index];
    }

    public static void ClearAnchorData()
    {
        //Same as above but to clear it
        anchorConfigData = new ServerMessages.IPSFrameAnchorData[8];
    }

    public void AcceptConfig()
    {
        //Accepts the config of the anchors
        confirmManual.interactable = false;
        confirmAuto.interactable = false;
        editConfig.interactable = false;
        redoAutocalibration.interactable = false;
        confirmManual.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 0.4f);
        for (int i = 0; i < anchorConfigData.Length; i++)
        {
            int decValue = HexStringToInt(anchorCubeParent.transform.GetChild(i).GetChild(0).GetComponent<TextMesh>().text);
            if (decValue == -1)
            {
                UnityEngine.Debug.Log("Unable to parse value");
                return;
            }
            //UnityEngine.Debug.Log("Anchor ID: " + decValue);
            // ## Check hex value
            anchorConfigData[i] = new ServerMessages.IPSFrameAnchorData(
                decValue,
                anchorCubeParent.transform.GetChild(i).GetChild(1).localPosition,
                (byte)i
            );
        }
        //Sends the commands to the drone, to autocalibrate of manual
        if (AnchorsCalibration.autoCalib && (AnchorsCalibration.state != AnchorsCalibration.CalibrationState.AUTOCALIBRATION_FINNISHED))
        {
            clientUnity.client.sendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_ANCHOR_TOBE_AUTOCALIBRATED, anchorConfigData);
            autocalibratingThrobber.parent.gameObject.SetActive(true);
        }
        else
            clientUnity.client.sendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_ANCHOR_MANUAL_CONFIG, anchorConfigData);
    }

    public void RedoAutocalibration()
    {
        //Button to redo autocalibration
        editConfig.interactable = false;
        editConfig.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 0.4f);
        redoAutocalibration.interactable = false;
        redoAutocalibration.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 0.4f);
        confirmAuto.interactable = false;
        confirmAuto.transform.GetChild(0).GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 0.4f);
        AnchorsCalibration.state = AnchorsCalibration.CalibrationState.IDLE;
        autocalibratingThrobber.parent.gameObject.SetActive(true);
        for (int i = 0; i < anchorConfigData.Length; i++)
        {

            int hex = HexStringToInt(anchorCubeParent.transform.GetChild(i).GetChild(0).GetComponent<TextMesh>().text);
            if (hex == -1)
            {
                UnityEngine.Debug.Log("Unable to parse value");
                return;
            }
            // ## Check hex value
            anchorConfigData[i] = new ServerMessages.IPSFrameAnchorData(
                hex,
                anchorCubeParent.transform.GetChild(i).GetChild(1).localPosition,
                (byte)i
            );

        }
        clientUnity.client.sendCommand((byte)Modules.POSITIONING_MODULE, (byte)PositioningCommandType.IPS_ANCHOR_TOBE_AUTOCALIBRATED, anchorConfigData);
    }

    int HexStringToInt(string hex)
    {
        //Transforms hex to int
        int x;
        hex = hex.Substring(2);
        if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out x))
            return x;
        else 
            return -1;
    }
}
