using UnityEngine;
using UnityEngine.UI; // Diperlukan untuk mengakses komponen UI

public class MusicToggleButton : MonoBehaviour
{
    [Tooltip("Hubungkan AudioSource yang memainkan musik background.")]
    public AudioSource backgroundMusicSource;

    [Tooltip("Hubungkan GameObject gambar/ikon yang muncul saat musik mati.")]
    public GameObject muteIcon;

    // Variabel privat untuk melacak status musik (true = mati, false = nyala)
    private bool isMuted = false;

    void Start()
    {
        // Pastikan kondisi awal sudah benar saat game dimulai
        // Musik menyala dan ikon mati, sesuai dengan nilai default isMuted = false.
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.mute = isMuted;
        }

        if (muteIcon != null)
        {
            muteIcon.SetActive(isMuted);
        }
    }

    // Fungsi ini akan dipanggil oleh tombol setiap kali ditekan
    public void ToggleMusic()
    {
        // Periksa apakah referensi sudah ada untuk menghindari error
        if (backgroundMusicSource == null || muteIcon == null)
        {
            Debug.LogError("AudioSource atau MuteIcon belum dihubungkan di Inspector!");
            return;
        }

        // 1. Balikkan status mute
        // Jika sedang false (nyala), akan menjadi true (mati).
        // Jika sedang true (mati), akan menjadi false (nyala).
        isMuted = !isMuted;

        // 2. Terapkan status baru ke AudioSource
        backgroundMusicSource.mute = isMuted;

        // 3. Terapkan status baru untuk menampilkan/menyembunyikan ikon
        muteIcon.SetActive(isMuted);
    }
}