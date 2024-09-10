using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerState : NetworkBehaviour
{
    private const int DefaultHealth = 100;
    /// <summary>
    /// Default Permissions: Everyone can read, server can only write
    /// Player health is typically something determined (updated/written to) on the server
    ///  side, but a value everyone should be synchronized with (that is, read permissions).
    /// </summary>
    public NetworkVariable<int> Health = new NetworkVariable<int>(DefaultHealth);

    public HealthBarScript HealthBarScript;
    public DamageTakenScript DamageTakenScript;
    
    private void Start()
    {
        if (IsServer)
        {
            Debug.Log("Tultiin PlayerStaten server-osioon");
            // StartCoroutine(StartChangingNetworkVariable());
        }
        else
        {
            Debug.Log("Tultiin PlayerStaten client-osioon");
        }
        
        Health.OnValueChanged += OnHealthValueChanged;
    }

    private void OnHealthValueChanged(int previousvalue, int newvalue)
    {
        Debug.Log("onhealthvaluechanged");
        HealthBarScript.SetHealthBarValue(newvalue);
        DamageTakenScript.ShowDamage(previousvalue - newvalue);
    }

    private IEnumerator StartChangingNetworkVariable()
    {
        // Debug.Log("Tultiin co-routineen");

        var count = 0;
        var updateFrequency = new WaitForSeconds(2f);
        while (count < 4)
        {
            Health.Value -= Random.Range(0, 10);
            yield return updateFrequency;
        }
    }
}
