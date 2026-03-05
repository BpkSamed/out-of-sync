using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Item/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> semuaItem;

    public ItemData GetItemByName(string namaItem)
    {
        foreach (var item in semuaItem)
        {
            if (item.itemName == namaItem)
                return item;
        }
        Debug.LogWarning("Item tidak ditemukan: " + namaItem);
        return null;
    }

    public ItemData GetRandomItem()
    {
        if (semuaItem.Count == 0) return null;
        return semuaItem[Random.Range(0, semuaItem.Count)];
    }
}