using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public GameObject worldPrefab;
    public GameObject inventoryPrefab;
}