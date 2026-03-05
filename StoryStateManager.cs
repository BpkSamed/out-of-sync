using UnityEngine;
using System.Collections.Generic;

public class StoryStateManager : MonoBehaviour
{
    public static StoryStateManager Instance;

    // Dictionary untuk menyimpan "Flag" atau keputusan pemain
    // Contoh: <"IsKeyFound", true> atau <"RelationshipStatus", 5>
    private Dictionary<string, int> storyFlags = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // JANGAN HANCUR SAAT PINDAH SCENE
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Fungsi untuk mengubah state cerita
    public void SetFlag(string flagName, int value)
    {
        if (storyFlags.ContainsKey(flagName))
            storyFlags[flagName] = value;
        else
            storyFlags.Add(flagName, value);
        
        Debug.Log($"[StoryState] Flag '{flagName}' di-set ke {value}");
    }

    // Fungsi untuk mengecek state cerita (berguna di scene lain)
    public int GetFlag(string flagName)
    {
        if (storyFlags.ContainsKey(flagName))
            return storyFlags[flagName];
        return 0; // Default 0 jika tidak ditemukan
    }
}