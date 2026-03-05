// DialogueTriggerArea.cs (Versi 3D)
using UnityEngine;

// DIUBAH: Menggunakan Collider 3D
[RequireComponent(typeof(Collider))] 
public class DialogueTriggerArea : MonoBehaviour
{
    [Header("Pengaturan Dialog")]
    [Tooltip("ID Dialog yang akan terkait dengan area ini.")]
    public string dialogueID; 

    [Header("Pengaturan Pemicu")]
    [Tooltip("Tag GameObject yang bisa memicu (biasanya 'Player').")]
    public string triggeringTag = "Player";

    private PlayerInteractionController playerInteraction; 

    void Awake()
    {
        // DIUBAH: GetComponent<Collider>()
        Collider col = GetComponent<Collider>(); 
        if (col == null) { 
            Debug.LogError($"DialogueTriggerArea '{gameObject.name}' tidak punya Collider (3D)!", gameObject); 
            enabled = false; 
            return; 
        }
        
        // Pastikan IsTrigger aktif
        if (!col.isTrigger) { col.isTrigger = true; }

        if (string.IsNullOrEmpty(dialogueID)) {
            Debug.LogError($"DialogueTriggerArea '{gameObject.name}' tidak punya Dialogue ID!", gameObject);
            enabled = false; return;
        }
    }

    // DIUBAH: OnTriggerEnter(Collider other)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggeringTag))
        {
            // Mencari script PlayerInteractionController pada objek yang masuk trigger
            playerInteraction = other.GetComponent<PlayerInteractionController>(); 
            if (playerInteraction != null)
            {
                // Pastikan script PlayerInteractionController Anda mendukung parameter ini
                playerInteraction.EnterInteractionArea(this); 
                Debug.Log($"Player masuk area '{gameObject.name}', mendaftarkan Dialogue ID: '{dialogueID}' ke Player.");
            }
        }
    }

    // DIUBAH: OnTriggerExit(Collider other)
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(triggeringTag))
        {
            // Cek apakah yang keluar adalah player yang sedang terdaftar
            PlayerInteractionController exitingPlayer = other.GetComponent<PlayerInteractionController>();
            
            if (exitingPlayer != null && exitingPlayer == playerInteraction)
            {
                playerInteraction.ExitInteractionArea(this); 
                Debug.Log($"Player keluar area '{gameObject.name}', menghapus Dialogue ID: '{dialogueID}' dari Player.");
                playerInteraction = null; // Reset referensi
            }
        }
    }

    public void TriggerAssociatedDialogue()
    {
        // Logika ini tidak berubah karena tidak berhubungan dengan fisika
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>(); 
        if (dialogueManager != null && !string.IsNullOrEmpty(dialogueID))
        {
            Debug.Log($"DialogueTriggerArea '{gameObject.name}': Memicu dialog dengan ID: '{dialogueID}'");
            if (dialogueManager.dialoguePanel != null && !dialogueManager.dialoguePanel.activeSelf)
            {
                dialogueManager.dialoguePanel.SetActive(true);
            }
            dialogueManager.StartDialogueByID(dialogueID); 
        }
        else
        {
            Debug.LogError($"Gagal memicu dialog dari '{gameObject.name}'. DialogueManager atau DialogueID tidak valid.");
        }
    }
}