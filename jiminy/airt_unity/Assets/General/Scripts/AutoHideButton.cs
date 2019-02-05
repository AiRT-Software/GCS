using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoHideButton : MonoBehaviour {

    bool showTextLabel = true;

    public void ToggleTextLabel()
    {
        if (showTextLabel)
            this.GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
        else
            this.GetComponent<Text>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

        showTextLabel = !showTextLabel;
    }
}
