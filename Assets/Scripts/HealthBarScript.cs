using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private CanvasGroup canvasGroup;

    // private int _defaultHealthValue = 100;
    private int _healthValue;
    private float _timer;
    private float _hideInterval = 5f;

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer > _hideInterval)
        {
            if (_healthValue >= GlobalSettings.DefaultHealth)
            {
                Hide();
                _timer = 0;
            }
        }
    }

    private void Start()
    {
        slider.maxValue = GlobalSettings.DefaultHealth;
        _healthValue = GlobalSettings.DefaultHealth;
        slider.value = _healthValue;

        Hide();
    }

    public void SetHealthBarValue(int healthValue)
    {
        _healthValue = healthValue;
        slider.value = _healthValue;
        
        _timer = 0;
    }

    public void SubtractHealth(int subtractValue)
    {
        _healthValue -= subtractValue;
        slider.value = _healthValue;
    }

    public void Show()
    {
        canvasGroup.alpha = 1f;
        _timer = 0;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
    }

    public void StartHiding()
    {
        Invoke(nameof(Hide), 5f);
    }

    public void StopInvoking()
    {
        CancelInvoke();
    }
}
