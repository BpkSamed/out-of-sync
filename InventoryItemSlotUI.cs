// InventoryItemSlotUI.cs
using UnityEngine;
using UnityEngine.UI; // Untuk Button
using UnityEngine.EventSystems; // Untuk IPointerClickHandler (alternatif Button)

// Pastikan GameObject yang menggunakan script ini memiliki komponen Button atau bisa menerima klik
// [RequireComponent(typeof(Button))] // Jika kamu pasti pakai komponen Button
public class InventoryItemSlotUI : MonoBehaviour, IPointerClickHandler // Implement IPointerClickHandler jika tidak pakai Button component
{
    public ItemData itemData; // Akan diisi saat item ini di-instantiate di inventory
    // private Button button; // Jika menggunakan komponen Button

    // Referensi ke InventoryUIManager, akan dicari otomatis
    private InventoryUIManager inventoryUIManager;

    void Start()
    {
        inventoryUIManager = FindObjectOfType<InventoryUIManager>();
        if (inventoryUIManager == null)
        {
            Debug.LogError("InventoryUIManager tidak ditemukan di scene oleh InventoryItemSlotUI!", this.gameObject);
        }

        // Jika menggunakan komponen Button:
        // button = GetComponent<Button>();
        // if (button != null)
        // {
        //     button.onClick.AddListener(OnSlotClicked);
        // }
        // else
        // {
        //     // Jika tidak ada komponen Button, kita akan mengandalkan IPointerClickHandler
        //     Debug.LogWarning("InventoryItemSlotUI: Komponen Button tidak ditemukan, menggunakan IPointerClickHandler.", gameObject);
        // }
    }

    // Fungsi ini dipanggil jika menggunakan IPointerClickHandler
    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotClicked();
    }

    // Fungsi yang dipanggil saat slot item ini diklik
    public void OnSlotClicked()
    {
        if (itemData != null && inventoryUIManager != null)
        {
            Debug.Log($"Inventory slot diklik: {itemData.itemName}");
            inventoryUIManager.SelectItem(itemData, this.gameObject); // Kirim ItemData dan GameObject slot ini
        }
        else if (itemData == null)
        {
            Debug.LogWarning("Inventory slot diklik tapi tidak ada ItemData.", this.gameObject);
        }
    }

    // (Opsional) Fungsi untuk mengubah tampilan visual saat terpilih/tidak terpilih
    public void SetSelectedVisual(bool isSelected)
    {
        Image backgroundImage = GetComponent<Image>(); // Asumsi slot punya Image untuk background
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? Color.yellow : Color.white; // Contoh: kuning saat terpilih
        }
    }
}