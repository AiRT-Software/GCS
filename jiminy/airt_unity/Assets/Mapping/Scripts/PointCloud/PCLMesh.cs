//#define TRANSFORM_COORDS
#define POZYX_TRANSFORM_COORDS

using UnityEngine;  //Vector3
using System;  //Bitconverter

public class PCLMesh
{

#if TRANSFORM_COORDS
    public static Matrix4x4 toUnityCoordsMat =
        //Matrix4x4.TRS(new Vector3(0f, 0f, 0f), Quaternion.Euler(90f, 0f, 0f), new Vector3(1f, 1f, -1f));
            Matrix4x4.TRS(new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), new Vector3(1f, -1f, 1f));
#endif

    public static int id = 0;

    public Vector3[] points;
    public Vector3[] normals;
    public Color[] colors;
    public int[] indices;

    public PCLMesh() { }

    public void fillMesh(ref byte[] data_bytes, PCLMsgHeader header, int num_points, int offset_data)
    {
        //UnityEngine.Debug.Log("offset data " + offset_data);

        int N; //20000 * 3 < 65000 points allowed in Unity
        if (num_points > 20000) { N = 20000; }
        else { N = num_points; }

        points = new Vector3[N];
        normals = new Vector3[N];
        colors = new Color[N];
        indices = new int[N];
        //Matrix to transform from pozyx to Unity. Not used right now
         Matrix4x4 toUnityCoordsMat = Matrix4x4.TRS(new Vector3(header.x * 0.001f, header.z * 0.001f, header.y * 0.001f),
                Quaternion.Euler(-header.pitch, -header.yaw, -header.roll),
                new Vector3(1,1,1));

        for (int i = 0, byte_index = offset_data; i < N; byte_index += PCLMsgOffsets.POINTNORMAL_SIZE, i++)
        {
            //Position of the point from the pointCloud
            points[i].x = BitConverter.ToSingle(data_bytes, byte_index + PCLMsgOffsets.POINT_X);
            points[i].y = BitConverter.ToSingle(data_bytes, byte_index + PCLMsgOffsets.POINT_Z); // Changing from Pozyx to Unity
            points[i].z = BitConverter.ToSingle(data_bytes, byte_index + PCLMsgOffsets.POINT_Y);

#if TRANSFORM_COORDS
            points[i] = toUnityCoordsMat.MultiplyPoint(points[i]);
#elif POZYX_TRANSFORM_COORDS
            //points[i] = toUnityCoordsMat.MultiplyVector(points[i]);
#endif
            //Color
            colors[i].r = data_bytes[byte_index + PCLMsgOffsets.POINT_R] / 255f;
            colors[i].g = data_bytes[byte_index + PCLMsgOffsets.POINT_G] / 255f;
            colors[i].b = data_bytes[byte_index + PCLMsgOffsets.POINT_B] / 255f;
            //colors[i].a = msg_bytes[byte_index + PCLMsgOffsets.POINT_A] / 255f;
            colors[i].a = 1f;
            //Normal
            //normals[i] = new Vector3(0.1f, 0f, 0.9f).normalized;
            normals[i].x = BitConverter.ToSingle(data_bytes, byte_index + PCLMsgOffsets.NORMAL_X);
            normals[i].y = BitConverter.ToSingle(data_bytes, byte_index + PCLMsgOffsets.NORMAL_Z);
            normals[i].z = BitConverter.ToSingle(data_bytes, byte_index + PCLMsgOffsets.NORMAL_Y);

            indices[i] = i;
        }
    }

    public PCLMesh getCloudMesh()
    {
        return this;
    }
}

