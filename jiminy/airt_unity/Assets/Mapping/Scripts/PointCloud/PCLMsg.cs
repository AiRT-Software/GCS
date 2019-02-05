using UnityEngine;  //Mesh
using System;  //Bitconverter

public class PCLMsgHeader
{
    public byte airt;
    public byte module;
    public byte action;
    public UInt32 numPoints;
    public float x, y, z;
    public float roll, pitch, yaw;
    public PointCloudID pointCloudID;
}

public class PCLMsg
{
    public PCLMsgHeader hdr;
    public PCLMesh cloud_mesh;

    public byte[] header_bytes;
    public byte[] data_bytes;
    /* old code
    public PCLMsg(ref byte[] all_data_bytes)
    {
        hdr = new PCLMsgHeader();
        cloud_mesh = new PCLMesh();

        this.data_bytes = all_data_bytes;
        setHeader(ref all_data_bytes);
    }

    public void setHeader(ref byte[] all_data_bytes)
    {
        hdr.airt = all_data_bytes[PCLMsgOffsets.AIRT];
        hdr.module = all_data_bytes[PCLMsgOffsets.MODULE];
        hdr.action = all_data_bytes[PCLMsgOffsets.ACTION];

        hdr.numPoints = BitConverter.ToUInt32(all_data_bytes, PCLMsgOffsets.NUMPOINTS);
        //Debug.Log(hdr.numPoints);

        hdr.i = BitConverter.ToInt32(all_data_bytes, PCLMsgOffsets.I);
        hdr.j = BitConverter.ToInt32(all_data_bytes, PCLMsgOffsets.J);
        hdr.k = BitConverter.ToInt32(all_data_bytes, PCLMsgOffsets.K);
    }
        * */

    public PCLMsg(byte[] header_bytes)
    {
        hdr = new PCLMsgHeader();
        cloud_mesh = new PCLMesh();

        this.header_bytes = header_bytes;

        setHeaderValues();
    }

    public void setHeaderValues()
    {

        hdr.airt = this.header_bytes[PCLMsgOffsets.AIRT];
        hdr.module = this.header_bytes[PCLMsgOffsets.MODULE];
        hdr.action = this.header_bytes[PCLMsgOffsets.ACTION];
        //Number of points. There can't be more than 20k, because each point will be turned into three vertex and 65k is the vertex limit of Unity
        hdr.numPoints = BitConverter.ToUInt32(this.header_bytes, PCLMsgOffsets.NUMPOINTS);
        //Position of the pointCloud
        hdr.x = BitConverter.ToSingle(this.header_bytes, PCLMsgOffsets.X);
        hdr.y = BitConverter.ToSingle(this.header_bytes, PCLMsgOffsets.Y);
        hdr.z = BitConverter.ToSingle(this.header_bytes, PCLMsgOffsets.Z);
        //Rotation fo the pointcloud
        hdr.pitch = BitConverter.ToSingle(this.header_bytes, PCLMsgOffsets.PITCH);
        hdr.roll =  BitConverter.ToSingle(this.header_bytes, PCLMsgOffsets.ROLL);
        hdr.yaw =   BitConverter.ToSingle(this.header_bytes, PCLMsgOffsets.YAW);
        //Block where the pointcloud belongs to
        hdr.pointCloudID = new PointCloudID();
        hdr.pointCloudID.i = BitConverter.ToInt32(this.header_bytes, PCLMsgOffsets.ID_I);
        hdr.pointCloudID.j = BitConverter.ToInt32(this.header_bytes, PCLMsgOffsets.ID_J);
        hdr.pointCloudID.k = BitConverter.ToInt32(this.header_bytes, PCLMsgOffsets.ID_K);

        hdr.pointCloudID.heading = this.header_bytes[PCLMsgOffsets.ID_HEADING];


    }
    //Adds the points from the pointcloud

    public void fillMesh(byte[] body_bytes)
    {
        if (hdr != null)
        {
            this.data_bytes = body_bytes;
            cloud_mesh.fillMesh(ref this.data_bytes, hdr, getNumPoints(), 0);
        }
        else
        {
            UnityEngine.Debug.Log("Hdr in PCLMsg instance is null");
        }
    }
    //Adds the points from the pointcloud

    public void fillMesh(byte[] body_bytes, int bytes_offset)
    {
        if (hdr != null)
        {
            this.data_bytes = body_bytes;
            cloud_mesh.fillMesh(ref this.data_bytes, hdr, getNumPoints(), bytes_offset);
        }
        else
        {
            UnityEngine.Debug.Log("Hdr is null");
        }
    }

    public PCLMesh getCloudMesh()
    {
        return cloud_mesh.getCloudMesh();
    }

    public int getNumPoints()
    {
        return (int)hdr.numPoints;
    }

    public string getHeaderInfo()
    {
        string info = "";
        info += "\n" + hdr.airt.ToString();
        info += "\n" + hdr.module.ToString();
        info += "\n" + hdr.action.ToString();

        info += "\n" + hdr.numPoints.ToString();

        //Debug.Log(hdr.airt.ToString() + " " + hdr.module.ToString() +" "+ hdr.action.ToString() +" "+hdr.numPoints.ToString());

        info += "\n" + hdr.x.ToString();
        info += "\n" + hdr.y.ToString();
        info += "\n" + hdr.z.ToString();
        return info;
    }
}
