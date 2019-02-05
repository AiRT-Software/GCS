using NetMQ;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FCSModule  {
    ClientUnity clientUnity;
    public static bool debug = false;
    public FCSModule()
    {
        clientUnity = UnityEngine.GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.FCS_MULTIPLEXER_NOTIFICATIONS_MODULE, (byte)FCSMultiplexerNotificationType.FCS_BATTERY_NOTIFICATION, onBatteryNotification));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.FCS_MULTIPLEXER_NOTIFICATIONS_MODULE, (byte)FCSMultiplexerNotificationType.FCS_MOTORS_NOTIFICATION, onMotorsNotification));
        
    }
    void onBatteryNotification(NetMQMessage m)
    {
        byte batteryLevel = m[0].Buffer[3];

        //The message to land only appears if the battery is under 10%, but if the battery is under 25 the emergency button will turn red without a message
        if ( batteryLevel <= 50 && batteryLevel >= 25 && BatteryManager.state != BatteryManager.batteryState.WARNING)
        {
            BatteryManager.state = BatteryManager.batteryState.WARNING;
            BatteryManager.graphicChanged = false;
        }
        else if (batteryLevel <= 25 && batteryLevel > 10 && BatteryManager.state != BatteryManager.batteryState.CRITICAL)
        {
            BatteryManager.state = BatteryManager.batteryState.CRITICAL;
            BatteryManager.graphicChanged = false;
        }
        else if (batteryLevel <= 10 && BatteryManager.state != BatteryManager.batteryState.SHUTDOWN && debug == false)
        {
            BatteryManager.state = BatteryManager.batteryState.SHUTDOWN;
            BatteryManager.graphicChanged = false;
        }
        else if(batteryLevel > 50 && BatteryManager.state != BatteryManager.batteryState.FULL || debug == true)
        {
            BatteryManager.state = BatteryManager.batteryState.FULL;
            BatteryManager.graphicChanged = false;
        }
        //UnityEngine.Debug.Log(batteryLevel);
    }
    void onMotorsNotification(NetMQMessage m)
    {
        byte motorPower = m[0].Buffer[3];
        if (motorPower < 1 && LowBatteryWarning.sendLandingCommand == true) // If the engine power is at 3 and we have received a landing command, the drone has landed
        {
            LowBatteryWarning.landed = true;
        }
        //UnityEngine.Debug.Log(motorPower);
    }

    /*
     if(LowBatteryWarning.landed = true){
     *      LowBatteryWarning.sendLandingCommand = false;
     *      LowBatteryWarning.landed = false;
     *      sendCommand(STOP_POSITIONING)
     * }
     */
}
