using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//Is thsi class used anymore? Half of the referenced content isn't in the scene
public class UIMapManager : MonoBehaviour {

    public static bool droneFlying = false;
    bool anchorsActivated = false;

    public RectTransform MappingTypePanel, WarningPanel, semiAutomaticSettings, MappingPanel;
    public RectTransform ButtonBack, ButtonNext, buttonSemiAutomatic, buttonManual;
    public RectTransform FixedHeights, MinFixedDistance, sliderHeight, sliderDistance, TextHeigth, TextDistance, ContinueButton;
    public RectTransform MappingTypeTitle;
    public RectTransform warningTitle, warningText;
    public RectTransform moveDronButton, RotateDronButton, dropdownHeight, ScanTakeoffButton, goToPlanning, backToMappingTypeButton;
    public RectTransform binButton, binLastButton; // scanningThrobber;
    public GameObject anchorPrefab, anchorParent, anchorIDParent;
    public GameObject originAxis;
    public Transform drone;

    GameObject[] anchors = new GameObject[8];
    RectTransform[] anchorIDs = new RectTransform[8];
    Vector2[] anchorIDPos = new Vector2[8];
    ClientUnity clientUnity;

    // Use this for initialization

    void Awake()
    {
        //Warning Panel
        WarningPanel.sizeDelta = new Vector2(0, Screen.height / 1.5f);
        warningTitle.GetComponent<Text>().fontSize = Screen.width / 30;
        warningTitle.anchoredPosition = new Vector2(0, Screen.height / 4);
        warningText.GetComponent<Text>().fontSize = Screen.width / 40;
        warningText.anchoredPosition = new Vector2(0, 0);
        ButtonBack.anchoredPosition = new Vector3(Screen.width / 10, Screen.height / 10);
        //ButtonBack.sizeDelta = new Vector2(Screen.width / 6, Screen.height / 6);
        ButtonBack.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 20, Screen.height / 20);
        ButtonBack.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 20, Screen.height / 20);
        ButtonBack.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 8, Screen.height / 20);
        ButtonBack.GetChild(0).GetComponent<Text>().fontSize = Screen.width / 60;
        ButtonBack.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(ButtonBack.GetChild(1).GetComponent<RectTransform>().sizeDelta.x - 20, 0);

        ButtonNext.anchoredPosition = new Vector2(0, Screen.height / 10);
        ButtonNext.sizeDelta = new Vector2(Screen.width / 10, Screen.height / 12);
        ButtonNext.GetChild(0).GetComponent<Text>().fontSize = Screen.width / 50;

        //Settings Panel
        semiAutomaticSettings.sizeDelta = new Vector2(0, Screen.height / 1.5f);
        semiAutomaticSettings.anchoredPosition = new Vector2(0, Screen.height / 8);

        FixedHeights.anchoredPosition = new Vector2(Screen.width / 6, 4 * Screen.height / 6);
        MinFixedDistance.anchoredPosition = new Vector2(4 * Screen.width / 6, 4 * Screen.height / 6);
        FixedHeights.GetComponent<Text>().fontSize = Screen.width / 40;
        MinFixedDistance.GetComponent<Text>().fontSize = Screen.width / 40;

        sliderHeight.anchoredPosition = new Vector2(Screen.width / 4, 2 * Screen.height / 6);
        sliderHeight.localScale = new Vector2(Screen.height / 500, Screen.height / 500);

        sliderDistance.anchoredPosition = new Vector2(3 * Screen.width / 4 - sliderDistance.sizeDelta.x, 2 * Screen.height / 6);
        sliderDistance.localScale = new Vector2(Screen.width / 600, Screen.height / 500);

        TextDistance.anchoredPosition = new Vector2(4 * Screen.width / 6, 3 * Screen.height / 6);
        TextDistance.sizeDelta = new Vector2(Screen.width / 10, Screen.height / 12);
        TextDistance.GetChild(0).GetComponent<Text>().fontSize = Screen.width / 30;

        TextHeigth.anchoredPosition = new Vector2(Screen.width / 3, 2 * Screen.height / 6);
        TextHeigth.sizeDelta = new Vector2(Screen.width / 10, Screen.height / 12);
        TextHeigth.GetChild(0).GetComponent<Text>().fontSize = Screen.width / 30;

        ContinueButton.anchoredPosition = new Vector2(0, Screen.height / 10);
        ContinueButton.sizeDelta = new Vector2(Screen.width / 8, Screen.height / 10);
        ContinueButton.GetChild(0).GetComponent<Text>().fontSize = Screen.width / 40;
        //Select type panel
        MappingTypePanel.sizeDelta = new Vector2(0, Screen.height / 1.5f);
        MappingTypePanel.anchoredPosition = new Vector2(0, Screen.height / 8);
        buttonManual.sizeDelta = new Vector2(Screen.width / 4, Screen.height / 6);
        buttonManual.GetChild(0).GetComponent<Text>().fontSize = Screen.width / 30;
        buttonManual.anchoredPosition = new Vector2( 0, Screen.height / 8);
        buttonSemiAutomatic.sizeDelta = new Vector2(Screen.width / 4, Screen.height / 6);
        buttonSemiAutomatic.GetChild(0).GetComponent<Text>().fontSize = Screen.width / 40;
        buttonSemiAutomatic.anchoredPosition = new Vector2(4 * Screen.width / 6, Screen.height / 4);
        MappingTypeTitle.sizeDelta = new Vector2(Screen.width / 4, Screen.height / 4);
        MappingTypeTitle.anchoredPosition = new Vector2(0, 1.5f * Screen.height / 4);

        MappingTypeTitle.GetComponent<Text>().fontSize = Screen.width / 25;
        //Mapping
        MappingPanel.sizeDelta = new Vector2(0, Screen.height / 1.5f);
        MappingPanel.anchoredPosition = new Vector2(0, Screen.height / 8);
        ScanTakeoffButton.anchoredPosition = new Vector2(-Screen.width / 30, Screen.height / 25);
        ScanTakeoffButton.sizeDelta = new Vector2(Screen.width / 8, Screen.height / 12);
        ScanTakeoffButton.GetChild(0).GetComponent<Text>().fontSize = Screen.width / 50;
        //scanningThrobber.sizeDelta = new Vector2(Screen.width / 25, Screen.width / 25);
        //scanningThrobber.anchoredPosition = new Vector2(0, -(Screen.width / 40) - scanningThrobber.sizeDelta.y);
        // dropdownHeight;
        RotateDronButton.anchoredPosition = new Vector2(Screen.width / 8, Screen.height / 4);
        RotateDronButton.sizeDelta = new Vector2(Screen.width / 4, Screen.height / 4);
        moveDronButton.anchoredPosition = new Vector2(6 * Screen.width / 8, Screen.height / 4);
        moveDronButton.sizeDelta = new Vector2(Screen.width / 4, Screen.height / 4);

        backToMappingTypeButton.anchoredPosition = new Vector3(Screen.width / 10, 20);

        backToMappingTypeButton.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 20, Screen.height / 20);
        backToMappingTypeButton.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 8, Screen.height / 20);
        backToMappingTypeButton.GetChild(0).GetComponent<Text>().fontSize = Screen.width / 60;
        backToMappingTypeButton.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(backToMappingTypeButton.GetChild(1).GetComponent<RectTransform>().sizeDelta.x - 20, 0);

        goToPlanning.anchoredPosition = new Vector3(0, Screen.height / 25);
        goToPlanning.sizeDelta = new Vector3(Screen.width / 8, Screen.height / 12);
        goToPlanning.GetChild(0).GetComponent<Text>().fontSize = Screen.width / 50;
        //goToPlanning.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 20, Screen.height / 20);
        //goToPlanning.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(-goToPlanning.GetChild(1).GetComponent<RectTransform>().sizeDelta.x + 20, 0);
        //goToPlanning.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 8, Screen.height / 20);

        dropdownHeight.anchoredPosition = new Vector2(6 * Screen.width / 8, 5 * Screen.height / 8);
        dropdownHeight.localScale = new Vector2(Screen.width / 750, Screen.height / 500);

        binButton.sizeDelta = new Vector2(Screen.width / 30, Screen.width / 30);
        binButton.anchoredPosition = new Vector2(50, -50);

        binLastButton.sizeDelta = new Vector2(Screen.width / 30, Screen.width / 30);
        binLastButton.anchoredPosition = new Vector2(ScanTakeoffButton.anchoredPosition.x, ScanTakeoffButton.anchoredPosition.y + ScanTakeoffButton.sizeDelta.y * 2);

    }
    //Draws the anchors on mapping
    void Start () {

        GeneralSceneManager.sceneState = GeneralSceneManager.SceneState.Mapping;

        for (int i = 0; i < CalibrationSettings.anchorConfigData.Length; i++)
        {
            
            Vector3 anchorPos = new Vector3(CalibrationSettings.anchorConfigData[i].position.x, CalibrationSettings.anchorConfigData[i].position.z, CalibrationSettings.anchorConfigData[i].position.y);
            if (i == 0)
            {
                GameObject axis = Instantiate(originAxis, Vector3.zero, Quaternion.identity, anchorParent.transform);
                axis.transform.localScale = new Vector3(1, -1, 1);
                axis.transform.Rotate(new Vector3(-90, 0, 0));
            }
            anchors[i] = Instantiate(anchorPrefab, anchorPos * 0.001f, Quaternion.identity, anchorParent.transform);
            anchors[i].name = CalibrationSettings.anchorConfigData[i].id.ToString();
            anchorIDs[i] = anchorIDParent.transform.GetChild(i).GetComponent<RectTransform>();
            //if (!int.TryParse(anchors[i].name, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out hex))
            //{
            //    UnityEngine.Debug.LogWarning("Unable to parse anchor Value!");
            //}
            anchorIDs[i].GetComponent<Text>().text = "0x" + int.Parse(anchors[i].name).ToString("x");
        }
        if (AtreyuModule.jumpToMappingDirectly == true)
        {
            goToMapping();
        }

    }
    public void goToSettings()
    {
        MappingTypePanel.gameObject.SetActive(false);
        semiAutomaticSettings.gameObject.SetActive(true);
    }
    public void goToWarning()
    {
        MappingTypePanel.gameObject.SetActive(false);
        semiAutomaticSettings.gameObject.SetActive(false);
        WarningPanel.gameObject.SetActive(true);
    }
    public void goToMapping()
    {
        WarningPanel.gameObject.SetActive(false);
        MappingPanel.gameObject.SetActive(true);

    }
    public void goBackToSelectType()
    {
        MappingTypePanel.gameObject.SetActive(true);
        WarningPanel.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate () {

        ServerMessages.IPSFrameData frameData = DroneModule.DequeueIPSDronFrame();
        if (frameData != null)
        {
            drone.transform.position = frameData.position;
            drone.transform.rotation = Quaternion.Euler(frameData.rotation);
        }

        //if (droneFlying) {
        //    if (!anchorsActivated) { 
        //        anchorIDParent.SetActive(true);
        //        anchorsActivated = true;
        //    }
        //    for (int i = 0; i < anchors.Length; i++)
        //    {
        //        if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), anchors[i].GetComponent<Collider>().bounds))
        //        { 
        //            anchorIDPos[i] = Camera.main.WorldToScreenPoint(anchors[i].transform.position);
        //            anchorIDs[i].anchoredPosition = new Vector2(anchorIDPos[i].x - 125, anchorIDPos[i].y + 25);
        //        }
        //        else
        //        {
        //            anchorIDs[i].anchoredPosition = new Vector2(-400, -400);
        //        }
        //    }
        //}
	}
}
