using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICalibrationManager : MonoBehaviour {

    public GameObject anchorsCalibratePanel;

    // RectTransform labels
    public RectTransform lookingForAnchorsLabel, startCalibratingLabel, indoorPositioningLabel, autocalibratingLabel;

    // RectTransform rects and images
    public RectTransform loadingThrobber, anchorRect, autocalibratingThrobber, cubeRect;

    // RectTransform texts
    public RectTransform calibrationText, anchorListText;

    // RectTransform buttons
    public RectTransform manualButton, autoButton, autocalibrateButton, discoverButton;
    public RectTransform confirmManualButton, editConfigButton, redoAutocalibrationButton, confirmAutoButton, endEditConfig;

    // Help positioning UI variables
    int width, height;

    void Awake()
    {
        width = Screen.width;
        height = Screen.height;

        // Labels position and size
        calibrationText.anchoredPosition = new Vector2(0, height / 8);
        lookingForAnchorsLabel.anchoredPosition = new Vector2(width / 8, -height / 4);
        startCalibratingLabel.anchoredPosition = new Vector2(width / 8, -height / 4);
        indoorPositioningLabel.anchoredPosition = new Vector2(width / 3, -height / 6);
        autocalibratingLabel.anchoredPosition = new Vector2(0, -height / 5);
        calibrationText.sizeDelta = new Vector2(width / 3, height / 10);
        lookingForAnchorsLabel.sizeDelta = new Vector2(width / 6, height / 6);
        startCalibratingLabel.sizeDelta = new Vector2(width / 6, height / 4);
        indoorPositioningLabel.sizeDelta = new Vector2(width / 2, height / 6);
        autocalibratingLabel.sizeDelta = new Vector2(width / 6, height / 6);
        calibrationText.GetComponent<Text>().fontSize = (int)(width * 0.032);
        lookingForAnchorsLabel.GetComponent<Text>().fontSize = (int)(width * 0.020f);
        startCalibratingLabel.GetComponent<Text>().fontSize = (int)(width * 0.015f);
        indoorPositioningLabel.GetComponent<Text>().fontSize = (int)(width * 0.035f);
        autocalibratingLabel.GetComponent<Text>().fontSize = (int)(width * 0.020f);

        // Rects & images position and size
        loadingThrobber.anchoredPosition = new Vector2(0.0f, -height / 6);
        anchorRect.anchoredPosition = new Vector2(0.0f, -height / 12);
        autocalibratingThrobber.anchoredPosition = new Vector2(0.0f, -height / 40);
        cubeRect.anchoredPosition = new Vector2(0.0f, height / 4);
        loadingThrobber.sizeDelta = new Vector2(width / 20, width / 20);
        anchorRect.sizeDelta = new Vector2(width / 2, height / 2);
        autocalibratingThrobber.sizeDelta = new Vector2(width / 20, width / 20);
        cubeRect.sizeDelta = new Vector2(width / 2, height / 2);

        // Anchor text font size
        anchorListText.GetComponent<Text>().fontSize = (int)(width * 0.02f);

        // Buttons position and size
        manualButton.anchoredPosition = new Vector2(-width / 8, -height / 12);
        autoButton.anchoredPosition = new Vector2(width / 8, -height / 12);
        autocalibrateButton.anchoredPosition = new Vector2(width / 8, -height / 1.5f);
        discoverButton.anchoredPosition = new Vector2(width / 8, -height / 1.5f);
        confirmManualButton.anchoredPosition = new Vector2(width / 2, -height / 1.3f);
        editConfigButton.anchoredPosition = new Vector2(width / 2, -height / 1.3f);
        redoAutocalibrationButton.anchoredPosition = new Vector2((width / 2) + ((width/ 6) + (width / 200)), -height / 1.3f);
        confirmAutoButton.anchoredPosition = new Vector2((width / 2) + (2 * ((width / 6) + (width / 200))), -height / 1.3f);
        endEditConfig.anchoredPosition = new Vector2(width / 2, -height / 1.3f);

        manualButton.sizeDelta = new Vector2(width / 5, height / 12);
        autoButton.sizeDelta = new Vector2(width / 5, height / 12);
        autocalibrateButton.sizeDelta = new Vector2(width / 6, height / 12);
        discoverButton.sizeDelta = new Vector2(width / 6, height / 12);
        confirmManualButton.sizeDelta = new Vector2(width / 6, height / 12);
        editConfigButton.sizeDelta = new Vector2(width / 8, height / 12);
        redoAutocalibrationButton.sizeDelta = new Vector2(width / 8, height / 12);
        confirmAutoButton.sizeDelta = new Vector2(width / 8, height / 12);
        endEditConfig.sizeDelta = new Vector2(width / 8, height / 12);

        manualButton.GetChild(0).GetComponent<Text>().fontSize = (int)(width * 0.025f);
        autoButton.GetChild(0).GetComponent<Text>().fontSize = (int)(width * 0.025f);
        autocalibrateButton.GetChild(0).GetComponent<Text>().fontSize = (int)(width * 0.02f);
        discoverButton.GetChild(0).GetComponent<Text>().fontSize = (int)(width * 0.02f);
        confirmManualButton.GetChild(0).GetComponent<Text>().fontSize = (int)(width * 0.016f);
        editConfigButton.GetChild(0).GetComponent<Text>().fontSize = (int)(width * 0.016f);
        redoAutocalibrationButton.GetChild(0).GetComponent<Text>().fontSize = (int)(width * 0.016f);
        confirmAutoButton.GetChild(0).GetComponent<Text>().fontSize = (int)(width * 0.016f);
        endEditConfig.GetChild(0).GetComponent<Text>().fontSize = (int)(width * 0.016f);
    }
}
