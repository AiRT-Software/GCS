using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//This class holds the downloaded pointcloud to see if we have one already that should be replaced.
public class SavedPointCloud {

    public List<SavedMeshPointCloud> PointCloud;

    public int isTheCloudAlreadyIn(PointCloudID id)
    {
        for (int i = 0; i < PointCloud.Count; i++)
        {
            PointCloudID aux = PointCloud[i].pointCloudID;
            if (aux.i == id.i && aux.j == id.j && aux.k == id.k && aux.heading == id.heading)
            {
                return i;
            }
        }
        return -1;
    }

}

public class SavedMeshPointCloud
{

    //public Vector3[] vertex;

    public Vector3[] colors;
    public Matrix4x4 matrix;
    public PointCloudID pointCloudID;
    public SavedMeshPointCloud( Vector3 posId, byte otherID)
    {
        //vertex = _Vertex;
        //int i = 0;
        //colors = new Vector3[vertex.Length];
        //foreach (var color in _Colors)
        //{
        //    colors[i] = new Vector3(color.r, color.g, color.b);
        //    i++;
        //}
        //matrix = _Matrix;
        pointCloudID = new PointCloudID();
        pointCloudID.i = (int)posId.x;
        pointCloudID.j = (int)posId.y;
        pointCloudID.k = (int)posId.z;

        pointCloudID.heading = otherID;
    }
   

}
