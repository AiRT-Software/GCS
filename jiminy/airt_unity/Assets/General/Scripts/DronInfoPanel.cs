using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronInfoPanel : MonoBehaviour {

    public RectTransform speedLevel, heightLevel, ipsLevel, batteryLevel, emergencyButton;
    public RectTransform cameraToggle;
    public GameObject middleBar;
    RectTransform bottomPanel;

    public bool hasFPV = true;

	// Use this for initialization
	void Start () {

        //This scales botDronPanel in every scene

        int width = Screen.width;
        int height = Screen.height;
        float iconSize = width / 10.0f;
        float iconOffset = width / 10.0f;

        bottomPanel = speedLevel.parent.GetComponent<RectTransform>();
        bottomPanel.anchoredPosition = new Vector2(0, Screen.height / 40);
        bottomPanel.sizeDelta = new Vector2(Screen.width / 2, Screen.height / 12);

        speedLevel.anchoredPosition = new Vector2(iconOffset * -2, 0.0f);
        speedLevel.sizeDelta = new Vector2(iconSize, height / 25.0f);
        speedLevel.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize / 2, 0);

        heightLevel.anchoredPosition = new Vector2(iconOffset * -1, 0.0f);
        heightLevel.sizeDelta = new Vector2(iconSize, height / 25.0f);
        heightLevel.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize / 2, 0);

        emergencyButton.anchoredPosition = new Vector2(iconOffset * 0, 0.0f);
        emergencyButton.sizeDelta = new Vector2(iconSize, height / 25.0f);
        emergencyButton.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize / 2, 0);

        ipsLevel.anchoredPosition = new Vector2(iconOffset * 1, 0.0f);
        ipsLevel.sizeDelta = new Vector2(iconSize, height / 25.0f);
        ipsLevel.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize / 2, 0);

        batteryLevel.anchoredPosition = new Vector2(iconOffset * 2, 0.0f);
        batteryLevel.sizeDelta = new Vector2(iconSize, height / 25.0f);
        batteryLevel.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize / 2, 0);

        if (hasFPV) { 
            //If the scene can show the fpv, then we show it
            cameraToggle.anchoredPosition = new Vector2(Screen.width / 2 - (3 * iconOffset / 4), 0.0f);
            cameraToggle.sizeDelta = new Vector2(iconSize / 2, height / 20.0f);

            //cameraOpen.anchoredPosition = new Vector2(Screen.width / 2 - (6 * iconOffset / 4), 0.0f);
            //cameraOpen.sizeDelta = new Vector2(iconSize / 2, height / 20.0f);
        }

        GameObject prefabCopy = Instantiate(middleBar);

        prefabCopy.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        prefabCopy.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        prefabCopy.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

        prefabCopy.GetComponent<RectTransform>().sizeDelta = new Vector2(5, (height / 25.0f) * 0.9f);
        float bottomBarWidth = prefabCopy.GetComponent<RectTransform>().sizeDelta.y * 9 / 92;
        prefabCopy.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 * iconOffset / 2 - bottomBarWidth / 2, 0);
        Instantiate(prefabCopy, speedLevel.transform.parent);
        prefabCopy.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1 * iconOffset / 2 - bottomBarWidth / 2, 0);
        Instantiate(prefabCopy, speedLevel.transform.parent);
        prefabCopy.GetComponent<RectTransform>().anchoredPosition = new Vector2(+1 * iconOffset / 2 - bottomBarWidth / 2, 0);
        Instantiate(prefabCopy, speedLevel.transform.parent);
        prefabCopy.GetComponent<RectTransform>().anchoredPosition = new Vector2(+3 * iconOffset / 2 - bottomBarWidth / 2, 0);
        Instantiate(prefabCopy, speedLevel.transform.parent);

        //if (hasFPV) { 
        //    prefabCopy.GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.width / 2 - (9 * iconOffset / 8), 0);
        //    Instantiate(prefabCopy, speedLevel.transform.parent);
        //}

        Destroy(prefabCopy);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
