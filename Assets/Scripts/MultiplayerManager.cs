using TMPro;
using Unity.Services.Core;
using Unity.Services.Multiplay;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class MultiplayerManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipAddressInputField;
    [SerializeField] private TMP_InputField portInputField;
    
    private IServerQueryHandler _serverQueryHandler;
    private float _timer;
    private float _timeInterval = 0.1f;

    private async void Start()
    {
        Debug.Log("Application.platform is: " + Application.platform);

        if (Application.platform != RuntimePlatform.LinuxServer) return;
        
        Application.targetFrameRate = 60;

        await UnityServices.InitializeAsync();

        ServerConfig serverConfig = MultiplayService.Instance.ServerConfig;

        _serverQueryHandler =
            await MultiplayService.Instance.StartServerQueryHandlerAsync(10, "ShadowsOfTuonelaServer", "FREEFORALL",
                "0", "TestMap");

        Debug.Log("ServerConfig.AllocationId is: " + serverConfig.AllocationId);
        
        if (serverConfig.AllocationId != string.Empty)
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("0.0.0.0", serverConfig.Port, "0.0.0.0");

            NetworkManager.Singleton.StartServer();

            await MultiplayService.Instance.ReadyServerForPlayersAsync();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (Application.platform != RuntimePlatform.LinuxServer) return;

        UpdateQuery();
    }

    private void UpdateQuery()
    {
        _timer += Time.deltaTime;

        if (_timer > _timeInterval)
        {
            if (_serverQueryHandler != null)
            {
                _serverQueryHandler.CurrentPlayers = (ushort)NetworkManager.Singleton.ConnectedClientsIds.Count;
                
                Debug.Log("CurrentPlayers are: " + _serverQueryHandler.CurrentPlayers);

                _serverQueryHandler.UpdateServerCheck();
            }
            
            _timer = 0f;
        }    
    }

    public void JoinServer()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        
        transport.SetConnectionData(ipAddressInputField.text, ushort.Parse(portInputField.text));

        NetworkManager.Singleton.StartClient();
    }
}
