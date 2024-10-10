using System;
using TMPro;
using UnityEngine;

public class RespawnHandler : MonoBehaviour
{
   [SerializeField] private CanvasGroup canvasGroup;
   [SerializeField] private TextMeshProUGUI textMeshProUGUI;
   [SerializeField] private PlayerController playerController;
   private float _timer;
   private bool _timerIsOn;

   public void ShowRespawnCanvas()
   {
      _timer = GlobalSettings.RespawnTimer;
      textMeshProUGUI.text = GlobalSettings.RespawnTimer.ToString();
      
      canvasGroup.alpha = 1;

      playerController.IsRespawning = true;
      _timerIsOn = true;
   }

   private void Update()
   {
      if (!_timerIsOn) return;

      _timer -= Time.deltaTime;

      textMeshProUGUI.text = _timer.ToString("F2");
      
      if (_timer <= 0)
      {
         textMeshProUGUI.text = "0";
          _timerIsOn = false;
          canvasGroup.alpha = 0;
          playerController.IsRespawning = false;
      }
   }
}
