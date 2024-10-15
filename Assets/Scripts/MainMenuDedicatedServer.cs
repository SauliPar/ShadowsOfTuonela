using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuDedicatedServer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
#if DEDICATED_SERVER
        Debug.Log("Dedicated_Server käynnistettiin");
        SceneManager.LoadScene("GameScene");
#endif
    }
}
