using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//This contains the default flight parameters that the user inputs and that will be consulted once the panel that contains the speed is brought up and once the user creates a waypoint
[System.Serializable]
public class DefaultFlightParams {

    public int height = 0, speed = 1, duration = 60;

    public DefaultFlightParams()
    {
        height = 0;
        speed = 1;
        duration = 60;
    }

    public DefaultFlightParams(int height_, int speed_, int duration_)
    {
        height = height_;
        speed = speed_;
        duration = duration_;
    }
}
