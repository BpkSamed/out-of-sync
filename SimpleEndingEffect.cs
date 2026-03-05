using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SimpleEndingEffect : MonoBehaviour
{
    [Header("UI & Scene")]
    public Image fadePanel; 
    public string nextSceneName;

    [Header("Pengaturan Fade")]
    public float durasiFade = 2.0f;

    void OnEnable()
    {
        DialogueManager.OnAllDialoguesFinished += CekDanJalankanEfek;
    }

    void OnDisable()
    {
        DialogueManager.OnAllDialoguesFinished -= CekDanJalankanEfek;
    }

    void CekDanJalankanEfek()
    {
        Debug.Log("1. Memulai Efek Ending...");

        int mentalState = 0;
        if (StoryStateManager.Instance != null)
        {
            mentalState = StoryStateManager.Instance.GetFlag("MentalState");
        }

        Color warnaFade = (mentalState == 2) ? Color.red : Color.white;

        // --- SETUP ANTI GLITCH ---
        this.transform.SetParent(null); 
        
        // Tambah Canvas & Scaler otomatis agar mandiri
        Canvas myCanvas = this.gameObject.GetComponent<Canvas>();
        if (myCanvas == null) myCanvas = this.gameObject.AddComponent<Canvas>();
        myCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        myCanvas.sortingOrder = 999; 

        CanvasScaler scaler = this.gameObject.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = this.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Ambil Panel
        if (fadePanel != null)
        {
            fadePanel.transform.SetParent(this.transform, false); 
            fadePanel.gameObject.SetActive(true);
            
            RectTransform rect = fadePanel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }

        DontDestroyOnLoad(this.gameObject); 
        // ---------------------------

        StartCoroutine(ProsesFadeDanPindah(warnaFade));
    }

    IEnumerator ProsesFadeDanPindah(Color warnaTarget)
    {
        float timer = 0f;
        Time.timeScale = 1f; 

        // 1. FADE IN (Transparan ke Warna Penuh)
        while (timer < durasiFade)
        {
            timer += Time.unscaledDeltaTime; 
            float alpha = Mathf.Clamp01(timer / durasiFade);

            if (fadePanel != null)
            {
                fadePanel.color = new Color(warnaTarget.r, warnaTarget.g, warnaTarget.b, alpha);
            }
            yield return null;
        }

        // Kunci warna penuh
        if (fadePanel != null) fadePanel.color = new Color(warnaTarget.r, warnaTarget.g, warnaTarget.b, 1f);

        // Tunggu 1 detik dalam keadaan layar tertutup penuh
        Debug.Log("Layar tertutup penuh. Menunggu...");
        yield return new WaitForSecondsRealtime(1.0f);

        // 2. PINDAH SCENE
        Debug.Log($"Pindah ke Scene: {nextSceneName}");
        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError($"ERROR: Scene '{nextSceneName}' tidak ditemukan!");
            yield break; // Stop jika scene tidak ada
        }

        // 3. --- PERBAIKAN UTAMA: BERSIH-BERSIH ---
        // Tunggu 1 frame agar Scene baru benar-benar aktif
        yield return null; 

        Debug.Log("Scene baru aktif. Menghancurkan tirai fade...");
        // Hancurkan objek Fade ini agar scene "Bersambung" terlihat
        Destroy(this.gameObject); 
    }
}