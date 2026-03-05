using UnityEngine;
using TMPro; // Wajib untuk TextMeshPro

public class QuestUI : MonoBehaviour
{
    [Header("Satu-satunya Teks")]
    [Tooltip("Seret TextMeshProUGUI kamu ke sini.")]
    public TextMeshProUGUI singleQuestText; 

    void Start()
    {
        // Cek misi saat game baru mulai
        if (QuestManager.Instance != null) 
        {
            UpdateSimpleDisplay(QuestManager.Instance.GetActiveQuest());
        }
    }

    void OnEnable()
    {
        // Langganan Event: Jika ada quest mulai atau update, ganti teksnya
        QuestManager.OnQuestStarted += UpdateSimpleDisplay;
        QuestManager.OnQuestUpdated += UpdateSimpleDisplay;
    }

    void OnDisable()
    {
        // Berhenti langganan saat mati/pindah scene
        QuestManager.OnQuestStarted -= UpdateSimpleDisplay;
        QuestManager.OnQuestUpdated -= UpdateSimpleDisplay;
    }

    // Fungsi Update yang to-the-point
    void UpdateSimpleDisplay(QuestData quest)
    {
        if (singleQuestText == null) return;

        // Cek apakah ada quest yang aktif
        if (quest != null && quest.isRuntimeActive && !quest.isRuntimeCompleted)
        {
            // Cari tugas (objective) pertama yang belum selesai
            string currentTask = "";
            
            if (quest.objectives.Count > 0)
            {
                foreach(var obj in quest.objectives)
                {
                    if (!obj.isCompleted)
                    {
                        currentTask = obj.description;
                        break; // Ambil satu saja yang paling atas, lalu berhenti
                    }
                }
            }
            else
            {
                currentTask = quest.description; // Fallback jika tidak ada objective khusus
            }

            // TAMPILKAN TEKS: (Misal: "Buka Pintu Depan")
            singleQuestText.text = currentTask;
        }
        else
        {
            // Jika tidak ada quest aktif, kosongkan teks
            singleQuestText.text = ""; 
        }
    }
}