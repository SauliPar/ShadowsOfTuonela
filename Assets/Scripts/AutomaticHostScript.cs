using System;
using Unity.Netcode;
using UnityEngine;

public class AutomaticHostScript : MonoBehaviour
{
    private void Start()
    {
        // InvokeRepeating(nameof(StartHost), 2f, 2f);
    }

    public void StartHost()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartHost();
        }
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}
