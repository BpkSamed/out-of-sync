// IntroTyper.cs
using UnityEngine;
using TMPro; // Atau UnityEngine.UI jika pakai Text standar
using System.Collections;
using System.Collections.Generic;
using System; // Untuk Action

public class IntroTyper : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Komponen TextMeshProUGUI untuk menampilkan teks intro.")]
    public TextMeshProUGUI introTextDisplay; // Ganti Text jika pakai UI Text standar
    [Tooltip("Panel background hitam yang akan diaktifkan.")]
    public GameObject backgroundPanel;

    [Header("Typing Effect Settings")]
    [Tooltip("Waktu jeda antar kata (detik).")]
    public float timePerWord = 0.2f;
    [Tooltip("Waktu jeda antar kalimat (detik), setelah satu kalimat selesai diketik.")]
    public float timeBetweenSentences = 0.5f;

    private List<string> sentencesToShow = new List<string>();
    private int currentSentenceIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTypingThisSentence = false;
    private bool allSentencesShown = false;

    // Event untuk memberi tahu bahwa semua kalimat telah ditampilkan
    public static event Action OnAllIntroSentencesCompleted;

    void Awake()
    {
        if (introTextDisplay == null) {
            Debug.LogError("IntroTextDisplay belum dihubungkan ke IntroTyper!");
            enabled = false; return;
        }
        if (backgroundPanel == null) {
            Debug.LogError("BackgroundPanel belum dihubungkan ke IntroTyper!");
            enabled = false; return;
        }
        introTextDisplay.text = ""; // Kosongkan teks di awal
    }

    // Fungsi untuk memulai sekuens intro dengan kalimat yang diberikan
    public void StartIntroSequence(List<string> introSentences)
    {
        if (introSentences == null || introSentences.Count == 0) {
            Debug.LogWarning("Tidak ada kalimat intro yang diberikan ke IntroTyper.");
            CompleteIntro(); // Langsung selesaikan jika tidak ada kalimat
            return;
        }

        sentencesToShow = new List<string>(introSentences);
        currentSentenceIndex = 0;
        allSentencesShown = false;
        isTypingThisSentence = false;

        if (backgroundPanel != null) backgroundPanel.SetActive(true);
        if (introTextDisplay != null) introTextDisplay.gameObject.SetActive(true);

        ShowNextSentence();
    }

    private void ShowNextSentence()
    {
        if (currentSentenceIndex < sentencesToShow.Count)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeSentenceWordByWord(sentencesToShow[currentSentenceIndex]));
        }
        else
        {
            CompleteIntro();
        }
    }

    private IEnumerator TypeSentenceWordByWord(string sentence)
    {
        isTypingThisSentence = true;
        introTextDisplay.text = ""; // Kosongkan untuk kalimat baru

        string[] words = sentence.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            introTextDisplay.text += words[i];
            if (i < words.Length - 1) // Tambah spasi kecuali kata terakhir
            {
                introTextDisplay.text += " ";
            }
            yield return new WaitForSecondsRealtime(timePerWord); // Pakai Realtime agar tidak terpengaruh Time.timeScale
        }

        isTypingThisSentence = false;
        // Beri jeda sejenak setelah kalimat selesai diketik sebelum menunggu klik berikutnya
        if (timeBetweenSentences > 0) {
            yield return new WaitForSecondsRealtime(timeBetweenSentences);
        }
    }

    // Dipanggil saat player klik untuk melanjutkan
    public void Advance()
    {
        if (allSentencesShown) return; // Jika sudah selesai, jangan lakukan apa-apa

        if (isTypingThisSentence)
        {
            // Jika sedang mengetik, selesaikan kalimat saat ini
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            isTypingThisSentence = false;
            if (currentSentenceIndex < sentencesToShow.Count) {
                introTextDisplay.text = sentencesToShow[currentSentenceIndex];
            }
             // Setelah menyelesaikan kalimat, klik berikutnya akan lanjut ke kalimat baru
        }
        else
        {
            // Jika tidak sedang mengetik (kalimat sudah selesai tampil), lanjut ke kalimat berikutnya
            currentSentenceIndex++;
            ShowNextSentence();
        }
    }

    private void CompleteIntro()
    {
        allSentencesShown = true;
        Debug.Log("Semua kalimat intro selesai.");
        OnAllIntroSentencesCompleted?.Invoke();

        // Panel dan teks bisa disembunyikan oleh IntroSequenceHandler setelah ini
    }

    // Fungsi untuk mengecek apakah intro sedang aktif menampilkan sesuatu
    public bool IsIntroActive() {
        return !allSentencesShown;
    }

    // Pastikan untuk menghentikan coroutine jika objek dihancurkan
    void OnDestroy() {
        if (typingCoroutine != null) {
            StopCoroutine(typingCoroutine);
        }
    }
}