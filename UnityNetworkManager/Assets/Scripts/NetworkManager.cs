using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArduinoBluetoothAPI;
using System.Threading;
using System.Text;
using System.Net;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

/// <summary>
/// UDP Message object
/// </summary>
public class Message
{
    public DateTime timestamp { get; set; }
    public string str_msg { get; set; }
    public byte[] byte_msg { get; set; }
    public int id { get; set; }
    public bool fromUnity { get; set; } //did the message come from the Unity side, or the Arduino side?
}

/// <summary>
/// This script handles all communication with the Simetri or SensoryX tracking devices.
/// </summary>
public class NetworkManager : MonoBehaviour
{
    #region Variables

    [SerializeField]
    public bool udpEnabled;

    [SerializeField]
    public bool bleEnabled;

    [SerializeField]
    public bool tcpEnabled;

    [SerializeField]
    public bool useStringFormat;

    [SerializeField]
    public List<NetworkObject> networkObjects;

    [SerializeField]
    GameObject bleMenuItemPrefab;

    [SerializeField]
    Transform bleScrollViewContent;

    [SerializeField]
    GameObject configPopup;
    [SerializeField]
    TextMeshProUGUI configPopupText;

    [SerializeField]
    InputField TMP_ssid;
    [SerializeField]
    InputField TMP_pass;

    public ExampleNetworkObject exampleNetworkObject;

    //BLE variables
    private BluetoothHelper bleHelper;
    private BluetoothHelperCharacteristic bluetoothHelperCharacteristic; // todo move this to network object? 
    private LinkedList<BluetoothDevice> devices;
    private bool isScanning;
    private bool isConnecting;
    private int intValue;
    private string strBLEaddressPrefix;
    private List<BLEMenuItem> bleMenuItems;
    private List<string> connectedBleAddresses;

    //Wifi variables
    private System.Net.Sockets.UdpClient udpClient;
    private Thread exchangeThread;
    private bool udpExchangeThreadRunning = false;
    private bool udpExchangeStopRequested = false;
    public bool udpConnected = false;
    public string ip = "192.168.88.230";
    public int port = 5006;
    public IPEndPoint IPEndPoint;
    //public string receivedStrData;
    public byte[] receivedBytes;
    protected bool receivedNothing = false;
    private int gotNothingCounter = 0;

    public static readonly int BYTE_FORMAT = 0;
    public static readonly int STRING_FORMAT = 1;

    #endregion

    #region Unity Monobehavior 

    // Start is called before the first frame update
    void Start()
    {
        networkObjects = new List<NetworkObject>();

        exampleNetworkObject = new ExampleNetworkObject(NetworkObject.NetworkObjectType.udp, 0);

        networkObjects.Add(exampleNetworkObject);

        bleMenuItems = new List<BLEMenuItem>();

        ip = "192.168.88.231";
        port = 5006;

        try
        {
            if (udpEnabled)
            {
                //Set up UDP exchange thread
                StartUDP();
            }

            if (bleEnabled)
            {

#if PLATFORM_ANDROID
                if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                {
                    Permission.RequestUserPermission(Permission.FineLocation);
                    Debug.Log("Location permission required for BLE scan. ");
                }
#endif
                //todo check for other platforms/requirments

                //check if ble, udp , tcp are enabled 

                //Set up BLE helper
                //BluetoothHelper.BLE = true;
                //bleHelper = BluetoothHelper.GetInstance();

                ////add to ble helper events
                //bleHelper.OnConnected += OnConnected;
                //bleHelper.OnConnectionFailed += OnConnectionFailed;
                //bleHelper.OnScanEnded += OnScanEnded;
                //bleHelper.OnServiceNotFound += OnServiceNotFound;

                //Do Set up stuff for network objects ?
                //foreach (var netObject in networkObjects)
                //{
                //    switch (netObject.type)
                //    {
                //        case NetworkObject.NetworkObjectType.ble:
                //            {
                //                //Do any start up stuff for each ble network object, if needed.
                //            }
                //            break;
                //        case NetworkObject.NetworkObjectType.udp:
                //            {
                //                //Do any start up stuff for each UDP network object, if needed.
                //                //UDP connect only needs to be called once.
                //            }
                //            break;
                //        case NetworkObject.NetworkObjectType.tcp:
                //            {
                //                //Do any start up stuff for each TCP Network object, if needed.
                //            }
                //            break;
                //        default:
                //            {
                //                Debug.LogError("Network object " + netObject.id + " has invalid type! ");
                //            }
                //            break;
                //    }
                //}
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

    }

    // Update is called once per frame
    void Update()
    {

        try
        {
            if (networkObjects.Count == 0)
            {
                return;
            }
            //loop through all network objects,
            //attempt to reconnect with them if disconnected
            foreach (var netObject in networkObjects)
            {
                switch (netObject.type)
                {
                    case NetworkObject.NetworkObjectType.ble:
                        {
                            if (netObject.connected && bleEnabled)
                            {
                                //get this net object's specific blehelper instance
                                //bleHelper = BluetoothHelper.GetInstanceById(netObject.id);
                                if (bleHelper != null)
                                {
                                    Debug.Log("Reading ble Characteristic");
                                    bleHelper.ReadCharacteristic(bluetoothHelperCharacteristic);
                                }
                            }
                            if (!netObject.connected && bleEnabled)
                            {
                                //instead of netObject.conncted, use blehelper.isConnected(),
                                //set netobject.connect = blehelper.isConnected.

                                // if no longer connected, try to find the device / reconnect,
                                // and/or tell the user that the ble device has lost connection
                            }
                        }
                        break;
                    case NetworkObject.NetworkObjectType.udp:
                        {
                            if (!netObject.connected && udpEnabled)
                            {
                                //reconnect to this UDP network object
                            }
                        }
                        break;
                    case NetworkObject.NetworkObjectType.tcp:
                        {
                        }
                        break;
                    default:
                        break;
                }
            }

            if (bleEnabled)
            {
                //todo check and see if any networkobject wants to change a characteristic

            }

            //call UDP broadcast to combine all outgoing messages from network objects, if any.
            if (udpEnabled)
            {
                UDPSend();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void OnApplicationQuit()
    {
        BLECloseAll();
    }

    void OnDestroy()
    {
        if (bleHelper != null)
            bleHelper.Disconnect();

        StopUDPExchange();

        //do tcp disconnect /cleanup here
    }

    #endregion

    #region BLE 

    /// <summary>
    /// called when the user pushes the connect button on the ble settings panel
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool addConnectedBLE(string deviceName)
    {
        bool added = false;
        try
        {
            LinkedListNode<BluetoothDevice> node = devices.First;
            for (int i = 0; i < devices.Count; i++)
            {

                string bluetoothName = node.Value.DeviceName;
                Debug.Log("BLE device found : " + bluetoothName);

                if (deviceName.Equals(bluetoothName))
                {

                    if (node.Value.DeviceAddress.Contains(strBLEaddressPrefix))
                    {
                        foreach (var obj in networkObjects)
                        {
                            if (node.Value.DeviceAddress.Contains(obj.networkObjectAddressID))
                            {
                                obj.bleHelper = bleHelper;
                                obj.bleDevice = node.Value;
                                bleHelper.Connect();

                                //add and subscribe to characteristics here.
                                //bluetoothHelperCharacteristic = new BluetoothHelperCharacteristic("3c3957d2-c7a6-11eb-b8bc-0242ac130002", "3c3955a2-c7a6-11eb-b8bc-0242ac130003");

                                foreach (var characteristic in exampleNetworkObject.bleCharacteristics)
                                {
                                    bleHelper.Subscribe(characteristic);
                                }

                                connectedBleAddresses.Add(node.Value.DeviceAddress);

                                //update the BLE menu Item to connected, so the disconnect button is displayed
                                foreach (var item in bleMenuItems)
                                {
                                    if (item.bleName.Equals(bluetoothName))
                                    {
                                        item.connected = true;
                                    }
                                }
                                added = true;

                                return added;
                            }
                        }
                    }
                }

                node = node.Next;

                if (node == null)
                {
                    added = false;
                }

            }
        }
        catch (Exception e)
        {
            //isConnecting = false;
            Debug.Log("error in addConnectedBLE(string bleName)  " + e.ToString());
            return false;
        }

        return added;
    }

    public bool removeConnectedBLE(string bleName)
    {
        bool removed = false;

        //todo find the device with that ID and disconnect from it.

        //update the BLE menu Item to disconnected, so the connect button is displayed
        foreach (var item in bleMenuItems)
        {
            if (item.bleName.Equals(bleName))
            {
                item.connected = false;
            }
        }

        return removed;

    }

    void OnScanEnded(BluetoothHelper helper, LinkedList<BluetoothDevice> devices)
    {
        isScanning = false;
        Debug.Log("Found " + devices.Count);

        if (devices.Count == 0)
        {
            bleHelper.ScanNearbyDevices();
            return;
        }

        //For debugging purposes, print out all found devices
        foreach (var d in devices)
        {
            Debug.Log(d.DeviceName);
        }

        try
        {
            Debug.Log("ble scan ended.");
            this.isScanning = false;
            this.devices = devices;

            if (devices == null || devices.First == null)
            {
                //todo : provide feedback to the user that no ble devices were found;
                Debug.Log("No ble devices found!");
                return;
            }
            else
            {
                Debug.Log(devices.Count + " devices found.");
            }

            //clear the list of previously found devices / ble ui items, if any.
            if (bleMenuItems.Count > 0)
            {
                foreach (var item in bleMenuItems)
                {
                    GameObject.Destroy(item);
                    bleMenuItems.Remove(item);
                }
            }

            LinkedListNode<BluetoothDevice> node = devices.First;
            for (int i = 0; i < devices.Count; i++)
            {


                string bluetoothName = node.Value.DeviceName;
                Debug.Log("Found: " + bluetoothName);

                //look through all of this project's ble network objects,
                //if a device contains a name/id we are expecting, then create a BLEMenuItem prefab and
                //add that to the list of devices the user can connect to. 

                if (node.Value.DeviceAddress.ToLower().Contains(strBLEaddressPrefix))
                {
                    GameObject newMenuItemObj = GameObject.Instantiate(bleMenuItemPrefab, bleScrollViewContent);
                    BLEMenuItem newBleMenuItem = newMenuItemObj.GetComponent<BLEMenuItem>();
                    newBleMenuItem.deviceName.text = bluetoothName;
                    newBleMenuItem.deviceID.text = node.Value.DeviceAddress;

                    if (connectedBleAddresses.Count > 0)
                    {
                        if (connectedBleAddresses.Contains(node.Value.DeviceAddress))
                        {
                            newBleMenuItem.connected = true;
                        }
                    }

                    bleMenuItems.Add(newBleMenuItem);
                }

                node = node.Next;
                if (node == null)
                    return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    void OnConnected(BluetoothHelper helper)
    {
        Debug.Log("Connected to " + helper.getDeviceName());
    }

    void OnConnectionFailed(BluetoothHelper helper)
    {
        Debug.Log("Connection to " + helper.getDeviceName() + " was lost.");
    }

    void OnServiceNotFound(BluetoothHelper helper, string service)
    {
        Debug.Log($"Service [{service}] not found");
    }

    /// <summary>
    /// todo, make this method scan for all available Simetri BLE devices, and add them to the BLE 
    /// UI
    /// </summary>
    public void BLEScan()
    {
        try
        {
            if (!bleEnabled)
            {
                return;
            }
            if (!isScanning)
            {
                Debug.Log("starting ble scan");
                BluetoothHelper.BLE = true;
                bleHelper = BluetoothHelper.GetInstance();

                isScanning = bleHelper.ScanNearbyDevices();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void BLECloseAll()
    {
        if (bleHelper != null)
            bleHelper.Disconnect();
    }

    private void BLEWriteCharacteristic(BluetoothHelperCharacteristic characteristic, Message msg)
    {
        string fullMessageString = "";

        foreach (var netObj in networkObjects)
        {
            if (netObj.type == NetworkObject.NetworkObjectType.ble && netObj.readyToSend)
            {
                //TODO : Get the correct instance of the ble helper, and call blehelper.WriteCharacteristic(characteristic, string msg);

            }
        }

    }

    #endregion

    #region UDP Wifi

    public void StartUDP()
    {
        if (exchangeThread != null)
        {
            StopUDPExchange();
        }

        // binds to local port
        udpClient = new System.Net.Sockets.UdpClient(port);
        udpClient.EnableBroadcast = true;
        IPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

        // connect to remote host ip and port
        // udpClient.Connect(ip, port);

        RestartUDPExchange();
    }

    /// <summary>
    /// The main communication task/thread to exchange network data.
    /// </summary>
    public void UDPExchangeThread()
    {
        try
        {
            while (!udpExchangeStopRequested)
            {
                udpExchangeThreadRunning = true;
                //connected = true;

                UDPReceive();
            }
        }
        catch (Exception e)
        {
            //  Debug.Log("ExchangeThread Encountered Error on Port: " + port + " IP: " + ip + " Error: " + e);
        }
    }

    public void RestartUDPExchange()
    {
        if (exchangeThread != null)
        {
            StopUDPExchange();
        }
        udpExchangeStopRequested = false;
        exchangeThread = new System.Threading.Thread(UDPExchangeThread);
        Debug.Log("Starting Exchange thread");
        exchangeThread.Start();
    }

    public void StopUDPExchange()
    {
        Debug.Log("Stopping Exchange thread");
        udpExchangeStopRequested = true;

        if (exchangeThread != null)
        {
            exchangeThread.Abort();
            exchangeThread = null;

            udpClient.Close();
        }
    }

    /// <summary>
    /// TODO: 
    /// Make this method broadcast a message that is a combination of all network object's outgoing messages, separated by ID.
    /// The arduino side should then parse through that message.
    /// Will need to encrypt this data before sending.
    /// </summary>
    /// <param name="data"></param>
    protected void UDPSend()//object data)
    {
        //todo
        //loop through all network objects and combine all outgoing messages into one outgoing message.

        string fullMessageString = "";

        foreach (var netObj in networkObjects)
        {
            if (netObj.type == NetworkObject.NetworkObjectType.udp && netObj.readyToSend)
            {
                Message msg = netObj.GetOutgoingMessage();

                //todo check if message has an ID already? If so, don't add one?
                if (msg.str_msg != null)
                {
                    fullMessageString += "ID" + netObj.id + msg.str_msg;
                    netObj.readyToSend = false;
                }
            }
        }

        byte[] buffer = Encoding.ASCII.GetBytes(fullMessageString);
        if (buffer.Length > 0)
        {
            udpClient.Send(buffer, buffer.Length, IPEndPoint);
        }


        //Send the data
        //if (data is byte[])
        //{
        //    byte[] buffer = (byte[])data;
        //    udpClient.Send(buffer, buffer.Length, IPEndPoint);
        //}
        //else if (data is string)
        //{
        //    byte[] buffer = Encoding.ASCII.GetBytes((string)data);
        //    udpClient.Send(buffer, buffer.Length, IPEndPoint);
        //}
    }


    /// <summary>
    /// Todo: make this function parse incoming data and send it to the
    /// appropriate network object based on the ID, check to make sure all incoming messages have an ID.
    /// set network object's connection status.
    /// once encryption is added, this method will also have to decrypt the incoming message.
    /// </summary>
    private void UDPReceive()
    {
        udpClient.EnableBroadcast = true;
        IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

        if (udpClient.Available > 0)
        {
            // Blocks until a message returns on this socket from a remote host.
            receivedBytes = udpClient.Receive(ref remoteIpEndPoint);
            string tmp = Encoding.ASCII.GetString(receivedBytes);

            if (!string.IsNullOrEmpty(tmp))
            {
                // receivedStrData = tmp;
                string[] words = tmp.Split(';');

                //todo populate list of messages from parsed string tmp
                Message incomingMsg = new Message()
                {
                    id = int.Parse(words[0]),
                    timestamp = DateTime.Now,
                    str_msg = words[1],
                    fromUnity = false,
                    byte_msg = receivedBytes
                };

                List<Message> messages = new List<Message>();

                foreach (var netObj in networkObjects)
                {
                    if (incomingMsg.id == netObj.id)
                    {
                        netObj.UDPProcessStrMessage(incomingMsg);
                    }
                }
            }
        }
        else
        {
            //todo Handle the connection status of each UDP device

        }
    }

    public void displayConfigPopup(string msg)
    {
        configPopupText.text = msg;
    }

    /// <summary>
    /// This is called by the ENTER button on the Settings page, after 
    /// </summary>
    public void SetNetConfig()
    {
        if (!string.IsNullOrEmpty(TMP_ssid.text))
        {
            Message resetMsg = new Message()
            {
                timestamp = DateTime.Now,
                fromUnity = true,
                str_msg = "RESET:" + TMP_ssid.text + ";" + TMP_pass.text + ";"
            };

            //TODO should we ask the user if they are sure they want to change this setting.
            foreach (var netObject in networkObjects)
            {
                resetMsg.id = netObject.id;
                netObject.SetOutgoingMessage(resetMsg);
            }
        }
        else
        {
            //display pop up asking the user for valid data
        }
    }

    //Wifi helper functions 
    protected static Vector3 ReadPosition(byte[] buffer, int startIndex)
    {
        return new Vector3(ReadFloat(buffer, startIndex), ReadFloat(buffer, startIndex + 4), ReadFloat(buffer, startIndex + 8));
    }

    protected static Quaternion ReadRotation(byte[] buffer, int startIndex)
    {
        return new Quaternion(ReadFloat(buffer, startIndex), ReadFloat(buffer, startIndex + 4), ReadFloat(buffer, startIndex + 8), ReadFloat(buffer, startIndex + 12));
    }

    protected unsafe static float ReadFloat(byte[] buffer, int startIndex)
    {
        uint temp = (uint)(buffer[startIndex] | buffer[startIndex + 1] << 8 | buffer[startIndex + 2] << 16 | buffer[startIndex + 3] << 24);
        return *((float*)&temp);
    }

    protected unsafe static double ReadDouble(byte[] buffer, int startIndex)
    {
        uint lo = (uint)(buffer[startIndex + 0] | buffer[startIndex + 1] << 8 | buffer[startIndex + 2] << 16 | buffer[startIndex + 3] << 24);
        uint hi = (uint)(buffer[startIndex + 4] | buffer[startIndex + 5] << 8 | buffer[startIndex + 6] << 16 | buffer[startIndex + 7] << 24);

        ulong tmpBuffer = ((ulong)hi) << 32 | lo;
        return *((double*)&tmpBuffer);
    }

    protected static uint ReadUInt(byte[] buffer, int startIndex)
    {
        return (uint)(buffer[startIndex + 0] | buffer[startIndex + 1] << 8 | buffer[startIndex + 2] << 16 | buffer[startIndex + 3] << 24);
    }

    #endregion

    #region TCP Wifi


    #endregion


}
