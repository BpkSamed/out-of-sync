// VideoIntroController.cs
using UnityEngine;
using UnityEngine.SceneManagement; // Untuk SceneManager
using UnityEngine.Video;         // Untuk VideoPlayer

[RequireComponent(typeof(VideoPlayer))] // Pastikan ada komponen VideoPlayer
public class VideoIntroController : MonoBehaviour
{
    [Header("Pengaturan Video")]
    [Tooltip("Komponen VideoPlayer yang akan dikontrol. Akan diambil otomatis jika script ini dipasang di GameObject yang sama.")]
    public VideoPlayer videoPlayer;

    [Header("Navigasi Scene")]
    [Tooltip("Nama scene game utama yang akan dimuat setelah video selesai.")]
    public string mainGameSceneName = "NamaSceneGameUtama"; // GANTI DENGAN NAMA SCENE UTAMAMU

    [Header("Opsi Skip (Opsional)")]
    [Tooltip("Aktifkan jika ingin video bisa di-skip.")]
    public bool allowSkip = true;
    [Tooltip("Tombol keyboard untuk skip video.")]
    public KeyCode skipKey = KeyCode.Space; // Atau KeyCode.Escape, KeyCode.Return

    private bool sceneHasBeenLoaded = false; // Mencegah load scene berkali-kali

    void Awake()
    {
        // Dapatkan komponen VideoPlayer jika belum di-assign
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer component tidak ditemukan! Intro video tidak akan berjalan.", this.gameObject);
            enabled = false; // Nonaktifkan script jika tidak ada VideoPlayer
            return;
        }

        // Pastikan video tidak di-loop oleh VideoPlayer jika kita ingin mendeteksi akhirnya
        videoPlayer.isLooping = false;
    }

    void Start()
    {
        // Pastikan Time.timeScale normal agar video dan event berjalan
        Time.timeScale = 1f;

        // Langganan event loopPointReached yang akan dipanggil saat video selesai (jika tidak looping)
        videoPlayer.loopPointReached += OnVideoFinished;

        // Putar video jika belum 'Play On Awake' atau untuk memastikan
        if (!videoPlayer.playOnAwake)
        {
            videoPlayer.Play();
        }
        Debug.Log("Video intro dimulai.");
    }

    void Update()
    {
        // Cek input untuk skip video (jika diizinkan)
        if (allowSkip && Input.GetKeyDown(skipKey))
        {
            Debug.Log("Video intro di-skip.");
            videoPlayer.Stop(); // Hentikan video
            LoadMainGameScene(); // Langsung pindah scene
        }

        // Alternatif untuk skip dengan sentuhan di mobile (jika tidak ada tombol khusus)
        // if (allowSkip && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        // {
        //     Debug.Log("Video intro di-skip via sentuhan.");
        //     videoPlayer.Stop();
        //     LoadMainGameScene();
        // }
    }

    // Fungsi ini akan dipanggil oleh event VideoPlayer.loopPointReached
    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video intro selesai.");
        LoadMainGameScene();
    }

    void LoadMainGameScene()
    {
        // Pastikan scene hanya dimuat sekali
        if (sceneHasBeenLoaded) return;
        sceneHasBeenLoaded = true;

        // Hentikan langganan event sebelum pindah scene
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }

        if (string.IsNullOrEmpty(mainGameSceneName))
        {
            Debug.LogError("Nama Scene Game Utama belum diisi di VideoIntroController!", this.gameObject);
            return;
        }

        Debug.Log($"Memuat scene: {mainGameSceneName}");
        SceneManager.LoadScene(mainGameSceneName);
    }

    void OnDestroy()
    {
        // Penting untuk unsubscribe saat objek dihancurkan untuk menghindari error
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}