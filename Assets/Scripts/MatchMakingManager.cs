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
    
    private string _serverIp;
    private ushort _serverPort;

    private async void Start()
    {
        if (Application.platform == RuntimePlatform.LinuxServer)
        {
            while (UnityServices.State == ServicesInitializationState.Uninitialized ||
                   UnityServices.State == ServicesInitializationState.Initializing)
            {
                await Task.Yield();
            }
            
            var config = MultiplayService.Instance.ServerConfig;
            _serverIp = config.IpAddress;
            _serverPort = config.Port;
            
            _matchmakerService = MatchmakerService.Instance;
            _payloadAllocation = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<PayloadAllocation>();
            _backfillTicketId = _payloadAllocation.BackfillTicketId;
            Debug.Log("backfillticketid: " + _backfillTicketId);
        }
        else
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            
            // await Task.Delay(5000);
            //
            // await ClientJoin();
        }
    }

    private async void Update()
    {
        if (Application.platform == RuntimePlatform.LinuxServer)
        {
            if (NetworkManager.Singleton.ConnectedClientsList.Count == 0 && !_isDeallocating)
            {
                _isDeallocating = true;
                deallocatingCancellationToken = false;
                Deallocate();
            }

            if (NetworkManager.Singleton.ConnectedClientsList.Count > 0)
            {
                _isDeallocating = false;
                deallocatingCancellationToken = true;
            }

            if (string.IsNullOrEmpty(_backfillTicketId))
            {
                _backfillTicketId = await CreateABackfillTicket();
            }

            // if (!string.IsNullOrEmpty(_backfillTicketId) && NetworkManager.Singleton.ConnectedClientsList.Count < 10)
            // {
            //     BackfillTicket newBackfillTicket =
            //         await MatchmakerService.Instance.ApproveBackfillTicketAsync(_backfillTicketId);
            //     _backfillTicketId = newBackfillTicket.Id;
            // }

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
            Debug.Log("ONPLAYERCONNECTED CALLED");
            UpdateBackfillTicket();
        }
    }

    private void OnPlayerDisconnected()
    {
        if (Application.platform == RuntimePlatform.LinuxServer)
        {
            Debug.Log("ONPLAYERDISCONNECTED CALLED");
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

        if (!string.IsNullOrEmpty(_backfillTicketId))
        {
            await MatchmakerService.Instance.UpdateBackfillTicketAsync(_payloadAllocation.BackfillTicketId,
                new BackfillTicket(_backfillTicketId, properties: new BackfillTicketProperties(matchProperties)));
        }
        else
        {
            Debug.Log("Backfillticketid is empty or null!");
        }
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
                    Debug.Log("Match in progress");
                    break;
                case MultiplayAssignment.StatusOptions.Failed:
                    gotAssignment = true;
                    Debug.LogError("Failed to get ticket status. Error: " + assignment.Message);
                    StartClient(IpAddresses.IpAddress, IpAddresses.Port);
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


    // private void InitializeBackfillProcedure9000(string assignmentIp, int? assignmentPort)
    // {
    //     CreateABackfillTicket(assignmentIp, assignmentPort);
    // }

    private async Task<string> CreateABackfillTicket()
    {
        // var teams = new List<Team>{
        //     new Team( "Red", "9c8e302e-9cf3-4ad6-a005-b2604e6851e3", new List<string>{ "c9e6857b-a810-488f-bacc-08d18d253b0a"  } ),
        //     new Team( "Blue", "e2d8f4fd-5db8-4153-bca7-72dfc9b2ac09", new List<string>{ "fe1a52cd-535a-4e34-bd24-d6db489eaa19"  } ),
        // };

        // Define the Players of the match with their data.
                var players = new List<Player>
                {
                    new Player(
                        "Player1")
                        // new Dictionary<string, object>
                        // {
                        //     { "Team", "Red" }
                        // }),
                    // new Player(
                    //     "fe1a52cd-535a-4e34-bd24-d6db489eaa19",
                    //     new Dictionary<string, object>
                    //     {
                    //         { "Team", "Blue" }
                    //     })
                };

                var matchProperties = new MatchProperties(null, players);


                var backfillTicketProperties = new BackfillTicketProperties(matchProperties);

                var connection = $"{_serverIp}:{_serverPort}";
        // Set options for matchmaking
                var options = new CreateBackfillTicketOptions("TestQueue", connection, new Dictionary<string, object>(), backfillTicketProperties);


        // Create backfill ticket
                string ticketId = await MatchmakerService.Instance.CreateBackfillTicketAsync
                    (options);

        // Print the created ticket id
                Debug.Log(ticketId);

                // approve that shit
                var backfillTicket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(ticketId);

                return backfillTicket.Id;
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

        // Debug.LogError("server ip: " + assignment.Ip + ", and port: " + assignment.Port);

        NetworkManager.Singleton.StartClient();
    }
    private void StartClient(string ip, string port)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, ushort.Parse(port));

        // Debug.LogError("server ip: " + assignment.Ip + ", and port: " + assignment.Port);

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
