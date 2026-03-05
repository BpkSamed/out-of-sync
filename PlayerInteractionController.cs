// PlayerInteractionController.cs
using UnityEngine;
using UnityEngine.UI; // Untuk Button UI global

public class PlayerInteractionController : MonoBehaviour
{
    [Header("Tombol Interaksi UI Global")]
    [Tooltip("Hubungkan Tombol UI global untuk interaksi.")]
    public Button globalInteractionButton; // Tombol "E" atau tombol sentuh di layar
    [Tooltip("Tingkat transparansi tombol saat tidak ada interaksi.")]
    public float disabledButtonAlpha = 0.5f;
    private CanvasGroup interactionButtonCanvasGroup;

    // Menyimpan referensi ke DialogueTriggerArea yang sedang aktif/dimasuki player
    private DialogueTriggerArea currentActiveInteractionArea = null;

    void Start()
    {
        if (globalInteractionButton != null)
        {
            interactionButtonCanvasGroup = globalInteractionButton.GetComponent<CanvasGroup>();
            if (interactionButtonCanvasGroup == null) {
                Debug.LogError("Tombol Interaksi Global tidak punya CanvasGroup!", globalInteractionButton.gameObject);
            }
            globalInteractionButton.onClick.AddListener(OnGlobalInteractionButtonPressed);
            SetGlobalInteractionButtonState(false); // Awalnya tombol tidak aktif
        }
        else
        {
            Debug.LogError("Tombol Interaksi Global belum dihubungkan ke PlayerInteractionController!", this.gameObject);
        }
    }

    void OnDestroy()
    {
        if (globalInteractionButton != null)
        {
            globalInteractionButton.onClick.RemoveListener(OnGlobalInteractionButtonPressed);
        }
    }

    // Dipanggil oleh DialogueTriggerArea saat player masuk
    public void EnterInteractionArea(DialogueTriggerArea newArea)
    {
        // Jika sudah ada area aktif lain, dan ini area baru, prioritaskan yang baru
        // atau bisa juga menggunakan list jika ingin menangani beberapa area tumpang tindih
        currentActiveInteractionArea = newArea;
        SetGlobalInteractionButtonState(true); // Aktifkan tombol interaksi
        Debug.Log($"PlayerInteractionController: Masuk area '{newArea.gameObject.name}', Dialogue ID siap: '{newArea.dialogueID}'");
    }

    // Dipanggil oleh DialogueTriggerArea saat player keluar
    public void ExitInteractionArea(DialogueTriggerArea exitedArea)
    {
        // Hanya nonaktifkan tombol jika player keluar dari area yang sedang aktif
        if (currentActiveInteractionArea == exitedArea)
        {
            currentActiveInteractionArea = null;
            SetGlobalInteractionButtonState(false); // Nonaktifkan tombol interaksi
            Debug.Log($"PlayerInteractionController: Keluar dari area '{exitedArea.gameObject.name}'.");
        }
    }

    // Dipanggil saat tombol interaksi global ditekan
    private void OnGlobalInteractionButtonPressed()
    {
        if (currentActiveInteractionArea != null)
        {
            Debug.Log($"PlayerInteractionController: Tombol interaksi global ditekan. Memicu dialog untuk area '{currentActiveInteractionArea.gameObject.name}' dengan ID '{currentActiveInteractionArea.dialogueID}'.");
            currentActiveInteractionArea.TriggerAssociatedDialogue();
            // Setelah dialog dipicu, mungkin kita ingin tombolnya kembali non-interaktif
            // sampai player keluar dan masuk lagi atau interaksi selesai.
            // Atau UIManager akan menghandle visibility tombol ini jika ia bagian dari uiElementsToToggle.
            // SetGlobalInteractionButtonState(false); // Opsional: langsung nonaktifkan setelah diklik
        }
        else
        {
            Debug.LogWarning("PlayerInteractionController: Tombol interaksi global ditekan tapi tidak ada area interaksi aktif.");
        }
    }

    private void SetGlobalInteractionButtonState(bool active)
    {
        if (globalInteractionButton == null) return;

        if (!globalInteractionButton.gameObject.activeSelf) {
            globalInteractionButton.gameObject.SetActive(true); // Pastikan GameObject-nya aktif
        }

        if (interactionButtonCanvasGroup != null)
        {
            interactionButtonCanvasGroup.alpha = active ? 1f : disabledButtonAlpha;
            interactionButtonCanvasGroup.interactable = active;
            interactionButtonCanvasGroup.blocksRaycasts = active;
        }
        else // Fallback jika tidak ada CanvasGroup
        {
            globalInteractionButton.interactable = active;
        }
    }
}