using UnityEngine;
using UnityEngine.SceneManagement; // Wajib untuk pindah scene
using System.Collections;

public class DialogueToSceneSwitcher : MonoBehaviour
{
    [Header("Pengaturan Pindah Scene")]
    [Tooltip("Tulis nama Scene tujuan (Rumah Cewek) persis seperti di Build Settings.")]
    public string sceneNameToLoad;

    [Tooltip("Beri jeda sedikit setelah dialog tutup sebelum pindah scene (detik).")]
    public float delayBeforeLoad = 1.0f;

    [Header("Validasi (Opsional)")]
    [Tooltip("Jika dicentang, scene hanya akan pindah jika Flag 'MentalState' sudah terisi (artinya sudah bicara sama Pak Budi).")]
    public bool requireMentalStateFlag = true;

    // --- MENDENGARKAN EVENT DARI DIALOGUE MANAGER ---
    void OnEnable()
    {
        // Saat script aktif, pasang kuping untuk mendengar event "Dialog Selesai"
        DialogueManager.OnAllDialoguesFinished += HandleDialogueEnded;
    }

    void OnDisable()
    {
        // Jangan lupa lepas kuping saat script mati/pindah scene
        DialogueManager.OnAllDialoguesFinished -= HandleDialogueEnded;
    }

    // Fungsi ini otomatis terpanggil saat Dialog Manager bilang "Selesai!"
    void HandleDialogueEnded()
    {
        // Cek apakah kita harus memvalidasi Flag dulu?
        // Ini berguna jika di scene itu ada dialog lain (misal tong sampah) yang tidak boleh memicu pindah scene.
        if (requireMentalStateFlag)
        {
            // Cek ke StoryManager: Apakah Flag 'MentalState' nilainya sudah bukan 0?
            // (Ingat: Pak Budi set MentalState jadi 1 atau 2. Kalau 0 berarti belum bicara/batal).
            if (StoryStateManager.Instance != null)
            {
                int state = StoryStateManager.Instance.GetFlag("MentalState");
                if (state == 0) 
                {
                    Debug.Log("Dialog selesai, tapi MentalState masih 0. Tidak pindah scene.");
                    return; // Batal pindah scene
                }
            }
        }

        Debug.Log("Dialog Pak Budi selesai. Pindah scene dalam " + delayBeforeLoad + " detik...");
        StartCoroutine(LoadSceneProcess());
    }

    IEnumerator LoadSceneProcess()
    {
        // Tunggu sebentar biar tidak kaget (layar hitam/fade out transisi bisa ditaruh sini)
        yield return new WaitForSeconds(delayBeforeLoad);

        // Pastikan nama scene tidak kosong
        if (!string.IsNullOrEmpty(sceneNameToLoad))
        {
            SceneManager.LoadScene(sceneNameToLoad);
        }
        else
        {
            Debug.LogError("[DialogueToSceneSwitcher] Nama Scene Tujuan belum diisi di Inspector!");
        }
    }
}