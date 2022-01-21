
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using ArduinoBluetoothAPI;

/// <summary>
/// This is an abstract class, that is used to communicate with a networked device
/// through either BLE, or WiFi. This class is not derived from monoBehavior, it is intended that 
/// you make a new instance of this class, and use it inside a monoBehavior script.
/// </summary>
[System.Serializable]
public abstract class NetworkObject //: MonoBehaviour
{
    public enum NetworkObjectType { ble = 0, udp = 1, tcp = 2 }
    public int id { get; set; }
    public NetworkObjectType type { get; set; }
    public string bleName { get; set; }
    public bool connected { get; set; }

    public Message lastRecvdMessage;

    public Message outgoingMessage;

    public bool readyToSend = false;

    public string newSSID, newPass; // used to change the wifi being used by Unity and this device
    public bool resetNetwork;

    public List<BluetoothHelperCharacteristic> bleCharacteristics;
    public BluetoothDevice bleDevice;
    public BluetoothHelper bleHelper;
    public string networkObjectAddressID;

    #region UDP

    public void SetOutgoingMessage(Message msg)
    {
        msg.fromUnity = true;
        outgoingMessage = msg;
        readyToSend = true;
    }


    public Message GetOutgoingMessage()
    {
        return outgoingMessage;
    }


    /// <summary>
    /// This method processes a message received in string format. 
    /// This is a virtual method, intended to be overriten in a custom made child class
    /// to do whatever with the recieved data. 
    /// </summary>
    /// <param name="msg"></param>
    public virtual void UDPProcessStrMessage(Message msg)
    {
        if (!string.IsNullOrEmpty(msg.str_msg))
        {
            lastRecvdMessage = msg;
        }
        else
        {
            //Invalid msg
            Debug.Log("Unable to process string message " + msg.str_msg);
        }
    }

    /// <summary>
    /// This method processes a message recieved in byte format. 
    /// This is a virtual method, intended to be overriten in a custom made child class
    /// to do whatever with the recieved data. 
    /// </summary>
    /// <param name="msg"></param>
    public virtual void UDPProcessByteMessage(Message msg)
    {
        //This base method simply saves the message, if it is valid.
        if ( msg.byte_msg.Length > 0)
        {
            lastRecvdMessage = msg;
        }
        else
        {
            //Invalid msg
        }
    }

    #endregion

    #region BLE

    /// <summary>
    /// 
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="data"></param>
    /// <param name="characteristic"></param>
    public virtual void OnCharacteristicChanged(BluetoothHelper helper, byte[] data, BluetoothHelperCharacteristic characteristic)
    {

        //fill in desired logic here, parse through ble data and use data as needed. 

        //EXAMPLE:
        //string values = data[0].ToString();
        //intValue = Int32.Parse(values);
        //Debug.Log("Incoming BLE Message : " + intValue);

    }

    public virtual void OnCharacteristicNotFound(BluetoothHelper helper, byte[] data, BluetoothHelperCharacteristic characteristic)
    {

        //fill in desired logic here,
        Debug.Log("Characteristic " + characteristic.getName() + " Not found!");

    }

    #endregion

}
