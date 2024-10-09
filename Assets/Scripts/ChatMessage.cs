using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class ChatMessage : NetworkBehaviour
{
   [SerializeField] private TextMeshProUGUI textMeshProUGUI;
   [SerializeField] private CanvasGroup chatCanvasGroup;
   [SerializeField] private CanvasGroup nameTagCanvasGroup;

   public int LongMessageLength = 60;
   public float LongMessageDuration = 6;
   public float DefaultMessageDuration = 4;

   [Rpc(SendTo.Everyone)]
   public void ShowTextMessageToEveryoneRpc(FixedString128Bytes textToShow)
   {
      CancelInvoke();
      
      if (textToShow.Length > LongMessageLength)
      {
         Invoke(nameof(HideTextView), LongMessageDuration);
      }
      else
      {
         Invoke(nameof(HideTextView), DefaultMessageDuration);
      }

      textMeshProUGUI.text = textToShow.ToString();
      chatCanvasGroup.alpha = 1f;
      nameTagCanvasGroup.alpha = 0f;
   }

   private void HideTextView()
   {
      textMeshProUGUI.text = "";
      chatCanvasGroup.alpha = 0f;
      nameTagCanvasGroup.alpha = 1f;
   }
}
