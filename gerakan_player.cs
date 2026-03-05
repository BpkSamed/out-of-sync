using UnityEngine;

public class gerakan_player : MonoBehaviour

{
    [Tooltip("Kecepatan gerak karakter.")]
    public float moveSpeed = 5f;

    [Tooltip("Hubungkan GameObject yang memiliki script VirtualJoystick.")]
    public VirtualJoystick joystick; // <-- TAMBAHKAN INI

    private Vector2 moveInput = Vector2.zero;

    // --- OPSIONAL TAPI DIREKOMENDASIKAN: Gunakan Rigidbody2D ---
    // private Rigidbody2D rb;
    // void Awake()
    // {
    //     rb = GetComponent<Rigidbody2D>();
    //     if (rb == null) {
    //         Debug.LogError("MobileMovementController membutuhkan Rigidbody2D pada GameObject " + gameObject.name);
    //         enabled = false;
    //     } else {
    //         rb.gravityScale = 0;
    //     }
    // }
    // ----------------------------------------------------------

    void Start() {
        // Pastikan joystick sudah terhubung
        if (joystick == null) {
            Debug.LogError("VirtualJoystick belum dihubungkan ke MobileMovementController di Inspector!");
            // Coba cari otomatis jika tidak di-assign (kurang ideal, tapi bisa jadi fallback)
            joystick = FindObjectOfType<VirtualJoystick>();
            if (joystick == null) {
                 Debug.LogError("Tidak ada VirtualJoystick yang ditemukan di scene!");
                 enabled = false; // Nonaktifkan script jika joystick tidak ada
                 return;
            }
        }
    }

    void Update()
    {
        if (joystick == null) return; // Jangan lakukan apa-apa jika joystick tidak ada

        // 1. Dapatkan Input dari Joystick
        moveInput = joystick.GetInputVector(); // Ini sudah normalized (-1 sampai 1)

        // (Tidak perlu normalisasi lagi karena joystick.GetInputVector() sudah menghasilkan vektor yang sesuai)
        // moveInput = moveInput.normalized; // <-- HAPUS ATAU KOMENTARI BARIS INI JIKA ADA DARI VERSI D-PAD

        // 2. Gerakkan Karakter (Logika tetap sama)
        // --- Metode 1: Mengubah Transform.Position ---
        Vector3 moveDelta = new Vector3(moveInput.x, moveInput.y, 0f) * moveSpeed * Time.deltaTime;
        transform.position += moveDelta;
        // --- Akhir Metode 1 ---

        // --- Metode 2: Menggunakan Rigidbody2D.velocity ---
        // if (rb != null)
        // {
        //     rb.velocity = new Vector2(moveInput.x, moveInput.y) * moveSpeed;
        // }
        // --- Akhir Metode 2 ---
    }

    // HAPUS SEMUA FUNGSI PointerDown/Up UNTUK D-PAD YANG LAMA DARI SINI
    // public void OnUpButtonPressed() { ... }
    // public void OnUpButtonReleased() { ... }
    // ... dan seterusnya untuk Down, Left, Right ...
}
    

