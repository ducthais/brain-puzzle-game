using UnityEngine;
// 1. Phải gọi thư viện Input System mới
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider2D))]
public class RotatableBridge : MonoBehaviour
{
    [Header("Cài đặt Nghiêng Cầu")]
    public float rotationSpeed = 5f;
    public float maxTiltAngle = 40f;

    private Vector3 _startPointerPos;
    private float _startZAngle;
    private bool _isDragging = false;
    private float _rotationDirection = 1f;

    void Update()
    {
        // Đảm bảo hệ thống nhận diện được thiết bị tương tác (chuột/màn hình cảm ứng)
        if (Pointer.current == null) return;

        // 1. Khi người chơi nhấn chuột / chạm ngón tay xuống (Tương đương GetMouseButtonDown)
        if (Pointer.current.press.wasPressedThisFrame)
        {
            // Lấy tọa độ chạm trên màn hình
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

            // Quét xem tia chạm có trúng cây cầu không
            Collider2D hitCollider = Physics2D.OverlapPoint(worldPos);

            if (hitCollider != null && hitCollider.gameObject == this.gameObject)
            {
                _isDragging = true;
                _startPointerPos = worldPos;
                _startZAngle = transform.eulerAngles.z;

                if (_startZAngle > 180f) _startZAngle -= 360f;

                // Kiểm tra xem người chơi chạm vào nửa trái hay nửa phải của cây cầu
                if (worldPos.x < transform.position.x)
                {
                    _rotationDirection = -1f; // Chạm trái -> Đảo chiều
                }
                else
                {
                    _rotationDirection = 1f;  // Chạm phải -> Giữ nguyên
                }
            }
        }

        // 2. Trong lúc đang giữ và kéo vuốt (Tương đương GetMouseButton)
        else if (Pointer.current.press.isPressed && _isDragging)
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Vector3 currentWorldPos = Camera.main.ScreenToWorldPoint(screenPos);

            // Tính toán khoảng cách vuốt ngón tay trục Y
            float dragY = currentWorldPos.y - _startPointerPos.y;

            float targetAngle = _startZAngle + (dragY * rotationSpeed * _rotationDirection);
            targetAngle = Mathf.Clamp(targetAngle, -maxTiltAngle, maxTiltAngle);

            transform.rotation = Quaternion.Euler(0, 0, targetAngle);
        }

        // 3. Khi nhấc ngón tay / nhả chuột ra (Tương đương GetMouseButtonUp)
        else if (Pointer.current.press.wasReleasedThisFrame)
        {
            _isDragging = false;
        }
    }
}