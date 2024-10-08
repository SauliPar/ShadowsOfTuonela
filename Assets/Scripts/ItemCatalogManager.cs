using System.Collections.Generic;
using UnityEngine;

public class ItemCatalogManager : Singleton<ItemCatalogManager>
{
    public List<Item> ItemList;
    
    public Item GetItemById(int id)
    {
        return ItemList.Find(item => item.Id == id);
    }

    public int GetRandomItemFromDatabase()
    {
        int randomIndex = Random.Range(0, ItemList.Count);
        return ItemList[randomIndex].Id;
    }

    public int GetRandomItemWithDropChances()
    {
        float totalDropChance = 0;
        foreach (var item in ItemList)
            totalDropChance += item.DropChance;
        
        float randomPoint = Random.value * totalDropChance;
        
        for (int i = 0; i < ItemList.Count; i++)
        {
            // If the randomPoint lands in the interval of this item's DropChance, we return the Item
            if (randomPoint < ItemList[i].DropChance)
                return ItemList[i].Id;
            
            // Otherwise, we subtract this item's DropChance from the randomly selected point
            randomPoint -= ItemList[i].DropChance;
        }
        // In a very rare case, when no section is selected, we could return null or any fallback value you'd like
        return -1;
    }
}
