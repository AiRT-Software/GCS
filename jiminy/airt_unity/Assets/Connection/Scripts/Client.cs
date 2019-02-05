using System.Collections.Generic;
using System;  //Func
using System.Threading;

using NetMQ; // for NetMQConfig, NetMQMessage
using NetMQ.Sockets;


public class MsgHandler
{
    public byte module;
    public byte action;
    public Action<NetMQMessage> f;

    public MsgHandler(byte module_, byte action_, Action<NetMQMessage> f_)
    {
        module = module_;
        action = action_;
        f = f_;
    }
}

public class Client
{
    public bool isConnected = false;

    // sockets
    private ReliableExternalClient reqSrv;
    private SubscriberSocket subscriber;

    // timing
    private float serverTimeout = 0.0f;  // seconds
    private float elasticServerTimeout = 0.0f;  // seconds
    private HRTimer timer;  // ms
    private long lastMessageTS;  // ms
    private bool done;

    private SortedDictionary<System.UInt64, Action<NetMQMessage>> messageHandlers;

    public Client()
    {
        // first of all force asyncio
        AsyncIO.ForceDotNet.Force();

        // time to send all messages before any socket gets disposed, default: 0.
        NetMQConfig.Linger = System.TimeSpan.FromMilliseconds(250);

        timer = new HRTimer();
        subscriber = new SubscriberSocket();
        messageHandlers = new SortedDictionary<System.UInt64, Action<NetMQMessage>>();

        config();
    }

    private void config()
    {
        subscriber.Connect(GlobalSettings.Instance.getSubscriptionPort());
        //subscriber.Subscribe("");  // ## TODO: Cambiar y suscribirse solo a los topicos necesarios según el estado de la app
        byte[] topics = { (byte)Modules.STD_NOTIFICATIONS_MODULE, (byte)Modules.OS_NOTIFICATIONS_MODULE, (byte)Modules.FCS_MULTIPLEXER_NOTIFICATIONS_MODULE, (byte)Modules.ATREYU_NOTIFICATIONS_MODULE };
        subscribeTo(topics);
        //subscriber.Subscribe("A" + (byte)Modules.STD_NOTIFICATIONS_MODULE);
        //subscriber.Subscribe("A" + (byte)Modules.OS_NOTIFICATIONS_MODULE);
        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        serverTimeout = GlobalSettings.Instance.getServerTimeout();

        UnityEngine.Debug.Log(
            String.Format("Client: subscription port {0}, request port {1}",
            GlobalSettings.Instance.getSubscriptionPort(), GlobalSettings.Instance.getRequestPort()));
    }

    public void subscribeTo(byte[] topics)
    {

        for (int i = 0; i < topics.Length; i++)
        {
            byte[] topic = {(byte)'A', topics[i]};
            subscriber.Subscribe(topic);
        }
    }

    public void unsubscribeTo(byte[] topics)
    {
        for (int i = 0; i < topics.Length; i++)
        {
            byte[] topic = { (byte)'A', topics[i] };
            subscriber.Unsubscribe(topic);
        }
    }

    public void unSuscribeAll()
    {
        subscriber.Unsubscribe("");
    }

    protected void startReceiving()
    {
        elasticServerTimeout = serverTimeout;
        done = false;
        timer.Start();
        lastMessageTS = timer.getElapsedMS();

        UnityEngine.Debug.Log("Client: start processing incoming messages");
        while (!done)
        {
            NetMQMessage msg = new NetMQMessage();
            //If client receives any message, it means that the client is connected
            if (subscriber.TryReceiveMultipartMessage(TimeSpan.FromMilliseconds(1), ref msg, 2))
            {
                isConnected = true;
                lastMessageTS = timer.getElapsedMS();
                elasticServerTimeout = serverTimeout;

                System.Diagnostics.Debug.Assert(msg.First.Buffer[0] == (byte)'A');
                UInt64 signature = messageSignature(msg.First.Buffer[1], msg.First.Buffer[2]);

                Action<NetMQMessage> f = null;
                try
                {
                    f = messageHandlers[signature];
                }
                catch (KeyNotFoundException)
                {
                    //UnityEngine.Debug.Log(
                    //    String.Format("Client: key {0} was not present in messageHandlers, module {1}, action {2}",
                    //    signature, msg.First.Buffer[1], msg.First.Buffer[2]));
                }
                if (f != null)
                {
                    f(msg);
                }
            }
            else
            {
                float elapsed = (timer.getElapsedMS() - lastMessageTS) * 0.001f;
                if (elapsed > elasticServerTimeout)
                {
                    isConnected = false;

                    UnityEngine.Debug.Log(String.Format("Connection to the server lost for {0} seconds", elapsed));
                    elasticServerTimeout *= 2;
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(1));
            }
        }

        UnityEngine.Debug.Log("Client: main-loop ends");
    }

    public void stopReceiving()
    {
        done = true;
        UnityEngine.Debug.Log("Client: stop processing messages");

        subscriber.Dispose();
        subscriber.Close();
        subscriber = null;

        Thread.Sleep(TimeSpan.FromMilliseconds(500));

        if(subscriber == null)
        {
            UnityEngine.Debug.Log("Client: subscriber socket closed and null");
        }

        // clean up netmq before quitting the application
        NetMQConfig.Cleanup(false);  // block = true/false, wait or not wait for sockets to send all messages
        UnityEngine.Debug.Log("Client: cleaning up zeromq");
    }

    public void run(List<MsgHandler> mhandlers)
    {
        for (int i = 0; i < mhandlers.Count; i++)
        {
            addHandler(mhandlers[i].module, mhandlers[i].action, mhandlers[i].f);
        }
        UnityEngine.Debug.Log("Client: there are " + messageHandlers.Count + " handlers attending");

        startReceiving();

        UnityEngine.Debug.Log("Client: shutting down");
    }
   /// <summary>
   /// Command to send a module and an action
   /// </summary>
   /// <param name="module">The module to send. See messagecodes</param>
   /// <param name="action">The action that belongs to the module</param>
   /// <returns></returns>
    public bool SendCommand(byte module, byte action)
    {
        NetMQ.Msg req_msg = new StdMessage(module, action).to_Msg();
        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        // Just after the request the atreyu server must respond with a notification
        // (ACK, UNKNOWN_MODULE, or UNDEFINED_MODULE) through the same 'requests' socket.
        if (StdMessage.isStdNotification(ref resp_msg) && StdMessage.getMessageAction(ref resp_msg) != (byte)NotificationType.ACK)
        {
            if (module == (byte)Modules.STD_COMMANDS_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: STD command was not accepted {0}", new StdMessage(module, action).to_string()));
            }
            else if (module == (byte)Modules.POSITIONING_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: POSITIONING_MODULE command was not accepted {0}", new StdMessage(ref resp_msg).to_string()));
            }
            else if (module == (byte)Modules.POINTCLOUD_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: POINTCLOUD_MODULE command was not accepted {0}", new StdMessage(ref resp_msg).to_string()));
            }
            return false;
        }

        return true;
    }
    /// <summary>
    /// Command to send parameters config such as rcam
    /// </summary>
    /// <param name="module"></param>
    /// <param name="action"></param>
    /// <param name="config">See RCamConfigParameter in messagecodes</param>
    /// <param name="value">The value of the config</param>
    /// <returns></returns>
    public bool sendCommand(byte module, byte action, byte config, byte value)
    {
        byte[] header_bytes = new byte[5] { (byte)'A', module, action, config, value};
        NetMQMessage req_msg = new NetMQMessage();
        req_msg.Append(header_bytes);
        
        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response ## la respuesta de dos partes sigue siendo de este tipo?

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        return true;
    }
    /// <summary>
    /// This one is to delete pointclouds
    /// </summary>
    /// <param name="module"></param>
    /// <param name="action"></param>
    /// <param name="pointCloudID"></param>
    /// <returns></returns>
    public bool sendCommand(byte module, byte action,  PointCloudID pointCloudID)
    {
        byte[] header_bytes = new byte[17];
        header_bytes[0] = (byte)'A';
        header_bytes[1] = module;
        header_bytes[2] = action;
        header_bytes[3] = (byte)' ';
        header_bytes[16] = pointCloudID.heading;
        byte[] iArray = BitConverter.GetBytes(pointCloudID.i);
        byte[] jArray = BitConverter.GetBytes(pointCloudID.j);
        byte[] kArray = BitConverter.GetBytes(pointCloudID.k);


        for (int i = 4; i < 8; i++)
        {
            header_bytes[i + 1 * 0] = iArray[i - 4];
            header_bytes[i + 1 * 4] = jArray[i - 4];
            header_bytes[i + 1 * 8] = kArray[i - 4];

        }
        NetMQMessage req_msg = new NetMQMessage();
        req_msg.Append(header_bytes);

        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response ## la respuesta de dos partes sigue siendo de este tipo?

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        return true;
    }
    /// <summary>
    /// Send parameters of the reccam which aren't bytes. Convert the value to send to 4 bytes
    /// </summary>
    /// <param name="module"></param>
    /// <param name="action"></param>
    /// <param name="config"></param>
    /// <param name="valueByte1"></param>
    /// <param name="valueByte2"></param>
    /// <param name="valueByte3"></param>
    /// <param name="valueByte4"></param>
    /// <returns></returns>
    public bool sendCommand(byte module, byte action, byte config, byte valueByte1, byte valueByte2, byte valueByte3, byte valueByte4)
    {
        byte[] header_bytes = new byte[8] { (byte)'A', module, action, config, valueByte1, valueByte2, valueByte3, valueByte4 };
        NetMQMessage req_msg = new NetMQMessage();
        req_msg.Append(header_bytes);

        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response ## la respuesta de dos partes sigue siendo de este tipo?

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        return true;
    }
    /// <summary>
    /// Command to send anchors to calibration
    /// </summary>
    /// <param name="module"></param>
    /// <param name="action"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool sendCommand(byte module, byte action, ServerMessages.IPSFrameAnchorData[] data)
    {
        NetMQMessage req_msg = new NetMQMessage();
        byte[] header_bytes = new byte[8]; // { (byte)'A', module, action, data.id, data.order, data.position };
        header_bytes[0] = (byte)'A';
        header_bytes[1] = module;
        header_bytes[2] = action;
        header_bytes[3] = 0;
        byte[] converted = BitConverter.GetBytes(data.Length);
        System.Buffer.BlockCopy(converted, 0, header_bytes, 4, 4);
        req_msg.Append(header_bytes);
        
        for (int j = 0; j < data.Length; j++) {
            byte[] data_bytes = new byte[17];
            UnityEngine.Debug.Log("Anchor ID: " + data[j].id);
            converted = BitConverter.GetBytes(data[j].id);
            System.Buffer.BlockCopy(converted, 0, data_bytes, 0, 4);

            converted = BitConverter.GetBytes((int)data[j].position.x);
            System.Buffer.BlockCopy(converted, 0, data_bytes, 4, 4);

            converted = BitConverter.GetBytes((int)data[j].position.y);
            System.Buffer.BlockCopy(converted, 0, data_bytes, 8, 4);

            converted = BitConverter.GetBytes((int)data[j].position.z);
            System.Buffer.BlockCopy(converted, 0, data_bytes, 12, 4);

            data_bytes[16] = data[j].order;
            req_msg.Append(data_bytes);
        }

        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        // Just after the request the atreyu server must respond with a notification
        // (ACK, UNKNOWN_MODULE, or UNDEFINED_MODULE) through the same 'requests' socket.
        if (StdMessage.isStdNotification(ref resp_msg) && StdMessage.getMessageAction(ref resp_msg) != (byte)NotificationType.ACK)
        {
            if (module == (byte)Modules.STD_COMMANDS_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: STD command was not accepted {0}", new StdMessage(module, action).to_string()));
            }
            else if (module == (byte)Modules.POSITIONING_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: POSITIONING_MODULE command was not accepted {0}", new StdMessage(ref resp_msg).to_string()));
            }
            else if (module == (byte)Modules.POINTCLOUD_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: POINTCLOUD_MODULE command was not accepted {0}", new StdMessage(ref resp_msg).to_string()));
            }
            return false;
        }

        return true;
    }
    /// <summary>
    /// Send the tags configuration
    /// </summary>
    /// <param name="module"></param>
    /// <param name="action"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool sendCommand(byte module, byte action, ServerMessages.IPSDroneTag data)
    {
        byte[] data_bytes = new byte[31]; // { (byte)'A', module, action, data.id, data.order, data.position };
        data_bytes[0] = (byte)'A';
        data_bytes[1] = module;
        data_bytes[2] = action;
        byte[] converted = BitConverter.GetBytes(data.width);
        for (int i = 0; i < 4; i++)
            data_bytes[i + 3] = converted[i];
        converted = BitConverter.GetBytes(data.height);
        for (int i = 0; i < 4; i++)
            data_bytes[i + 7] = converted[i];
        converted = BitConverter.GetBytes(data.idSW);
        for (int i = 0; i < 4; i++)
            data_bytes[i + 11] = converted[i];
        converted = BitConverter.GetBytes(data.idNW);
        for (int i = 0; i < 4; i++)
            data_bytes[i + 15] = converted[i];
        converted = BitConverter.GetBytes(data.idNE);
        for (int i = 0; i < 4; i++)
            data_bytes[i + 19] = converted[i];
        converted = BitConverter.GetBytes(data.idSE);
        for (int i = 0; i < 4; i++)
            data_bytes[i + 23] = converted[i];
        converted = BitConverter.GetBytes(data.camDist);
        for (int i = 0; i < 4; i++)
            data_bytes[i + 27] = converted[i];
        NetMQMessage req_msg = new NetMQMessage();
        req_msg.Append(data_bytes);

        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        // Just after the request the atreyu server must respond with a notification
        // (ACK, UNKNOWN_MODULE, or UNDEFINED_MODULE) through the same 'requests' socket.
        if (StdMessage.isStdNotification(ref resp_msg) && StdMessage.getMessageAction(ref resp_msg) != (byte)NotificationType.ACK)
        {
            if (module == (byte)Modules.STD_COMMANDS_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: STD command was not accepted {0}", new StdMessage(module, action).to_string()));
            }
            else if (module == (byte)Modules.POSITIONING_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: POSITIONING_MODULE command was not accepted {0}", new StdMessage(ref resp_msg).to_string()));
            }
            else if (module == (byte)Modules.POINTCLOUD_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: POINTCLOUD_MODULE command was not accepted {0}", new StdMessage(ref resp_msg).to_string()));
            }
            return false;
        }

        return true;
    }
   /// <summary>
   /// Sends gimbal angle
   /// </summary>
   /// <param name="module"></param>
   /// <param name="action"></param>
   /// <param name="gimbalAngle"></param>
   /// <returns></returns>
    public bool sendCommand(byte module, byte action, GimbalAngle gimbalAngle)
    {
        byte[] data_bytes = new byte[31]; // { (byte)'A', module, action, data.id, data.order, data.position };
        data_bytes[0] = (byte)'A';
        data_bytes[1] = module;
        data_bytes[2] = action;
        byte[] converted = BitConverter.GetBytes(gimbalAngle.gimbalPitch);
        for (int i = 0; i < 4; i++)
            data_bytes[i + 4] = converted[i];
        converted = BitConverter.GetBytes(gimbalAngle.gimbalRoll);
        for (int i = 0; i < 4; i++)
            data_bytes[i + 8] = converted[i];
        converted = BitConverter.GetBytes(gimbalAngle.gimbalYaw);
        for (int i = 0; i < 4; i++)
            data_bytes[i + 12] = converted[i];
        converted = BitConverter.GetBytes(gimbalAngle.gimbalSpeed);
        for (int i = 0; i < 4; i++)
            data_bytes[i + 16] = converted[i];
        NetMQMessage req_msg = new NetMQMessage();
        req_msg.Append(data_bytes);

        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        // Just after the request the atreyu server must respond with a notification
        // (ACK, UNKNOWN_MODULE, or UNDEFINED_MODULE) through the same 'requests' socket.
        if (StdMessage.isStdNotification(ref resp_msg) && StdMessage.getMessageAction(ref resp_msg) != (byte)NotificationType.ACK)
        {
            if (module == (byte)Modules.STD_COMMANDS_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: STD command was not accepted {0}", new StdMessage(module, action).to_string()));
            }
            else if (module == (byte)Modules.POSITIONING_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: POSITIONING_MODULE command was not accepted {0}", new StdMessage(ref resp_msg).to_string()));
            }
            else if (module == (byte)Modules.POINTCLOUD_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: POINTCLOUD_MODULE command was not accepted {0}", new StdMessage(ref resp_msg).to_string()));
            }
            return false;
        }

        return true;
    }
    /// <summary>
    /// Sends z camera offset
    /// </summary>
    /// <param name="module"></param>
    /// <param name="action"></param>
    /// <param name="camInfo"></param>
    /// <returns></returns>
    public bool sendCommand(byte module, byte action, ServerMessages.DroneCamInfo camInfo)
    {
        byte[] data_bytes = new byte[31]; // { (byte)'A', module, action, data.id, data.order, data.position };
        data_bytes[0] = (byte)'A';
        data_bytes[1] = module;
        data_bytes[2] = action;
        byte[] converted = BitConverter.GetBytes(camInfo.camPos.x);
        for (int i = 0; i < 4; i++)
            data_bytes[i + 4] = converted[i];
        converted = BitConverter.GetBytes(camInfo.camPos.y);
        for (int i = 0; i < 4; i++)
            data_bytes[i + 8] = converted[i];
        converted = BitConverter.GetBytes(camInfo.camPos.z);
        for (int i = 0; i < 4; i++)
            data_bytes[i + 12] = converted[i];
        NetMQMessage req_msg = new NetMQMessage();
        req_msg.Append(data_bytes);

        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        // Just after the request the atreyu server must respond with a notification
        // (ACK, UNKNOWN_MODULE, or UNDEFINED_MODULE) through the same 'requests' socket.
        if (StdMessage.isStdNotification(ref resp_msg) && StdMessage.getMessageAction(ref resp_msg) != (byte)NotificationType.ACK)
        {
            if (module == (byte)Modules.STD_COMMANDS_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: STD command was not accepted {0}", new StdMessage(module, action).to_string()));
            }
            else if (module == (byte)Modules.POSITIONING_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: POSITIONING_MODULE command was not accepted {0}", new StdMessage(ref resp_msg).to_string()));
            }
            else if (module == (byte)Modules.POINTCLOUD_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: POINTCLOUD_MODULE command was not accepted {0}", new StdMessage(ref resp_msg).to_string()));
            }
            return false;
        }

        return true;
    }
    /// <summary>
    /// Sets drone filter. Only pozyx used this
    /// </summary>
    /// <param name="module"></param>
    /// <param name="action"></param>
    /// <param name="space"></param>
    /// <param name="updatePeriod"></param>
    /// <param name="movementFreedom"></param>
    /// <returns></returns>
    public bool sendSetDroneFilter(byte module, byte action,byte space, float updatePeriod, Int32 movementFreedom)
    {
        byte[] header_bytes = new byte[12] { (byte)'A', module, action, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        byte[] convertedUpdatePeriod = BitConverter.GetBytes(updatePeriod);
        byte[] convertedMovementFreedom = BitConverter.GetBytes(movementFreedom);

        System.Buffer.BlockCopy(convertedUpdatePeriod, 0, header_bytes, 4, 4);
        System.Buffer.BlockCopy(convertedMovementFreedom, 0, header_bytes, 8, 4);

        NetMQMessage req_msg = new NetMQMessage();
        req_msg.Append(header_bytes);
        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response ## la respuesta de dos partes sigue siendo de este tipo?

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        return true;
    }
    /// <summary>
    /// Sends takeoff command, not tested
    /// </summary>
    /// <param name="module"></param>
    /// <param name="action"></param>
    /// <param name="number"></param>
    /// <returns></returns>
    public bool sendTakeOffCommand(byte module, byte action, float number)
    {
        byte[] header_bytes = new byte[8] { (byte)'A', module, action, 0, 0, 0, 0, 0 };
        byte[] converted = BitConverter.GetBytes(number);
        System.Buffer.BlockCopy(converted, 0, header_bytes, 4, 4);
        NetMQMessage req_msg = new NetMQMessage();
        req_msg.Append(header_bytes);
        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response ## la respuesta de dos partes sigue siendo de este tipo?

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        return true;
    }
    /// <summary>
    /// Sends a byte alongside a command, for commands that need a byte, such as getting previous reccam parameters config
    /// </summary>
    /// <param name="module"></param>
    /// <param name="action"></param>
    /// <param name="number"></param>
    /// <returns></returns>
    public bool sendCommand(byte module, byte action, int number)
    {
        byte[] header_bytes = new byte[7] { (byte)'A', module, action, 0,0,0,0 };
        NetMQMessage req_msg = new NetMQMessage();
        
        byte[] bytesNumber = BitConverter.GetBytes(number);
        for (int i = 3; i < 7; i++)
        {
            header_bytes[i] = bytesNumber[i - 3];
        }
        req_msg.Append(header_bytes);
        //req_msg.Append((byte)number);
        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response ## la respuesta de dos partes sigue siendo de este tipo?

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        return true;
    }
    /// <summary>
    /// Command to send a path to the server. The path is the second part of the message
    /// </summary>
    /// <param name="module"></param>
    /// <param name="action"></param>
    /// <param name="data_str"></param>
    /// <returns></returns>
    public bool sendTwoPartCommand(byte module, byte action, string data_str)
    {
        byte[] header_bytes = new byte[3] { (byte)'A', module, action };
        NetMQMessage req_msg = new NetMQMessage();
        req_msg.Append(header_bytes);
        req_msg.Append(data_str);
        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response ## la respuesta de dos partes sigue siendo de este tipo?

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }
        
        return true;
    }
    /*
    public bool sendTwoPartCommand(byte module, byte action, int number)
    {
        byte[] header_bytes = new byte[3] { (byte)'A', module, action };
        NetMQMessage req_msg = new NetMQMessage();
        req_msg.Append(header_bytes);
        req_msg.Append((byte)number);
        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response ## la respuesta de dos partes sigue siendo de este tipo?

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        return true;
    }
    */
    /// <summary>
    /// Command to send the matrix to the server
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public bool sendCommand(RegistrationMatrix matrix)
    {
        byte[] header_bytes = new byte[68];
        NetMQMessage req_msg = new NetMQMessage();
        header_bytes[0] = (byte)'A';
        header_bytes[1] = matrix.MatrixHeader.module;
        header_bytes[2] = matrix.MatrixHeader.action;
        header_bytes[3] = matrix.rowMajor;
        for (int i = 0; i < 16; i++)
        {
            byte[] aux = BitConverter.GetBytes(matrix.elems[i]);
            header_bytes[i * 4 + 4] = aux[0];
            header_bytes[i * 4 + 5] = aux[1];
            header_bytes[i * 4 + 6] = aux[2];
            header_bytes[i * 4 + 7] = aux[3];
        }
        req_msg.Append(header_bytes);        

        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response ## la respuesta de dos partes sigue siendo de este tipo?

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        return true;
    }
    /// <summary>
    /// Command to load a plan on the Plan executor
    /// </summary>
    /// <param name="loadPlan"></param>
    /// <param name="guid"></param>
    /// <returns></returns>
    public bool sendTwoPartCommand(LoadPlan loadPlan, string guid)
    {
        byte[] header_bytes = new byte[4] { (byte)'A', loadPlan.PlanHeader.module, loadPlan.PlanHeader.action, loadPlan.indexPath };
        NetMQMessage req_msg = new NetMQMessage();
        req_msg.Append(header_bytes);
        req_msg.Append(guid);
        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response ## la respuesta de dos partes sigue siendo de este tipo?

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        return true;
    }
    /// <summary>
    /// Command to download files
    /// </summary>
    /// <param name="module"></param>
    /// <param name="action"></param>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    /// <param name="fileName">It must end in \0</param>
    /// <returns></returns>
    public bool sendTwoPartCommand(byte module, byte action, ulong offset, ulong size, string fileName)
    {
        byte[] header_bytes = new byte[19];
        header_bytes[0] = (byte)'A';
        header_bytes[1] = module;
        header_bytes[2] = action;
        byte[] bytesConverted = BitConverter.GetBytes(offset);
        for (int i = 0; i < bytesConverted.Length; i++)
        {
            header_bytes[3 + i] = bytesConverted[i];
        }
        bytesConverted = BitConverter.GetBytes(size);
        for (int i = 0; i < bytesConverted.Length; i++)
        {
            header_bytes[11 + i] = bytesConverted[i];
        }
        NetMQMessage req_msg = new NetMQMessage();
        req_msg.Append(header_bytes);
        req_msg.Append(fileName);
        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response ## la respuesta de dos partes sigue siendo de este tipo?

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        return true;
    }
    /// <summary>
    /// Command to request a file on the server ( the array of bytes data is a path)
    /// </summary>
    /// <param name="module"></param>
    /// <param name="action"></param>
    /// <param name="data">It must end in \0</param>
    /// <returns></returns>
    public bool sendTwoPartCommand(byte module, byte action, byte[] data)
    {
        byte[] header_bytes = new byte[3] { (byte) 'A', module, action };

        NetMQMessage req_msg = new NetMQMessage();
        req_msg.Append(header_bytes);
        req_msg.Append(data);

        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response ## la respuesta de dos partes sigue siendo de este tipo?

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        // Just after the request the atreyu server must respond with a notification
        // (ACK, UNKNOWN_MODULE, or UNDEFINED_MODULE) through the same 'requests' socket.
        
        if (StdMessage.isStdNotification(ref resp_msg) && StdMessage.getMessageAction(ref resp_msg) != (byte)NotificationType.ACK)
        {
            if (module == (byte)Modules.STD_COMMANDS_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: STD command was not accepted {0}", new StdMessage(module, action).to_string()));
            }
            else if (module == (byte)Modules.POSITIONING_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: POSITIONING_MODULE command was not accepted {0}", new StdMessage(ref resp_msg).to_string()));
            }
            else if (module == (byte)Modules.POINTCLOUD_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: POINTCLOUD_MODULE command was not accepted {0}", new StdMessage(ref resp_msg).to_string()));
            }
            return false;
        }

        return true;
    }
    /// <summary>
    /// Command to upload a file
    /// </summary>
    /// <param name="module"></param>
    /// <param name="action"></param>
    /// <param name="path">It must end in \0</param>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool sendThreePartCommand(byte module, byte action, string path, byte[] data)
    {
        byte[] header_bytes = new byte[3] { (byte)'A', module, action };
        NetMQMessage req_msg = new NetMQMessage();

        req_msg.Append(header_bytes);
        req_msg.Append(path);
        req_msg.Append(data);

        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response ## la respuesta de dos partes sigue siendo de este tipo?

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }
        return true;

    }
    //No one uses this
    public bool sendThreePartCommand(byte module, byte action, byte[] data1, byte[] data2)
    {
        return sendThreePartCommand(module, action, data1, data1.Length, data2, data2.Length);
    }
    //No one uses this

    public bool sendThreePartCommand(byte module, byte action, byte[] data1, byte[] data2, int size2)
    {
        return sendThreePartCommand(module, action, data1, data1.Length, data2, size2);
    }
    //No one uses this
    public bool sendThreePartCommand(byte module, byte action, byte[] data1, int size1, byte[] data2, int size2)
    {
        byte[] header_bytes = new byte[3] { (byte)'A', module, action };

        NetMQMessage req_msg = new NetMQMessage();
        req_msg.Append(header_bytes);
        req_msg.Append(data1);
        req_msg.Append(data2);
        NetMQ.Msg resp_msg = new StdMessage(0x00, 0x00).to_Msg();  // it will be filled when receiving the response ## la respuesta de dos partes sigue siendo de este tipo?

        reqSrv = new ReliableExternalClient(
            GlobalSettings.Instance.getRequestPort(), TimeSpan.FromMilliseconds(1000), 3);
        if (!reqSrv.sendAndReceive(ref req_msg, ref resp_msg))
        {
            UnityEngine.Debug.Log("Client: server not respoding");
            return false;
        }

        // Just after the request the atreyu server must respond with a notification
        // (ACK, UNKNOWN_MODULE, or UNDEFINED_MODULE) through the same 'requests' socket.
        /*
        if (StdMessage.isStdNotification(ref resp_msg) && StdMessage.getMessageAction(ref resp_msg) != (byte)NotificationType.ACK)
        {
            if (module == (byte)Modules.STD_COMMANDS_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: STD command was not accepted {0}", new StdMessage(module, action).to_string()));
            }
            else if (module == (byte)Modules.POSITIONING_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: POSITIONING_MODULE command was not accepted {0}", new StdMessage(ref resp_msg).to_string()));
            }
            else if (module == (byte)Modules.POINTCLOUD_MODULE)
            {
                UnityEngine.Debug.Log(
                    String.Format("Client: POINTCLOUD_MODULE command was not accepted {0}", new StdMessage(ref resp_msg).to_string()));
            }
            return false;
        }
         */

        return true;
    }

    protected UInt64 messageSignature(byte module, byte action)
    {
        UInt64 msignature = (UInt64)((module << 16) + action);
        return msignature;
    }
    //To add handlers in the modules
    protected void addHandler(byte module, byte action, Action<NetMQMessage> f)
    {
        UInt64 msignature = messageSignature(module, action);
        //UnityEngine.Debug.Log("Client: addHandler: messageSignature " + msignature + ", module " + module + ", action " + action);
        if (!messageHandlers.ContainsKey(msignature))
        {
            messageHandlers.Add(msignature, f);
        }
        else
        {
            UnityEngine.Debug.Log(
                String.Format("Client: key {0} is already present in messageHandlers. Not added.", msignature));
        }
    }

    protected void clearHandlers()
    {
        messageHandlers.Clear();
    }
}
