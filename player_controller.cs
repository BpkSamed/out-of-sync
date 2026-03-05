using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class player_controller : MonoBehaviour
{
    [Header("Pengaturan")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Mode Animasi")]
    [Tooltip("Centang jika pakai Blend Tree 8 Arah. Hapus centang jika pakai 1 animasi Run.")]
    public bool useStrafing = false; 

    [Header("Input")]
    public analog joystick; 

    // Komponen
    private Rigidbody rb;
    private Animator animator;
    private Transform mainCameraTransform; // <-- TAMBAHAN 1: Referensi Kamera

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        
        // <-- TAMBAHAN 2: Ambil Transform Kamera Utama otomatis
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Main Camera tidak ditemukan! Pastikan kamera punya tag 'MainCamera'.");
        }
    }

    void FixedUpdate() 
    {
        // 1. AMBIL INPUT MENTAH (JOYSTICK/KEYBOARD)
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        if (joystick != null && joystick.gameObject.activeInHierarchy)
        {
            Vector2 joyInput = joystick.GetInputVector();
            if(joyInput.magnitude > 0.01f) 
            {
                x = joyInput.x;
                z = joyInput.y;
            }
        }

        // --- PERUBAHAN UTAMA DI SINI (LOGIKA KAMERA) ---
        Vector3 direction = Vector3.zero;

        // Cek apakah ada input
        if (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f)
        {
            // Ambil arah Depan & Kanan dari Kamera
            Vector3 camForward = mainCameraTransform.forward;
            Vector3 camRight = mainCameraTransform.right;

            // "Gepengkan" vektor agar y = 0 (supaya player gak nunduk ke tanah/terbang ke langit)
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            // Gabungkan Input dengan Arah Kamera
            // Maju (z) ikut arah kamera depan, Kanan (x) ikut arah kamera kanan
            direction = (camForward * z + camRight * x).normalized;
        }
        // ------------------------------------------------

        bool isMoving = direction.magnitude >= 0.1f;

        // 2. GERAKKAN (MovePosition)
        if (isMoving)
        {
            Vector3 targetPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }

        // 3. ROTASI
        if (!useStrafing)
        {
            if (isMoving)
            {
                // Karakter memutar badan ke arah gerak (Camera Relative)
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Quaternion newRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                rb.MoveRotation(newRotation);
            }
        }

        // 4. ANIMASI
        if (animator != null)
        {
            // Kirim input mentah ke Blend Tree (tetap X dan Z joystick asli, bukan direction dunia)
            // Ini agar animasi blend tree tetap sinkron dengan jempol pemain
            animator.SetFloat("InputX", x, 0.1f, Time.fixedDeltaTime);
            animator.SetFloat("InputZ", z, 0.1f, Time.fixedDeltaTime);
            //animator.SetBool("IsMoving", isMoving);
        }
    }
}