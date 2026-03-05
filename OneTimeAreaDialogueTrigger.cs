// OneTimeAreaDialogueTrigger.cs (Revisi - Tanpa TypingMode)
using UnityEngine;
using System.Collections.Generic;

public class OneTimeAreaDialogueTrigger : MonoBehaviour
{
    [Header("Pengaturan Dialog")]
    [Tooltip("Hubungkan instance DialogueManager dari scene.")]
    public DialogueManager dialogueManager;
    [Tooltip("ID Dialog yang akan dimainkan saat player pertama kali masuk area ini.")]
    public string dialogueIDToPlay;
    // HAPUS field typingMode karena DialogueManager saat ini tidak mendukungnya
    // public DialogueManager.TypingMode typingMode = DialogueManager.TypingMode.PerCharacter;

    [Header("Status Pemicu (Unik per Area)")]
    [Tooltip("ID unik untuk area pemicu ini. Pastikan ID ini BERBEDA untuk setiap OneTimeAreaDialogueTrigger di game-mu jika ada banyak.")]
    public string triggerAreaID;

    private static HashSet<string> triggeredAreasInThisSession = new HashSet<string>();
    private bool thisAreaHasBeenTriggered = false;

    void Awake()
    {
        if (dialogueManager == null) {
            dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager == null) {
                Debug.LogError($"OneTimeAreaDialogueTrigger '{gameObject.name}': DialogueManager tidak ditemukan di scene!", gameObject);
                enabled = false; return;
            }
        }
        if (string.IsNullOrEmpty(dialogueIDToPlay)) {
            Debug.LogError($"OneTimeAreaDialogueTrigger '{gameObject.name}': Dialogue ID To Play belum diisi!", gameObject);
            enabled = false; return;
        }
        if (string.IsNullOrEmpty(triggerAreaID)) {
            Debug.LogError($"OneTimeAreaDialogueTrigger '{gameObject.name}': Trigger Area ID belum diisi! Ini penting untuk memastikan dialog hanya muncul sekali.", gameObject);
            enabled = false; return;
        }

        if (triggeredAreasInThisSession.Contains(triggerAreaID)) {
            thisAreaHasBeenTriggered = true;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col == null) {
            Debug.LogError($"OneTimeAreaDialogueTrigger '{gameObject.name}' tidak memiliki Collider2D.", gameObject);
            enabled = false; return;
        }
        if (!col.isTrigger) {
            Debug.LogWarning($"OneTimeAreaDialogueTrigger '{gameObject.name}': Collider2D sebaiknya di-set sebagai 'Is Trigger = true'. Mengubah otomatis.", gameObject);
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!thisAreaHasBeenTriggered && other.CompareTag("Player"))
        {
            Debug.Log($"Player memasuki area pemicu dialog '{triggerAreaID}' untuk pertama kali.");

            thisAreaHasBeenTriggered = true;
            triggeredAreasInThisSession.Add(triggerAreaID);

            if (dialogueManager.dialoguePanel != null) { // Menggunakan dialoguePanel langsung dari DialogueManager
                dialogueManager.dialoguePanel.SetActive(true);
            } else {
                 Debug.LogWarning($"OneTimeAreaDialogueTrigger: DialoguePanel di DialogueManager '{dialogueManager.gameObject.name}' adalah null. Dialog mungkin tidak terlihat.");
            }

            // Panggil StartDialogueByID TANPA parameter mode ketik
            dialogueManager.StartDialogueByID(dialogueIDToPlay);

            // gameObject.SetActive(false); // Opsional
        }
        else if (thisAreaHasBeenTriggered && other.CompareTag("Player"))
        {
            // Debug.Log($"Player memasuki area '{triggerAreaID}', tapi dialog sudah pernah muncul.");
        }
    }

    public static void ResetAllTriggeredAreasStatus()
    {
        triggeredAreasInThisSession.Clear();
        Debug.Log("Status semua area pemicu dialog sekali pakai telah direset.");
    }
}