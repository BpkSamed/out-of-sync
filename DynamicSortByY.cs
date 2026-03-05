using UnityEngine;

public class DynamicSortByY : MonoBehaviour
{
     private SpriteRenderer spriteRenderer;
    private const int MIN_SORT_ORDER = -32767;
    private const int MAX_SORT_ORDER = 32767;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Hitung order seperti biasa
        int calculatedOrder = (int)(-transform.position.y * 10f);
        
        // Paksa nilainya agar selalu berada di dalam rentang aman!
        spriteRenderer.sortingOrder = Mathf.Clamp(calculatedOrder, MIN_SORT_ORDER, MAX_SORT_ORDER);
    }
}