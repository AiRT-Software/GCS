using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Contains the gimball parmeters that will be sent to the drone
[System.Serializable]
public class GimballParameters {
    public int id_pointer = 0;

    public Vector3 poi_or_angles = new Vector3();
    public enum GimbalMode
    {
        LOOK_AT_POI,
        LOOK_AHEAD,
        LOOK_AHEAD_FIX_PITCH,
        BLOCK_DIRECTION
    }
    public byte mode = 0;
}
