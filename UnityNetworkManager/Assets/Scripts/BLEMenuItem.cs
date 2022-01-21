using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BLEMenuItem : MonoBehaviour
{
    public int id;
    public string bleName;
    public bool connected = false;
    NetworkManager netMan;

    [SerializeField]
    public TMP_InputField deviceName;

    [SerializeField]
    public TMP_Text deviceID;

    [SerializeField]
    public GameObject connectButton;
    [SerializeField]
    public GameObject disconnectButton;

    // Start is called before the first frame update
    void Start()
    {
        netMan = GameObject.FindObjectOfType<NetworkManager>();

        if (netMan == null)
        {
            Debug.Log("NO NETWORK MANAGER FOUND!!!");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (connected)
        {
           connectButton.SetActive(true);
           disconnectButton.SetActive(false);
        }
        else
        {
            connectButton.SetActive(false);
            disconnectButton.SetActive(true);
        }
    }

    public void Connect()
    {
        //tell the network manager to connect to this device
       connected = netMan.addConnectedBLE(bleName);
    }

    public void Disconnect()
    {
        //todo tell network manager to disconnect from this device;
        connected = netMan.removeConnectedBLE(bleName);

    }
}
