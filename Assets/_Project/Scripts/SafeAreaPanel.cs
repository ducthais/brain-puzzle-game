using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class SafeAreaPanel : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea = Rect.zero;
    private Vector2 lastScreenSize = Vector2.zero;
    private ScreenOrientation lastOrientation;

    void Awake()
    {
        lastOrientation = Screen.orientation;
        rectTransform = GetComponent<RectTransform>();

        // Đảm bảo Anchor cố định toàn màn hình để tính toán tỷ lệ chính xác
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        ApplySafeArea();
    }

    void Update()
    {
        // Kiểm tra thay đổi cấu hình màn hình (độ phân giải, xoay màn hình) để cập nhật động
        if (lastSafeArea != Screen.safeArea ||
            lastScreenSize.x != Screen.width ||
            lastScreenSize.y != Screen.height ||
            Screen.orientation != lastOrientation)
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        // Lưu lại trạng thái kiểm tra hiện tại
        lastSafeArea = safeArea;
        lastScreenSize = new Vector2(Screen.width, Screen.height);
        lastOrientation = Screen.orientation;

        // Bỏ qua xử lý nếu dữ liệu trả về lỗi từ hệ thống
        if (Screen.width == 0 || Screen.height == 0) return;

        // Chuyển đổi tọa độ pixel của Safe Area sang tỷ lệ từ 0.0 -> 1.0 ứng với Canvas
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Gán lại Anchor mới ép các thành phần UI Panel co cụm vào vùng an toàn
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}