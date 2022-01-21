using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArduinoBluetoothAPI;
using System;

public class ExampleNetworkObject : NetworkObject
{
    public int pressureData = 0;

    public ExampleNetworkObject(NetworkObjectType _type, int _id)
    {
        type = _type;
        id = _id;
    }

    public ExampleNetworkObject()
    {

    }

    //UDP 
    public override void UDPProcessStrMessage(Message msg)
    {

        Debug.Log(" str message: " + msg.str_msg);

        if (string.IsNullOrEmpty(msg.str_msg))
        {
            //message is blank.
            return;
        }

        lastRecvdMessage = msg;

        if (msg.str_msg.Contains("CONFIG"))
        {
            msg.str_msg.Replace("CONFIG", "");
            //  netMan.displayConfigPopup("PLEASE CONNECT THIS DEVICE TO : " + msg.str_msg);
            Debug.Log("PLEASE CONNECT THIS DEVICE TO : " + msg.str_msg);

            //TODO Display pop up to user

        }
        else
        {
            if (int.TryParse(msg.str_msg, out pressureData))
            {
                Debug.Log(pressureData);
            }
        }

        //string[] words = msg.str_msg.Split('P');
        //if (words.Length <= 1)
        //{
        //    return;
        //}
        //char[] charsToTrim = { ' ', '\'' };
        //string tmpData = words[1].Trim(charsToTrim);

       
    }

    public override void UDPProcessByteMessage(Message msg)
    {



    }

    public override void OnCharacteristicChanged(BluetoothHelper helper, byte[] data, BluetoothHelperCharacteristic characteristic)
    {
        string values = data[0].ToString();
        pressureData = Int32.Parse(values);
        Debug.Log("Incoming BLE Message : " + pressureData);
    }


}