using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class DraggableCharacterV2 : MonoBehaviour
{
    private Vector2 offset; 
    private Rigidbody2D rb;
    private Collider2D col;
    private Camera mainCam; 
    
    private bool isDragging = false;
    private Vector2 targetPosition; 
    private float originalGravity; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        mainCam = Camera.main; 
        originalGravity = rb.gravityScale; 
    }

    void Update()
    {
        // THAY ĐỔI: Dùng Pointer thay cho Mouse
        if (Pointer.current == null) return;

        Vector2 pointerScreenPos = Pointer.current.position.ReadValue();
        Vector2 pointerWorldPos = mainCam.ScreenToWorldPoint(pointerScreenPos);

        // 1. KHI VỪA CHẠM NGÓN TAY / NHẤN CHUỘT
        if (Pointer.current.press.wasPressedThisFrame)
        {
            Collider2D hit = Physics2D.OverlapPoint(pointerWorldPos);
            if (hit == col) 
            {
                isDragging = true;
                rb.gravityScale = 0f;     
                rb.linearVelocity = Vector2.zero; 
                offset = rb.position - pointerWorldPos; 
            }
        }

        // 2. KHI ĐANG GIỮ VÀ KÉO
        if (isDragging && Pointer.current.press.isPressed)
        {
            targetPosition = pointerWorldPos + offset; 
        }

        // 3. KHI NHẤC NGÓN TAY / THẢ CHUỘT LÊN
        if (Pointer.current.press.wasReleasedThisFrame)
        {
            if (isDragging)
            {
                isDragging = false;
                rb.gravityScale = originalGravity; 
            }
        }
    }

    void FixedUpdate() 
    {
        if (isDragging)
        {
            rb.MovePosition(targetPosition); 
        }
    }
}