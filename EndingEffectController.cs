using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndingEffectController : MonoBehaviour
{
    [Header("Referensi Wajib")]
    [Tooltip("Panel UI (Image) yang menutupi seisi layar. Pastikan Raycast Target dimatikan.")]
    public Image overlayPanel; 
    public Transform mainCamera; // Kamera utama untuk efek getar
    public AudioSource audioSource; // Untuk SFX ending

    [Header("Konfigurasi Scene")]
    public string sceneBersambung = "Scene_To_Be_Continued"; // Ganti dengan nama scene kamu

    [Header("Efek Good Ending (Jalur Sopan)")]
    public Color warnaGood = Color.white;
    public float durasiFadeGood = 2.0f;
    public AudioClip sfxGood;

    [Header("Efek Bad Ending (Jalur Marah)")]
    public Color warnaBad = Color.black;
    public float durasiFadeBad = 0.5f; // Lebih cepat biar kaget
    public float kekuatanGetar = 0.3f; // Seberapa kuat kameranya goyang
    public float durasiGetar = 0.5f;
    public AudioClip sfxBad;

    void Start()
    {
        // Pastikan panel transparan di awal
        if (overlayPanel != null)
        {
            overlayPanel.color = new Color(0, 0, 0, 0); 
            overlayPanel.gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        DialogueManager.OnAllDialoguesFinished += TriggerEndingSequence;
    }

    void OnDisable()
    {
        DialogueManager.OnAllDialoguesFinished -= TriggerEndingSequence;
    }

    private void TriggerEndingSequence()
    {
        int mentalState = 0;
        if (StoryStateManager.Instance != null)
        {
            mentalState = StoryStateManager.Instance.GetFlag("MentalState");
        }

        StartCoroutine(PlayEndingEffect(mentalState));
    }

    private IEnumerator PlayEndingEffect(int mentalState)
    {
        // Aktifkan Panel
        if (overlayPanel != null) overlayPanel.gameObject.SetActive(true);

        if (mentalState == 2) // --- BAD ENDING (NIGHTMARE) ---
        {
            Debug.Log("Memainkan Efek BAD ENDING");
            
            // 1. Mainkan Suara Seram
            if (audioSource != null && sfxBad != null) audioSource.PlayOneShot(sfxBad);

            // 2. Mulai Getaran Kamera (Camera Shake)
            StartCoroutine(ShakeCamera(durasiGetar, kekuatanGetar));

            // 3. Fade Out ke Hitam (Cepat/Cut)
            if (overlayPanel != null) 
            {
                overlayPanel.color = new Color(warnaBad.r, warnaBad.g, warnaBad.b, 0);
                yield return StartCoroutine(FadeCanvas(overlayPanel, 0f, 1f, durasiFadeBad));
            }
        }
        else // --- GOOD ENDING (STABLE) atau Default ---
        {
            Debug.Log("Memainkan Efek GOOD ENDING");

            // 1. Mainkan Suara Tenang
            if (audioSource != null && sfxGood != null) audioSource.PlayOneShot(sfxGood);

            // 2. Fade Out ke Putih (Pelan/Damai)
            if (overlayPanel != null) 
            {
                overlayPanel.color = new Color(warnaGood.r, warnaGood.g, warnaGood.b, 0);
                yield return StartCoroutine(FadeCanvas(overlayPanel, 0f, 1f, durasiFadeGood));
            }
        }

        // --- PINDAH SCENE ---
        Debug.Log("Pindah ke scene bersambung...");
        SceneManager.LoadScene(sceneBersambung);
    }

    // Logika Fade In/Out
    private IEnumerator FadeCanvas(Image img, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        Color c = img.color;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            img.color = new Color(c.r, c.g, c.b, newAlpha);
            yield return null;
        }
        img.color = new Color(c.r, c.g, c.b, endAlpha);
    }

    // Logika Camera Shake
    private IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Vector3 originalPos = mainCamera.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            mainCamera.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.localPosition = originalPos;
    }
}