// MobileMovementController.cs (Dengan Tambahan Efek Suara Langkah Kaki)
using UnityEngine;

// Pastikan Player memiliki komponen AudioSource
[RequireComponent(typeof(Animator))] // Animator tetap ada dari setup 8 arah
[RequireComponent(typeof(AudioSource))] // Tambahkan ini
public class MobileMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Input Source (Opsional)")]
    public VirtualJoystick joystick;

    [Header("Sound Effects")] // <-- BAGIAN BARU
    [Tooltip("Satu atau lebih klip audio untuk suara langkah kaki. Akan dipilih acak jika lebih dari satu.")]
    public AudioClip[] footstepSounds;
    [Tooltip("Interval waktu antar suara langkah kaki saat bergerak (detik).")]
    public float footstepInterval = 0.4f; // Sesuaikan nilai ini agar pas dengan animasi jalan

    private Vector2 moveInput = Vector2.zero;
    private Animator animator;
    private AudioSource audioSource; // <-- BARU: Referensi ke AudioSource

    private float lastMoveX = 0f;
    private float lastMoveY = -1f;

    private float footstepTimer = 0f; // <-- BARU: Timer untuk interval langkah kaki

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null) {
            Debug.LogError("Animator component tidak ditemukan pada Player!", this.gameObject);
        }

        // --- BAGIAN BARU: Dapatkan AudioSource ---
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            Debug.LogError("AudioSource component tidak ditemukan pada Player! Tidak akan ada suara langkah kaki.", this.gameObject);
        } else {
            // Pastikan setting AudioSource yang penting sudah benar
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
        // --- AKHIR BAGIAN BARU ---
    }

    void Start()
    {
        if (joystick == null) {
            joystick = FindObjectOfType<VirtualJoystick>();
            // (Pesan warning jika joystick tidak ada)
        }

        if (animator != null) {
            animator.SetFloat("MoveX", lastMoveX);
            animator.SetFloat("MoveY", lastMoveY);
            animator.SetBool("IsMoving", false);
            animator.speed = 0; // Mulai dengan animasi berhenti
        }
    }

    void Update()
    {
        // 1. Dapatkan Input
        if (joystick != null && joystick.gameObject.activeInHierarchy) {
            moveInput = joystick.GetInputVector();
        } else {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(horizontalInput, verticalInput);
            if (moveInput.sqrMagnitude > 1) {
                moveInput.Normalize();
            }
        }

        bool isCurrentlyMoving = moveInput.sqrMagnitude > 0.01f;

        // 2. Atur Parameter Animator
        /*if (animator != null)
        {
            animator.SetBool("IsMoving", isCurrentlyMoving);

            if (isCurrentlyMoving)
            {
                animator.SetFloat("MoveX", moveInput.x);
                animator.SetFloat("MoveY", moveInput.y);
                lastMoveX = moveInput.x;
                lastMoveY = moveInput.y;
                animator.speed = 1;
            }
            else
            {
                animator.SetFloat("MoveX", lastMoveX);
                animator.SetFloat("MoveY", lastMoveY);
                animator.speed = 0;
            }
        }*/

        // 3. Gerakkan Karakter
        if (isCurrentlyMoving)
        {
            Vector3 moveDelta = new Vector3(moveInput.x, moveInput.y, 0f) * moveSpeed * Time.deltaTime;
            transform.position += moveDelta;

            // --- BAGIAN BARU: Logika Suara Langkah Kaki ---
            HandleFootstepSounds();
            // --- AKHIR BAGIAN BARU ---
        }
        else
        {
            // Saat tidak bergerak, reset timer agar langkah kaki langsung bunyi saat mulai jalan lagi
            footstepTimer = 0f;
        }
    }

    // --- FUNGSI BARU UNTUK MENANGANI SUARA LANGKAH KAKI ---
    void HandleFootstepSounds()
    {
        if (audioSource == null || footstepSounds == null || footstepSounds.Length == 0)
        {
            return; // Tidak ada AudioSource atau tidak ada klip suara langkah kaki
        }

        footstepTimer -= Time.deltaTime; // Kurangi timer

        if (footstepTimer <= 0f)
        {
            // Pilih suara langkah kaki secara acak jika ada lebih dari satu
            AudioClip clipToPlay = footstepSounds[Random.Range(0, footstepSounds.Length)];

            if (clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay); // Mainkan suara sekali
            }

            footstepTimer = footstepInterval; // Reset timer ke interval awal
        }
    }
    // --- AKHIR FUNGSI BARU ---
}