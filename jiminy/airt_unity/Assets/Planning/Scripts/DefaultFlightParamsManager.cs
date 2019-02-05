using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//This is the class that handles default height and speed from the waypoints which the user selects before entering planning.
public class DefaultFlightParamsManager : MonoBehaviour {

    public int height;
    public int speed;
    public int duration;

    void OnEnable()
    {
        height = 1;
        speed = 1;
        duration = 60;
    }

    public void IncreaseHeight()
    {
        height++;
        transform.GetChild(4).GetComponent<InputField>().text = height.ToString() + " m";
        MissionManager.planDefaultHeight = height;
    }
    public void DecreaseHeight()
    {
        height--;
        transform.GetChild(4).GetComponent<InputField>().text = height.ToString() + " m";
        MissionManager.planDefaultHeight = height;
    }

    public void IncreaseSpeed()
    {
        speed++;
        transform.GetChild(5).GetComponent<InputField>().text = speed.ToString() + " m/s";
        MissionManager.planDefaultSpeed = speed;
    }
    public void DecreaseSpeed()
    {
        speed--;
        transform.GetChild(5).GetComponent<InputField>().text = speed.ToString() + " m/s";
        MissionManager.planDefaultSpeed = speed;
    }

    public void IncreaseDuration()
    {
        duration++;
        transform.GetChild(6).GetComponent<InputField>().text = duration.ToString() + " s";
        MissionManager.planDefaultDuration = duration;
    }
    public void DecreaseDuration()
    {
        duration--;
        transform.GetChild(6).GetComponent<InputField>().text = duration.ToString() + " s";
        MissionManager.planDefaultDuration = duration;
    }
}
