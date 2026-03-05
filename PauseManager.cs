using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Tooltip("Hubungkan Panel UI Pause dari Hierarchy ke sini.")]
    public GameObject pauseMenuPanel;

    [Tooltip("Hubungkan Panel UI buram/gelap yang akan muncul di belakang menu pause.")] // <-- BARU
    public GameObject pauseDimmingPanel; // <<-- TAMBAHKAN INI

    [Tooltip("Tombol Pause utama di HUD (opsional).")]
    public Button mainPauseButton;

    private bool isPaused = false;

    void Start()
    {
        // Sembunyikan panel pause dan panel buram di awal
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("PauseMenuPanel belum dihubungkan ke PauseManager!");
        }

        if (pauseDimmingPanel != null) // <-- BARU
        {
            pauseDimmingPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("PauseDimmingPanel belum dihubungkan ke PauseManager. Efek buram tidak akan muncul.");
        }

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (pauseMenuPanel == null) return;

        Time.timeScale = 0f;
        isPaused = true;

        if (pauseDimmingPanel != null) // <-- BARU
        {
            pauseDimmingPanel.SetActive(true); // Tampilkan panel buram
        }
        pauseMenuPanel.SetActive(true); // Tampilkan menu pause (di atas panel buram)


        if (mainPauseButton != null)
        {
            mainPauseButton.interactable = false;
        }
        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel == null) return;

        Time.timeScale = 1f;
        isPaused = false;

        pauseMenuPanel.SetActive(false); // Sembunyikan menu pause
        if (pauseDimmingPanel != null) // <-- BARU
        {
            pauseDimmingPanel.SetActive(false); // Sembunyikan panel buram
        }

        if (mainPauseButton != null)
        {
            mainPauseButton.interactable = true;
        }
        Debug.Log("Game Resumed");
    }

    public void RestartGame()
    {
        Debug.Log("PauseManager: Restarting game...");
        Time.timeScale = 1f;
        isPaused = false;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (mainPauseButton != null) mainPauseButton.interactable = true;

        // --- PANGGIL FUNGSI RESET DARI SEMUA SISTEM YANG MENGGUNAKAN DATA STATIS PER SESI ---
        // Untuk AreaButtonObjectRemover
        AreaButtonObjectRemover.ResetAllCompletedActions();

        // Untuk TempatKunciPintu (jika kamu masih menggunakannya dan punya fungsi reset serupa)
        // TempatKunciPintu.ResetAllActivatedLockMechanisms(); // Jika ada
        // TempatKunciPintu.ResetAllVisuallyOpenedDoors(); // Jika ada

        // Untuk QuestManager (jika ada fungsi reset untuk completed quests per sesi)
        // QuestManager.ResetAllQuestProgressInSession(); // Jika ada

        // Untuk OneTimeAreaDialogueTrigger (jika ada fungsi reset)
        // OneTimeAreaDialogueTrigger.ResetAllTriggeredAreasStatus(); // Jika ada
        // ------------------------------------------------------------------------------------

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (pauseDimmingPanel != null) pauseDimmingPanel.SetActive(false); // <-- BARU

        Debug.Log("Exiting to Main Menu (Scene: menu_depan)");
        SceneManager.LoadScene("menu_depan");
    }

    public void OpenOptions()
    {
        Debug.Log("Tombol Option ditekan (belum diimplementasikan).");
        // Jika kamu membuka panel options, pastikan panel pause utama dan dimming panel
        // mungkin perlu disembunyikan sementara atau panel options muncul di atasnya.
    }

    public bool IsGamePaused()
    {
        return isPaused;
    }
}