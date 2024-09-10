using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private int _defaultHealthValue = 100;
    private int _healthValue;

    private void Start()
    {
        _healthValue = _defaultHealthValue;
        slider.value = _healthValue;
    }

    public void SetHealthBarValue(int healthValue)
    {
        _healthValue = healthValue;
        slider.value = _healthValue;
    }

    public void SubtractHealth(int subtractValue)
    {
        _healthValue -= subtractValue;
        slider.value = _healthValue;
    }
}
