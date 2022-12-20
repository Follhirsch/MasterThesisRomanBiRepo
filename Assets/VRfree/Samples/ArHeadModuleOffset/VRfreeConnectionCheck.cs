using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VRfreeConnectionCheck : MonoBehaviour
{
    [Header("Configuration")]
    public List<VRfree.DeviceType> requiredDevices = new List<VRfree.DeviceType>();

    [Header("Events")]
    public UnityEvent onDevicesConnected = new UnityEvent();
    public UnityEvent onDevicesDisconnected = new UnityEvent();

    [Header("Output")]
    public List<VRfree.DeviceType> connectedDevices = new List<VRfree.DeviceType>();
    public bool requiredDevicesConnected = false;

    // Start is called before the first frame update
    void Start()
    {
        onDevicesDisconnected.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        connectedDevices = VRfree.VRfreeAPI.GetConnectedDevices();

        bool requiredDevicesConnectedNew = true;
        foreach (VRfree.DeviceType d in requiredDevices) {
            requiredDevicesConnectedNew = connectedDevices.Contains(d);
            if (!requiredDevicesConnectedNew) {
                if (requiredDevicesConnected)
                    onDevicesDisconnected.Invoke();
                break;
            }
        }

        if (requiredDevicesConnectedNew && !requiredDevicesConnected)
            onDevicesConnected.Invoke();
        requiredDevicesConnected = requiredDevicesConnectedNew;

    }
}
