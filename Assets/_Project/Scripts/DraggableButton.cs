using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class DraggableButton : MonoBehaviour
{
    private Rigidbody2D rb;
    private Camera mainCam;
    private bool isDragging;
    private Vector2 dragOffset;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.useFullKinematicContacts = true;
    }
    private void Update()
    {
        var pointer = Pointer.current;
        if (pointer == null) return;
        if (pointer.press.wasPressedThisFrame)
        {
            HandleDragStart(pointer.position.ReadValue());
        }
        else if (pointer.press.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }
    private void FixedUpdate()
    {
        if (isDragging && Pointer.current != null)
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Vector2 worldPos = mainCam.ScreenToWorldPoint(screenPos);
            Vector2 targetPos = worldPos + dragOffset;
            rb.position = targetPos;
        }
    }
    private void HandleDragStart(Vector2 screenPosition)
    {
        Vector2 worldPos = mainCam.ScreenToWorldPoint(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            isDragging = true;
            dragOffset = rb.position - worldPos;
        }
    }
}