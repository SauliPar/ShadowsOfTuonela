using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;

public class MatchMakingManager : MonoBehaviour
{
    private IMatchmakerService _matchmakerService;
    private PayloadAllocation _payloadAllocation;
    private string _currentTicket;
    private string _backfillTicketId;

    private bool _isDeallocating = false;
    private bool deallocatingCancellationToken = false;
    
    private async void Start()
    {
        if (Application.platform != RuntimePlatform.LinuxEditor)
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            
            // await Task.Delay(5000);
            //
            // await ClientJoin();
        }
        else
        {
            while (UnityServices.State == ServicesInitializationState.Uninitialized ||
                   UnityServices.State == ServicesInitializationState.Initializing)
            {
                await Task.Yield();
            }
            
            _matchmakerService = MatchmakerService.Instance;
            _payloadAllocation = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<PayloadAllocation>();
            _backfillTicketId = _payloadAllocation.BackfillTicketId;
        }
    }

    private async void Update()
    {
        if (Application.platform == RuntimePlatform.LinuxServer)
        {
            if (NetworkManager.Singleton.ConnectedClientsList.Count != 0 || !_isDeallocating)
            {
                _isDeallocating = true;
                deallocatingCancellationToken = false;
                Deallocate();
            }

            if (NetworkManager.Singleton.ConnectedClientsList.Count != 0)
            {
                _isDeallocating = false;
                deallocatingCancellationToken = true;
            }

            if (_backfillTicketId != null && NetworkManager.Singleton.ConnectedClientsList.Count < 10)
            {
                BackfillTicket newBackfillTicket =
                    await MatchmakerService.Instance.ApproveBackfillTicketAsync(_backfillTicketId);
                _backfillTicketId = newBackfillTicket.Id;
            }

            await Task.Delay(1000);
        }
    }

    private async void Deallocate()
    {
        await Task.Delay(60 * 1000);

        if (!deallocatingCancellationToken)
        {
            Application.Quit();
        }
    }

    private void OnPlayerConnected()
    {
        if (Application.platform == RuntimePlatform.LinuxServer)
        {
            UpdateBackfillTicket();
        }
    }

    private void OnPlayerDisconnected()
    {
        if (Application.platform == RuntimePlatform.LinuxServer)
        {
            UpdateBackfillTicket();
        }
    }

    private async void UpdateBackfillTicket()
    {
        List<Player> players = new List<Player>();

        foreach (var playerId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            players.Add(new Player(playerId.ToString()));
        }

        MatchProperties matchProperties = new MatchProperties(null, players, _backfillTicketId);

        await MatchmakerService.Instance.UpdateBackfillTicketAsync(_payloadAllocation.BackfillTicketId,
            new BackfillTicket(_backfillTicketId, properties: new BackfillTicketProperties(matchProperties)));
    }

    public async void ClientJoin()
    {
        var ticket = await CreateTicket();

        _currentTicket = ticket.Id;
        Debug.Log("matchmaker ticket created");

        PollTicket();
    }

    private async void PollTicket()
    {
        MultiplayAssignment assignment = null;
        bool gotAssignment = false;
        do
        {
            //Rate limit delay
            await Task.Delay(TimeSpan.FromSeconds(1f));

            // Poll ticket
            var ticketStatus = await MatchmakerService.Instance.GetTicketAsync(_currentTicket);
            if (ticketStatus == null)
            {
                continue;
            }

            //Convert to platform assignment data (IOneOf conversion)
            if (ticketStatus.Type == typeof(MultiplayAssignment))
            {
                assignment = ticketStatus.Value as MultiplayAssignment;
            }

            switch (assignment?.Status)
            {
                case MultiplayAssignment.StatusOptions.Found:
                    gotAssignment = true;
                    StartClient(assignment);
                    Debug.Log("Match found");
                    break;
                case MultiplayAssignment.StatusOptions.InProgress:
                    //...
                    Debug.Log("Match in progress");
                    break;
                case MultiplayAssignment.StatusOptions.Failed:
                    gotAssignment = true;
                    Debug.LogError("Failed to get ticket status. Error: " + assignment.Message);
                    UpdateBackfillTicket();
                    break;
                case MultiplayAssignment.StatusOptions.Timeout:
                    gotAssignment = true;
                    Debug.LogError("Failed to get ticket status. Ticket timed out.");
                    break;
                default:
                    throw new InvalidOperationException();
            }

        } while (!gotAssignment);    
    }

    // private async void CreateABackfillTicket()
    // { 
    //     Debug.LogError("Trying to join with a backfill");
    //     
    //     // Set options for matchmaking
    //     var options = new CreateBackfillTicketOptions("MyQueue", "127.0.0.1:8080", new Dictionary<string, object>(), backfillTicketProperties);
    //     
    //     // Create backfill ticket
    //             string ticketId = await MatchmakerService.Instance.CreateBackfillTicketAsync
    //                 (options);
    //
    //     // Print the created ticket id
    //             Debug.Log(ticketId);
    // }

    private async Task<CreateTicketResponse> CreateTicket()
    {
        // List<Player> players = new List<Player> { new Player(AuthenticationService.Instance.PlayerId) };

        var players = new List<Player>
        {
            new Player("Player1", new Dictionary<string, object>())
        };    
        
        // Set options for matchmaking
        // var options = new CreateTicketOptions(
        //     "Default", // The name of the queue defined in the previous step,
        //     new Dictionary<string, object>());
        CreateTicketOptions createTicketOptions = new CreateTicketOptions("TestQueue");
        
        CreateTicketResponse createTicketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, createTicketOptions);

        return createTicketResponse;
    }

    private void StartClient(MultiplayAssignment assignment)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(assignment.Ip, ushort.Parse(assignment.Port.ToString()));

        NetworkManager.Singleton.StartClient();
    }

    [System.Serializable]
    public class PayloadAllocation
    {
        public MatchProperties MatchProperties;
        public string GeneratorName;
        public string QueueName;
        public string PoolName;
        public string EnvironmentId;
        public string BackfillTicketId;
        public string MatchId;
        public string PoolId;
    }
}
