using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Interpolation between points with a Catmull-Rom spline
public class CatmullRomSpline : MonoBehaviour
{
    /// <summary>
    /// This one is called for the camera that looks at front. Going to have to change to for the 3D.
    /// </summary>
    /// <param name="path">The path that contains the waypoints</param>
    /// <param name="pos">The position of the path array around where the 4 points will be taken</param>
    /// <param name="middlePoints">The list where the points of the curve will be saved</param>
    /// <param name="size"> Camera size to get the number of points in a curve right, for optimizations</param>
    /// <param name="update">If the array of middlepoints should be updated</param>
    /// <param name="cam">The camera where the curve will be displayed. As this works wit a GL Unity method, take into account the camera transformations when creating this.</param>
    public static void DisplayCatmullRomSpline(Path path, int pos, ref List<Vector3> middlePoints, float size, bool update, Camera cam) // ## La lista "path" se cogerá del singleton PATH
    {
        //The 4 points we need to form a spline between p1 and p2
        Vector3 p0 = new Vector3(path.GetPoint(ClampListPos(pos - 1, path.Count())).PointTransf.position.x, path.GetPoint(ClampListPos(pos - 1, path.Count())).PointTransf.position.y, path.GetPoint(ClampListPos(pos - 1, path.Count())).PointTransf.position.z);
        Vector3 p1 = new Vector3(path.GetPoint(pos).PointTransf.position.x, path.GetPoint(pos).PointTransf.position.y, path.GetPoint(pos).PointTransf.position.z);
        Vector3 p2 = new Vector3(path.GetPoint(ClampListPos(pos + 1, path.Count())).PointTransf.position.x, path.GetPoint(ClampListPos(pos + 1, path.Count())).PointTransf.position.y, path.GetPoint(ClampListPos(pos + 1, path.Count())).PointTransf.position.z);
        Vector3 p3 = new Vector3(path.GetPoint(ClampListPos(pos + 2, path.Count())).PointTransf.position.x, path.GetPoint(ClampListPos(pos + 2, path.Count())).PointTransf.position.y, path.GetPoint(ClampListPos(pos + 2, path.Count())).PointTransf.position.z);

        //The start position of the line
        Vector3 lastPos = p1;
        Vector3 prevPerpendicular = new Vector3();
        Vector3 v1P = new Vector3();
        Vector3 v2P = new Vector3();
        //The spline's resolution
        //Make sure it's is adding up to 1, so 0.3 will give a gap, but 0.2 will work
        if (size > 15)
        {
            size /= 3;
        }

        Vector2 p0Aux, p1Aux, p2Aux, p3Aux;

        Vector3 min = new Vector3(1, 1, 1);
        Vector3 max = new Vector3(0, 0, 0);
        int dot = 0;
        //Translating from the camera position to viewport, in order to calculate the distance between points better, for optimizations
        p0Aux = Vector3.Max(Vector3.Min(cam.WorldToViewportPoint(p0), min), max) * 10;
        p1Aux = Vector3.Max(Vector3.Min(cam.WorldToViewportPoint(p1), min), max) * 10;
        p2Aux = Vector3.Max(Vector3.Min(cam.WorldToViewportPoint(p2), min), max) * 10;
        p3Aux = Vector3.Max(Vector3.Min(cam.WorldToViewportPoint(p3), min), max) * 10;
        //UnityEngine.Debug.Log("P0: " + p0Aux + " P1: " + p1Aux + " P2: " + p2Aux + " P3: " + p3Aux);
        //The spline's resolution
        //Make sure it's is adding up to 1, so 0.3 will give a gap, but 0.2 will work
        float distance = Vector3.Distance(p0Aux, p1Aux) + Vector3.Distance(p1Aux, p2Aux) + Vector3.Distance(p2Aux, p3Aux);
        distance = Mathf.CeilToInt(distance);
        distance = 1.0f / distance;
        //middlePoints.Add(lastPos);
        int loops = Mathf.CeilToInt(1f / distance);
        //If the formula didn't give enough loops, make sure there are enough
        if (loops < 2)
        {
            loops = 2;
        }
        //We assign the number of points there will be
        path.GetPoint(pos).SegmentsRight = loops;

        Matrix4x4 camMat = cam.transform.localToWorldMatrix;

        Vector3 upCam = new Vector3(camMat.m01, camMat.m11, camMat.m21);
        Vector3 rightCam = new Vector3(camMat.m00, camMat.m10, camMat.m20);

        //If there are only two loops, we create a straight line. If this if doesn't happen, the line bugs and spreads all over the screen. 
        if (loops <= 2)
        {
            //Calculate the perpendicular, to create a quad instead of a line
            Vector3 perpendicular = (new Vector3(p2.y, p1.x, 0) -
                                   new Vector3(p1.y, p2.x, 0)).normalized * (size / 160);
            Vector3 v1 = new Vector3(p1.x, p1.y, p1.z + 0.1f);
            Vector3 v2 = new Vector3(p2.x, p2.y, p2.z + 0.1f);
            GL.Begin(GL.QUADS);

            //Draw the line
            GL.TexCoord2(0, 0);
            GL.Vertex((v1 + perpendicular + cam.transform.up * 0.1f));

            GL.TexCoord2(1, 0);
            GL.Vertex((v1 - perpendicular - cam.transform.up * 0.1f));

            GL.TexCoord2(1, 1);
            GL.Vertex((v2 - perpendicular - cam.transform.up * 0.1f));

            GL.TexCoord2(0, 1);
            GL.Vertex((v2 + perpendicular + cam.transform.up * 0.1f));

            GL.End();
            //Add the points to the array
            if (update)
            {
                middlePoints.Add(p1);
                middlePoints.Add(p2);
            
            }
            dot++;
            return;
        }
        //We create a curve
        for (int i = 1; i <= loops; i++)
        {
            //Which t position are we at?
            float t = i * distance;

            //Find the coordinate between the end points with a Catmull-Rom spline
            Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);
            //Draw this line segment
            Vector3 perpendicular = (new Vector3(newPos.y, lastPos.x, newPos.z) -
                                   new Vector3(lastPos.y, newPos.x, lastPos.z)).normalized * (size/40);
            Vector3 v1 = new Vector3(lastPos.x, lastPos.y, lastPos.z );
            Vector3 v2 = new Vector3(newPos.x, newPos.y, newPos.z );
            //To make a seamless transition, we do this by parts
            if (i == 1)
            {
                GL.Begin(GL.QUADS);

                //We draw the first two points of the Quad
                GL.TexCoord2(0, 0);
                GL.Vertex((v1 + perpendicular + upCam * 0.1f + rightCam * 0f));
                
                GL.TexCoord2(1, 0);
                GL.Vertex((v1 - perpendicular + upCam * -0.1f + rightCam * 0f));

                prevPerpendicular = perpendicular;
            }
            else if (i == loops)
            {
                //We draw the last quad

                GL.TexCoord2(1, 1);
                GL.Vertex((v2 - perpendicular + upCam * -0.1f + rightCam * 0f));

                GL.TexCoord2(0, 1);
                GL.Vertex((v2 + perpendicular + upCam * 0.1f + rightCam * 0f));

                GL.End();
            }
            else
            {
                                //We draw the middle of the curve

                v2P = ((v1 + prevPerpendicular) + (v1 + perpendicular)) / 2.0f;
                v1P = ((v1 - prevPerpendicular) + (v1 - perpendicular)) / 2.0f;

                
                GL.TexCoord2(1, 1);
                GL.Vertex((v1P + upCam * -0.1f + rightCam * 0f));

                GL.TexCoord2(0, 1);
                GL.Vertex((v2P + upCam * 0.1f + rightCam * 0f));
                GL.End();
                GL.Begin(GL.QUADS);
                
                GL.TexCoord2(0, 0);
                GL.Vertex((v2P + upCam * 0.1f + rightCam * 0f));

                GL.TexCoord2(1, 0);
                GL.Vertex((v1P + upCam * -0.1f + rightCam * 0f));

                prevPerpendicular = perpendicular;

            }
            //GL.Vertex((v1 - perpendicular));
            //GL.Vertex((v1 + perpendicular));
            //GL.Vertex((v2 + perpendicular));
            //GL.Vertex((v2 - perpendicular));


            //GL.Vertex3(lastPos.x, lastPos.y, lastPos.z);
            //GL.Vertex3(newPos.x, newPos.y, newPos.z);

            //Save this pos so we can draw the next line segment
            lastPos = newPos;

            if(update)
                middlePoints.Add(lastPos);
            dot++;
        }
    }
    /// <summary>
    /// Same as the other function but for a top down camera
    /// </summary>
    /// <param name="path"></param>
    /// <param name="pos"></param>
    /// <param name="middlePoints"></param>
    /// <param name="size"></param>
    /// <param name="update"></param>
    /// <param name="cam"></param>
    public static void DisplayCatmullRomSpline2(Path path, int pos, ref List<Vector3> middlePoints, float size, bool update, Camera cam) // ## La lista "path" se cogerá del singleton PATH
    {
        //The 4 points we need to form a spline between p1 and p2
        Vector3 p0 = new Vector3(path.GetPoint(ClampListPos(pos - 1, path.Count())).PointTransf.position.x, path.GetPoint(ClampListPos(pos - 1, path.Count())).PointTransf.position.y, path.GetPoint(ClampListPos(pos - 1, path.Count())).PointTransf.position.z);
        Vector3 p1 = new Vector3(path.GetPoint(pos).PointTransf.position.x, path.GetPoint(pos).PointTransf.position.y, path.GetPoint(pos).PointTransf.position.z);
        Vector3 p2 = new Vector3(path.GetPoint(ClampListPos(pos + 1, path.Count())).PointTransf.position.x, path.GetPoint(ClampListPos(pos + 1, path.Count())).PointTransf.position.y, path.GetPoint(ClampListPos(pos + 1, path.Count())).PointTransf.position.z);
        Vector3 p3 = new Vector3(path.GetPoint(ClampListPos(pos + 2, path.Count())).PointTransf.position.x, path.GetPoint(ClampListPos(pos + 2, path.Count())).PointTransf.position.y, path.GetPoint(ClampListPos(pos + 2, path.Count())).PointTransf.position.z);

        //The start position of the line
        Vector3 lastPos = p1;
        Vector3 prevPerpendicular = new Vector3();
        Vector3 v1P = new Vector3();
        Vector3 v2P = new Vector3();

        if (size > 15)
        {
            size /= 3;
        }

        Vector2 p0Aux, p1Aux, p2Aux, p3Aux;

        Vector3 min = new Vector3(1, 1, 1);
        Vector3 max = new Vector3(0, 0, 0);

        p0Aux = Vector3.Max(Vector3.Min(cam.WorldToViewportPoint(p0), min), max) * 10;
        p1Aux = Vector3.Max(Vector3.Min(cam.WorldToViewportPoint(p1), min), max) * 10;
        p2Aux = Vector3.Max(Vector3.Min(cam.WorldToViewportPoint(p2), min), max) * 10;
        p3Aux = Vector3.Max(Vector3.Min(cam.WorldToViewportPoint(p3), min), max) * 10;
        //UnityEngine.Debug.Log("P0: " + p0Aux + " P1: " + p1Aux + " P2: " + p2Aux + " P3: " + p3Aux);
        //The spline's resolution
        //Make sure it's is adding up to 1, so 0.3 will give a gap, but 0.2 will work
        float distance = Vector3.Distance(p0Aux, p1Aux) + Vector3.Distance(p1Aux, p2Aux) + Vector3.Distance(p2Aux, p3Aux);
        distance = Mathf.CeilToInt(distance);
        distance = 1.0f / distance;
        //How many times should we loop?
        int loops = Mathf.CeilToInt(1f / distance);
        path.GetPoint(pos).SegmentsTop = loops;

        if (loops <= 2)
        {

            Vector3 perpendicular = (new Vector3(p1.z, 0, p2.x) -
                                         new Vector3(p2.z, 0, p1.x)).normalized * (size / 40);
            Vector3 v1 = new Vector3(p1.x, p1.y - 0.1f, p1.z);
            Vector3 v2 = new Vector3(p2.x, p2.y - 0.1f, p2.z);
            GL.Begin(GL.QUADS);


            GL.TexCoord2(0, 0);
            GL.Vertex((v1 - perpendicular));

            GL.TexCoord2(1, 0);
            GL.Vertex((v1 + perpendicular));

            GL.TexCoord2(1, 1);
            GL.Vertex((v2 + perpendicular));

            GL.TexCoord2(0, 1);
            GL.Vertex((v2 - perpendicular));

            GL.End();
            if (update)
            {
                middlePoints.Add(p1);
                middlePoints.Add(p2);

            }
            return;
        }
        //path.GetPoint(pos).Segments = loops;

        for (int i = 1; i <= loops; i++)
        {
            //Which t position are we at?
            float t = i * distance;
            //UnityEngine.Debug.Log(t);

            //Find the coordinate between the end points with a Catmull-Rom spline
            Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

            //Draw this line segment
            
            Vector3 perpendicular = (new Vector3(lastPos.z,0,newPos.x) -
                                         new Vector3(newPos.z, 0, lastPos.x)).normalized * (size / 40);
            Vector3 v1 = new Vector3(lastPos.x, lastPos.y - 0.1f, lastPos.z);
            Vector3 v2 = new Vector3(newPos.x, newPos.y - 0.1f, newPos.z);

            if (i == 1)
            {
                GL.Begin(GL.QUADS);

                GL.TexCoord2(0, 0);
                GL.Vertex((v1 - perpendicular));
                GL.TexCoord2(1, 0);
                GL.Vertex((v1 + perpendicular));

                prevPerpendicular = perpendicular;
            }
            else if (i == loops) {

                GL.TexCoord2(1, 1);
                GL.Vertex((v2 + perpendicular));
                GL.TexCoord2(0, 1);
                GL.Vertex((v2 - perpendicular));

                GL.End();
            }
            else
            {
                v2P = ((v1 + prevPerpendicular) + (v1 + perpendicular)) / 2.0f;
                v1P = ((v1 - prevPerpendicular) + (v1 - perpendicular)) / 2.0f;

                GL.TexCoord2(1, 1);
                GL.Vertex(v2P);
                GL.TexCoord2(0, 1);
                GL.Vertex(v1P);
                GL.End();
                GL.Begin(GL.QUADS);
                GL.TexCoord2(0, 0);
                GL.Vertex(v1P);
                GL.TexCoord2(1, 0);
                GL.Vertex(v2P);

                prevPerpendicular = perpendicular;

            }
            //GL.Vertex((v1 - perpendicular));
            //GL.Vertex((v1 + perpendicular));
            //GL.Vertex((v2 + perpendicular));
            //GL.Vertex((v2 - perpendicular));


            //GL.Vertex3(lastPos.x, lastPos.y, lastPos.z);
            //GL.Vertex3(newPos.x, newPos.y, newPos.z);

            //Save this pos so we can draw the next line segment
            lastPos = newPos;

            if(update)
                middlePoints.Add(lastPos);

        }
    }
    /// <summary>
    /// The function that gives the catmull rom for 4 points
    /// </summary>
    /// <param name="t"></param>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <returns></returns>
    static Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        //The cubic polynomial: a + b * t + c * t^2 + d * t^3
        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

        return pos;
    }
    /// <summary>
    /// Gives us the necessary points for the catmull function
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="pathCount"></param>
    /// <returns></returns>
    static int ClampListPos(int pos, int pathCount) // ## pathCount se cogerá del singleton PATH
    {
        if (pos < 0)
        {
            pos = 0;
        }

        if (pos > pathCount)
        {
            pos = pathCount - 1;
        }
        else if (pos > pathCount - 1)
        {
            pos = pathCount - 1;
        }

        return pos;
    }

}