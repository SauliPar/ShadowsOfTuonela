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
    // public NetworkVariable<int> Damage = new NetworkVariable<int>(0);
    // public NetworkVariable<bool> IsDead = new NetworkVariable<bool>(false);
    
    public NetworkVariable<ControllerState> CharacterState = new NetworkVariable<ControllerState>(ControllerState.Default);

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
        
        // IsDead.OnValueChanged += OnDeath;
        CharacterState.OnValueChanged += OnCharacterStateChanged;
        Health.OnValueChanged += OnHealthValueChanged;
    }

    private void OnCharacterStateChanged(ControllerState previousvalue, ControllerState newvalue)
    {
        // BaseController.CharacterState = newvalue;
    }

    private void OnHealthValueChanged(int previousvalue, int newvalue)
    {
        // if (IsOwner)
        // {
            var substractValue = previousvalue - newvalue;
            HealthBarScript.SetHealthBarValue(newvalue);
            
            if (substractValue < 0) return;
           
            DamageTakenScript.ShowDamage(substractValue);
        // }
       
        
        // // Debug.Log("onhealthvaluechanged");
        // HealthBarScript.SetHealthBarValue(newvalue);
        //
        // // Debug.Log("damagenumber 1: " + (previousvalue - newvalue));
        //
        // DamageTakenScript.ShowDamage(previousvalue - newvalue);
    }

    public bool DecreaseHealthPoints(int damageValue)
    {
        Health.Value -= damageValue;
        // Damage.Value = damageValue;

        if (Health.Value <= 0)
        {
            return true;
        }

        return false;
    }

    public void ResetHealth()
    {
        Health.Value = GlobalSettings.DefaultHealth;
        HealthBarScript.SetHealthBarValue(Health.Value);
    }
    
    [Rpc(SendTo.Everyone)]
    public void DeathRpc()
    {
        if (IsOwner)
        {
            Debug.Log("oltiin owner ja toistettiin RPC");
            BaseController.OnDeath();
            BaseController.TeleportCharacter(Vector3.zero);
        }
    }

    [Rpc(SendTo.Owner)]
    public void StartCombatRpc(Vector3 fightPosition, int faceIndex)
    {
        BaseController.StartFight(fightPosition, faceIndex);
    }
}
