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
        float totalDropChance = 0;
        foreach (var item in ItemCatalogManager.Instance.ItemList)
            totalDropChance += item.DropChance;
        
        float randomPoint = Random.value * totalDropChance;
        
        for (int i = 0; i < ItemCatalogManager.Instance.ItemList.Count; i++)
        {
            // If the randomPoint lands in the interval of this item's DropChance, we return the Item
            if (randomPoint < ItemCatalogManager.Instance.ItemList[i].DropChance)
                return ItemCatalogManager.Instance.ItemList[i].Id;
            
            // Otherwise, we subtract this item's DropChance from the randomly selected point
            randomPoint -= ItemCatalogManager.Instance.ItemList[i].DropChance;
        }
        // In a very rare case, when no section is selected, we could return null or any fallback value you'd like
        return -1;
    }
}
