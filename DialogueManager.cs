using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine.EventSystems;
using System;

// --- STRUKTUR DATA (DIPERBARUI) ---

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;       
    public string nextDialogueID;   
    [Header("FSM Side Effect")]
    public string flagToSet;        
    public int flagValue;           
}

[System.Serializable]
public class DialogueSequence {
    [Header("Identitas Dialog")]
    public string dialogueID;       // ID Unik (Misal: "Intro_Hero", "Intro_Villain")
    
    [Header("Tampilan")]
    public Sprite characterPortrait; // <<< BARU: Foto wajah pembicara saat ini
    public string characterName;     // <<< BARU: Nama pembicara (Opsional, jika mau ditampilkan)

    [Header("Isi Percakapan")]
    [TextArea(3, 10)] public string dialogueText;
    
    [Header("Alur Selanjutnya (PILIH SALAH SATU)")]
    [Tooltip("Jika diisi, setelah teks habis akan LANJUT ke ID ini otomatis (Dialog Linear). Kosongkan jika ada Choices.")]
    public string nextDialogueID;    // <<< BARU: Untuk dialog sahut-sahutan (Chain)

    [Tooltip("Isi ini jika ingin memberikan pilihan ke pemain.")]
    public List<DialogueChoice> choices; // Percabangan
}
// --------------------------------------------------------------

public class DialogueManager : MonoBehaviour, IPointerClickHandler {
    [Header("UI References - Tampilan")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;      // <<< BARU: Hubungkan UI Image untuk wajah karakter di sini
    public TextMeshProUGUI nameText; // <<< BARU: Hubungkan UI Text untuk nama karakter (Opsional)

    [Header("UI References - Tombol Pilihan")]
    public Transform choiceButtonContainer; 
    public GameObject choiceButtonPrefab;   

    [Header("Data Dialog")]
    public List<DialogueSequence> dialogueSequences;

    [Header("Settings")]
    public float typingSpeed = 0.04f;

    // Variables Internal
    private List<string> currentSentences;
    private int currentSentenceIndex = 0;
    private DialogueSequence currentSequence; 
    
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isDialogueActive = false;
    private bool isProcessingClick = false;
    private bool isWaitingForChoice = false; 

    public static event Action OnDialogueSystemStarted;
    public static event Action OnAllDialoguesFinished;

    void Start() {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (choiceButtonContainer != null) ClearChoices(); 
    }

    public bool StartDialogueByID(string id) {
        // Logika validasi start
        if ((isDialogueActive && !isWaitingForChoice && currentSequence != null && string.IsNullOrEmpty(currentSequence.nextDialogueID)) || isProcessingClick) {
            // Debug.LogWarning($"[DialogueManager] Sedang sibuk. Abaikan request ID: {id}");
            // return false; 
            // Catatan: Kita perbolehkan overwrite jika itu adalah "Next Dialogue" dari chain
        }

        DialogueSequence sequenceToPlay = FindDialogueSequence(id);
        if (sequenceToPlay == null) {
            Debug.LogError($"[DialogueManager] ID '{id}' tidak ditemukan di database!");
            return false;
        }

        // --- UPDATE UI WAJAH & NAMA (BAGIAN BARU) ---
        UpdateCharacterDisplay(sequenceToPlay);
        // --------------------------------------------

        ClearChoices(); 
        currentSequence = sequenceToPlay; 
        currentSentences = ParseSentences(sequenceToPlay.dialogueText);
        
        isProcessingClick = false;
        isTyping = false;
        isWaitingForChoice = false; 
        typingCoroutine = null;

        if (!isDialogueActive) {
            isDialogueActive = true;
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            OnDialogueSystemStarted?.Invoke();
        }

        currentSentenceIndex = 0;
        StartTypingCurrentSentence();
        return true;
    }

    // Fungsi Baru: Mengganti gambar dan nama sesuai ID yang aktif
    // Fungsi Baru: Mengganti gambar dan nama sesuai ID yang aktif
    private void UpdateCharacterDisplay(DialogueSequence sequence)
    {
        // 1. Update Gambar Wajah
        if (portraitImage != null)
        {
            if (sequence.characterPortrait != null)
            {
                // Jika ada data wajah di dialog ini:
                portraitImage.gameObject.SetActive(true); // Munculkan objeknya
                portraitImage.sprite = sequence.characterPortrait; // Ganti gambarnya
                
                // PENTING: Agar gambar tidak gepeng/lebar jika rasio aslinya beda
                portraitImage.preserveAspect = true; 
                
                // (Opsional) Jika Anda ingin pakai Native Size:
                // portraitImage.SetNativeSize(); 
            }
            else
            {
                // Jika kolom portrait di inspector KOSONG:
                // Sembunyikan Image UI-nya (jadi hanya teks saja)
                portraitImage.gameObject.SetActive(false);
            }
        }

        // 2. Update Nama Karakter (Opsional)
        if (nameText != null)
        {
            if (!string.IsNullOrEmpty(sequence.characterName))
            {
                nameText.gameObject.SetActive(true); // Pastikan teks nama aktif
                nameText.text = sequence.characterName;
            }
            else
            {
                // Jika tidak ada nama, sembunyikan area teks nama atau kosongkan
                nameText.text = ""; 
                // nameText.gameObject.SetActive(false); // Bisa di-uncomment jika ingin benar-benar hilang
            }
        }
    }
    private DialogueSequence FindDialogueSequence(string id) {
        foreach (DialogueSequence seq in dialogueSequences) { if (seq.dialogueID == id) return seq; }
        return null;
    }

    private List<string> ParseSentences(string textToParse) {
        // (Sama seperti sebelumnya: Memecah paragraf jadi kalimat)
        List<string> parsedSentences = new List<string>();
        if (string.IsNullOrWhiteSpace(textToParse)) return parsedSentences;
        string pattern = @"[^.!?]+[.!?]*";
        MatchCollection matches = Regex.Matches(textToParse, pattern, RegexOptions.Singleline);
        foreach (Match match in matches) {
            string sentence = match.Value.Trim();
            if (!string.IsNullOrWhiteSpace(sentence)) parsedSentences.Add(sentence);
        }
        if (parsedSentences.Count == 0 && !string.IsNullOrWhiteSpace(textToParse)) parsedSentences.Add(textToParse.Trim());
        return parsedSentences;
    }

    private void StartTypingCurrentSentence() {
        StopTypingCoroutine();
        if (currentSentenceIndex < currentSentences.Count) {
            typingCoroutine = StartCoroutine(TypeText(currentSentences[currentSentenceIndex]));
        } else {
            // --- LOGIKA ALUR (CHAINING) ---
            // Kalimat di ID ini sudah habis. Apa langkah selanjutnya?
            CheckNextStep();
        }
    }

    private void CheckNextStep()
    {
        // Prioritas 1: Apakah ada PILIHAN (Percabangan)?
        if (currentSequence.choices != null && currentSequence.choices.Count > 0) {
            ShowChoices(); 
        } 
        // Prioritas 2: Apakah ada NEXT DIALOGUE ID (Percakapan Lanjut/Linear)?
        else if (!string.IsNullOrEmpty(currentSequence.nextDialogueID)) {
            // Lanjut ke ID berikutnya (Ganti Wajah & Teks)
            Debug.Log($"[DialogueManager] Lanjut ke chain berikutnya: {currentSequence.nextDialogueID}");
            StartDialogueByID(currentSequence.nextDialogueID);
        }
        // Prioritas 3: Tidak ada keduanya -> Selesai.
        else {
            EndDialogue(); 
        }
    }

    private IEnumerator TypeText(string lineToType) {
        isTyping = true;
        if(dialogueText != null) dialogueText.text = ""; 
        float waitTime = typingSpeed > 0 ? typingSpeed : 0.001f; 
        char[] letters = lineToType.ToCharArray();

        for (int i = 0; i < letters.Length; i++) {
            if (!isTyping) {
                if(dialogueText != null) dialogueText.text = lineToType;
                yield break;
            }
            if (dialogueText != null) dialogueText.text += letters[i];
            yield return new WaitForSeconds(waitTime);
        }
        isTyping = false;
        typingCoroutine = null;
    }

    public void AdvanceDialogue() {
        if (isWaitingForChoice) return; 
        if (isProcessingClick || !isDialogueActive) return;

        isProcessingClick = true;
        try {
            if (isTyping && typingCoroutine != null) {
                StopTypingCoroutine();
                if(currentSentenceIndex < currentSentences.Count) {
                    if(dialogueText != null) dialogueText.text = currentSentences[currentSentenceIndex];
                }
            } else {
                currentSentenceIndex++;
                if (currentSentenceIndex < currentSentences.Count) {
                    StartTypingCurrentSentence();
                } else {
                    // Kalimat habis, cek langkah selanjutnya (NextID atau Choice atau End)
                    CheckNextStep();
                }
            }
        } finally {
            isProcessingClick = false;
        }
    }

    // --- LOGIKA UI PERCABANGAN ---
    private void ShowChoices() {
        isWaitingForChoice = true; 
        ClearChoices(); 

        foreach (DialogueChoice choice in currentSequence.choices) {
            GameObject btnObj = Instantiate(choiceButtonPrefab, choiceButtonContainer);
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            Button btn = btnObj.GetComponent<Button>();

            if (btnText != null) btnText.text = choice.choiceText;
            btn.onClick.AddListener(() => OnChoiceSelected(choice));
        }
    }

    private void OnChoiceSelected(DialogueChoice choice) {
        if (!string.IsNullOrEmpty(choice.flagToSet) && StoryStateManager.Instance != null) {
            StoryStateManager.Instance.SetFlag(choice.flagToSet, choice.flagValue);
        }

        if (!string.IsNullOrEmpty(choice.nextDialogueID)) {
            StartDialogueByID(choice.nextDialogueID);
        } else {
            EndDialogue();
        }
    }

    private void ClearChoices() {
        foreach (Transform child in choiceButtonContainer) {
            Destroy(child.gameObject);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (isDialogueActive) AdvanceDialogue();
    }

    private void EndDialogue() {
        StopTypingCoroutine();
        bool wasActive = isDialogueActive;
        
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        ClearChoices(); 
        
        isDialogueActive = false;
        isProcessingClick = false;
        isWaitingForChoice = false;

        if (wasActive) {
            OnAllDialoguesFinished?.Invoke();
        }
    }

    private void StopTypingCoroutine() {
        if (typingCoroutine != null) {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
    }
}