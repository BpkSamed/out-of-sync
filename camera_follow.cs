using UnityEngine;
using System.Collections;

public class camera_follow : MonoBehaviour
{
    [Header("Target & Settings")]
    public Transform target;       // Player
    public float smoothing = 5f;   // Kecepatan gerak kamera
    public float rotateSpeed = 5f; // Kecepatan putar kamera

    [Header("Isometric Properties")]
    private Vector3 initialOffset; // Jarak awal ke player
    private float initialXRotation; // Kemiringan kamera (Pitch)
    
    // Variabel Rotasi
    private float startYRotation;   // Sudut Y awal saat game dimulai
    private float currentYRotation; // Sudut Y saat ini (berjalan)
    private float targetYRotation;  // Sudut Y tujuan (untuk animasi)

    // Property untuk dibaca script Player
    public bool IsRotating { get; private set; } = false;

    void Start()
    {
        if (target == null) 
        {
            Debug.LogError("Target belum di-assign di Inspector!", this);
            return;
        }

        // 1. Simpan Offset Awal (Posisi relatif terhadap player saat ini)
        // Ini menjaga posisi (-52.1, dst) yang sudah kamu atur di Editor
        initialOffset = transform.position - target.position;

        // 2. Simpan Sudut Awal
        Vector3 startRotation = transform.eulerAngles;
        initialXRotation = startRotation.x;
        startYRotation = startRotation.y; // <-- Ini kunci perbaikannya

        // 3. Inisialisasi variabel tracking rotasi
        currentYRotation = startYRotation;
        targetYRotation = startYRotation;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // --- LOGIKA BARU YANG DIPERBAIKI ---

        // 1. Lerp sudut saat ini menuju target
        currentYRotation = Mathf.LerpAngle(currentYRotation, targetYRotation, rotateSpeed * Time.deltaTime);

        // 2. Hitung SELISIH sudut dari posisi awal
        // Jika baru mulai, selisihnya 0, jadi kamera tidak akan lompat.
        float rotationDifference = currentYRotation - startYRotation;

        // 3. Buat rotasi hanya berdasarkan selisih tersebut
        Quaternion rotation = Quaternion.Euler(0, rotationDifference, 0);

        // 4. Putar Offset Awal menggunakan selisih sudut
        Vector3 rotatedOffset = rotation * initialOffset;

        // 5. Tentukan posisi akhir
        Vector3 finalPosition = target.position + rotatedOffset;

        // 6. Gerakkan kamera
        transform.position = Vector3.Lerp(transform.position, finalPosition, smoothing * Time.deltaTime);

        // 7. Atur rotasi kamera (Menghadap player + kemiringan isometrik)
        // Di sini kita pakai currentYRotation mutlak agar arah hadapnya benar
        transform.rotation = Quaternion.Euler(initialXRotation, currentYRotation, 0);
    }

    // --- FUNGSI TOMBOL (TIDAK BERUBAH) ---

    public void RotateRight()
    {
        if (IsRotating) return;
        targetYRotation -= 90f; 
        StartCoroutine(DisableInputRoutine());
    }

    public void RotateLeft()
    {
        if (IsRotating) return; 
        targetYRotation += 90f; 
        StartCoroutine(DisableInputRoutine());
    }

    private IEnumerator DisableInputRoutine()
    {
        IsRotating = true; 
        
        // Tunggu sampai rotasi hampir selesai
        while (Mathf.Abs(Mathf.DeltaAngle(currentYRotation, targetYRotation)) > 0.5f)
        {
            yield return null;
        }

        currentYRotation = targetYRotation;
        IsRotating = false; 
    }
}