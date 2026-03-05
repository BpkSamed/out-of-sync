// VirtualJoystick.cs (Pendekatan Kalibrasi Ulang pada Sentuhan Awal)
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform handleRectTransform;
    private Vector2 initialTouchPosition;

    public RectTransform backgroundRectTransform;
    public float joystickRadius = 100f; // Beri nilai default yang masuk akal

    private Vector2 moveInputVector = Vector2.zero;
    private bool firstDragFrame = false; // Tandai frame drag pertama setelah sentuhan

    void Start()
    {
        if (backgroundRectTransform == null) backgroundRectTransform = GetComponent<RectTransform>();
        if (handleRectTransform == null && transform.childCount > 0) handleRectTransform = transform.GetChild(0).GetComponent<RectTransform>();

        if (handleRectTransform == null || backgroundRectTransform == null) {
            Debug.LogError("Handle atau Background RectTransform belum di-assign atau tidak ditemukan!");
            enabled = false; return;
        }
        // Pastikan pivot background di tengah
        if (backgroundRectTransform.pivot.x != 0.5f || backgroundRectTransform.pivot.y != 0.5f) {
             Debug.LogError("PIVOT JoystickBackground HARUS (0.5, 0.5)!", backgroundRectTransform);
        }
        // Pastikan anchor dan pivot handle di tengah
        handleRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        handleRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        handleRectTransform.pivot = new Vector2(0.5f, 0.5f);

        if (joystickRadius <= float.Epsilon) joystickRadius = backgroundRectTransform.sizeDelta.x / 2f;
        if (joystickRadius <= float.Epsilon) joystickRadius = 50f; // Fallback

        ResetHandleAndInput();
    }

    

    public void OnDrag(PointerEventData eventData)
{
    Vector2 localPoint;
    Camera eventCam = (GetComponentInParent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay) ? null : eventData.pressEventCamera ?? Camera.main;

    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            backgroundRectTransform, eventData.position, eventCam, out localPoint))
    {
        if (firstDragFrame)
        {
            handleRectTransform.anchoredPosition = Vector2.zero;
            moveInputVector = Vector2.zero;
            firstDragFrame = false;
            return;
        }

        // Adjust the local point relative to the initial touch position
        Vector2 direction = localPoint - initialTouchPosition;

        if (direction.magnitude > joystickRadius) {
            direction = direction.normalized * joystickRadius;
        }
        handleRectTransform.anchoredPosition = direction;

        if (joystickRadius > float.Epsilon) {
            moveInputVector = direction / joystickRadius;
        } else {
            moveInputVector = Vector2.zero;
        }
    }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetHandleAndInput();
    }

    private void ResetHandleAndInput()
    {
        handleRectTransform.anchoredPosition = Vector2.zero;
        moveInputVector = Vector2.zero;
        firstDragFrame = false; // Pastikan direset juga di sini
        // Debug.Log("PointerUp: Handle reset.");
    }

    public Vector2 GetInputVector() { return moveInputVector; }
    public void OnPointerDown(PointerEventData eventData)
{
    Vector2 localPoint;
    Camera eventCam = (GetComponentInParent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay) ? null : eventData.pressEventCamera ?? Camera.main;
    
    // Store the initial touch position in local space
    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            backgroundRectTransform, eventData.position, eventCam, out localPoint))
    {
        initialTouchPosition = localPoint;
    }
    
    firstDragFrame = true;
    OnDrag(eventData);
}
}
