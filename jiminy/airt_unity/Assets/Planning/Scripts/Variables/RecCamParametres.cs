using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class contains the rec cam parameters that are not serializables, but that get consulted when the path is saved and turn into an array of commands to the reccam that the drone will execute
/// </summary>

[System.Serializable]
public class RecCamParameters {
    public int id_pointer = 0;

    [SerializeField]
    public List<ByteArray> reccam_parameters = new List<ByteArray>();
    [SerializeField]
    public bool edited = false;
    [SerializeField]
    public bool active = true;
    [NonSerialized]
    public byte switchToRec = 1;
    [NonSerialized]
    public byte[] brightness = new byte[4] { 00, 00, 00, 00 };
    [NonSerialized]
    public byte resolution = 22;
    [NonSerialized]
    public byte megaPixels = 0;
    [NonSerialized]
    public byte autoManualWB = 0; //0 auto 254 manual
                                  //public float focus = 0.0f;
    [NonSerialized]
    public byte AF = 0;
    [NonSerialized]
    public byte WBTint = 0;
    [NonSerialized]
    public byte ISO = 0;
    [NonSerialized]
    public byte sharpness = 0;
    [NonSerialized]
    public byte[] contrast = new byte[4] { 00, 00, 00, 00 };
    [NonSerialized]
    public byte AE = 0;
    [NonSerialized]
    public byte[] saturation = new byte[4] { 00, 00, 00,00};
    [NonSerialized]
    public byte photoQuality = 0;
    [NonSerialized]
    public byte upsideDown = 0;
    [NonSerialized]
    public byte irisAperture = 0;
    [NonSerialized]
    public byte burstMode = 0;
    [NonSerialized]
    public byte burstSpeed = 0;
}
