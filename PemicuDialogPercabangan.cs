using UnityEngine;

// Script ini WAJIB dipasang berdampingan dengan DialogueTriggerArea
[RequireComponent(typeof(DialogueTriggerArea))]
public class PemicuDialogPercabangan : MonoBehaviour
{
    [Header("Konfigurasi Dialog Rian")]
    [Tooltip("ID Dialog jika MentalState = 1 (Sopan)")]
    public string idJalurAman = "RIAN_AMAN"; 

    [Tooltip("ID Dialog jika MentalState = 2 (Marah)")]
    public string idJalurBahaya = "RIAN_BAHAYA";

    [Tooltip("ID Default jika data tidak ditemukan (misal MentalState 0)")]
    public string idDefault = "RIAN_AMAN";

    private DialogueTriggerArea triggerArea;

    void Awake()
    {
        // Ambil komponen DialogueTriggerArea di object yang sama
        triggerArea = GetComponent<DialogueTriggerArea>();
    }

    void Start()
    {
        UpdateDialogueID();
    }

    // Dipanggil saat object aktif atau pemain mendekat (bisa dipanggil di OnTriggerEnter juga jika mau lebih dinamis)
    void OnEnable()
    {
        UpdateDialogueID();
    }

    private void UpdateDialogueID()
    {
        if (triggerArea == null) return;

        string finalID = idDefault;
        int nilaiMental = 0;

        // 1. Cek StoryStateManager
        if (StoryStateManager.Instance != null)
        {
            nilaiMental = StoryStateManager.Instance.GetFlag("MentalState");
            Debug.Log($"[PemicuDialogPercabangan] MentalState terdeteksi: {nilaiMental}");

            // 2. Tentukan ID berdasarkan nilai
            if (nilaiMental == 1)
            {
                finalID = idJalurAman;
            }
            else if (nilaiMental == 2)
            {
                finalID = idJalurBahaya;
            }
            else
            {
                // Jika 0 atau nilai lain, pakai default
                finalID = idDefault;
            }
        }
        else
        {
            Debug.LogWarning("[PemicuDialogPercabangan] StoryStateManager tidak ditemukan! Menggunakan ID Default.");
        }

        // 3. TIMPA ID di DialogueTriggerArea
        // Ini adalah kuncinya: Kita ubah ID di trigger area sebelum pemain menekannya.
        triggerArea.dialogueID = finalID;
        
        Debug.Log($"[PemicuDialogPercabangan] ID Dialog pada area ini di-set ke: {finalID}");
    }
}