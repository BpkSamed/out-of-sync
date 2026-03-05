// PlayerInteractionIndicator.cs
using UnityEngine;
using System.Collections.Generic; // Menggunakan HashSet untuk efisiensi

public class PlayerInteractionIndicator : MonoBehaviour
{
    [Tooltip("Hubungkan GameObject 'IndikatorSeru' (anak dari Player) ke sini.")]
    public GameObject exclamationMarkVisual;

    // Menyimpan daftar Collider2D dari item yang bisa diambil yang saat ini ada dalam jangkauan
    // Menggunakan HashSet agar tidak ada duplikasi dan penghapusan lebih efisien.
    private HashSet<Collider2D> nearbyPickupableItems = new HashSet<Collider2D>();

    void Start()
    {
        if (exclamationMarkVisual != null)
        {
            exclamationMarkVisual.SetActive(false); // Pastikan tanda seru mati di awal
        }
        else
        {
            Debug.LogError("PlayerInteractionIndicator: 'ExclamationMarkVisual' belum di-assign di Inspector!", this.gameObject);
            enabled = false; // Nonaktifkan script jika visual tidak ada
        }

        // Pastikan GameObject ini memiliki Collider2D yang diset sebagai Trigger
        // Jika script ini ada di Player utama dan kamu pakai RadiusDeteksiItem,
        // maka pastikan RadiusDeteksiItem yang punya trigger.
        // Jika script ini ada di RadiusDeteksiItem, maka RadiusDeteksiItem harus punya trigger.
        bool triggerFound = false;
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(); // Cek juga anak jika RadiusDeteksiItem adalah child
        if (colliders.Length == 0) colliders = GetComponents<Collider2D>(); // Cek di GameObject ini sendiri

        foreach (Collider2D col in colliders) {
            if (col.isTrigger) {
                triggerFound = true;
                break;
            }
        }
        if (!triggerFound) {
            // Debug.LogWarning("PlayerInteractionIndicator: Tidak ditemukan Collider2D dengan 'Is Trigger' aktif pada GameObject ini atau anaknya. Deteksi item mungkin tidak berfungsi.", this.gameObject);
            // Jika script dipasang di Player, dan kamu menggunakan RadiusDeteksiItem sebagai child,
            // maka OnTriggerEnter2D/Exit2D harus ada di script pada RadiusDeteksiItem.
            // Jika script ini di Player dan RadiusDeteksiItem adalah child dengan collider trigger,
            // maka event trigger akan diterima oleh Rigidbody2D di Player.
        }
    }

    // Fungsi ini akan dipanggil jika GameObject ini (atau child dengan Rigidbody yang terhubung)
    // memiliki Collider2D (dengan IsTrigger=true) dan Rigidbody2D (bisa di parent).
    void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah objek yang masuk memiliki script WorldItem dan bisa diambil
        // Atau cek berdasarkan Tag jika kamu menggunakan Tag "CollectibleItem"
        WorldItem item = other.GetComponent<WorldItem>(); // Asumsi item punya script WorldItem.cs

        // Ganti 'item != null && item.bisaDiambil' dengan kondisimu
        // Misalnya, jika item hanya perlu punya tag "CollectibleItem":
        // if (other.CompareTag("CollectibleItem"))
        if (item != null && item.bisaDiambil) // Menggunakan kondisi dari script WorldItem.cs mu
        {
            if (nearbyPickupableItems.Add(other)) // Hanya proses jika ini item baru dalam jangkauan
            {
                UpdateIndicatorVisibility();
                // Debug.Log("Item masuk jangkauan: " + other.name);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        WorldItem item = other.GetComponent<WorldItem>();
        // if (other.CompareTag("CollectibleItem"))
        if (item != null) // Tidak perlu cek item.bisaDiambil lagi, jika sudah masuk list pasti valid
        {
            if (nearbyPickupableItems.Remove(other)) // Hanya proses jika item ini memang ada di list
            {
                UpdateIndicatorVisibility();
                // Debug.Log("Item keluar jangkauan: " + other.name);
            }
        }
    }

    void UpdateIndicatorVisibility()
    {
        if (exclamationMarkVisual != null)
        {
            // Tampilkan tanda seru jika ada minimal satu item yang bisa diambil di dekatnya
            if (nearbyPickupableItems.Count > 0)
            {
                exclamationMarkVisual.SetActive(true);
            }
            else
            {
                exclamationMarkVisual.SetActive(false);
            }
        }
    }
}