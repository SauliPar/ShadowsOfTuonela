using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : Singleton<EventManager>
{
    private Dictionary<Events, UnityEvent<object>> _eventDictionary;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Awake()
    {
        _eventDictionary ??= new Dictionary<Events, UnityEvent<object>>();
    }

    public static void StartListening(Events eventName, UnityAction<object> listener)
    {
        if (Instance._eventDictionary.TryGetValue(eventName, out UnityEvent<object> thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent<object>();
            thisEvent.AddListener(listener);
            Instance._eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(Events eventName, UnityAction<object> listener)
    {
        if (Instance._eventDictionary.TryGetValue(eventName, out UnityEvent<object> thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(Events eventName, object data = null)
    {
        if (Instance._eventDictionary.TryGetValue(eventName, out UnityEvent<object> thisEvent))
        {
            thisEvent.Invoke(data);
        }
    }
}

public enum Events
{
    DamageEvent,
    Click,
}
