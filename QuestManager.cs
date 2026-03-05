// QuestManager.cs
using UnityEngine;
using System.Collections.Generic;
using System; // Diperlukan untuk System.Action (event)

public class QuestManager : MonoBehaviour
{
    // --- Singleton Pattern Sederhana ---
    private static QuestManager _instance;
    public static QuestManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<QuestManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("QuestManager_AutoCreated");
                    _instance = go.AddComponent<QuestManager>();
                    Debug.LogWarning("QuestManager instance tidak ditemukan di scene, dibuat secara otomatis.");
                }
            }
            return _instance;
        }
    }
    // ---------------------------------

    [Header("Database Misi")]
    [Tooltip("Masukkan semua aset QuestData yang ada di game ke sini.")]
    public List<QuestData> allQuestsInGame; // Daftar semua QuestData yang akan digunakan

    [Header("Status Misi Saat Ini")]
    [Tooltip("Misi yang sedang aktif dijalankan oleh pemain. Diisi otomatis oleh sistem.")]
    public QuestData currentActiveQuest; // Hanya untuk inspeksi, dikelola oleh script

    // Menyimpan ID quest yang sudah selesai di sesi ini untuk mencegah pengulangan
    // (static agar bertahan antar load scene, tapi akan reset saat game ditutup)
    public static HashSet<string> completedQuestIDsThisSession = new HashSet<string>();

    // --- Events untuk UI atau sistem lain ---
    public static event Action<QuestData> OnQuestStarted;       // Saat quest baru dimulai/diaktifkan
    public static event Action<QuestData> OnQuestUpdated;       // Saat ada progres di objective quest aktif
    public static event Action<QuestData> OnQuestCompleted;     // Saat semua objective quest aktif selesai (sebelum UI "Misi Selesai" hilang)
    public static event Action<QuestData> OnQuestTurnedIn;      // Setelah "Misi Selesai" UI di-acknowledge dan quest benar-benar final

    void Awake()
    {
        // Penegakan Singleton
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Instance QuestManager lain sudah ada. Menghancurkan yang ini.");
            Destroy(gameObject);
            return;
        }
        _instance = this;
        // DontDestroyOnLoad(gameObject); // Aktifkan jika QuestManager ingin persisten antar scene

        // Reset status runtime quest saat game dimulai (penting jika QuestData adalah ScriptableObject)
        // dan untuk memastikan status bersih di awal sesi.
        InitializeQuests();
        Debug.Log("QuestManager: Awake selesai, semua quest diinisialisasi.");
    }

    void InitializeQuests()
    {
        if (allQuestsInGame == null) {
            allQuestsInGame = new List<QuestData>(); // Inisialisasi jika null
            Debug.LogWarning("QuestManager: List 'allQuestsInGame' belum diisi di Inspector.");
        }

        foreach (QuestData quest in allQuestsInGame)
        {
            if (quest == null) continue;

            quest.isRuntimeActive = false;
            quest.isRuntimeCompleted = completedQuestIDsThisSession.Contains(quest.questID);
            quest.ResetObjectivesStatus(); // Pastikan semua objective juga direset

            if (quest.isRuntimeCompleted)
            {
                // Jika quest sudah tercatat selesai di sesi ini, pastikan semua objective-nya juga ditandai selesai
                foreach (QuestObjective obj in quest.objectives)
                {
                    obj.isCompleted = true;
                }
            }
        }
    }

    // Fungsi untuk memulai/mengaktifkan sebuah misi
    public void StartQuest(string questID)
    {
        if (string.IsNullOrEmpty(questID)) {
            Debug.LogWarning("QuestManager: Mencoba memulai quest dengan ID kosong.");
            return;
        }

        // Cek apakah quest sudah pernah selesai di sesi ini
        if (completedQuestIDsThisSession.Contains(questID))
        {
            Debug.Log($"QuestManager: Misi '{questID}' sudah pernah selesai di sesi ini.");
            QuestData previouslyCompletedQuest = FindQuestByID(questID);
            if (previouslyCompletedQuest != null && !string.IsNullOrEmpty(previouslyCompletedQuest.nextQuestID))
            {
                // Jika quest sudah selesai dan punya quest lanjutan, coba mulai quest lanjutannya
                // Ini untuk mencegah stuck jika quest awal sudah selesai tapi player kembali ke pemicunya.
                // Hati-hati jika ini menyebabkan loop tak terbatas jika nextQuestID juga sudah selesai.
                // Untuk sekarang, kita tidak otomatis start next, biarkan pemicu lain yang handle.
                // Debug.Log($"Mencoba memulai next quest: {previouslyCompletedQuest.nextQuestID} karena {questID} sudah selesai.");
                // StartQuest(previouslyCompletedQuest.nextQuestID);
            }
            return;
        }

        QuestData questToStart = FindQuestByID(questID);
        if (questToStart != null)
        {
            if (currentActiveQuest != null && currentActiveQuest.questID == questID && currentActiveQuest.isRuntimeActive) {
                Debug.LogWarning($"QuestManager: Misi '{questID}' sudah aktif. Tidak memulai ulang.");
                OnQuestUpdated?.Invoke(currentActiveQuest); // Panggil update UI jika diperlukan
                return;
            }

            // Jika ada quest lain yang aktif dan belum selesai, mungkin beri peringatan atau batalkan quest lama.
            // Untuk sekarang, kita akan menimpa quest aktif jika ada yang baru dimulai.
            if (currentActiveQuest != null && currentActiveQuest != questToStart && currentActiveQuest.isRuntimeActive && !currentActiveQuest.isRuntimeCompleted)
            {
                Debug.LogWarning($"QuestManager: Memulai quest '{questID}' menimpa quest aktif sebelumnya '{currentActiveQuest.questID}'.");
                currentActiveQuest.isRuntimeActive = false; // Nonaktifkan quest lama
            }

            currentActiveQuest = questToStart;
            currentActiveQuest.isRuntimeActive = true;
            currentActiveQuest.isRuntimeCompleted = false; // Pastikan status selesai direset jika quest bisa diulang (meskipun kita cegah dengan completedQuestIDsThisSession)
            currentActiveQuest.ResetObjectivesStatus();

            Debug.Log($"Misi DIMULAI: [{currentActiveQuest.questID}] {currentActiveQuest.title}");
            OnQuestStarted?.Invoke(currentActiveQuest);
        }
        else
        {
            Debug.LogWarning($"QuestManager: Quest dengan ID '{questID}' tidak ditemukan di database.");
        }
    }

    // Fungsi untuk memberi tahu QuestManager bahwa sebuah tujuan mungkin tercapai
    public void AdvanceObjective(ObjectiveType type, string targetID)
    {
        if (currentActiveQuest == null || !currentActiveQuest.isRuntimeActive || currentActiveQuest.isRuntimeCompleted)
        {
            // Debug.Log("QuestManager: Tidak ada quest aktif atau quest sudah selesai, advance objective diabaikan.");
            return;
        }

        bool objectiveAdvancedThisCall = false;
        foreach (QuestObjective obj in currentActiveQuest.objectives)
        {
            if (!obj.isCompleted && obj.type == type && obj.targetID == targetID)
            {
                obj.isCompleted = true;
                objectiveAdvancedThisCall = true;
                Debug.Log($"QuestManager: Objective SELESAI untuk Quest '{currentActiveQuest.questID}' -> Objective '{obj.description}' (Target: {targetID})");
                // Bisa tambahkan feedback spesifik per objective di sini jika perlu
                break; // Asumsi satu aksi hanya menyelesaikan satu objective yang cocok
            }
        }

        if (objectiveAdvancedThisCall)
        {
            Debug.Log($"QuestManager: Memanggil OnQuestUpdated untuk Quest '{currentActiveQuest.questID}'.");
            OnQuestUpdated?.Invoke(currentActiveQuest);
            CheckForQuestCompletion();
        }
    }

    private void CheckForQuestCompletion()
    {
        if (currentActiveQuest == null || !currentActiveQuest.isRuntimeActive || currentActiveQuest.isRuntimeCompleted) return;

        if (currentActiveQuest.AreAllObjectivesCompleted())
        {
            Debug.Log($"QuestManager: Semua objective untuk misi [{currentActiveQuest.questID}] SELESAI!");
            currentActiveQuest.isRuntimeCompleted = true; // Tandai selesai secara logika
            // currentActiveQuest.isRuntimeActive = false; // Jangan set false di sini dulu, biarkan UI tampilkan "Misi Selesai"

            if (!completedQuestIDsThisSession.Contains(currentActiveQuest.questID)) {
                 completedQuestIDsThisSession.Add(currentActiveQuest.questID);
            }

            Debug.Log($"QuestManager: Memanggil OnQuestCompleted untuk Quest '{currentActiveQuest.questID}'.");
            OnQuestCompleted?.Invoke(currentActiveQuest); // Kirim event untuk UI "Misi Selesai"
        }
    }

    // Fungsi ini dipanggil oleh UI setelah pesan "Misi Selesai" ditampilkan dan di-acknowledge player (atau timer selesai)
    public void FinalizeCompletedQuestAndContinue()
    {
        if (currentActiveQuest == null || !currentActiveQuest.isRuntimeCompleted) {
            Debug.LogWarning("QuestManager: FinalizeCompletedQuestAndContinue dipanggil saat tidak ada quest yang baru selesai atau quest aktif null.");
            // Jika currentActiveQuest sudah null karena next quest langsung dimulai oleh event lain, ini bisa terjadi.
            // Atau jika player klik tombol "Lanjut" di UI Misi Selesai padahal tidak ada quest yg baru selesai.
            // Kita bisa coba cari quest terakhir yang completed tapi belum turned-in jika sistemnya kompleks.
            // Untuk sekarang, kita asumsikan ini dipanggil dengan benar.
            // Update UI untuk membersihkan tampilan jika tidak ada quest baru.
            OnQuestTurnedIn?.Invoke(null); // Kirim null jika tidak ada quest spesifik untuk di-turn-in
            return;
        }

        Debug.Log($"QuestManager: Misi [{currentActiveQuest.questID}] telah di-acknowledge.");
        QuestData completedQuest = currentActiveQuest; // Simpan referensi sebelum di-null-kan
        completedQuest.isRuntimeActive = false; // Sekarang baru nonaktifkan

        OnQuestTurnedIn?.Invoke(completedQuest); // Event bahwa quest sudah benar-benar selesai & di-acknowledge

        string nextQuest = completedQuest.nextQuestID;
        currentActiveQuest = null; // Reset quest aktif saat ini SETELAH mengirim event OnQuestTurnedIn

        if (!string.IsNullOrEmpty(nextQuest))
        {
            Debug.Log($"QuestManager: Memulai misi berikutnya: {nextQuest}");
            StartQuest(nextQuest);
        }
        else
        {
            Debug.Log("QuestManager: Tidak ada misi berikutnya.");
            // Event OnQuestStarted tidak akan dipanggil jika tidak ada next quest.
            // QuestUI akan menangkap OnQuestTurnedIn dan jika GetActiveQuest() null, ia akan sembunyikan diri.
        }
    }

    // Helper untuk mencari QuestData berdasarkan ID dari list allQuestsInGame
    private QuestData FindQuestByID(string id)
    {
        if (allQuestsInGame == null) return null;
        return allQuestsInGame.Find(q => q != null && q.questID == id);
    }

    // Fungsi untuk mendapatkan detail misi yang sedang aktif
    public QuestData GetActiveQuest() {
        // Hanya kembalikan jika quest itu ada, aktif, dan belum selesai secara logika
        // if (currentActiveQuest != null && currentActiveQuest.isRuntimeActive && !currentActiveQuest.isRuntimeCompleted) {
        //     return currentActiveQuest;
        // }
        // Kita kembalikan saja currentActiveQuest, biarkan UI yang menentukan cara menampilkannya.
        return currentActiveQuest;
    }

    // (Opsional) Fungsi untuk mereset semua status quest jika memulai game baru dari awal sesi
    public static void ResetAllQuestProgressInSession()
    {
        completedQuestIDsThisSession.Clear();
        // Jika QuestManager adalah DontDestroyOnLoad, kita perlu mereset instance field-nya juga
        if (_instance != null) {
            _instance.currentActiveQuest = null;
            // Re-inisialisasi status runtime semua QuestData di list-nya
             _instance.InitializeQuests();
            // Panggil event untuk memberitahu UI agar refresh (menampilkan tidak ada quest)
            OnQuestTurnedIn?.Invoke(null); // atau OnQuestStarted dengan parameter null
        }
        Debug.Log("QuestManager: Status semua misi (completed IDs) dalam sesi ini telah direset.");
    }
}