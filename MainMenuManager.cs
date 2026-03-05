using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        string namaSceneGame = "animasi_intro";
        Debug.Log($"Mencoba memuat scene: {namaSceneGame}");
        SceneManager.LoadScene(namaSceneGame);
    }
    public void QuitGame()
    {
        Debug.Log("Keluar dari aplikasi...");
        Application.Quit();
    }
}