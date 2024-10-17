using System;
using TMPro;
using UnityEngine;

public class KillCountHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private GameObject killCountGameobject;
    [SerializeField] private Color defaultKillColor;
    [SerializeField] private Color killingSpreeKillColor;
    [SerializeField] private Color godlikeKillColor;

    public void ShowKillCount(int killCount)
    {
        killCountGameobject.SetActive(true);

        textMeshProUGUI.text = killCount.ToString();

        HandleKillCountColor(killCount);
    }

    private void HandleKillCountColor(int killCount)
    {
        if (killCount > 3 && killCount <= 6)
        {
            textMeshProUGUI.color = killingSpreeKillColor;
        }
        else if (killCount > 6)
        {
            textMeshProUGUI.color = godlikeKillColor;
        }
        else
        {
            textMeshProUGUI.color = defaultKillColor;
        }
    }

    public void HideKillCount()
    {
        killCountGameobject.SetActive(false);
    }
}
