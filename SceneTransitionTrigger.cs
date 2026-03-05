// SceneTransitionTrigger.cs (Versi 3D)
using UnityEngine;
using UnityEngine.SceneManagement; // Penting untuk menggunakan SceneManager

// DIUBAH: dari Collider2D ke Collider (3D)
[RequireComponent(typeof(Collider))] 
public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Pengaturan Transisi Scene")]
    [Tooltip("Nama scene yang akan dimuat saat Player bersentuhan dengan trigger ini.")]
    public string sceneNameToLoad;

    [Tooltip("Tag GameObject yang bisa memicu transisi (biasanya 'Player').")]
    public string triggeringTag = "Player"; 

    private bool playerIsInTrigger = false; 

    void Awake()
    {
        // --- PERUBAHAN DI SINI ---
        Collider col = GetComponent<Collider>(); // DIUBAH: dari Collider2D
        if (col == null)
        {
            // Pesan error diperbarui
            Debug.LogError($"SceneTransitionTrigger pada '{gameObject.name}' tidak memiliki Collider (3D)!", gameObject);
            enabled = false; 
            return;
        }
        if (!col.isTrigger)
        {
            // Pesan warning diperbarui
            Debug.LogWarning($"SceneTransitionTrigger pada '{gameObject.name}': Collider (3D) sebaiknya di-set sebagai 'Is Trigger = true' untuk transisi otomatis saat sentuhan. Mengubah otomatis.", gameObject);
            col.isTrigger = true;
        }
        // --- AKHIR PERUBAHAN ---

        // Validasi nama scene (Tidak berubah)
        if (string.IsNullOrEmpty(sceneNameToLoad))
        {
            Debug.LogError($"SceneTransitionTrigger pada '{gameObject.name}': 'Scene Name To Load' belum diisi!", gameObject);
            enabled = false; 
        }
    }

    // --- PERUBAHAN DI SINI ---
    // DIUBAH: dari OnTriggerEnter2D(Collider2D other) ke OnTriggerEnter(Collider other)
    private void OnTriggerEnter(Collider other) 
    {
        // Logika di dalamnya 100% sama
        if (other.CompareTag(triggeringTag))
        {
            Debug.Log($"SceneTransitionTrigger: '{other.gameObject.name}' (Tag: {triggeringTag}) memasuki area transisi ke scene '{sceneNameToLoad}'.");
            playerIsInTrigger = true; 
            LoadTargetScene(); // Langsung pindah scene saat bersentuhan
        }
    }
    // --- AKHIR PERUBAHAN ---


    // (Opsional) Jika kamu ingin player harus menekan tombol saat berada di area trigger:
    /*
    // DIUBAH: dari OnTriggerExit2D ke OnTriggerExit
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(triggeringTag))
        {
            playerIsInTrigger = false;
            Debug.Log($"SceneTransitionTrigger: '{other.gameObject.name}' keluar dari area transisi.");
            // Sembunyikan UI prompt interaksi jika ada
        }
    }

    // Update tidak perlu diubah
    void Update()
    {
        if (playerIsInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"Tombol interaksi ditekan untuk pindah ke scene '{sceneNameToLoad}'.");
            LoadTargetScene();
        }
    }
    */

    // Fungsi ini tidak perlu diubah sama sekali
    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(sceneNameToLoad))
        {
            Debug.LogError($"SceneTransitionTrigger pada '{gameObject.name}': Tidak bisa memuat scene karena 'Scene Name To Load' kosong!");
            return;
        }

        Debug.Log($"SceneTransitionTrigger: Memuat scene '{sceneNameToLoad}'...");
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneNameToLoad);
    }
}