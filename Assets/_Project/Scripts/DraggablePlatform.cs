using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // Kích hoạt hệ thống nhận diện Input mới chuẩn chỉnh cho dự án

public class DraggablePlatform : MonoBehaviour
{
    private bool isUnlocked = false; // MẶC ĐỊNH KHÓA: Vừa vào màn chơi người chơi sẽ chưa thể kéo
    private bool isDragging = false;
    private float initialYOffset;

    [Header("Platform Limits")]
    public float minY;

    [Header("Drag Area")]
    public Collider2D dragHandleCollider;

    void Start()
    {
        // Truy xuất cấu trúc dữ liệu an toàn bằng FirstOrDefault theo tiêu chuẩn hệ thống
        if (dragHandleCollider == null)
        {
            var childTransforms = transform.Cast<Transform>();
            var handleTransform = childTransforms.FirstOrDefault(t => t.name == "DragHandle");

            if (handleTransform != null)
            {
                var colliders = handleTransform.GetComponents<Collider2D>();
                dragHandleCollider = colliders.FirstOrDefault();
            }
        }
    }

    // Hàm public được gọi trực tiếp từ BoxAndStarController khi ngôi sao bay trúng đích
    public void UnlockPlatform()
    {
        isUnlocked = true;
        Debug.Log("✅ [NEW INPUT SYSTEM] Ngôi sao đã nhập vào chóp cỏ thành công! Đã kích hoạt quyền kéo thả.");
    }

    void Update()
    {
        // ĐIỀU KIỆN RÀNG BUỘC CỐT LÕI: Chưa kích hoạt ngôi sao hoặc thiếu collider tay nắm -> chặn đứng tương tác
        if (!isUnlocked || dragHandleCollider == null) return;

        bool isBegan = false;
        bool isMovedOrHeld = false;
        bool isEnded = false;
        Vector2 screenPos = Vector2.zero;
        int pointerId = -1;

        // 1. XỬ LÝ ĐA ĐIỂM TRÊN THIẾT BỊ ANDROID
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            var primaryTouch = Touchscreen.current.primaryTouch;
            isBegan = primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began;
            isMovedOrHeld = primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved ||
                            primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Stationary;
            isEnded = primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended ||
                      primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled;

            screenPos = primaryTouch.position.ReadValue();
            pointerId = primaryTouch.touchId.ReadValue();
        }
        // 2. XỬ LÝ CHUỘT TRÊN UNITY EDITOR (Để bạn Test nhanh)
        else if (Mouse.current != null)
        {
            isBegan = Mouse.current.leftButton.wasPressedThisFrame;
            isMovedOrHeld = Mouse.current.leftButton.isPressed;
            isEnded = Mouse.current.leftButton.wasReleasedThisFrame;

            screenPos = Mouse.current.position.ReadValue();
        }
        else
        {
            return;
        }

        // --- LUỒNG TÍNH TOÁN VÀ ĐIỀU KHIỂN ĐỊA HÌNH ---
        float cameraDistance = Mathf.Abs(Camera.main.transform.position.z);

        if (isBegan)
        {
            // Chống chạm xuyên thấu qua các nút bấm Giao diện UI (Pause, Nút di chuyển)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId)) return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cameraDistance));
            worldPos.z = 0;

            // Chỉ chấp nhận lệnh khi ngón tay/chuột chạm lọt lòng vào đúng vùng chóp cỏ DragHandle
            if (dragHandleCollider.OverlapPoint(worldPos))
            {
                isDragging = true;
                initialYOffset = transform.position.y - worldPos.y;
                Debug.Log("🎯 Đã chộp trúng chóp cỏ! Bắt đầu hành trình kéo mượt mà.");
            }
        }
        else if (isMovedOrHeld && isDragging)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cameraDistance));
            float newY = worldPos.y + initialYOffset;

            // Khóa giới hạn dưới để bệ không sụt xuống vực, hướng lên trên cho phép kéo dài vô hạn
            newY = Mathf.Max(newY, minY);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        else if (isEnded)
        {
            isDragging = false;
        }
    }
}