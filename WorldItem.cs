// WorldItem.cs (Versi 3D)
using UnityEngine;
using System; // Diperlukan untuk System.Action (event)

// Mengganti 2D komponen dengan 3D
[RequireComponent(typeof(Collider))] // Bukan Collider2D
[RequireComponent(typeof(Renderer))] // Bukan SpriteRenderer (biasanya MeshRenderer)
public class WorldItem : MonoBehaviour
{
    [Tooltip("Data ScriptableObject atau komponen yang berisi detail item ini.")]
    public ItemData itemData;
    [Tooltip("Status apakah item ini bisa diambil oleh player atau tidak.")]
    public bool bisaDiambil = true;

    // Variabel untuk logika TempatKhusus (Reward) - tetap sama
    private bool sedangDiTempatKhususCocok = false;
    private bool rewardSudahDiberikan = false;
    public static event Action<ItemData> OnItemBenarDitempatkan;

    [Header("Tampilan Material Saat Disentuh Player")]
    [Tooltip("Material yang akan ditampilkan saat item ini disentuh/disorot oleh Player.")]
    public Material highlightedMaterial; // Mengganti Sprite dengan Material
    private Material defaultMaterial;    // Akan diisi otomatis dari Renderer
    private Renderer itemRenderer; // Cache komponen Renderer (MeshRenderer)

    private bool isPlayerCurrentlyTouching = false; // Untuk melacak sentuhan player

    void Awake()
    {
        // Cache Renderer
        itemRenderer = GetComponent<Renderer>(); // Menggunakan Renderer
        if (itemRenderer != null)
        {
            // .material membuat instance baru, yang aman untuk diubah
            defaultMaterial = itemRenderer.material; 
        }
        else
        {
            Debug.LogError($"WorldItem [{gameObject.name}] tidak memiliki komponen Renderer! Fitur ganti material tidak akan berfungsi.", this);
            enabled = false; // Nonaktifkan script jika tidak ada Renderer
            return;
        }

        if (highlightedMaterial == null)
        {
            // Debug.LogWarning($"WorldItem [{itemData?.itemName ?? gameObject.name}] tidak memiliki 'Highlighted Material'. Efek ganti material saat disentuh tidak akan aktif.", this);
        }

        // Pengaturan Collider dan Rigidbody 3D
        Collider col = GetComponent<Collider>(); // Menggunakan Collider (base class 3D)
        if (col == null) {
             Debug.LogError($"WorldItem [{gameObject.name}] tidak memiliki Collider!", gameObject);
             enabled = false; return;
        }
        if (!col.isTrigger) {
            Debug.LogWarning($"WorldItem [{gameObject.name}] Collider sebaiknya 'Is Trigger = true' untuk efek highlight saat disentuh. Mengubah otomatis.", gameObject);
            col.isTrigger = true;
        }

        if (GetComponent<Rigidbody>() == null) { // Menggunakan Rigidbody (3D)
             Rigidbody rb = gameObject.AddComponent<Rigidbody>(); // Menggunakan Rigidbody (3D)
             rb.isKinematic = true; // Kinematic agar tidak jatuh atau bergerak aneh karena fisika
             // Properti 'useFullKinematicContacts' tidak ada di Rigidbody 3D,
             // tapi interaksi trigger dengan kinematic Rigidbody sudah standar.
             Debug.LogWarning($"WorldItem [{gameObject.name}] tidak punya Rigidbody, menambahkannya (Kinematic).");
        }
        rewardSudahDiberikan = false; // Reset flag reward
    }

    public void SetBisaDiambil(bool status) {
        bisaDiambil = status;
    }

    // --- LOGIKA TRIGGER 3D UNTUK INTERAKSI ---
    // Mengganti ...2D dengan versi 3D (parameter Collider)
    void OnTriggerEnter(Collider other) 
    {
        // Logika untuk ganti material saat disentuh Player
        if (other.CompareTag("Player")) // Pastikan Player punya tag "Player"
        {
            isPlayerCurrentlyTouching = true;
            if (highlightedMaterial != null && itemRenderer != null)
            {
                itemRenderer.material = highlightedMaterial; // Mengganti material
                // Debug.Log($"Player menyentuh {itemData?.itemName ?? gameObject.name}, material diubah ke highlight.");
            }
        }
    }

    // Mengganti ...2D dengan versi 3D (parameter Collider)
    void OnTriggerStay(Collider other) 
    {
        // Logika untuk TempatKhusus (Reward) - dari kodemu sebelumnya
        if (other.CompareTag("TempatKhusus"))
        {
            // Pastikan script TempatKhusus juga ada di trigger 3D
            TempatKhusus tempat = other.GetComponent<TempatKhusus>(); 
            if (tempat != null && itemData != null)
            {
                if (tempat.ApakahItemCocok(this.itemData))
                {
                    if (bisaDiambil && !rewardSudahDiberikan) {
                        bisaDiambil = false; // Item "diletakkan"
                        sedangDiTempatKhususCocok = true;
                        // Debug.Log($"WorldItem [{itemData.itemName}] masuk Tempat Khusus COCOK [{tempat.name}]. Set bisaDiambil = false.");

                        if (tempat.itemHasil != null)
                        {
                            // Debug.Log($"Memicu event OnItemBenarDitempatkan untuk reward: {tempat.itemHasil.itemName}");
                            OnItemBenarDitempatkan?.Invoke(tempat.itemHasil);
                            rewardSudahDiberikan = true;
                        }
                    }
                }
                else // Item tidak cocok dengan TempatKhusus
                {
                    if (sedangDiTempatKhususCocok && !rewardSudahDiberikan) { 
                        bisaDiambil = true;
                    }
                    sedangDiTempatKhususCocok = false;
                }
            }
        }
    }

    // Mengganti ...2D dengan versi 3D (parameter Collider)
    void OnTriggerExit(Collider other) 
    {
        // Logika untuk mengembalikan material saat Player meninggalkan item
        if (other.CompareTag("Player"))
        {
            isPlayerCurrentlyTouching = false;
            if (defaultMaterial != null && itemRenderer != null)
            {
                itemRenderer.material = defaultMaterial; // Mengembalikan material default
                // Debug.Log($"Player meninggalkan {itemData?.itemName ?? gameObject.name}, material kembali ke default.");
            }
        }

        // Logika untuk TempatKhusus saat keluar - dari kodemu sebelumnya
        if (other.CompareTag("TempatKhusus"))
        {
            if (sedangDiTempatKhususCocok && !rewardSudahDiberikan) { // Jika keluar dari tempat cocok sebelum reward
                bisaDiambil = true; // Bisa diambil lagi
            }
            sedangDiTempatKhususCocok = false; // Tidak lagi di tempat khusus yang cocok
            
            rewardSudahDiberikan = false; // Logika ini tetap sama dari kodemu
            // Debug.Log($"WorldItem [{itemData?.itemName ?? "Item"}] keluar dari Tempat Khusus [{other.name}].");
        }
    }
}