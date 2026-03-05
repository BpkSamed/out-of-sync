// ObjectSwitcherOnTrigger.cs
using UnityEngine;

// Pastikan GameObject yang menggunakan script ini memiliki Collider2D
[RequireComponent(typeof(Collider2D))]
public class ObjectSwitcherOnTrigger : MonoBehaviour
{
    [Header("Pengaturan Objek")]
    [Tooltip("GameObject yang akan HILANG saat player masuk trigger dan MUNCUL saat player keluar.")]
    public GameObject objectToDeactivateOnEnter; // Ini adalah Objek A

    [Tooltip("GameObject yang akan MUNCUL saat player masuk trigger dan HILANG saat player keluar.")]
    public GameObject objectToActivateOnEnter;   // Ini adalah Objek B

    [Header("Pengaturan Pemicu")]
    [Tooltip("Tag GameObject yang bisa memicu pergantian objek (biasanya 'Player').")]
    public string triggeringTag = "Player"; // Pastikan Player-mu memiliki tag ini

    void Awake()
    {
        // Validasi awal dan pengaturan collider
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"ObjectSwitcherOnTrigger pada '{gameObject.name}' tidak memiliki Collider2D!", gameObject);
            enabled = false; // Nonaktifkan script jika tidak ada collider
            return;
        }
        if (!col.isTrigger)
        {
            Debug.LogWarning($"ObjectSwitcherOnTrigger pada '{gameObject.name}': Collider2D sebaiknya di-set sebagai 'Is Trigger = true'. Mengubah otomatis.", gameObject);
            col.isTrigger = true;
        }

        // Validasi referensi objek
        if (objectToDeactivateOnEnter == null)
        {
            Debug.LogWarning($"ObjectSwitcherOnTrigger pada '{gameObject.name}': 'Object To Deactivate On Enter' (Objek A) belum di-assign.", gameObject);
        }
        if (objectToActivateOnEnter == null)
        {
            Debug.LogWarning($"ObjectSwitcherOnTrigger pada '{gameObject.name}': 'Object To Activate On Enter' (Objek B) belum di-assign.", gameObject);
        }

        // Atur keadaan awal objek saat scene dimulai
        // Asumsikan Objek A terlihat dan Objek B tersembunyi di awal.
        if (objectToDeactivateOnEnter != null)
        {
            objectToDeactivateOnEnter.SetActive(true);
        }
        if (objectToActivateOnEnter != null)
        {
            objectToActivateOnEnter.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah objek yang masuk memiliki tag yang sesuai
        if (other.CompareTag(triggeringTag))
        {
            Debug.Log($"ObjectSwitcher: Player '{other.gameObject.name}' memasuki area trigger. Mengaktifkan Objek B, menonaktifkan Objek A.");

            if (objectToDeactivateOnEnter != null)
            {
                objectToDeactivateOnEnter.SetActive(false); // Sembunyikan Objek A
            }
            if (objectToActivateOnEnter != null)
            {
                objectToActivateOnEnter.SetActive(true);   // Tampilkan Objek B
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Cek apakah objek yang keluar memiliki tag yang sesuai
        if (other.CompareTag(triggeringTag))
        {
            Debug.Log($"ObjectSwitcher: Player '{other.gameObject.name}' keluar dari area trigger. Mengaktifkan Objek A, menonaktifkan Objek B.");

            if (objectToActivateOnEnter != null)
            {
                objectToActivateOnEnter.SetActive(false); // Sembunyikan Objek B
            }
            if (objectToDeactivateOnEnter != null)
            {
                objectToDeactivateOnEnter.SetActive(true);   // Tampilkan kembali Objek A
            }
        }
    }
}