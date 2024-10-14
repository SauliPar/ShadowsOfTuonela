using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TestButtonScript : MonoBehaviour
{
    [SerializeField] private Button testButton;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

    public UnityEvent OnTestButtonPress;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        testButton.onClick.AddListener(OnButtonPressed);
    }

    private void OnButtonPressed()
    {
        int r2hAmount = 0;

        for (int i = 0; i < 1000; i++)
        {
            if (GetRandomItemWithDropChances() == 0)
            {
                r2hAmount++;
            }
        }

        textMeshProUGUI.text = r2hAmount.ToString();
    }
    
    public int GetRandomItemWithDropChances()
    {
        // first we calculate the sum of all the items dropchance
        float totalDropChance = ItemCatalogManager.Instance.ItemList.Sum(item => item.DropChance);
        // then we take a random value from 0 to 1
        var randomValue = Random.Range(0, totalDropChance);

        // then we check if randomvalue is bigger than totaldropchance, this means that
        // we did not "hit", then we drop nothing
        // if (randomValue > totalDropChance) return -1;

        float cumulativeDropChance = 0f;
        
        // then we loop through the itemlist to see if our random value was a "hit" in the dropchances
        foreach (var item in ItemCatalogManager.Instance.ItemList)
        {
            cumulativeDropChance += item.DropChance;
            if (randomValue <= cumulativeDropChance)
                return item.Id;
        }

        // last if for some reason we failed, we return -1
        return -1;
    }
}
