using UnityEngine;

public class follow_player : MonoBehaviour
{
    public Transform target; // Objek pemain yang ingin diikuti kamera
    public float smoothing = 5f; // Kecepatan kamera mengikuti pemain

    private Vector3 offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        offset = transform.position - target.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
       Vector3 targetCamPos = target.position + offset;

        // Pindahkan kamera secara halus ke posisi target
        transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
     
    }
}
