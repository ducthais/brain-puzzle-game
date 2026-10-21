using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class VerticalDraggable : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isDragging = false;
    private float offsetY;
    
    [Header("Giới hạn độ cao (Tránh kéo ra ngoài màn hình)")]
    public float minY = -6f;
    public float maxY = 3f;

    private float fixedXPosition; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        fixedXPosition = transform.position.x; 
    }

    void Update()
    {
        // THAY ĐỔI: Gom chung xử lý thành Pointer
        if (Pointer.current == null) return;

        Vector2 pointerPosition = Pointer.current.position.ReadValue();
        bool isPointerDown = Pointer.current.press.wasPressedThisFrame;
        bool isPointerPressing = Pointer.current.press.isPressed;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(pointerPosition);

        // Bắt đầu chạm vào khối đất
        if (isPointerDown)
        {
            Collider2D hitCollider = Physics2D.OverlapPoint(worldPos);
            if (hitCollider != null && hitCollider.gameObject == gameObject)
            {
                isDragging = true;
                offsetY = transform.position.y - worldPos.y;
            }
        }

        // Thả tay ra
        if (!isPointerPressing)
        {
            isDragging = false;
        }

        // Đang giữ và kéo
        if (isDragging)
        {
            float targetY = worldPos.y + offsetY;
            
            // Giới hạn không cho kéo quá cao hoặc quá thấp
            targetY = Mathf.Clamp(targetY, minY, maxY);

            rb.position = new Vector2(fixedXPosition, targetY);
        }
    }
}