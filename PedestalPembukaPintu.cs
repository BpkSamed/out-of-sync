// PedestalPembukaPintu.cs (Versi 3D)
using UnityEngine;
using UnityEngine.UI; // Untuk Button jika tombolnya adalah Button UI
using System.Collections.Generic; // Untuk HashSet

[RequireComponent(typeof(Collider))] // Diubah dari Collider2D
public class PedestalPembukaPintu : MonoBehaviour
{
    [Header("Pengaturan Kunci & Pintu")]
    [Tooltip("ItemData yang dianggap sebagai kunci untuk pedestal ini.")]
    public ItemData itemKunciData;
    [Tooltip("GameObject Pintu yang akan dinonaktifkan/dibuka.")]
    public GameObject pintuYangAkanDibuka;

    [Header("Tombol Buka Pintu (UI)")]
    [Tooltip("GameObject Tombol UI 'Buka Pintu' yang akan dikontrol oleh pedestal ini.")]
    public GameObject tombolBukaPintuUI; // Harus punya CanvasGroup
    [Tooltip("Tingkat transparansi tombol saat nonaktif.")]
    public float alphaTombolNonaktif = 0.5f;
    private CanvasGroup tombolBukaPintuCanvasGroup;
    private Button uiButtonComponent; // Komponen Button dari tombolBukaPintuUI

    [Header("Status & SFX")]
    [Tooltip("ID Unik untuk pedestal dan pintu ini agar status terbukanya diingat per sesi.")]
    public string uniquePedestalID; // Contoh: "PEDESTAL_PINTU_UTAMA"
    public AudioClip suaraPintuTerbuka;
    [Range(0f, 1f)]
    public float volumeSuaraPintu = 0.8f;

    // Menyimpan ID pedestal yang kuncinya sudah diletakkan & pintunya sudah dibuka
    private static HashSet<string> pedestalSudahDipakaiDanPintuTerbuka = new HashSet<string>();

    private bool kunciSudahDiletakkan = false; // Status instance ini

    void Start()
    {
        // Validasi (Tidak ada perubahan di sini)
        if (itemKunciData == null) { Debug.LogError($"Pedestal '{gameObject.name}': ItemKunciData belum di-set!", gameObject); enabled = false; return; }
        if (pintuYangAkanDibuka == null) { Debug.LogError($"Pedestal '{gameObject.name}': PintuYangAkanDibuka belum di-set!", gameObject); enabled = false; return; }
        if (tombolBukaPintuUI == null) { Debug.LogError($"Pedestal '{gameObject.name}': TombolBukaPintuUI belum di-set!", gameObject); enabled = false; return; }
        if (string.IsNullOrEmpty(uniquePedestalID)) { Debug.LogError($"Pedestal '{gameObject.name}': UniquePedestalID belum di-set!", gameObject); enabled = false; return; }

        // Menggunakan Collider 3D
        Collider col = GetComponent<Collider>(); // Diubah dari Collider2D
        if (!col.isTrigger) { col.isTrigger = true; } // Pastikan trigger

        // Setup Tombol Buka Pintu UI (Tidak ada perubahan di sini)
        tombolBukaPintuCanvasGroup = tombolBukaPintuUI.GetComponent<CanvasGroup>();
        if (tombolBukaPintuCanvasGroup == null) {
            Debug.LogError($"Pedestal '{gameObject.name}': TombolBukaPintuUI tidak memiliki komponen CanvasGroup!", tombolBukaPintuUI);
            uiButtonComponent = tombolBukaPintuUI.GetComponent<Button>();
            if(uiButtonComponent != null) uiButtonComponent.interactable = false; else tombolBukaPintuUI.SetActive(false);
        }

        uiButtonComponent = tombolBukaPintuUI.GetComponent<Button>();
        if (uiButtonComponent != null) {
            uiButtonComponent.onClick.RemoveAllListeners(); // Hapus listener lama
            uiButtonComponent.onClick.AddListener(OnTombolBukaPintuDitekan);
        } else {
            Debug.LogError($"Pedestal '{gameObject.name}': TombolBukaPintuUI tidak memiliki komponen Button!", tombolBukaPintuUI);
        }

        // Cek status (Tidak ada perubahan di sini)
        if (pedestalSudahDipakaiDanPintuTerbuka.Contains(uniquePedestalID))
        {
            kunciSudahDiletakkan = true; 
            pintuYangAkanDibuka.SetActive(false); 
            SetTombolBukaPintuState(false, false); 
            Debug.Log($"Pedestal [{uniquePedestalID}]: Pintu sudah terbuka dari data sesi ini.");
        }
        else
        {
            SetTombolBukaPintuState(false, false);
        }
    }

    void OnDestroy() {
        // (Tidak ada perubahan di sini)
        if (uiButtonComponent != null) {
            uiButtonComponent.onClick.RemoveListener(OnTombolBukaPintuDitekan);
        }
    }

    // Menggunakan Trigger 3D
    void OnTriggerEnter(Collider other) // Diubah dari OnTriggerEnter2D(Collider2D other)
    {
        if (kunciSudahDiletakkan) 
        {
            // Logika ini tetap sama, 'other' sekarang adalah Collider 3D
            WorldItem keyDroppedAgain = other.GetComponent<WorldItem>();
            if (keyDroppedAgain != null && keyDroppedAgain.itemData == itemKunciData) {
                Destroy(other.gameObject);
            }
            return;
        }

        // Logika ini tetap sama, 'other' sekarang adalah Collider 3D
        WorldItem worldItemKunci = other.GetComponent<WorldItem>(); 
        if (worldItemKunci != null && worldItemKunci.itemData == itemKunciData)
        {
            Debug.Log($"Pedestal [{uniquePedestalID}]: KUNCI BENAR ({worldItemKunci.itemData.itemName}) diterima!");
            Destroy(worldItemKunci.gameObject); // Hancurkan item kunci

            kunciSudahDiletakkan = true;
            SetTombolBukaPintuState(true, true); // Aktifkan tombol "Buka Pintu"

            Debug.Log($"Pedestal [{uniquePedestalID}]: Kunci diletakkan. Tombol Buka Pintu sekarang aktif.");
        }
    }

    // Fungsi untuk mengatur state tombol buka pintu (Tidak ada perubahan di sini)
    void SetTombolBukaPintuState(bool aktif, bool interactable)
    {
        if (tombolBukaPintuCanvasGroup != null)
        {
            tombolBukaPintuCanvasGroup.alpha = aktif ? 1f : alphaTombolNonaktif;
            tombolBukaPintuCanvasGroup.interactable = interactable;
            tombolBukaPintuCanvasGroup.blocksRaycasts = interactable;
        } else if (tombolBukaPintuUI != null) { 
            tombolBukaPintuUI.SetActive(aktif); 
            Button btn = tombolBukaPintuUI.GetComponent<Button>();
            if(btn != null) btn.interactable = interactable;
        }
    }

    // Fungsi ini dipanggil oleh OnClick() UI (Tidak ada perubahan di sini)
    public void OnTombolBukaPintuDitekan()
    {
        if (!kunciSudahDiletakkan) 
        {
            Debug.LogWarning($"Pedestal [{uniquePedestalID}]: Tombol Buka Pintu ditekan tapi kunci belum diletakkan?");
            return;
        }

        Debug.Log($"Pedestal [{uniquePedestalID}]: Tombol Buka Pintu ditekan. Membuka pintu...");

        // 1. Nonaktifkan (buka) pintu
        if (pintuYangAkanDibuka != null)
        {
            pintuYangAkanDibuka.SetActive(false);
        }

        // 2. Mainkan SFX Pintu Terbuka
        if (SfxToggleButton.isSfxMuted == false && suaraPintuTerbuka != null)
        {
            AudioSource.PlayClipAtPoint(suaraPintuTerbuka, transform.position, volumeSuaraPintu);
        }

        // 3. Tandai bahwa pintu ini sudah dibuka permanen
        pedestalSudahDipakaiDanPintuTerbuka.Add(uniquePedestalID);
        SetTombolBukaPintuState(false, false); // Tombol kembali nonaktif/transparan
    }

    // (Opsional) Fungsi reset (Tidak ada perubahan di sini)
    public static void ResetSemuaPedestalPintu()
    {
        pedestalSudahDipakaiDanPintuTerbuka.Clear();
        Debug.Log("Status semua PedestalPembukaPintu telah direset.");
    }
}