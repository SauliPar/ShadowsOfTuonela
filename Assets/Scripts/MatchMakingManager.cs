using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class MatchMakingManager : MonoBehaviour
{
    private string _currentTicket;
    
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
    }
    
    public async void ClientJoin()
    {
        var players = new List<Player>
        {
            new Player("Player1", new Dictionary<string, object>())
        };


        // Set options for matchmaking
        var options = new CreateTicketOptions(
            "Default", // The name of the queue defined in the previous step,
            new Dictionary<string, object>());
        CreateTicketOptions createTicketOptions = new CreateTicketOptions("TestQueue");

        // List<Player> players = new List<Player> { new Player(AuthenticationService.Instance.PlayerId) };
        
        CreateTicketResponse createTicketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, createTicketOptions);

        _currentTicket = createTicketResponse.Id;
        Debug.Log("matchmaker ticket created");
        
        MultiplayAssignment assignment = null;
        bool gotAssignment = false;
        do
        {
            //Rate limit delay
            await Task.Delay(TimeSpan.FromSeconds(1f));

            // Poll ticket
            var ticketStatus = await MatchmakerService.Instance.GetTicketAsync(createTicketResponse.Id);
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
                    // gotAssignment = true;
                    Debug.LogError("Failed to get ticket status. Error: " + assignment.Message);
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

    private void StartClient(MultiplayAssignment assignment)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(assignment.Ip, ushort.Parse(assignment.Port.ToString()));

        NetworkManager.Singleton.StartClient();
    }
}
