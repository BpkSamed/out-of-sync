// AreaButtonObjectRemover.cs (Versi 3D)
using UnityEngine;
using UnityEngine.UI; // Diperlukan untuk Button
using System.Collections.Generic; // Diperlukan untuk HashSet

[RequireComponent(typeof(Collider))] // DIUBAH: dari Collider2D ke Collider (3D)
public class AreaButtonObjectRemover : MonoBehaviour
{
    [Header("Tombol UI yang Dikontrol")]
    [Tooltip("Hubungkan GameObject Tombol UI yang akan muncul/aktif. HARUS memiliki komponen CanvasGroup.")]
    public GameObject interactionButtonObject;
    [Tooltip("Tingkat transparansi tombol saat tidak bisa digunakan (0.0 - 1.0).")]
    public float disabledButtonAlpha = 0.5f;

    [Header("GameObject Target")]
    [Tooltip("GameObject yang akan dihilangkan/dinonaktifkan saat tombol ditekan.")]
    public GameObject objectToRemove;

    [Header("Status Permanen (Per Sesi Game)")]
    [Tooltip("ID Unik untuk area/aksi ini. Harus unik jika ada banyak area seperti ini agar statusnya tidak tercampur.")]
    public string uniqueActionID; 

    [Header("Pengaturan Pemicu")]
    [Tooltip("Tag GameObject yang bisa memicu aktifnya tombol (biasanya 'Player').")]
    public string triggeringTag = "Player";

    [Header("SFX (Opsional)")]
    [Tooltip("Suara yang diputar saat objek berhasil dihilangkan.")]
    public AudioClip actionSuccessSound;
    [Range(0f, 1f)]
    public float sfxVolume = 0.7f;

    private CanvasGroup buttonCanvasGroup;
    private Button uiButton; 
    private bool playerInRange = false; 

    // (Logika HashSet ini tidak berubah, 100% sama)
    private static HashSet<string> completedActionIDsInThisSession = new HashSet<string>();
    private bool thisSpecificActionHasBeenDone = false; 

    void Start()
    {
        // (Validasi awal tidak berubah)
        if (interactionButtonObject == null) { Debug.LogError($"[{this.GetType().Name}] '{gameObject.name}': Interaction Button Object belum di-assign!", gameObject); enabled = false; return; }
        if (objectToRemove == null) { Debug.LogError($"[{this.GetType().Name}] '{gameObject.name}': Object To Remove belum di-assign!", gameObject); enabled = false; return; }
        if (string.IsNullOrEmpty(uniqueActionID)) { Debug.LogError($"[{this.GetType().Name}] '{gameObject.name}': Unique Action ID belum di-assign!", gameObject); enabled = false; return; }

        // (Setup UI tidak berubah)
        buttonCanvasGroup = interactionButtonObject.GetComponent<CanvasGroup>();
        if (buttonCanvasGroup == null) { Debug.LogError($"[{this.GetType().Name}] '{gameObject.name}': Interaction Button Object tidak punya CanvasGroup!", interactionButtonObject); }

        uiButton = interactionButtonObject.GetComponent<Button>();
        if (uiButton != null)
        {
            uiButton.onClick.RemoveAllListeners();
            uiButton.onClick.AddListener(OnInteractionButtonPressed);
        }
        else { Debug.LogError($"[{this.GetType().Name}] '{gameObject.name}': Interaction Button Object tidak punya komponen Button!", interactionButtonObject); enabled = false; return; }

        // --- PERUBAHAN DI SINI ---
        Collider col = GetComponent<Collider>(); // DIUBAH: dari Collider2D
        if (col == null) { Debug.LogError($"[{this.GetType().Name}] '{gameObject.name}' tidak punya Collider (3D)!", gameObject); enabled = false; return; }
        if (!col.isTrigger) { col.isTrigger = true; } // Pastikan ini trigger
        // --- AKHIR PERUBAHAN ---

        interactionButtonObject.SetActive(true);

        // (Logika cek status Start tidak berubah)
        if (completedActionIDsInThisSession.Contains(uniqueActionID))
        {
            thisSpecificActionHasBeenDone = true;
            if (objectToRemove != null) objectToRemove.SetActive(false);
            SetButtonState(false);
            Debug.Log($"AreaButtonObjectRemover [{uniqueActionID}]: Aksi sudah pernah dilakukan di sesi ini (saat Start).");
        }
        else
        {
            if (objectToRemove != null) objectToRemove.SetActive(true); 
            SetButtonState(false);
        }
    }

    void OnDestroy()
    {
        // (Tidak berubah)
        if (uiButton != null)
        {
            uiButton.onClick.RemoveListener(OnInteractionButtonPressed);
        }
    }

    // --- PERUBAHAN DI SINI ---
    private void OnTriggerEnter(Collider other) // DIUBAH: dari OnTriggerEnter2D(Collider2D other)
    {
        if (thisSpecificActionHasBeenDone) return;

        if (other.CompareTag(triggeringTag))
        {
            playerInRange = true;
            SetButtonState(true); 
            Debug.Log($"AreaButtonObjectRemover: Player MASUK area '{gameObject.name}'. Tombol diaktifkan.");
        }
    }
    // --- AKHIR PERUBAHAN ---

    // --- PERUBAHAN DI SINI ---
    private void OnTriggerExit(Collider other) // DIUBAH: dari OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(triggeringTag))
        {
            playerInRange = false;
            if (!thisSpecificActionHasBeenDone)
            {
                SetButtonState(false);
            }
            Debug.Log($"AreaButtonObjectRemover: Player KELUAR area '{gameObject.name}'.");
        }
    }
    // --- AKHIR PERUBAHAN ---

    // (Fungsi SetButtonState tidak berubah, murni UI)
    private void SetButtonState(bool isActiveAndInteractable)
    {
        if (interactionButtonObject == null) return;
        if (!interactionButtonObject.activeSelf) interactionButtonObject.SetActive(true);

        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.alpha = isActiveAndInteractable ? 1f : disabledButtonAlpha;
            buttonCanvasGroup.interactable = isActiveAndInteractable;
            buttonCanvasGroup.blocksRaycasts = isActiveAndInteractable;
        }
        else if (uiButton != null) 
        {
            uiButton.interactable = isActiveAndInteractable;
            Image btnImage = uiButton.GetComponent<Image>();
            if (btnImage != null)
            {
                Color c = btnImage.color;
                c.a = isActiveAndInteractable ? 1f : disabledButtonAlpha;
                btnImage.color = c;
            }
        }
        else 
        {
            interactionButtonObject.SetActive(isActiveAndInteractable);
        }
    }

    // (Fungsi OnInteractionButtonPressed tidak berubah, murni logika)
    public void OnInteractionButtonPressed()
    {
        if (thisSpecificActionHasBeenDone)
        {
            Debug.Log($"Tombol interaksi '{gameObject.name}' ditekan, tapi aksi untuk [{uniqueActionID}] sudah selesai.");
            return; 
        }

        if (buttonCanvasGroup != null && !buttonCanvasGroup.interactable) return;
        if (uiButton != null && !uiButton.interactable) return;


        Debug.Log($"AreaButtonObjectRemover [{uniqueActionID}]: Tombol ditekan. Menghilangkan '{objectToRemove.name}'.");

        if (objectToRemove != null)
        {
            objectToRemove.SetActive(false); 
        }

        if (SfxToggleButton.isSfxMuted == false && actionSuccessSound != null)
        {
            AudioSource.PlayClipAtPoint(actionSuccessSound, transform.position, sfxVolume);
        }

        thisSpecificActionHasBeenDone = true; 
        if (!string.IsNullOrEmpty(uniqueActionID))
        {
            completedActionIDsInThisSession.Add(uniqueActionID); 
        }

        SetButtonState(false); 
    }

    // (Fungsi ResetAllCompletedActions tidak berubah, murni logika)
    public static void ResetAllCompletedActions()
    {
        completedActionIDsInThisSession.Clear();
        Debug.Log("Status semua aksi di AreaButtonObjectRemover telah direset.");
    }
}