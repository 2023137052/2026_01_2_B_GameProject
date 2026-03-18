using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataBaseSO", menuName = "Inventory/DataBase")]
public class ItemDataBaseSO : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();


    private Dictionary<int, ItemSO> itemsById;
    private Dictionary<string, ItemSO> itemsByName;


    public void Initialze()
    {
        itemsById = new Dictionary<int, ItemSO>();
        itemsByName = new Dictionary<string, ItemSO>();

        foreach(var item in items)
        {
            itemsById[item.id] = item;
            itemsByName[item.itemName] = item;
        }
    }

    public ItemSO GetItemById(int id)
    {
        if(itemsById == null)
        {
            Initialze();
        }

        if(itemsById.TryGetValue(id, out ItemSO item))
            return item;

        return null;
    }

    public ItemSO GetItemByName(string name)
    {
        if(itemsByName == null)
        {
            Initialze();
        }

        if(itemsByName.TryGetValue(name, out ItemSO item))
            return item;

        return null;
    }

    public List<ItemSO> GetItemByType(ItemType type)
    {
        return items.FindAll(item => item.itemType == type);
    }

}
