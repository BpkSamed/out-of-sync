// QuestObjective.cs
using UnityEngine;

public enum ObjectiveType
{
    TalkToNPC,      // Bicara dengan NPC tertentu
    ReachLocation,  // Mencapai area tertentu
    CollectItem,    // Mengumpulkan item (untuk pengembangan selanjutnya)
    InteractObject  // Berinteraksi dengan objek (untuk pengembangan selanjutnya)
}

[System.Serializable] // Agar bisa dilihat dan diedit di Inspector jika menjadi bagian dari List di QuestData
public class QuestObjective
{
    [Tooltip("Deskripsi tujuan yang akan ditampilkan di UI, misal: 'Bicara dengan Pak Kades'")]
    public string description;
    public ObjectiveType type;
    [Tooltip("ID target, misal: nama NPC unik, nama GameObject area, atau ID item.")]
    public string targetID; // ID unik dari NPC, nama GameObject area, dll.

    [HideInInspector] // Akan diatur oleh QuestManager
    public bool isCompleted = false;

    // (Untuk pengembangan selanjutnya, bisa ditambahkan)
    // public int requiredAmount = 1;
    // [HideInInspector] public int currentAmount = 0;

    // Constructor untuk memudahkan pembuatan objective dari kode (jika perlu)
    public QuestObjective(string desc, ObjectiveType objType, string target)
    {
        description = desc;
        type = objType;
        targetID = target;
        isCompleted = false;
    }
}