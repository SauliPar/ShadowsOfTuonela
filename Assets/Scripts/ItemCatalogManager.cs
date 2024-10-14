using System.Collections.Generic;
using System.Linq;
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
        // first we take a random value from 0 to 1
        // NOTE: IF TOTAL DROP CHANCE EXCEEDS 1, THIS DOESN'T WORK
        var randomValue = Random.value;
        
        float cumulativeDropChance = 0f;
        
        // then we loop through the itemlist to see if our random value was a "hit" in the dropchances
        foreach (var item in ItemList)
        {
            // we must sum the dropchances to take into account the cumulative nature
            cumulativeDropChance += item.DropChance;
            if (randomValue <= cumulativeDropChance)
                return item.Id;
        }

        // last if for some reason we failed, we return -1
        return -1;
    }
}
