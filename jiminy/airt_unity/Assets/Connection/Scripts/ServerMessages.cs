using UnityEngine;


public class ServerMessages
{
    public class IPSFrameData
    {
        public double timestamp;
        public Vector3 rotation;
        public Vector3 position;
        public IPSFrameData(double timestamp_, Vector3 rotation_, Vector3 position_)
        {
            timestamp = timestamp_;
            rotation = rotation_;
            position = position_;
        }
    }

    public class IPSFrameAnchorData
    {
        public int id;
        public Vector3 position;
        public byte order;
        public IPSFrameAnchorData(int id_, Vector3 position_, byte order_)
        {
            id = id_;
            position = position_;
            order = order_;
        }
    }

    public class IPSDroneTag
    {
        public int idSW, idNW, idNE, idSE;
        public int width, height, camDist;
        public IPSDroneTag(int idSW_, int idNW_, int idNE_, int idSE_, int width_, int height_, int camDist_)
        {
            idSW = idSW_;
            idNW = idNW_;
            idNE = idNE_;
            idSE = idSE_;
            width = width_;
            height = height_;
            camDist = camDist_;
        }
        public IPSDroneTag()
        {
            idSW = -1;
            idNW = -1;
            idNE = -1;
            idSE = -1;
            width = -1;
            height = -1;
            camDist = -1;
        }
    }

    public class DroneCamInfo
    {
        public Vector3 camPos;
        public DroneCamInfo(float camX_, float camY_, float camZ_)
        {
            camPos.x = camX_;
            camPos.y = camY_;
            camPos.z = camZ_;
        }
        public DroneCamInfo()
        {
            camPos = new Vector3(-1, -1, -1);
        }
    }
}