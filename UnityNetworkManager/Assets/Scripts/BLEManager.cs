using ArduinoBluetoothAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BLEManager : MonoBehaviour
{
    private BluetoothHelper helper;
    private BluetoothHelperCharacteristic bluetoothHelperCharacteristic;
    private LinkedList<BluetoothDevice> devices;

    private bool isScanning;
    private bool isConnecting;
    private int intValue;

    private bool ledStatus = false;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            BluetoothHelper.BLE = true;
            helper = BluetoothHelper.GetInstance();
            helper.OnConnected += OnConnected;
            helper.OnConnectionFailed += OnConnectionFailed;
            helper.OnScanEnded += OnScanEnded;
            helper.OnServiceNotFound += OnServiceNotFound;
            //helper.OnDataReceived += OnDataReceived;
            helper.OnCharacteristicChanged += OnCharacteristicChanged;
            //helper.OnCharacteristicNotFound += (helper, serviceName, characteristicName) =>
            //{
            //    Debug.Log(characteristicName);
            //};

        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (helper == null)
            return;

        if (!helper.isConnected() && !isScanning && !isConnecting)
        {
            isScanning = helper.ScanNearbyDevices();
            if (devices != null && devices.First != null)
            {
                draw();
            }
        }

        if (helper.isConnected())
        {
            helper.ReadCharacteristic(bluetoothHelperCharacteristic);
        }

    }

    void OnScanEnded(BluetoothHelper helper, LinkedList<BluetoothDevice> devices)
    {
        this.isScanning = false;
        this.devices = devices;
    }

    void OnConnected(BluetoothHelper helper)
    {
        isConnecting = false;
    }

    void OnConnectionFailed(BluetoothHelper helper)
    {
        isConnecting = false;
        Debug.Log("Connection lost");
    }

    void OnCharacteristicChanged(BluetoothHelper helper, byte[] data, BluetoothHelperCharacteristic characteristic)
    {
        string values = data[0].ToString();
        intValue = Int32.Parse(values);
        Debug.Log("Incoming BLE Message : " + intValue);
    }

    void OnServiceNotFound(BluetoothHelper helper, string service)
    {
        Debug.Log($"Service [{service}] not found");
    }

    void OnDestroy()
    {
        if (helper != null)
            helper.Disconnect();
    }

    private void draw()
    {
        LinkedListNode<BluetoothDevice> node = devices.First;
        for (int i = 0; i < devices.Count; i++)
        {
            string bluetoothName = node.Value.DeviceName;
            Debug.Log("Found: " + bluetoothName);

            if (bluetoothName.Equals("SIMETRI BLE Local"))
            {
                Debug.Log("device name found ");
                helper.setDeviceName(bluetoothName);
                try
                {
                    helper.Connect();
                    isConnecting = true;
                    bluetoothHelperCharacteristic = new BluetoothHelperCharacteristic("3c3957d2-c7a6-11eb-b8bc-0242ac130002", "3c3955a2-c7a6-11eb-b8bc-0242ac130003");
                    helper.Subscribe(bluetoothHelperCharacteristic);
                }
                catch (Exception e)
                {
                    isConnecting = false;
                    Debug.Log("SIMETRI BLE not found " + e.ToString());
                }
            }
            node = node.Next;
            if (node == null)
                return;
        }
    }

    private void CloseAll()
    {
        if (helper != null)
            helper.Disconnect();
    }

    private void OnApplicationQuit()
    {
        CloseAll();
    }

}
