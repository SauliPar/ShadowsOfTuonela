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
}
