using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BackgroundClicks : MonoBehaviour {

    public static GameObject plansContainer;
    GameObject mapsContainer;
    GameObject[] mapPanels;
    GameObject[] planPanels;

    void Awake()
    {
        mapsContainer = GameObject.Find("MapsContainer");
        plansContainer = GameObject.Find("PlansContainer");

        if (mapsContainer)
        {
            mapPanels = new GameObject[mapsContainer.transform.childCount];
            planPanels = new GameObject[plansContainer.transform.childCount];
        }
    }

    void OnEnable()
    {
        if (mapsContainer.transform.childCount != mapPanels.Length)
            Array.Resize<GameObject>(ref mapPanels, mapsContainer.transform.childCount);

        if (plansContainer.transform.childCount != planPanels.Length)
            Array.Resize<GameObject>(ref planPanels, plansContainer.transform.childCount);

        if (mapsContainer)
        {
            mapPanels = new GameObject[mapsContainer.transform.childCount];
            for (int i = 0; i < mapsContainer.transform.childCount; i++)
            {
                mapPanels[i] = mapsContainer.transform.GetChild(i).Find("ExtraPanel").gameObject;
            }

            planPanels = new GameObject[plansContainer.transform.childCount];
            for (int i = 0; i < plansContainer.transform.childCount; i++)
            {
                planPanels[i] = plansContainer.transform.GetChild(i).Find("ExtraPanel").gameObject;
            }
        }
    }
    /// <summary>
    /// This function is called when the backgruound is clicked on plan selection. It checks if an info screen from a map is open, and closes it. The info screen is the one that contains the info, delete local and delete server
    /// </summary>
    public void BackgroundClick()
    {
        if (mapsContainer.transform.childCount != mapPanels.Length)
            Array.Resize<GameObject>(ref mapPanels, mapsContainer.transform.childCount);

        if (plansContainer.transform.childCount != planPanels.Length)
            Array.Resize<GameObject>(ref planPanels, plansContainer.transform.childCount);

        if (mapsContainer)
        {
            mapPanels = new GameObject[mapsContainer.transform.childCount];
            for (int i = 0; i < mapsContainer.transform.childCount; i++)
            {
                mapPanels[i] = mapsContainer.transform.GetChild(i).Find("ExtraPanel").gameObject;
            }

            planPanels = new GameObject[plansContainer.transform.childCount];
            for (int i = 0; i < plansContainer.transform.childCount; i++)
            {
                planPanels[i] = plansContainer.transform.GetChild(i).Find("ExtraPanel").gameObject;
            }
        }

        if (mapsContainer && mapPanels.Length > 0)
        {
            for (int i = 0; i < mapPanels.Length; i++)
            {
                mapPanels[i].SetActive(false);
            }
        }

        if (plansContainer && planPanels.Length > 0)
        {
            for (int i = 0; i < planPanels.Length; i++)
            {
                planPanels[i].SetActive(false);
            }
        }
    }
}
