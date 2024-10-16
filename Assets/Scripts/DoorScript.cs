using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class DoorScript : NetworkBehaviour
{
    public NetworkVariable<bool> State = new NetworkVariable<bool>();

    public GameObject DoorOpenedTransform;
    public GameObject DoorClosedTransform;
    public Collider DoorCollider;
    public NavMeshObstacle NavMeshObstacle;
    public bool LastGate;

    public override void OnNetworkSpawn()
    {
        State.OnValueChanged += OnStateChanged;

        if (IsServer)
        {
            State.Value = false;
        }
        else
        {
            if (State.Value)
            {
                // door is open
                OpenDoor();
            }
            else
            {
                CloseDoor();
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        State.OnValueChanged -= OnStateChanged;
    }

    public void OnStateChanged(bool previous, bool current)
    {
        // note: `State.Value` will be equal to `current` here
        if (State.Value)
        {
            // door is open:
           OpenDoor();
        }
        else
        {
            // door is closed:
            CloseDoor();
        }
    }

    public void OpenDoor()
    {
        DoorOpenedTransform.SetActive(true);
        DoorClosedTransform.SetActive(false);
        DoorCollider.enabled = false;
        NavMeshObstacle.enabled = false;
    }

    public void CloseDoor()
    {
        DoorOpenedTransform.SetActive(false);
        DoorClosedTransform.SetActive(true);
        DoorCollider.enabled = true;
        NavMeshObstacle.enabled = true;
    }

    [Rpc(SendTo.Server)]
    public void ToggleServerRpc(int killCount)
    {
        // this will cause a replication over the network
        // and ultimately invoke `OnValueChanged` on receivers

        if (LastGate)
        {
            if (killCount >= GlobalSettings.EasterEggKillCount)
            {
                State.Value = !State.Value;
                Invoke(nameof(CloseDoor), 5f);
            }
        }
        else
        {
            State.Value = !State.Value;
            Invoke(nameof(CloseDoor), 5f);
        }
    }
}
