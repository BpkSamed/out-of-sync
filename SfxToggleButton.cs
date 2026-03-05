using UnityEngine;

public class SfxToggleButton : MonoBehaviour
{
    // --- PERUBAHAN DI SINI ---
    // Membuat variabel static yang bisa diakses dari mana saja.
    // Inilah yang akan menjadi "gerbang" mute global kita.
    public static bool isSfxMuted = false;

    [Tooltip("Hubungkan AudioSource musik background agar tidak ikut mati.")]
    public AudioSource backgroundMusicSource;

    [Tooltip("Hubungkan GameObject ikon yang muncul saat SFX mati.")]
    public GameObject sfxMuteIcon;

    void Start()
    {
        // Kondisi awal saat game dimulai
        isSfxMuted = false; // Pastikan status global juga false di awal
        if (sfxMuteIcon != null)
        {
            sfxMuteIcon.SetActive(isSfxMuted);
        }
        UpdateAllSfxMuteStatus(); // Panggil fungsi untuk sinkronisasi awal
    }

    public void ToggleAllSfx()
    {
        if (sfxMuteIcon == null || backgroundMusicSource == null)
        {
            Debug.LogError("Hubungkan semua referensi di SfxToggleButton pada Inspector!");
            return;
        }

        // 1. Balikkan status mute global
        isSfxMuted = !isSfxMuted;

        // 2. Tampilkan atau sembunyikan ikon
        sfxMuteIcon.SetActive(isSfxMuted);

        // 3. Panggil fungsi untuk mematikan semua suara yang ada
        UpdateAllSfxMuteStatus();
    }

    // Fungsi terpisah untuk mematikan semua suara
    private void UpdateAllSfxMuteStatus()
    {
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in allAudioSources)
        {
            if (source != backgroundMusicSource)
            {
                source.mute = isSfxMuted;
            }
        }
    }
}