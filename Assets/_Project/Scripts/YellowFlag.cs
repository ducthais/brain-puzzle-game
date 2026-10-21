using UnityEngine;

public class YellowFlag : MonoBehaviour
{
    [Header("Cài đặt Khóa (Dành cho các màn giải đố)")]
    [Tooltip("Nếu tích chọn, người chơi chạm vào sẽ không qua màn được cho đến khi cờ được mở khóa.")]
    public bool isLocked = false;

    // Hàm này sẽ được gọi bởi các vật thể khác (ví dụ: khu vực đặt thùng) để mở khóa cờ
    public void UnlockFlag()
    {
        isLocked = false;
        Debug.Log("Lá cờ đã được mở khóa! Sẵn sàng qua màn.");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem thứ vừa chạm vào cờ có đúng là Nhân vật (Player) hay không
        if (collision.CompareTag("Player"))
        {
            // Kiểm tra xem cờ có đang bị khóa không
            if (!isLocked)
            {
                // Cờ KHÔNG khóa -> Gọi hàm chiến thắng như bình thường
                Object.FindAnyObjectByType<LevelManager>().WinGame();
            }
            else
            {
                // Cờ ĐANG khóa -> Phản hồi lại cho người chơi biết
                Debug.Log("❌ Cờ đang bị khóa! Bạn phải hoàn thành nhiệm vụ trước.");
            }
        }
    }
}