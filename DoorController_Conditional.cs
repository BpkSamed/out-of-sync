// DoorController_Conditional.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // Pastikan ini ada untuk HashSet

[RequireComponent(typeof(Collider2D))]
public class DoorController_Conditional : MonoBehaviour
{
    [Header("UI dan Aksi Dasar")]
    public GameObject openDoorButtonObject;
    public string nextSceneName;
    public bool disableDoorOnOpen = true;
    public float disabledButtonAlpha = 0.5f;

    [Header("Identifikasi & Kondisi")]
    // VVVVVV PASTIKAN DEKLARASI INI ADA VVVVVV
    [Tooltip("ID unik untuk pintu ini (untuk tracking internal status terbuka visual). Jika diisi, status terbukanya akan diingat per sesi game.")]
    public string uniqueDoorID_ForVisualState; // <-- Deklarasi untuk ID visual
    // ^^^^^^ PASTIKAN DEKLARASI INI ADA ^^^^^^

    [Tooltip("ID unik mekanisme kunci yang HARUS aktif (kunci sudah diletakkan di TempatKunciPintu) agar pintu ini bisa di-unlock.")]
    public string requiredMechanismID_ToUnlock;

    [Header("Integrasi Quest (Opsional)")]
    public string doorInteractionID_ForQuest;
    public ObjectiveType objectiveTypeToTrigger = ObjectiveType.InteractObject;

    [Header("Sound Effects")]
    public AudioClip doorOpenSound;
    public AudioClip doorLockedSound;
    [Range(0f, 1f)] public float doorSoundVolume = 0.7f;

    private bool isPlayerNear = false;
    private CanvasGroup openDoorButtonCanvasGroup;
    private Button actualButtonComponent;

    // VVVVVV PASTIKAN DEKLARASI INI ADA (SATU STATIC, SATU INSTANCE) VVVVVV
    private static HashSet<string> visuallyOpenedDoorsIDCache = new HashSet<string>(); // Untuk mengingat pintu mana saja yang sudah dibuka secara visual di sesi ini
    private bool isVisuallyOpenedThisSession = false; // Status instance pintu ini, apakah sudah dibuka visual
    // ^^^^^^ PASTIKAN DEKLARASI INI ADA ^^^^^^

    void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        else Debug.LogError($"Pintu '{gameObject.name}' tidak memiliki Collider2D!");

        if (openDoorButtonObject != null)
        {
            openDoorButtonCanvasGroup = openDoorButtonObject.GetComponent<CanvasGroup>();
            if (openDoorButtonCanvasGroup == null) {
                Debug.LogError($"Pintu '{gameObject.name}': Tombol ({openDoorButtonObject.name}) tidak punya CanvasGroup!", openDoorButtonObject);
            }
            actualButtonComponent = openDoorButtonObject.GetComponent<Button>();
            if (actualButtonComponent != null) {
                actualButtonComponent.onClick.RemoveAllListeners();
                actualButtonComponent.onClick.AddListener(AttemptOpenDoorAction);
            } else {
                Debug.LogError($"Pintu '{gameObject.name}': Tombol ({openDoorButtonObject.name}) tidak punya komponen Button!", openDoorButtonObject);
            }
            openDoorButtonObject.SetActive(true);
        }
        else {
            Debug.LogError($"Tombol 'Buka Pintu' (openDoorButtonObject) belum dihubungkan untuk pintu '{gameObject.name}'!", this.gameObject);
        }

        // Validasi ID
        if (string.IsNullOrEmpty(requiredMechanismID_ToUnlock)) {
            Debug.LogWarning($"Pintu '{gameObject.name}': 'Required Mechanism ID To Unlock' belum di-set.", gameObject);
        }
        // Tidak error jika uniqueDoorID_ForVisualState kosong, tapi fitur persistensi visual tidak akan bekerja maksimal
        if (string.IsNullOrEmpty(uniqueDoorID_ForVisualState)) {
            Debug.LogWarning($"Pintu '{gameObject.name}': 'Unique Door ID For Visual State' belum di-set. Status visual terbuka tidak akan diingat secara unik antar pintu jika ada banyak.", gameObject);
        }

        // Cek apakah pintu ini sudah dibuka secara visual sebelumnya di sesi ini
        if (!string.IsNullOrEmpty(uniqueDoorID_ForVisualState) && visuallyOpenedDoorsIDCache.Contains(uniqueDoorID_ForVisualState))
        {
            isVisuallyOpenedThisSession = true;
            if (disableDoorOnOpen) {
                if(col != null) col.enabled = false;
                Debug.Log($"Pintu [{uniqueDoorID_ForVisualState}] sudah dibuka visual dari cache sesi ini.");
            }
        }
        UpdateOpenDoorButtonState();
    }

    // ... (OnDestroy, OnEnable, OnDisable, OnTriggerEnter2D, OnTriggerExit2D tetap sama) ...
    void OnDestroy() { /* ... */ }
    void OnEnable() { /* ... */ }
    void OnDisable() { /* ... */ }
    private void OnTriggerEnter2D(Collider2D other) { /* ... */ }
    private void OnTriggerExit2D(Collider2D other) { /* ... */ }


    private void UpdateOpenDoorButtonState()
    {
        if (openDoorButtonCanvasGroup == null) return;

        // Jika pintu sudah dibuka visual dan dinonaktifkan, tombol juga nonaktif
        if (isVisuallyOpenedThisSession && disableDoorOnOpen) { // Ditambahkan pengecekan disableDoorOnOpen
            Collider2D col = GetComponent<Collider2D>();
            if (col != null && !col.enabled) { // Hanya jika collidernya memang sudah mati
                openDoorButtonCanvasGroup.alpha = disabledButtonAlpha;
                openDoorButtonCanvasGroup.interactable = false;
                openDoorButtonCanvasGroup.blocksRaycasts = false;
                return;
            }
        }

        bool mechanismIsUnlocked = false;
        if (!string.IsNullOrEmpty(requiredMechanismID_ToUnlock)) {
            mechanismIsUnlocked = TempatKunciPintu.activatedLockMechanisms.Contains(requiredMechanismID_ToUnlock);
        } else {
            // Jika tidak ada requiredMechanismID, anggap tidak terkunci oleh mekanisme kunci
            // mechanismIsUnlocked = true; // Hati-hati, ini berarti bisa dibuka tanpa kunci
                                        // Atau biarkan false jika memang harus ada ID mekanisme
            // Untuk saat ini, kita biarkan false jika ID kosong, artinya perlu ID mekanisme
        }

        bool canInteract = isPlayerNear && mechanismIsUnlocked;

        // Debug.Log($"Pintu '{uniqueDoorID_ForVisualState ?? gameObject.name}': UpdateOpenDoorButtonState -> isPlayerNear={isPlayerNear}, reqMechID='{requiredMechanismID_ToUnlock}', mechanismIsUnlocked={mechanismIsUnlocked}, canInteract={canInteract}");

        if (canInteract) {
            openDoorButtonCanvasGroup.alpha = 1f;
            openDoorButtonCanvasGroup.interactable = true;
            openDoorButtonCanvasGroup.blocksRaycasts = true;
        } else {
            openDoorButtonCanvasGroup.alpha = disabledButtonAlpha;
            openDoorButtonCanvasGroup.interactable = false;
            openDoorButtonCanvasGroup.blocksRaycasts = false;
        }
    }

    public void AttemptOpenDoorAction()
    {
        // Debug.Log($"Tombol 'Buka Pintu' untuk [{uniqueDoorID_ForVisualState ?? gameObject.name}] diklik.");

        // Majukan Quest Objective (jika ada ID interaksi quest)
        if (QuestManager.Instance != null && !string.IsNullOrEmpty(doorInteractionID_ForQuest)) {
            QuestManager.Instance.AdvanceObjective(objectiveTypeToTrigger, doorInteractionID_ForQuest);
        }

        bool mechanismIsUnlocked = !string.IsNullOrEmpty(requiredMechanismID_ToUnlock) &&
                                 TempatKunciPintu.activatedLockMechanisms.Contains(requiredMechanismID_ToUnlock);

        if (mechanismIsUnlocked) {
            Debug.Log($"Pintu [{uniqueDoorID_ForVisualState ?? gameObject.name}] Terbuka! (Mekanisme: {requiredMechanismID_ToUnlock})");
            if (doorOpenSound != null) AudioSource.PlayClipAtPoint(doorOpenSound, transform.position, doorSoundVolume);

            isVisuallyOpenedThisSession = true; // Tandai sudah dibuka visual
            if(!string.IsNullOrEmpty(uniqueDoorID_ForVisualState)) { // Hanya tambahkan ke cache jika ID valid
                visuallyOpenedDoorsIDCache.Add(uniqueDoorID_ForVisualState);
            }


            if (!string.IsNullOrEmpty(nextSceneName)) {
                Time.timeScale = 1f;
                SceneManager.LoadScene(nextSceneName);
                return;
            }
            if (disableDoorOnOpen) {
                Collider2D col = GetComponent<Collider2D>();
                if(col != null) col.enabled = false;
            }
            // Setelah pintu dibuka, tombol menjadi non-interaktif
             if(openDoorButtonCanvasGroup != null){
                openDoorButtonCanvasGroup.alpha = disabledButtonAlpha;
                openDoorButtonCanvasGroup.interactable = false;
                openDoorButtonCanvasGroup.blocksRaycasts = false;
            }
        } else {
            Debug.LogWarning($"Aksi buka pintu [{uniqueDoorID_ForVisualState ?? gameObject.name}] tidak dilakukan. Mekanisme kunci [{requiredMechanismID_ToUnlock}] belum aktif.");
            if (doorLockedSound != null) AudioSource.PlayClipAtPoint(doorLockedSound, transform.position, doorSoundVolume);
        }
    }

    // void HandleQuestUpdate(QuestData changedQuest) { if (isPlayerNear) UpdateOpenDoorButtonState(); }
    public static void ResetAllVisuallyOpenedDoors() {
        visuallyOpenedDoorsIDCache.Clear();
    }
}