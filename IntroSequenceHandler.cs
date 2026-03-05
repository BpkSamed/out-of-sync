// IntroSequenceHandler.cs (Revisi untuk 1 klik setelah kalimat terakhir)
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // Untuk List<string>

public class IntroSequenceHandler : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Hubungkan instance IntroTyper yang ada di scene Intro ini.")]
    public IntroTyper introTyper;
    [Tooltip("Nama scene game utama yang akan dimuat setelah intro.")]
    public string mainGameSceneName = "NamaSceneGameUtama";

    [Header("Kalimat Intro")]
    [TextArea(3, 10)]
    public List<string> introSentences = new List<string>() {
        "Ini adalah kalimat pertama intro...",
        "Kemudian kalimat kedua akan muncul...",
        "Dan akhirnya, kalimat terakhir."
    };

    // Tidak perlu flag introCompleted lagi jika langsung pindah scene
    // private bool introCompleted = false;

    void Start()
    {
        if (introTyper == null) {
            Debug.LogError("IntroTyper belum dihubungkan ke IntroSequenceHandler!");
            enabled = false; return;
        }
        if (string.IsNullOrEmpty(mainGameSceneName)) {
            Debug.LogError("Nama Scene Game Utama belum diisi di IntroSequenceHandler!");
            enabled = false; return;
        }

        introTyper.StartIntroSequence(introSentences);
    }

    void OnEnable()
    {
        IntroTyper.OnAllIntroSentencesCompleted += HandleIntroCompleted;
    }

    void OnDisable()
    {
        IntroTyper.OnAllIntroSentencesCompleted -= HandleIntroCompleted;
    }

    void Update()
    {
        // Deteksi klik hanya untuk melanjutkan dialog IntroTyper
        if (Input.GetMouseButtonDown(0)) // Untuk mobile, ini adalah sentuhan pertama
        {
            // Hanya proses jika IntroTyper masih aktif (belum memanggil OnAllIntroSentencesCompleted)
            if (introTyper != null && introTyper.IsIntroActive())
            {
                introTyper.Advance();
            }
            // Tidak ada lagi pengecekan introCompleted di sini untuk pindah scene
        }
    }

    void HandleIntroCompleted()
    {
        Debug.Log("Event Intro Selesai diterima oleh Handler. Langsung pindah scene.");
        // introCompleted = true; // Tidak perlu lagi

        // LANGSUNG PINDAH SCENE SETELAH SEMUA KALIMAT INTRO SELESAI
        // DAN EVENT OnAllIntroSentencesCompleted DIPANGGIL.
        // Klik terakhir yang memicu selesainya kalimat terakhir di IntroTyper
        // akan secara efektif menjadi "klik" untuk pindah scene.
        LoadMainGameScene();
    }

    void LoadMainGameScene()
    {
        // Pastikan untuk menonaktifkan UI intro sebelum benar-benar pindah
        // untuk menghindari flicker atau terlihat di scene berikutnya sesaat.
        if (introTyper != null)
        {
            if (introTyper.backgroundPanel != null) {
                introTyper.backgroundPanel.SetActive(false);
            }
            if (introTyper.introTextDisplay != null) {
                 introTyper.introTextDisplay.gameObject.SetActive(false);
            }
        }

        Debug.Log($"Memuat scene: {mainGameSceneName}");
        SceneManager.LoadScene(mainGameSceneName);
    }
}