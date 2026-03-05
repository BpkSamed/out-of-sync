// InitialCueController.cs
using UnityEngine;
using UnityEngine.SceneManagement; // Untuk mengecek nama scene jika perlu

public class InitialCueController : MonoBehaviour
{
    [Header("Referensi Visual")]
    [Tooltip("Hubungkan GameObject visual tanda seru (anak dari Player) ke sini.")]
    public GameObject playerCueVisual; // Ini adalah 'CueAwalIndikator' dari Player

    [Header("Pengaturan Pemicu")]
    [Tooltip("Opsional: Isi dengan nama scene spesifik tempat cue ini harus muncul pertama kali. Biarkan kosong jika script ini hanya ada di satu scene game utama pertama.")]
    public string targetSceneName;

    // Static flag untuk memastikan cue ini hanya muncul sekali per sesi game
    private static bool initialCueHasBeenDisplayedThisSession = false;
    private bool isCueActiveAndWaitingForDialogue = false;

    void Start()
    {
        // Validasi awal
        if (playerCueVisual == null)
        {
            Debug.LogError("InitialCueController: Player Cue Visual belum di-assign di Inspector!", this.gameObject);
            enabled = false; // Nonaktifkan script jika tidak ada visual
            return;
        }

        // Cek apakah cue ini seharusnya aktif di scene ini
        if (!string.IsNullOrEmpty(targetSceneName) && SceneManager.GetActiveScene().name != targetSceneName)
        {
            playerCueVisual.SetActive(false); // Pastikan mati jika bukan scene yang tepat
            // Debug.Log($"InitialCueController: Bukan scene target ({targetSceneName}). Cue tidak akan ditampilkan.");
            enabled = false; // Nonaktifkan script jika bukan scene yang dituju
            return;
        }

        // Cek apakah cue ini sudah pernah ditampilkan di sesi game ini
        if (!initialCueHasBeenDisplayedThisSession)
        {
            // Jika belum, tampilkan cue dan mulai mendengarkan event dialog
            Debug.Log("InitialCueController: Menampilkan cue awal di atas Player.");
            playerCueVisual.SetActive(true);
            isCueActiveAndWaitingForDialogue = true;

            // Berlangganan event dari DialogueManager
            // Pastikan DialogueManager.cs milikmu punya event OnDialogueSystemStarted
            DialogueManager.OnDialogueSystemStarted += HandleFirstDialogueStarted;
        }
        else
        {
            // Jika sudah pernah ditampilkan, pastikan visualnya tidak aktif
            playerCueVisual.SetActive(false);
            Debug.Log("InitialCueController: Cue awal sudah pernah ditampilkan di sesi game ini.");
        }
    }

    // Fungsi yang akan dipanggil saat event OnDialogueSystemStarted dari DialogueManager terpicu
    void HandleFirstDialogueStarted()
    {
        Debug.Log("InitialCueController: Dialog pertama dimulai, menyembunyikan cue.");
        if (playerCueVisual != null)
        {
            playerCueVisual.SetActive(false);
        }

        // Tandai bahwa cue ini sudah selesai untuk sesi game ini
        initialCueHasBeenDisplayedThisSession = true;
        isCueActiveAndWaitingForDialogue = false;

        // Penting: Berhenti berlangganan event agar tidak terpanggil lagi untuk dialog berikutnya
        DialogueManager.OnDialogueSystemStarted -= HandleFirstDialogueStarted;
    }

    void OnDestroy()
    {
        // Selalu pastikan untuk berhenti berlangganan saat script ini hancur,
        // terutama jika cue sedang aktif menunggu dialog.
        if (isCueActiveAndWaitingForDialogue)
        {
            DialogueManager.OnDialogueSystemStarted -= HandleFirstDialogueStarted;
        }
    }

    // (Opsional) Fungsi untuk mereset status jika ada sistem "New Game" tanpa keluar aplikasi
    public static void ResetInitialCueStatus()
    {
        initialCueHasBeenDisplayedThisSession = false;
        Debug.Log("Status Initial Cue telah direset untuk sesi baru.");
    }
}