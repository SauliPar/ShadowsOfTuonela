using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TestButtonScript : MonoBehaviour
{
    [SerializeField] private Button testButton;

    public UnityEvent OnTestButtonPress;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        testButton.onClick.AddListener(OnButtonPressed);
    }

    private void OnButtonPressed()
    {
        EventManager.TriggerEvent(Events.DamageEvent, Random.Range(0,10));
    }
}
