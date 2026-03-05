// TempatKhusus.cs
using UnityEngine;

public class TempatKhusus : MonoBehaviour
{
    [Tooltip("ItemData yang BENAR untuk tempat ini.")]
    public ItemData itemYangDibutuhkan;

    [Tooltip("ItemData yang DIBERIKAN ke pemain jika item yang benar diletakkan.")]
    public ItemData itemHasil; // <-- TAMBAHKAN INI, isi di Inspector

    // Fungsi pengecekan kecocokan (berdasarkan nama)
    public bool ApakahItemCocok(ItemData itemUntukDicek)
    {
        return itemUntukDicek != null &&
               itemYangDibutuhkan != null &&
               itemUntukDicek.itemName == itemYangDibutuhkan.itemName;
    }

    // (Tidak perlu state lain di sini dengan logika trigger di WorldItem)
}