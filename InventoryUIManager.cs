// InventoryUIManager.cs
using UnityEngine;
using UnityEngine.UI; // Untuk Button (jika tombol Taruh adalah Button)

public class InventoryUIManager : MonoBehaviour
{
    [Header("Tombol Taruh (Drop)")]
    [Tooltip("Hubungkan GameObject Tombol 'Taruh' dari UI.")]
    public GameObject taruhButtonGameObject; // GameObject Tombol Taruh
    [Tooltip("Tingkat transparansi tombol saat tidak bisa digunakan (0.0 - 1.0).")]
    public float disabledAlpha = 0.5f;

    private CanvasGroup taruhButtonCanvasGroup;
    private ItemData currentlySelectedItemData = null;
    private GameObject currentlySelectedSlotUIObject = null; // GameObject dari slot UI yang dipilih
    private InventoryItemSlotUI lastSelectedSlotScript = null;

    // Referensi ke player_collect untuk memanggil fungsi DropItem
    private player_collect playerCollectScript;

    void Start()
    {
        if (taruhButtonGameObject != null)
        {
            taruhButtonCanvasGroup = taruhButtonGameObject.GetComponent<CanvasGroup>();
            if (taruhButtonCanvasGroup == null)
            {
                Debug.LogError("InventoryUIManager: Tombol 'Taruh' tidak memiliki komponen CanvasGroup! Tambahkan komponen tersebut.", taruhButtonGameObject);
                // Nonaktifkan tombol jika tidak ada CanvasGroup
                Button btn = taruhButtonGameObject.GetComponent<Button>();
                if (btn != null) btn.interactable = false;
            }
        }
        else
        {
            Debug.LogError("InventoryUIManager: Tombol 'Taruh' belum dihubungkan!", this.gameObject);
        }

         playerCollectScript = FindObjectOfType<player_collect>();
    if (playerCollectScript == null)
    {
        Debug.LogError("InventoryUIManager: Script 'player_collect' TIDAK DITEMUKAN di scene!", this.gameObject);
    }
    else
    {
        Debug.Log("InventoryUIManager: Script 'player_collect' berhasil ditemukan pada GameObject: " + playerCollectScript.gameObject.name, this.gameObject);
    }

        // Atur keadaan awal tombol "Taruh" (tidak ada item terpilih)
        UpdateTaruhButtonState(false);
    }

    // Fungsi ini dipanggil oleh InventoryItemSlotUI saat sebuah item di inventory diklik
    public void SelectItem(ItemData selectedData, GameObject selectedSlotObject)
    {
        if (selectedData == null || selectedSlotObject == null)
        {
            DeselectCurrentItem(); // Jika data tidak valid, deselect saja
            return;
        }

        // Deselect slot lama jika ada yang terpilih sebelumnya
        if (lastSelectedSlotScript != null)
        {
            lastSelectedSlotScript.SetSelectedVisual(false);
        }

        currentlySelectedItemData = selectedData;
        currentlySelectedSlotUIObject = selectedSlotObject; // Simpan GameObject slot UI-nya

        // Update visual slot baru yang terpilih
        lastSelectedSlotScript = selectedSlotObject.GetComponent<InventoryItemSlotUI>();
        if (lastSelectedSlotScript != null)
        {
            lastSelectedSlotScript.SetSelectedVisual(true);
        }

        Debug.Log($"Item dipilih: {currentlySelectedItemData.itemName}");
        UpdateTaruhButtonState(true); // Aktifkan tombol "Taruh"
    }

    // Fungsi untuk membatalkan pilihan item
    public void DeselectCurrentItem()
    {
        if (lastSelectedSlotScript != null)
        {
            lastSelectedSlotScript.SetSelectedVisual(false);
        }
        currentlySelectedItemData = null;
        currentlySelectedSlotUIObject = null;
        lastSelectedSlotScript = null;
        UpdateTaruhButtonState(false); // Nonaktifkan tombol "Taruh"
        Debug.Log("Pilihan item dibatalkan.");
    }


    // Fungsi ini akan dihubungkan ke OnClick() pada Tombol "Taruh"
    public void OnTaruhButtonPressed()
{
    // Log paling awal untuk memastikan fungsi ini terpanggil
    Debug.Log("OnTaruhButtonPressed: Fungsi dipanggil.");

    // Log status variabel krusial TEPAT SEBELUM memanggil DropItem
    string itemName = (currentlySelectedItemData != null) ? currentlySelectedItemData.itemName : "NULL";
    string slotName = (currentlySelectedSlotUIObject != null) ? currentlySelectedSlotUIObject.name : "NULL";
    bool playerCollectExists = (playerCollectScript != null);

    Debug.Log($"OnTaruhButtonPressed: Item Terpilih = {itemName}, Slot UI = {slotName}, PlayerCollectScript Ada = {playerCollectExists}");

    if (playerCollectScript == null)
    {
        Debug.LogError("OnTaruhButtonPressed: playerCollectScript adalah NULL! Tidak bisa memanggil DropItem.");
        DeselectCurrentItem(); // Tetap deselect agar UI kembali normal
        return;
    }
    if (currentlySelectedItemData == null)
    {
        Debug.LogError("OnTaruhButtonPressed: currentlySelectedItemData adalah NULL! Tidak ada item untuk di-drop.");
        DeselectCurrentItem();
        return;
    }
    if (currentlySelectedSlotUIObject == null)
    {
        // Ini mungkin tidak fatal jika DropItem bisa menangani slot null, tapi sebaiknya ada.
        Debug.LogWarning("OnTaruhButtonPressed: currentlySelectedSlotUIObject adalah NULL. Item UI mungkin tidak dihancurkan.");
    }

    // Panggil fungsi DropItem
    Debug.Log($"OnTaruhButtonPressed: Memanggil playerCollectScript.DropItem untuk '{itemName}'...");
    playerCollectScript.DropItem(currentlySelectedItemData, currentlySelectedSlotUIObject);

    // Setelah drop, reset pilihan dan status tombol
    DeselectCurrentItem(); // Log "Pilihan item dibatalkan." akan muncul dari sini
}

    // Mengatur status tombol "Taruh" (transparansi dan interaksi)
    private void UpdateTaruhButtonState(bool canTaruh)
    {
        if (taruhButtonCanvasGroup != null)
        {
            if (canTaruh)
            {
                taruhButtonCanvasGroup.alpha = 1f;
                taruhButtonCanvasGroup.interactable = true;
                taruhButtonCanvasGroup.blocksRaycasts = true;
            }
            else
            {
                taruhButtonCanvasGroup.alpha = disabledAlpha;
                taruhButtonCanvasGroup.interactable = false;
                taruhButtonCanvasGroup.blocksRaycasts = false;
            }
        }
        // Jika tidak pakai CanvasGroup, bisa atur Button.interactable secara langsung
        // dan ubah alpha dari Image-nya, tapi CanvasGroup lebih mudah.
    }
}