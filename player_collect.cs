// player_collect.cs (Versi 3D)
using UnityEngine;
using System.Collections.Generic;
// using UnityEngine.UI; 

public class player_collect : MonoBehaviour
{
    [Header("Referensi")]
    public ItemDatabase itemDatabase; 
    public Transform inventoryParent;  
    public GameObject pickupButtonUI;  

    [Header("Pengaturan")]
    public float pickupDistance = 2f;
    [Tooltip("Tingkat transparansi tombol saat tidak bisa digunakan (0.0 - 1.0).")]
    public float disabledAlpha = 0.5f;

    [Header("Sound Effects")] 
    [Tooltip("Suara yang diputar saat mengambil item.")]
    public AudioClip pickupSound;
    [Tooltip("Suara yang diputar saat meletakkan/drop item.")]
    public AudioClip dropSound;
    private AudioSource audioSource; 

    private WorldItem nearbyPickableWorldItem;
    // private GameObject nearbyPickableItemObject; 
    private TempatKhusus tempatKhususDekat; // Pastikan TempatKhusus juga 3D
    private List<GameObject> spawnedWorldItems = new List<GameObject>();

    private CanvasGroup pickupButtonCanvasGroup;

    void Awake() 
    {
        // --- Bagian AudioSource (Tidak Perlu Diubah) ---
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && transform.parent != null)
        {
            audioSource = transform.parent.GetComponent<AudioSource>(); 
        }
        if (audioSource == null)
        {
            Debug.LogWarning($"player_collect: AudioSource tidak ditemukan di {gameObject.name} atau parent-nya. Suara item mungkin tidak diputar via komponen AudioSource. Pertimbangkan untuk menambahkannya atau menggunakan PlayClipAtPoint.", this.gameObject);
        }
        else
        {
            audioSource.playOnAwake = false; 
            audioSource.loop = false;
        }
        // --- Akhir Bagian AudioSource ---
    }

    void OnEnable()
    {
        // Event listener (Tidak Perlu Diubah)
        WorldItem.OnItemBenarDitempatkan += HandleItemReward;
    }

    void OnDisable()
    {
        // Event listener (Tidak Perlu Diubah)
        WorldItem.OnItemBenarDitempatkan -= HandleItemReward;
    }

    void Start()
    {
        // Setup Tombol UI (Tidak Perlu Diubah)
        if (pickupButtonUI != null)
        {
            pickupButtonCanvasGroup = pickupButtonUI.GetComponent<CanvasGroup>();
            if (pickupButtonCanvasGroup == null)
            {
                Debug.LogError($"[{this.GetType().Name}] Tombol '{pickupButtonUI.name}' tidak memiliki komponen CanvasGroup! Tambahkan komponen tersebut.", pickupButtonUI);
                UnityEngine.UI.Button btn = pickupButtonUI.GetComponent<UnityEngine.UI.Button>();
                if (btn != null) btn.interactable = false;
                else pickupButtonUI.SetActive(false); 
            }
            SetPickupButtonState(false); 
        }
        else
        {
            Debug.LogWarning($"[{this.GetType().Name}] Peringatan: 'pickupButtonUI' belum dihubungkan.", this.gameObject);
        }

        if (itemDatabase == null) Debug.LogWarning($"[{this.GetType().Name}] Peringatan: 'itemDatabase' belum dihubungkan (jika memang diperlukan).", this.gameObject);
        if (inventoryParent == null) Debug.LogWarning($"[{this.GetType().Name}] Peringatan: 'inventoryParent' belum dihubungkan untuk menampilkan item di UI inventory.", this.gameObject);
    }

    void Update()
    {
        CheckForNearbyObjects();
    }

    // --- PERUBAHAN UTAMA DI SINI ---
    void CheckForNearbyObjects()
    {
        nearbyPickableWorldItem = null; // Reset dulu
        bool foundPickableItemThisFrame = false;

        // Menggunakan Fisika 3D: OverlapSphere
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupDistance); 
        float closestPickableItemDist = float.MaxValue; 
        WorldItem candidateItem = null;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("item")) // Pastikan item punya tag "item"
            {
                // Pastikan WorldItem menggunakan versi 3D
                WorldItem worldItem = hitCollider.GetComponent<WorldItem>(); 
                if (worldItem != null && worldItem.bisaDiambil)
                {
                    // Menggunakan Vector3.Distance
                    float dist = Vector3.Distance(transform.position, hitCollider.transform.position); 
                    if (dist < closestPickableItemDist)
                    {
                        closestPickableItemDist = dist;
                        candidateItem = worldItem;
                    }
                }
            }
            // Deteksi TempatKhusus (Pastikan TempatKhusus juga 3D)
             if (hitCollider.CompareTag("TempatKhusus")) {
                 TempatKhusus tk = hitCollider.GetComponent<TempatKhusus>();
                 if (tk != null) {
                     tempatKhususDekat = tk;
                 }
              }
        }

        if (candidateItem != null)
        {
            nearbyPickableWorldItem = candidateItem;
            foundPickableItemThisFrame = true;
        }

        SetPickupButtonState(foundPickableItemThisFrame);
    }
    // --- AKHIR PERUBAHAN UTAMA ---

    void SetPickupButtonState(bool isPickable)
    {
        // Logika UI (Tidak Perlu Diubah)
        if (pickupButtonCanvasGroup != null)
        {
            if (isPickable)
            {
                pickupButtonCanvasGroup.alpha = 1f;
                pickupButtonCanvasGroup.interactable = true;
                pickupButtonCanvasGroup.blocksRaycasts = true;
            }
            else
            {
                pickupButtonCanvasGroup.alpha = disabledAlpha;
                pickupButtonCanvasGroup.interactable = false;
                pickupButtonCanvasGroup.blocksRaycasts = false;
            }
        }
        else if (pickupButtonUI != null) 
        {
            UnityEngine.UI.Button btn = pickupButtonUI.GetComponent<UnityEngine.UI.Button>();
            if (btn != null) btn.interactable = isPickable;
            else pickupButtonUI.SetActive(isPickable); 
        }
    }

    public void PickupItem()
    {
        // Validasi UI (Tidak Perlu Diubah)
        if (pickupButtonCanvasGroup != null && !pickupButtonCanvasGroup.interactable)
        {
            return;
        }
        if (pickupButtonUI != null && !pickupButtonUI.activeSelf && pickupButtonCanvasGroup == null) return;

        // Logika pengambilan item (Tidak Perlu Diubah, karena mengandalkan 'nearbyPickableWorldItem'
        // yang sudah di-cache oleh CheckForNearbyObjects versi 3D)
        if (nearbyPickableWorldItem != null && nearbyPickableWorldItem.bisaDiambil)
        {
            ItemData pickedItemData = nearbyPickableWorldItem.itemData;
            Debug.Log("Mengambil item: " + (pickedItemData?.itemName ?? "Item Tanpa Nama"));

            // --- PUTAR SUARA PICKUP (Tidak Perlu Diubah) ---
            if (pickupSound != null)
            {
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(pickupSound);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(pickupSound, nearbyPickableWorldItem.transform.position);
                }
            }
            // --- AKHIR BAGIAN SUARA ---

            if (pickedItemData != null)
            {
                if (pickedItemData.inventoryPrefab != null && inventoryParent != null)
                {
                    Instantiate(pickedItemData.inventoryPrefab, Vector3.zero, Quaternion.identity, inventoryParent); // Vector3.zero lebih aman
                }
            }

            Destroy(nearbyPickableWorldItem.gameObject);
            nearbyPickableWorldItem = null;
            SetPickupButtonState(false);
        }
    }

    // Versi DropItem dengan 2 argumen
    public void DropItem(ItemData itemToDrop, GameObject inventoryItemUIDestroyTarget)
    {
        if (itemToDrop == null) { return; }
        if (itemToDrop.worldPrefab == null) { return; }

        // --- PERUBAHAN: Gunakan Vector3 untuk posisi drop ---
        // Offset (0, -0.5, 0) mungkin lebih masuk akal di 3D, atau (0, 0, 1) [di depan player]
        // Sesuaikan offset ini sesuai kebutuhan game 3D Anda
        Vector3 dropPosition = transform.position + (Vector3.down * 0.5f); // Contoh: 0.5 unit di bawah player
        // Alternatif: di depan player
        // Vector3 dropPosition = transform.position + (transform.forward * 1.0f); 
        // --- AKHIR PERUBAHAN ---

        GameObject droppedItemObject = Instantiate(itemToDrop.worldPrefab, dropPosition, Quaternion.identity);

        if (droppedItemObject == null) { return; }
        droppedItemObject.tag = "item";

        // Pastikan prefab item memiliki WorldItem versi 3D
        WorldItem worldItemComponent = droppedItemObject.GetComponent<WorldItem>();
        if (worldItemComponent == null) { Destroy(droppedItemObject); return; }

        worldItemComponent.itemData = itemToDrop;
        worldItemComponent.bisaDiambil = true;

        // --- PUTAR SUARA DROP (Tidak Perlu Diubah, dropPosition sudah Vector3) ---
        if (dropSound != null)
        {
            AudioSource.PlayClipAtPoint(dropSound, dropPosition);
        }
        // --- AKHIR BAGIAN SUARA ---

        if (inventoryItemUIDestroyTarget != null)
        {
            Destroy(inventoryItemUIDestroyTarget);
        }
    }

    private void HandleItemReward(ItemData itemReward)
    {
        // Logika Reward (Tidak Perlu Diubah)
        if (itemReward == null) return;
        if (itemReward.inventoryPrefab != null && inventoryParent != null)
        {
            Instantiate(itemReward.inventoryPrefab, Vector3.zero, Quaternion.identity, inventoryParent); // Vector3.zero lebih aman
        }
    }

    public void ResetSpawnedItems()
    {
        // ... (Logika ini seharusnya tidak perlu diubah) ...
    }

    void OnDrawGizmosSelected()
    {
        // Gizmos (Tidak Perlu Diubah, DrawWireSphere berfungsi di 3D)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }
}