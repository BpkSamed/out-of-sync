using UnityEngine;

public class GameStartHandler : MonoBehaviour
{
    [Header("Referensi Manager")]
    public DialogueManager dialogueManager;
    public UIManager uiManager; 

    [Header("Pengaturan Dialog Rian (Percabangan)")]
    [Tooltip("ID Dialog Rian jika MentalState = 1 (Sopan)")]
    public string idJalurAman = "RIAN_AMAN"; 

    [Tooltip("ID Dialog Rian jika MentalState = 2 (Marah)")]
    public string idJalurBahaya = "RIAN_BAHAYA";

    [Tooltip("ID Default jika data tidak ditemukan (misal test langsung scene 2)")]
    public string idDefault = "RIAN_AMAN";

    void Awake()
    {
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (dialogueManager == null) dialogueManager = FindObjectOfType<DialogueManager>();
    }

    void Start()
    {
        if (uiManager != null) uiManager.SetGameplayUIActive(false);
    }

    void OnEnable()
    {
        SceneLightingFader.OnFadeInComplete += TriggerSmartDialogue;
    }

    void OnDisable()
    {
        SceneLightingFader.OnFadeInComplete -= TriggerSmartDialogue;
    }

    private void TriggerSmartDialogue()
    {
        string finalDialogueID = idDefault; // Anggap default dulu
        int nilaiMental = 0;

        // 1. CEK APAKAH STORY STATE MANAGER ADA?
        if (StoryStateManager.Instance == null)
        {
            Debug.LogError("[GameStartHandler] ERROR BESAR: StoryStateManager tidak ditemukan! Pastikan Anda menaruh prefab StoryStateManager di Scene awal (Scene Pak Budi).");
            // Terpaksa pakai default
        }
        else
        {
            // 2. AMBIL NILAI FLAG
            nilaiMental = StoryStateManager.Instance.GetFlag("MentalState");
            Debug.Log($"[GameStartHandler] Data diterima dari Scene sebelumnya. Nilai MentalState = {nilaiMental}");

            // 3. PILIH DIALOG BERDASARKAN NILAI
            if (nilaiMental == 1)
            {
                finalDialogueID = idJalurAman;
                Debug.Log("-> Memilih Jalur AMAN.");
            }
            else if (nilaiMental == 2)
            {
                finalDialogueID = idJalurBahaya;
                Debug.Log("-> Memilih Jalur BAHAYA.");
            }
            else
            {
                Debug.LogWarning($"-> Nilai MentalState adalah 0 atau tidak dikenal. Masuk ke DEFAULT ({idDefault}).");
                Debug.LogWarning("-> Cek: Apakah Anda salah ketik 'MentalState' di Inspector tombol pilihan Scene 1?");
            }
        }

        // 4. JALANKAN DIALOG
        if (dialogueManager != null)
        {
            dialogueManager.StartDialogueByID(finalDialogueID);
        }
    }
}