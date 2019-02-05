public static class PCLMsgOffsets
{
    public const int AIRT = 0;  // byte position
    public const int MODULE = 1;
    public const int ACTION = 2;
    public const int SPACE = 3;
    public const int NUMPOINTS = 4;
    public const int X = 8;
    public const int Y = 12;
    public const int Z = 16;
    public const int PITCH = 20;
    public const int ROLL = 24;
    public const int YAW = 28;

    public const int ID_I = 32;
    public const int ID_J = 36;
    public const int ID_K = 40;
    public const int ID_HEADING = 44;

    public const int HEADER_SIZE = 45;  // num of bytes

    public const int POINT_X = 0;  // byte position
    public const int POINT_Y = 4;
    public const int POINT_Z = 8;

    public const int POINT_R = 12;
    public const int POINT_G = 13;
    public const int POINT_B = 14;
    public const int POINT_A = 15;

    public const int NORMAL_X = 16;
    public const int NORMAL_Y = 20;
    public const int NORMAL_Z = 24;

    public const int POINT_SIZE = 16;
    public const int POINTNORMAL_SIZE = 28;  // num of bytes
}

