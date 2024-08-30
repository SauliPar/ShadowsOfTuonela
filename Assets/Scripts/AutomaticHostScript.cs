using System;
using Unity.Netcode;
using UnityEngine;

public class AutomaticHostScript : MonoBehaviour
{
    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            StartHost();
        }
        else
        {
            Debug.Log("Networkmanager wasn't ready, trying again in 2 seconds");
            InvokeRepeating(nameof(StartHost), 2f, 2f);
        }
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        CancelInvoke();
    }
}
