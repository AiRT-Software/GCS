using NetMQ;
using System;

public class AIRT_Message_Header
{
    const byte AIRT_SIGNATURE = (byte)'A';

    public byte airt; // 'A'
    public byte module;
    public byte action;
    public byte[] data;

    public AIRT_Message_Header(byte module_, byte action_)
    {
        airt = AIRT_SIGNATURE;
        module = module_;
        action = action_;
    }

    // overload comparison operators
    public static bool operator ==(AIRT_Message_Header obj1, AIRT_Message_Header obj2)
    {
        if (object.ReferenceEquals(obj1, obj2)) { return true; }
        if (object.ReferenceEquals(obj1, null)) { return false; }
        if (object.ReferenceEquals(obj2, null)) { return false; }

        return (obj1.airt == obj2.airt && obj1.module == obj2.module && obj1.action == obj2.action);
    }

    public static bool operator !=(AIRT_Message_Header obj1, AIRT_Message_Header obj2)
    {
        return !(obj1 == obj2);
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        var b2 = (AIRT_Message_Header)obj;
        return (airt == b2.airt && module == b2.module && action == b2.action);
    }

    public override int GetHashCode()
    {
        return airt.GetHashCode() ^ module.GetHashCode() ^ action.GetHashCode();
    }
}

public class StdMessage : AIRT_Message_Header
{
    // constructor
    public StdMessage(byte module_, byte action_)
        : base(module_, action_)
    { }

    // constructor
    public StdMessage(ref NetMQ.Msg m)
        : base(m.Data[1], m.Data[2])
    { }

    public string to_string()
    {
        string actionname = "";
        switch (module)
        {
            case (byte)Modules.STD_COMMANDS_MODULE:
                actionname = Enum.GetName(typeof(CommandType), action);
                break;
            case (byte)Modules.POINTCLOUD_MODULE:
                actionname = Enum.GetName(typeof(PointcloudCommandType), action);
                break;
            case (byte)Modules.POSITIONING_MODULE:
                actionname = Enum.GetName(typeof(PositioningCommandType), action);
                break;
            case (byte)Modules.ATREYU_MODULE:
                actionname = Enum.GetName(typeof(AtreyuCommandType), action);
                break;
            case (byte)Modules.STD_NOTIFICATIONS_MODULE:
                actionname = Enum.GetName(typeof(NotificationType), action);
                break;
            case (byte)Modules.POINTCLOUD_NOTIFICATIONS_MODULE:
                actionname = Enum.GetName(typeof(PointcloudNotificationType), action);
                break;
            case (byte)Modules.POSITIONING_NOTIFICATIONS_MODULE:
                actionname = Enum.GetName(typeof(PositioningNotificationType), action);
                break;
            case (byte)Modules.ATREYU_NOTIFICATIONS_MODULE:
                actionname = Enum.GetName(typeof(AtreyuNotificationType), action);
                break;
        }
        if (actionname == null)
        {
            // std_command (start, stop, quit) addressed to an specific module
            actionname = Enum.GetName(typeof(CommandType), action);
        }

        if(data != null)
            return "msg {" + airt.ToString() + ", " + Enum.GetName(typeof(Modules), module) + ", " + actionname + ", "+ data + "}";

        return "msg {" + airt.ToString() + ", " + Enum.GetName(typeof(Modules), module) + ", " + actionname + "}";
    }

    public NetMQ.Msg to_Msg()
    {
        NetMQ.Msg msg = new NetMQ.Msg();
        byte[] msg_bytes = new byte[3] { airt, module, action };

        msg.InitPool(msg_bytes.Length);  // num of bytes
        msg.Put(msg_bytes, 0, msg_bytes.Length);

        return msg;
    }

    // utils
    public static bool isStdNotification(ref NetMQ.Msg m)
    {
        return m.Data[0] == (byte)'A' && m.Data[1] == (byte)Modules.STD_NOTIFICATIONS_MODULE;
    }

    public static bool isStdNotification(ref NetMQMessage m)
    {
        return m.First.Buffer[0] == (byte)'A' && m.First.Buffer[1] == (byte)Modules.STD_NOTIFICATIONS_MODULE;
    }

    public static byte getMessageAirtSignature(ref NetMQMessage m)
    {
        return m.First.Buffer[0];
    }

    public static byte getMessageModule(ref NetMQMessage m)
    {
        return (byte)(m.First.Buffer[1] & 0x7F);
    }

    public static byte getMessageAction(ref NetMQMessage m)
    {
        return m.First.Buffer[2];
    }

    public static byte getMessageAirtSignature(ref NetMQ.Msg m)
    {
        return m.Data[0];
    }

    public static byte getMessageModule(ref NetMQ.Msg m)
    {
        return (byte)(m.Data[1] & 0x7F);
    }

    public static byte getMessageAction(ref NetMQ.Msg m)
    {
        return m.Data[2];
    }

}
