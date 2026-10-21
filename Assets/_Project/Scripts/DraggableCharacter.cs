using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class DraggableCharacter : MonoBehaviour
{
    private Vector3 offset;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool isDragging = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        // Đảm bảo có thiết bị trỏ (chuột, cảm ứng, hoặc bút)
        if (Pointer.current == null) return;

        // Lấy tọa độ trỏ và chuyển sang tọa độ thế giới
        Vector2 pointerScreenPos = Pointer.current.position.ReadValue();
        Vector3 pointerWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(pointerScreenPos.x, pointerScreenPos.y, Camera.main.nearClipPlane));

        // 1. KHI VỪA NHẤN XUỐNG (Click chuột trái hoặc Ngón tay chạm)
        if (Pointer.current.press.wasPressedThisFrame)
        {
            Collider2D hit = Physics2D.OverlapPoint(pointerWorldPos);
            if (hit == col)
            {
                isDragging = true;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
                offset = transform.position - pointerWorldPos;
            }
        }

        // 2. KHI ĐANG GIỮ VÀ KÉO
        if (isDragging && Pointer.current.press.isPressed)
        {
            transform.position = pointerWorldPos + offset;
        }

        // 3. KHI NHẢ RA (Nhả chuột hoặc Nhấc ngón tay)
        if (Pointer.current.press.wasReleasedThisFrame)
        {
            if (isDragging)
            {
                isDragging = false;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }
    }
}