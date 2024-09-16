using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerState : NetworkBehaviour
{
    /// <summary>
    /// Default Permissions: Everyone can read, server can only write
    /// Player health is typically something determined (updated/written to) on the server
    ///  side, but a value everyone should be synchronized with (that is, read permissions).
    /// </summary>
    ///
    /// 
    public NetworkVariable<int> Health = new NetworkVariable<int>(GlobalSettings.DefaultHealth);

    public HealthBarScript HealthBarScript;
    public DamageTakenScript DamageTakenScript;
    public BaseController BaseController;
    
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
        
        // Health.OnValueChanged += OnHealthValueChanged;
    }

    private void OnHealthValueChanged(int previousvalue, int newvalue)
    {
        // Debug.Log("onhealthvaluechanged");
        HealthBarScript.SetHealthBarValue(newvalue);
        
        // Debug.Log("damagenumber 1: " + (previousvalue - newvalue));

        DamageTakenScript.ShowDamage(previousvalue - newvalue);
    }

    public bool DecreaseHealthPoints(int damageValue)
    {
        Health.Value -= damageValue;
        HealthBarScript.SubtractHealth(damageValue);
        DamageTakenScript.ShowDamage(damageValue);

        if (Health.Value <= 0)
        {
            Debug.Log("healthvalue meni nolliin");
            return true;
        }

        return false;
    }

    public void ResetHealth()
    {
        Health.Value = GlobalSettings.DefaultHealth;
        HealthBarScript.SetHealthBarValue(Health.Value);
    }
}
