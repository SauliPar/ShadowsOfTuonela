using UnityEngine;
using TMPro;
using Unity.Collections;
using UnityEngine.UI;

public class SendMessageButton : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button button;
    [SerializeField] private ChatMessage chatMessage;

    private void Start()
    {
        button.onClick.AddListener(HandleMessage);
    }

    public void HandleMessage()
    {
        var fixedString = new FixedString128Bytes(inputField.text);
        chatMessage.ShowTextMessageToEveryoneRpc(fixedString);
        inputField.text = "";
    }

    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }
}
