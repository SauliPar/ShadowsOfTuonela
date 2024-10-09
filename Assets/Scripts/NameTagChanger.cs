using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NameTagChanger : NetworkBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private Button button;

    private void Start()
    {
        button.onClick.AddListener(TryToChangeName);
    }

    public void TryToChangeName()
    {
        var fixedString = new FixedString128Bytes(inputField.text);
        playerState.ChangeNameTagServerRPC(fixedString);
    }

    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }
}
