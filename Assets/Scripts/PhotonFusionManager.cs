using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class PhotonFusionManager : Singleton<PhotonFusionManager>, INetworkRunnerCallbacks
{
    private INetworkRunnerCallbacks _networkRunnerCallbacksImplementation;

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerDefaultSpawnTransform;

    [SerializeField] private Button spawnPlayerButton;
    
    private NetworkRunner _networkRunner;

    private void Start()
    {
        spawnPlayerButton.onClick.AddListener(StartGame);
    }

    private async void StartGame()
    {
        // _networkRunner = gameObject.AddComponent<NetworkRunner>();
        //
        // StartGameResult result = await _networkRunner.StartGame(new StartGameArgs()
        // {
        //     GameMode = GameMode.Shared,
        //     SessionName = "Testisessio",
        // });
        //
        // Debug.Log("startgame resultti oli: " + result.Ok);
        //
        // try
        // {
        //     OnPlayerJoined(_networkRunner, _networkRunner.LocalPlayer);
        // }
        // catch (Exception e)
        // {
        //     Console.WriteLine(e);
        //     throw;
        // }
        //
        // Debug.Log("spawnattiin pelaaja :D trust bro :D");
    }
    
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        _networkRunnerCallbacksImplementation.OnObjectExitAOI(runner, obj, player);
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        _networkRunnerCallbacksImplementation.OnObjectEnterAOI(runner, obj, player);
    }

    // public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    // {
    //     _networkRunnerCallbacksImplementation.OnPlayerJoined(runner, player);
    // }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
    {
        bool isLocalPlayer = playerRef.PlayerId == runner.LocalPlayer.PlayerId;
        
        if (isLocalPlayer)
        {
            Debug.Log("oli muuten local player :D");
            SpawnPlayer(playerRef, true, runner);
        }
        // _networkRunnerCallbacksImplementation.OnPlayerJoined(runner, player);
    }

    private async void SpawnPlayer(PlayerRef playerRef, bool isLocalPlayer, NetworkRunner networkRunner)
    {
        if (isLocalPlayer)
        {
            try
            {
                var networkPlayer = _networkRunner.Spawn(playerPrefab, playerDefaultSpawnTransform.position,
                    Quaternion.identity);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        _networkRunnerCallbacksImplementation.OnPlayerLeft(runner, player);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        _networkRunnerCallbacksImplementation.OnInput(runner, input);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        _networkRunnerCallbacksImplementation.OnInputMissing(runner, player, input);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        _networkRunnerCallbacksImplementation.OnShutdown(runner, shutdownReason);
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("onconnectedtoserver");
        _networkRunnerCallbacksImplementation.OnConnectedToServer(runner);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        _networkRunnerCallbacksImplementation.OnDisconnectedFromServer(runner, reason);
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        _networkRunnerCallbacksImplementation.OnConnectRequest(runner, request, token);
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        _networkRunnerCallbacksImplementation.OnConnectFailed(runner, remoteAddress, reason);
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        _networkRunnerCallbacksImplementation.OnUserSimulationMessage(runner, message);
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        _networkRunnerCallbacksImplementation.OnSessionListUpdated(runner, sessionList);
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        _networkRunnerCallbacksImplementation.OnCustomAuthenticationResponse(runner, data);
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        _networkRunnerCallbacksImplementation.OnHostMigration(runner, hostMigrationToken);
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        _networkRunnerCallbacksImplementation.OnReliableDataReceived(runner, player, key, data);
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        _networkRunnerCallbacksImplementation.OnReliableDataProgress(runner, player, key, progress);
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        _networkRunnerCallbacksImplementation.OnSceneLoadDone(runner);
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        _networkRunnerCallbacksImplementation.OnSceneLoadStart(runner);
    }
}