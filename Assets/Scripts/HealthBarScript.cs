using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private float _defaultHealthValue = 100;
    private float _healthValue;

    private void Start()
    {
        _healthValue = _defaultHealthValue;
        slider.value = _healthValue;
    }

    public void SetHealthBarValue(float healthValue)
    {
        _healthValue = healthValue;
        slider.value = _healthValue;
    }

    public void SubtractHealth(float subtractValue)
    {
        _healthValue -= subtractValue;
        slider.value = _healthValue;
    }
}
