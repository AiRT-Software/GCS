using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//used to overcome the problem of not being able to serialize an array of an array
[System.Serializable]
public class ByteArray {

    public byte[] array;
    public ByteArray(byte[] bytes)
    {
        array = bytes;
    }
}
