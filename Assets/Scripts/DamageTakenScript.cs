using System;
using TMPro;
using UnityEngine;

public class DamageTakenScript : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    private void Start()
    {
        HideDamageTakenUI();
        // EventManager.StartListening(Events.DamageEvent, ShowDamage);
    }

    private void ShowDamage(object data)
    {
        if (data == null) return;
        
        CancelInvoke();
       
        var damageTaken = (int)data;
        
        textMeshProUGUI.text = damageTaken.ToString();
        canvasGroup.alpha = 1f;
        
        Invoke(nameof(HideDamageTakenUI), 1f);
    }
    
    public void ShowDamage(int damageNumber)
    {
        CancelInvoke();

        // Debug.Log("damagenumber 2: " + damageNumber);
        
        textMeshProUGUI.text = damageNumber.ToString();
        canvasGroup.alpha = 1f;
        
        Invoke(nameof(HideDamageTakenUI), 1f);
    }

    private void HideDamageTakenUI()
    {
        canvasGroup.alpha = 0f;
    }

    private void OnDisable()
    {
        // EventManager.StopListening(Events.DamageEvent, ShowDamage);
    }
}
