using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Referensi")]
    public DialogueManager dialogueManager;

    [Header("UI Elements to Control During Dialogue")]
    public List<GameObject> uiElementsToToggle = new List<GameObject>();

    void Awake()
    {
        // Hanya validasi referensi, JANGAN ubah state UI di sini.
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
        }
    }

    void OnEnable()
    {
        // Langganan event untuk dialog di tengah permainan (misal interaksi NPC)
        DialogueManager.OnDialogueSystemStarted += HandleDialogueSystemStarted;
        DialogueManager.OnAllDialoguesFinished += HandleAllDialoguesFinished;
    }

    void OnDisable()
    {
        DialogueManager.OnDialogueSystemStarted -= HandleDialogueSystemStarted;
        DialogueManager.OnAllDialoguesFinished -= HandleAllDialoguesFinished;
    }

    // Ubah menjadi PUBLIC agar GameStartHandler bisa memanggilnya
    public void SetGameplayUIActive(bool isActive)
    {
        foreach (GameObject uiElement in uiElementsToToggle)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(isActive);
            }
        }
    }

    // Event Handler: Saat dialog dimulai (oleh siapapun)
    void HandleDialogueSystemStarted()
    {
        // Debug.Log("UIManager: Event DialogueSystemStarted -> Sembunyikan UI.");
        SetGameplayUIActive(false);
    }

    // Event Handler: Saat semua dialog selesai
    void HandleAllDialoguesFinished()
    {
        // Debug.Log("UIManager: Event AllDialoguesFinished -> Munculkan UI.");
        SetGameplayUIActive(true);
    }
}