// TempatKunciPintu.cs (Revisi untuk tidak langsung membuka pintu)
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class TempatKunciPintu : MonoBehaviour
{
    [Header("Pengaturan Kunci")]
    [Tooltip("ItemData yang dianggap sebagai kunci.")]
    public ItemData itemKunciData;

    [Header("Identifikasi Mekanisme")]
    [Tooltip("ID unik untuk mekanisme kunci ini. Harus sama dengan 'Mechanism Unique ID' di DoorController_Conditional yang terkait.")]
    public string uniqueMechanismID; // Contoh: "PINTU_RUMAH_DEPAN_UNLOCKED"

    // Static HashSet untuk menyimpan ID mekanisme kunci yang sudah diaktifkan (kunci sudah ditaruh)
    public static HashSet<string> activatedLockMechanisms = new HashSet<string>();

    private bool mechanismAlreadyActivatedThisSession = false;

    void Start()
    {
        if (itemKunciData == null) {
            Debug.LogError($"TempatKunciPintu '{gameObject.name}': Item Kunci Data belum di-set!", gameObject);
            enabled = false; return;
        }
        if (string.IsNullOrEmpty(uniqueMechanismID)) {
            Debug.LogError($"TempatKunciPintu '{gameObject.name}': Unique Mechanism ID belum di-set!", gameObject);
            enabled = false; return;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col == null) { Debug.LogError($"TempatKunciPintu [{gameObject.name}] tidak memiliki Collider2D!", gameObject); enabled = false; return; }
        if (!col.isTrigger) { col.isTrigger = true; }

        // Cek apakah mekanisme ini sudah pernah diaktifkan di sesi ini
        if (activatedLockMechanisms.Contains(uniqueMechanismID))
        {
            mechanismAlreadyActivatedThisSession = true;
            Debug.Log($"TempatKunciPintu [{uniqueMechanismID}]: Mekanisme kunci sudah aktif dari data sesi ini.");
            // (Opsional) Ubah tampilan TempatKunciPintu ini jika sudah dipakai
            // GetComponent<SpriteRenderer>().color = Color.gray; // Contoh
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (mechanismAlreadyActivatedThisSession)
        {
            // Jika mekanisme sudah aktif, hancurkan saja kunci yang salah drop untuk membersihkan
            WorldItem droppedKeyAgain = other.GetComponent<WorldItem>();
            if (droppedKeyAgain != null && droppedKeyAgain.itemData == itemKunciData) {
                Debug.Log($"TempatKunciPintu [{uniqueMechanismID}]: Mekanisme sudah aktif. Item kunci '{droppedKeyAgain.itemData.itemName}' yang baru didrop dihancurkan.");
                Destroy(other.gameObject);
            }
            return;
        }

        WorldItem worldItemKunci = other.GetComponent<WorldItem>();
        if (worldItemKunci != null && worldItemKunci.itemData != null)
        {
            if (worldItemKunci.itemData == itemKunciData)
            {
                Debug.Log($"TempatKunciPintu [{uniqueMechanismID}]: KUNCI BENAR ({worldItemKunci.itemData.itemName}) diterima!");

                Destroy(worldItemKunci.gameObject); // Hancurkan item kunci

                // Tandai bahwa mekanisme kunci ini sudah aktif
                activatedLockMechanisms.Add(uniqueMechanismID);
                mechanismAlreadyActivatedThisSession = true;

                Debug.Log($"TempatKunciPintu [{uniqueMechanismID}]: Mekanisme kunci diaktifkan. Pintu terkait sekarang bisa dibuka.");

                // TIDAK LAGI MENONAKTIFKAN PINTU SECARA LANGSUNG DI SINI
                // if (pintuYangDikontrol != null) { pintuYangDikontrol.SetActive(false); }

                // (Opsional) Feedback visual/suara untuk penempatan kunci berhasil
            }
            else
            {
                // Debug.Log($"TempatKunciPintu [{uniqueMechanismID}]: Item SALAH ({worldItemKunci.itemData.itemName}) dideteksi.");
            }
        }
    }

    // (Opsional) Fungsi untuk mereset status semua mekanisme jika memulai game baru
    public static void ResetAllActivatedLockMechanisms()
    {
        activatedLockMechanisms.Clear();
        Debug.Log("Semua status mekanisme kunci yang dikontrol TempatKunciPintu telah direset.");
    }
}