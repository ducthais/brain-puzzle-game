using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class DraggableGround : MonoBehaviour
{
    private Vector3 offset;
    private Collider2D col;
    private bool isDragging = false;

    [Header("Cấu hình di chuyển")]
    [Tooltip("Khóa trục Y giúp người chơi chỉ kéo đất sang ngang, không bị lệch lên xuống")]
    [SerializeField] private bool lockYAxis = true;

    void Start()
    {
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (Pointer.current == null) return;

        Vector2 pointerScreenPos = Pointer.current.position.ReadValue();
        Vector3 pointerWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(pointerScreenPos.x, pointerScreenPos.y, Camera.main.nearClipPlane));

        if (Pointer.current.press.wasPressedThisFrame)
        {
            Collider2D hit = Physics2D.OverlapPoint(pointerWorldPos);
            if (hit == col)
            {
                isDragging = true;
                offset = transform.position - pointerWorldPos;
            }
        }

        if (isDragging && Pointer.current.press.isPressed)
        {
            Vector3 targetPosition = pointerWorldPos + offset;

            if (lockYAxis)
            {
                transform.position = new Vector3(targetPosition.x, transform.position.y, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
            }
        }

        if (Pointer.current.press.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }
}