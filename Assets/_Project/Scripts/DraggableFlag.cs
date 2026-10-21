using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class DraggableFlag : MonoBehaviour
{
    // Mảng chứa tất cả collider của object cha và các object con
    private Collider2D[] allColliders;
    private bool isDragging = false;
    private Vector3 offset;

    private void Start()
    {
        // Lấy toàn bộ Collider2D có trên GameObject này và tất cả các GameObjects con
        allColliders = GetComponentsInChildren<Collider2D>();
    }

    private void Update()
    {
        if (Pointer.current == null) return;

        Vector2 pointerScreenPos = Pointer.current.position.ReadValue();

        // Chuyển đổi toạ độ màn hình sang toạ độ thế giới
        Vector3 pointerWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(pointerScreenPos.x, pointerScreenPos.y, Camera.main.nearClipPlane));

        // 1. KHI VỪA NHẤP CHUỘT
        if (Pointer.current.press.wasPressedThisFrame)
        {
            // Lấy TẤT CẢ các collider đang nằm tại vị trí chuột
            Collider2D[] hits = Physics2D.OverlapPointAll(pointerWorldPos);

            bool hitThisFlag = false;

            // Kiểm tra xem chuột có chạm vào bất kỳ collider nào thuộc cụm cờ này không
            foreach (var hit in hits)
            {
                if (allColliders.Contains(hit))
                {
                    hitThisFlag = true;
                    break; // Chỉ cần chạm vào 1 phần của cờ là đủ
                }
            }

            if (hitThisFlag)
            {
                isDragging = true;
                // Tính khoảng cách từ tâm object cha đến vị trí chuột để giữ cờ không bị giật về con trỏ
                offset = transform.position - pointerWorldPos;
            }
        }

        // 2. KHI ĐANG GIỮ VÀ KÉO CỜ
        if (isDragging && Pointer.current.press.isPressed)
        {
            // Di chuyển object cha, các object con (sprites) sẽ tự động đi theo
            transform.position = pointerWorldPos + offset;
        }

        // 3. KHI NHẢ TAY
        if (Pointer.current.press.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }
}