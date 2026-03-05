// QuestStarter.cs
using UnityEngine;
public class QuestStarter : MonoBehaviour
{
    public string questIDToStart;
    public bool startOnAwake = true;
    private bool alreadyStarted = false;

    void Start()
    {
        if (startOnAwake && !alreadyStarted)
        {
            StartTheQuest();
        }
    }

    public void StartTheQuest()
    {
        if (!alreadyStarted && QuestManager.Instance != null && !string.IsNullOrEmpty(questIDToStart))
        {
            QuestManager.Instance.StartQuest(questIDToStart);
            alreadyStarted = true; // Agar tidak dipanggil berulang kali
        }
    }
    // Kamu bisa panggil StartTheQuest() dari event lain, misal tombol atau setelah dialog.
}