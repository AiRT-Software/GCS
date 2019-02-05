using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Don't know if anyone uses this, and if it uses it only to adjust UI
/// </summary>
public class EmptySceneUIManager : MonoBehaviour {

    int width, height;

    public RectTransform widthInput, heightInput, depthInput;
    public RectTransform widthText, heightText, depthText;
    public RectTransform createSceneButton;

    Text widthInputFont, heightInputFont, depthInputFont;
    Text widthTextFont, heightTextFont, depthTextFont;
    Text createSceneButtonText;

	// Use this for initialization
	void Start () {
        width = Screen.width;
        height = Screen.height;

        widthInputFont = widthInput.GetChild(1).GetComponent<Text>();
        heightInputFont = heightInput.GetChild(1).GetComponent<Text>();
        depthInputFont = depthInput.GetChild(1).GetComponent<Text>();
        widthTextFont = widthText.GetComponent<Text>();
        heightTextFont = heightText.GetComponent<Text>();
        depthTextFont = depthText.GetComponent<Text>();
        createSceneButtonText = createSceneButton.GetChild(0).GetComponent<Text>();

        widthInput.anchoredPosition = new Vector2(-width / 10, height / 10);
        heightInput.anchoredPosition = new Vector2(-width / 10, 0);
        depthInput.anchoredPosition = new Vector2(-width / 10, -height / 10);
        widthText.anchoredPosition = new Vector2(-width / 4, height / 10);
        heightText.anchoredPosition = new Vector2(-width / 4, 0);
        depthText.anchoredPosition = new Vector2(-width / 4, -height / 10);
        createSceneButton.anchoredPosition = new Vector2(-width / 7, -height / 4);

        widthInput.sizeDelta = new Vector2(width / 10, height / 20);
        heightInput.sizeDelta = new Vector2(width / 10, height / 20);
        depthInput.sizeDelta = new Vector2(width / 10, height / 20);
        widthText.sizeDelta = new Vector2(width / 10, height / 20);
        heightText.sizeDelta = new Vector2(width / 10, height / 20);
        depthText.sizeDelta = new Vector2(width / 10, height / 20);
        createSceneButton.sizeDelta = new Vector2(width / 8, height / 12);

        widthInputFont.fontSize = height / 42;
        heightInputFont.fontSize = height / 42;
        depthInputFont.fontSize = height / 42;
        widthTextFont.fontSize = height / 42;
        heightTextFont.fontSize = height / 42;
        depthTextFont.fontSize = height / 42;
        createSceneButtonText.fontSize = height / 38;
		
	}
}
