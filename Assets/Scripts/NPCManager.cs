using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NPCManager : NetworkBehaviour
{
    [SerializeField] private List<Transform> botSpawnPositions = new List<Transform>();
    [SerializeField] private GameObject botPrefab;

    public int NumberOfBotsInScene = 0;
    
    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();

    protected override void OnNetworkPostSpawn()
    {
        if (!IsServer) return;

        Debug.Log("oltiin servu");
        for (int i = 0; i < NumberOfBotsInScene; i++)
        {
            SpawnNpc();
        }
    }
    
    private void SpawnNpc()
    {
        Vector3 position;

        if (botSpawnPositions.Count == occupiedPositions.Count)
        {
            Debug.LogWarning("No more unique positions to spawn bots.");
            return;
        }

        do 
        {
            var randomSpawnIndex = Random.Range(0, botSpawnPositions.Count);
            position = botSpawnPositions[randomSpawnIndex].position;
        } 
        while (occupiedPositions.Contains(position));

        Debug.Log("spawnataan");

        occupiedPositions.Add(position);

        var instance = Instantiate(botPrefab, position, Quaternion.identity);
        var networkObject = instance.GetComponent<NetworkObject>();
        networkObject.Spawn();
        
        Debug.Log("on instantioitu");


        // assume there's a Bot component with a death event we can subscribe to
        var baseController = instance.GetComponent<BaseController>();
        baseController.OnNpcDeath += () => DespawnNpc(position);
    }

    private void DespawnNpc(Vector3 position)
    {
        occupiedPositions.Remove(position);
        
        Invoke(nameof(SpawnNpc), 10f);
    }

    // private void SpawnNpc()
    // {
    //     var randomSpawnIndex = Random.Range(0, botSpawnPositions.Count);
    //     var instance = Instantiate(botPrefab, botSpawnPositions[randomSpawnIndex], Quaternion.identity);
    //     var networkObject = instance.GetComponent<NetworkObject>();
    //     networkObject.Spawn();
    // }
}
