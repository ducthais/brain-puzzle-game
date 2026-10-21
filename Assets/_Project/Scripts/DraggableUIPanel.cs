using UnityEngine;
using UnityEngine.EventSystems; // Bắt buộc phải có để dùng IDragHandler

public class DraggableUIPanel : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;

    [Header("Tham chiếu tới Môi trường")]
    [Tooltip("Kéo GameObject chứa con đường ngầm và lá cờ thật vào đây")]
    public GameObject hiddenPathGroup;

    [Header("Cấu hình Kéo trượt")]
    [Tooltip("Khoảng cách cần kéo xuống để kích hoạt (Ví dụ: -400)")]
    public float revealThreshold = -400f;

    private bool isRevealed = false;

    void Start()
    {
        // Lấy component quản lý tọa độ của UI
        rectTransform = GetComponent<RectTransform>();

        // Đảm bảo đường ngầm bị ẩn khi mới bắt đầu
        if (hiddenPathGroup != null)
        {
            hiddenPathGroup.SetActive(false);
        }
    }

    // Hàm này chạy liên tục khi ngón tay/chuột đang giữ và di chuyển trên Panel
    public void OnDrag(PointerEventData eventData)
    {
        if (isRevealed) return;

        // Cập nhật vị trí UI. Chỉ thay đổi trục Y, khóa trục X để Panel trượt thẳng đứng
        rectTransform.anchoredPosition += new Vector2(0, eventData.delta.y);

        // Kiểm tra xem Panel đã bị kéo xuống đủ sâu chưa
        // (anchoredPosition.y mang giá trị âm khi trượt xuống dưới mốc ban đầu)
        if (rectTransform.anchoredPosition.y <= revealThreshold)
        {
            TriggerSecretPath();
        }
    }

    // Hàm này chạy khi người chơi nhấc ngón tay/nhả chuột ra
    public void OnEndDrag(PointerEventData eventData)
    {
        if (isRevealed) return;

        // Tùy chọn bổ sung: Nếu kéo chưa đủ sâu mà thả tay ra, tự động đưa UI về chỗ cũ
        if (rectTransform.anchoredPosition.y > revealThreshold)
        {
            rectTransform.anchoredPosition = Vector2.zero; // Trả về tọa độ ban đầu (Tâm)
        }
    }

    // Hàm xử lý khi thành công
    private void TriggerSecretPath()
    {
        isRevealed = true;

        // 1. Hiện con đường ngầm và lá cờ
        if (hiddenPathGroup != null)
        {
            hiddenPathGroup.SetActive(true);
        }

        // 2. Tắt luôn cái bảng thông báo này đi để người chơi thấy đường
        gameObject.SetActive(false);
    }
}