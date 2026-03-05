// ItemReceiver.cs (Revisi untuk menggunakan ItemData)
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemReceiver : MonoBehaviour
{
    // --- GANTI string DENGAN ItemData ---
    [Tooltip("Data Item (ScriptableObject/Prefab) yang diterima oleh tempat ini sebagai kunci.")]
    public ItemData acceptedItemData; // <-- Referensi ke ItemData Kunci
    // --- BATAS PERUBAHAN ---

    // Flag static untuk status unlock pintu (tetap sama)
    private static bool isDoorPermanentlyUnlocked = false;
    public static bool IsDoorUnlocked => isDoorPermanentlyUnlocked;

    [Header("Feedback (Opsional)")]
    public GameObject itemAcceptEffectPrefab;
    public AudioClip itemAcceptSound;
    public AudioClip itemRejectSound;
    private AudioSource audioSource;

    void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Validasi penting: Pastikan ItemData kunci sudah di-set di Inspector
        if (acceptedItemData == null) {
            Debug.LogError($"ItemReceiver [{gameObject.name}] perlu 'Accepted Item Data' (ItemData Kunci) di-set di Inspector!");
            enabled = false; // Nonaktifkan script jika kunci tidak di-set
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Abaikan jika pintu sudah terbuka permanen
        if (isDoorPermanentlyUnlocked) {
            // Opsi: Bersihkan item kunci yang didrop jika pintu sudah terbuka
             WorldItem droppedItemCheck = other.GetComponent<WorldItem>();
             if (droppedItemCheck != null && droppedItemCheck.itemData == acceptedItemData) {
                 Debug.Log("Pintu sudah terbuka, membersihkan item kunci yang didrop.");
                 Destroy(other.gameObject);
             }
            return;
        }

        // Cek apakah yang masuk adalah WorldItem
        WorldItem droppedItem = other.GetComponent<WorldItem>();
        // Pastikan WorldItem ada DAN itemData-nya tidak null
        if (droppedItem != null && droppedItem.itemData != null)
        {
            // Gunakan nama dari ItemData untuk log jika ada (ganti .name sesuai field di ItemData-mu)
            string droppedItemName = droppedItem.itemData.name; // <-- Asumsi ItemData punya field 'name'
            Debug.Log($"ItemReceiver mendeteksi WorldItem: {droppedItemName}");

            // --- GANTI PERBANDINGAN string DENGAN ItemData ---
            // Bandingkan referensi ItemData secara langsung
            if (droppedItem.itemData == acceptedItemData)
            // --- BATAS PERUBAHAN ---
            {
                // --- Item Diterima (Kunci Cocok) ---
                Debug.Log($"ITEM DITERIMA: {droppedItemName}. Pintu sekarang bisa dibuka permanen!");
                isDoorPermanentlyUnlocked = true; // <-- Set status global!

                PlaySound(itemAcceptSound);
                if (itemAcceptEffectPrefab != null) {
                    Instantiate(itemAcceptEffectPrefab, transform.position, Quaternion.identity);
                }

                // Hancurkan GameObject WorldItem kunci yang diterima
                Destroy(other.gameObject);
            }
            else
            {
                // --- Item Ditolak (Bukan Kunci yang Benar) ---
                string acceptedItemName = acceptedItemData.name; // Asumsi ItemData punya field 'name'
                Debug.Log($"ITEM DITOLAK: {droppedItemName}. Tempat ini hanya menerima {acceptedItemName}.");
                PlaySound(itemRejectSound);
                // Jangan hancurkan item yang salah
            }
        }
    }

    private void PlaySound(AudioClip clip) {
        if (audioSource != null && clip != null) {
            audioSource.PlayOneShot(clip);
        }
    }

    public static void ResetDoorLockStatus() {
         Debug.LogWarning("Mereset status kunci pintu secara global!");
         isDoorPermanentlyUnlocked = false;
    }
}