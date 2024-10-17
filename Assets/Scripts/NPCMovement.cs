using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class NPCMovement : MonoBehaviour
{
    [SerializeField] private BaseController baseController;

    private float _timer;
    private float _timeInterval = .5f;
    private int _idleTime;

    public NpcState CurrentNpcState = NpcState.Idle;

    public PlayerState PlayerState;
    
    [HideInInspector]
    public NPCManager NpcManager;

    private void Start()
    {
        _idleTime = Random.Range(10, 25);
    }

    private void Update()
    {
        if (PlayerState.CombatState.Value == CombatState.Combat ||
            PlayerState.CombatState.Value == CombatState.Flee)
        {
            // here we do something when we are fighting
            _timer = 8f;
            CurrentNpcState = NpcState.Idle;
            return;
        }

        _timer += Time.deltaTime;

        if (_timer < _timeInterval) return;

        switch (CurrentNpcState)
        {
            case NpcState.Idle:
                HandleIdle();
                break;
            case NpcState.Walking:
                HandleWalking();
                break;
        }
    }

    private void HandleWalking()
    {
        if (NpcManager == null) NpcManager = FindFirstObjectByType<NPCManager>();
        
        baseController.Move(NpcManager.GetNextBotWalkPosition());

        CurrentNpcState = NpcState.Idle;

        _timer = 0;
    }

    private void HandleIdle()
    {
        if (_timer < _idleTime) return;

        CurrentNpcState = NpcState.Walking;    
    }
}

public enum NpcState
{
    Idle,
    Walking,
}
