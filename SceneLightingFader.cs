// SceneLightingFader.cs (Versi 3D)
using UnityEngine;
using System.Collections;
// using UnityEngine.Rendering.Universal; // Tidak lagi diperlukan untuk Light 3D dasar
using System; // Diperlukan untuk System.Action (event)

[RequireComponent(typeof(Light))] // Diubah dari Light2D
public class SceneLightingFader : MonoBehaviour
{
    [Tooltip("Durasi pencerahan cahaya dalam detik.")]
    public float fadeInDuration = 3.0f;
    [Tooltip("Intensitas cahaya target (terang penuh).")]
    public float targetIntensity = 1.0f;
    [Tooltip("Intensitas cahaya awal (gelap).")]
    public float startIntensity = 0.0f;

    private Light globalLight; // Diubah dari Light2D

    // --- Event Selesai Fade In (Tidak berubah) ---
    public static event Action OnFadeInComplete; 
    // --- Akhir Event ---

    void Start()
    {
        globalLight = GetComponent<Light>(); // Diubah dari Light2D
        if (globalLight != null)
        {
            globalLight.intensity = startIntensity;
            StartCoroutine(FadeInLightCoroutine());
        }
        else
        {
            // Pesan error diperbarui
            Debug.LogError("Komponen Light (3D) tidak ditemukan!", this.gameObject);
        }
    }

    private IEnumerator FadeInLightCoroutine()
    {
        // Logika Coroutine ini SAMA PERSIS, karena hanya memanipulasi float .intensity
        float elapsedTime = 0f;
        Debug.Log("Memulai fade in cahaya...");

        while (elapsedTime < fadeInDuration)
        {
            float progress = elapsedTime / fadeInDuration;
            globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        globalLight.intensity = targetIntensity; // Pastikan nilai akhir pas
        Debug.Log("Fade in cahaya selesai.");

        // --- Picu Event Selesai (Tidak berubah) ---
        Debug.Log("Memicu event OnFadeInComplete.");
        OnFadeInComplete?.Invoke(); 
        // --- Akhir Picu Event ---
    }
}