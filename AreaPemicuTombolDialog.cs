// AreaInteractionButton.cs (Sederhana: Aktif di Area, Langsung Dialog Saat Ditekan)
using UnityEngine;
using UnityEngine.UI; // Diperlukan untuk Button

[RequireComponent(typeof(Collider2D))] // Pastikan ada Collider2D di GameObject ini
public class AreaInteractionButton : MonoBehaviour
{
    [Header("Tombol UI yang Dikontrol")]
    [Tooltip("Hubungkan GameObject Tombol UI yang akan muncul/aktif.")]
    public GameObject interactionButtonObject;
    [Tooltip("Tingkat transparansi tombol saat tidak bisa digunakan (0.0 - 1.0).")]
    public float disabledButtonAlpha = 0.5f;

    [Header("Pengaturan Dialog")]
    [Tooltip("Hubungkan instance DialogueManager dari scene.")]
    public DialogueManager dialogueManager;
    [Tooltip("ID Dialog yang akan dimainkan saat tombol ini ditekan.")]
    public string dialogueIDToPlay;
    // Tidak ada lagi referensi ke TypingMode

    [Header("Pengaturan Pemicu")]
    [Tooltip("Tag GameObject yang bisa memicu aktifnya tombol (biasanya 'Player').")]
    public string triggeringTag = "Player";

    private CanvasGroup buttonCanvasGroup;
    private Button uiButton;
    // Variabel 'playerInRange' tidak lagi secara eksplisit dibutuhkan di sini
    // karena state tombol akan langsung dikontrol oleh OnTriggerEnter/Exit

    void Start()
    {
        // Validasi dan setup awal
        if (interactionButtonObject == null) {
            Debug.LogError($"AreaInteractionButton '{gameObject.name}': Interaction Button Object belum di-assign!", gameObject);
            enabled = false; return;
        }

        buttonCanvasGroup = interactionButtonObject.GetComponent<CanvasGroup>();
        if (buttonCanvasGroup == null) {
            Debug.LogError($"AreaInteractionButton '{gameObject.name}': Interaction Button Object ({interactionButtonObject.name}) tidak memiliki komponen CanvasGroup! Tambahkan.", interactionButtonObject);
        }

        uiButton = interactionButtonObject.GetComponent<Button>();
        if (uiButton != null) {
            uiButton.onClick.RemoveAllListeners();
            uiButton.onClick.AddListener(OnInteractionButtonPressed);
        } else {
            Debug.LogError($"AreaInteractionButton '{gameObject.name}': Interaction Button Object ({interactionButtonObject.name}) tidak memiliki komponen Button!", interactionButtonObject);
            enabled = false; return;
        }

        if (dialogueManager == null) {
            dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager == null) {
                Debug.LogError($"AreaInteractionButton '{gameObject.name}': DialogueManager belum dihubungkan atau tidak ditemukan di scene!", gameObject);
                if(uiButton != null) uiButton.interactable = false; // Nonaktifkan tombol jika DM tidak ada
                enabled = false; return;
            }
        }
        if (string.IsNullOrEmpty(dialogueIDToPlay)) {
            Debug.LogError($"AreaInteractionButton '{gameObject.name}': 'Dialogue ID To Play' belum diisi!", gameObject);
            if(uiButton != null) uiButton.interactable = false; // Nonaktifkan tombol jika ID dialog kosong
            enabled = false; return;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col == null) { Debug.LogError($"AreaInteractionButton '{gameObject.name}' tidak memiliki Collider2D.", gameObject); enabled = false; return; }
        if (!col.isTrigger) { col.isTrigger = true; }

        // Pastikan GameObject tombolnya sendiri selalu aktif, state diatur CanvasGroup
        interactionButtonObject.SetActive(true);
        // Atur keadaan awal tombol menjadi nonaktif (karena player belum tentu di dalam trigger)
        SetButtonActiveState(false);
    }

    void OnDestroy()
    {
        if (uiButton != null) {
            uiButton.onClick.RemoveListener(OnInteractionButtonPressed);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(triggeringTag))
        {
            SetButtonActiveState(true); // Aktifkan tombol saat player masuk
            Debug.Log($"AreaInteractionButton: Player MASUK area '{dialogueIDToPlay}'. Tombol '{interactionButtonObject.name}' diaktifkan.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(triggeringTag))
        {
            SetButtonActiveState(false); // Nonaktifkan tombol saat player keluar
            Debug.Log($"AreaInteractionButton: Player KELUAR area. Tombol '{interactionButtonObject.name}' dinonaktifkan.");
        }
    }

    // Mengatur tampilan dan interaksi tombol
    private void SetButtonActiveState(bool isActiveAndInteractable)
    {
        if (interactionButtonObject == null) return;

        if (buttonCanvasGroup != null)
        {
            if (isActiveAndInteractable)
            {
                buttonCanvasGroup.alpha = 1f;
                buttonCanvasGroup.interactable = true;
                buttonCanvasGroup.blocksRaycasts = true;
            }
            else
            {
                buttonCanvasGroup.alpha = disabledButtonAlpha;
                buttonCanvasGroup.interactable = false;
                buttonCanvasGroup.blocksRaycasts = false;
            }
        }
        else if (uiButton != null) // Fallback jika tidak ada CanvasGroup
        {
            uiButton.interactable = isActiveAndInteractable;
            Image btnImage = uiButton.GetComponent<Image>(); // Untuk mengatur alpha manual jika perlu
            if (btnImage != null)
            {
                Color c = btnImage.color;
                c.a = isActiveAndInteractable ? 1f : disabledButtonAlpha;
                btnImage.color = c;
            }
        }
        else
        { // Jika tidak ada CanvasGroup dan tidak ada Button component (seharusnya tidak terjadi)
            interactionButtonObject.SetActive(isActiveAndInteractable);
        }
       
    }

    // Fungsi ini akan dipanggil saat tombol UI yang terhubung di klik
    public void OnInteractionButtonPressed()
    {
        // Pengecekan apakah tombol memang bisa diklik (sudah ditangani oleh CanvasGroup.interactable)
        // Tidak perlu cek 'playerInRange' lagi di sini karena tombol hanya akan interactable jika player di dalam range.

        if (dialogueManager != null && !string.IsNullOrEmpty(dialogueIDToPlay))
        {
            Debug.Log($"AreaInteractionButton: Tombol '{interactionButtonObject.name}' DITEKAN, memulai dialog ID '{dialogueIDToPlay}'.");

            // Aktifkan panel dialog milik DialogueManager sebelum memulai dialog.
            // Ini penting jika DialogueManager tidak otomatis mengaktifkan panelnya.
            if (dialogueManager.dialoguePanel != null && !dialogueManager.dialoguePanel.activeSelf)
            {
                dialogueManager.dialoguePanel.SetActive(true);
            }

            // Panggil StartDialogueByID HANYA dengan satu argumen (ID Dialog)
            Debug.Log($"AreaInteractionButton '{gameObject.name}': Memicu dialog dengan ID: '{dialogueIDToPlay}'");
            dialogueManager.StartDialogueByID(dialogueIDToPlay);

            // Setelah dialog dimulai, UIManager (jika ada) akan menyembunyikan tombol ini
            // jika tombol ini termasuk dalam uiElementsToToggle-nya.
            // Jika tidak, dan kamu ingin tombol ini langsung nonaktif setelah diklik sekali:
            // SetButtonActiveState(false);
        }
        else
        {
            Debug.LogError($"AreaInteractionButton pada '{gameObject.name}': Tidak bisa memulai dialog. DialogueManager atau DialogueID tidak valid.");
        }
    }
}