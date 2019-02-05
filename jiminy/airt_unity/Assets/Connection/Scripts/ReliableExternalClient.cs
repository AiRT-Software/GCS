using System;  //TimeSpan
using NetMQ;   //NetMQConfig
using NetMQ.Sockets;

/// <summary>
/// Reliable client that uses timeout for waiting to receive the response
/// and retries if no response was received
/// </summary>
public class ReliableExternalClient
{
    private NetMQSocket socket;
    private string srvAddress;
    private TimeSpan timeout;
    private uint retries;

    private const int min_trysend_ms = 100;

    public ReliableExternalClient(string srvAdress_, TimeSpan timeout_, uint retries_)
    {
        socket = null;

        srvAddress = srvAdress_;
        timeout = timeout_;
        retries = retries_;

        if (timeout.TotalMilliseconds < min_trysend_ms)
        {
            UnityEngine.Debug.Log(
                String.Format("Timeout {0} has to be greater than min_trysend_time {1} ms",
                timeout.TotalMilliseconds, min_trysend_ms));
        }
    }

    private void reconnect()
    {
        //UnityEngine.Debug.Log(String.Format("Connecting to server {0}", srvAddress));

        socket = new RequestSocket();
        socket.Connect(srvAddress);
        // By defaul a socket uses the global NetMQConfig.Linger value
        // but we will configure the socket to not wait for closing
        socket.Options.Linger = TimeSpan.FromMilliseconds(10);
    }

    public bool sendAndReceive(ref NetMQ.Msg req_msg, ref NetMQ.Msg resp_msg)
    {
        if (socket == null)
            reconnect();

        StdMessage m = new StdMessage(req_msg.Data[1], req_msg.Data[2]);
        NetMQ.Msg copy = new NetMQ.Msg();
        bool ok = false;

        for (uint i = 0; i < retries; i++)
        {
            copy.Copy(ref req_msg);

            // send
            if (!socket.TrySend(ref copy, TimeSpan.FromMilliseconds(min_trysend_ms), false))
            {
                ok = false;
                UnityEngine.Debug.Log("ReliableExternalClient: could not send");
                break;

                //TODO: clear enqueued messages when server is offline
            }
            //UnityEngine.Debug.Log("ReliableExternalClient: request sent " + m.to_string());

            // receive
            if (socket.TryReceive(ref resp_msg, timeout))
            {
                ok = true;
               // UnityEngine.Debug.Log("ReliableExternalClient: response received "
                    //+ new StdMessage(resp_msg.Data[1], resp_msg.Data[2]).to_string());
                break;
            }

            //UnityEngine.Debug.Log(String.Format("ReliableExternalClient: no response from server {0}. Retrying", srvAddress));
            reconnect();
        }

        if (!ok)
        {
            UnityEngine.Debug.Log(String.Format("ReliableExternalClient: server {0} seems to be offline. Abandoning", srvAddress));
        }

        copy.Close();
        socket.Dispose();  //call Dispose on all sockets before cleanup the netmq context
        socket.Close();

        socket = null;

        if (socket == null)
        {
            UnityEngine.Debug.Log("ReliableExternalClient: socket closed and null");
        }

        return ok;
    }

    public bool sendAndReceive(ref NetMQMessage req_msg, ref NetMQ.Msg resp_msg)
    {

        if (socket == null)
            reconnect();

        bool ok = false;
        NetMQMessage copy = new NetMQMessage(req_msg);

        socket.SendMultipartMessage(copy);

        // receive
        if (socket.TryReceive(ref resp_msg, timeout))
        {
            ok = true;
            //UnityEngine.Debug.Log("ReliableExternalClient: response received "
            //    + new StdMessage(resp_msg.Data[1], resp_msg.Data[2]).to_string());
        }

        copy.Clear();
        socket.Dispose(); 
        socket.Close();
        socket = null;
        if (socket == null)
        {
            UnityEngine.Debug.Log("ReliableExternalClient: socket closed and null");
        }

        return ok;
    }
}
