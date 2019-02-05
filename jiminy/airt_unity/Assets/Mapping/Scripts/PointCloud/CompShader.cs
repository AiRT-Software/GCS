using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Triangle
{
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;
};

struct BaseTriangle
{
    public Vector4 p1;
    public Vector4 p2;
    public Vector4 p3;
    public Vector4 normal;
    public BaseTriangle(Vector4 p1, Vector4 p2, Vector4 p3, Vector4 normal)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        this.normal = normal;
    }
};

public class CompShader
{
    public ComputeShader csCreateTriangles;

    private BaseTriangle baseTriValues;

    // Uniforms
    private ComputeBuffer points;
    private ComputeBuffer normals;
    private ComputeBuffer triangles;
    private ComputeBuffer base_triangle;

    public CompShader(ComputeShader csCreateTriangles_)
    {
        csCreateTriangles = csCreateTriangles_;
        createBaseTriangle();
    }

    // TODO: send only the side size and build the triangle into the compute shader
    public void createBaseTriangle()  //called only at Start function
    {
        baseTriValues = new BaseTriangle(
            new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
            new Vector4(0.03f * 1.75f, 0.0f, 0.0f, 1.0f),
            new Vector4(0.015f * 1.75f, 0.0259807621135332f * 1.75f, 0.0f, 1.0f),
            new Vector4(0.0f, 0.0f, -1.0f, 1.0f)  //triangle normal
            );
    }

    public void runComputeShader(ref Vector3[] in_points, ref Vector3[] in_normals)
    {
        int num_points = in_points.Length;

        //reserve memory
        points = new ComputeBuffer(num_points, 12);  //stride in number of bytes of each element
        normals = new ComputeBuffer(num_points, 12);
        triangles = new ComputeBuffer(num_points * 3, 36);
        base_triangle = new ComputeBuffer(1, 64);

        //fill data
        points.SetData(in_points);
        normals.SetData(in_normals);
        triangles.SetData(new Triangle[num_points]);  //CompShader will write the values

        BaseTriangle[] tmp = new BaseTriangle[1];
        tmp[0] = baseTriValues;
        base_triangle.SetData(tmp);

        //attach buffers to CompShader (uniforms)
        int kernel = csCreateTriangles.FindKernel("CreateTriangles");  //CompShader can have several main functions
        csCreateTriangles.SetBuffer(kernel, "points", points);
        csCreateTriangles.SetBuffer(kernel, "normals", normals);
        csCreateTriangles.SetBuffer(kernel, "triangles", triangles);
        csCreateTriangles.SetBuffer(kernel, "base_triangle", base_triangle);

        //dispatch (Ensure you have enough threads)
        const int thread_group_size_x = 32;  //the same in the compshader file
        int n_groups = (num_points / thread_group_size_x);
        csCreateTriangles.Dispatch(kernel, n_groups, 1, 1);

        //TODO: take into account the not included points

        UnityEngine.Debug.Log("not included " + (num_points % thread_group_size_x) + " points");
    }

    void releaseCSBuffers()
    {
        if (points != null)
        {
            points.Dispose();
            points.Release();
        }

        if (normals != null)
        {
            normals.Dispose();
            normals.Release();
        }

        if (triangles != null)
        {
            triangles.Dispose();
            triangles.Release();
        }

        if (base_triangle != null)
        {
            base_triangle.Dispose();
            base_triangle.Release();
        }
    }

    public void getData(ref Triangle[] result)
    {
        triangles.GetData(result);  //GPU-> CPU
        releaseCSBuffers();  // It is necessary to release compute buffers after using them
    }
}
