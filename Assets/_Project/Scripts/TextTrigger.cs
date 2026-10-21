using UnityEngine;

public class TextTrigger : MonoBehaviour
{
    [Header("Cấu hình UI")]
    [SerializeField] private GameObject winCanvas; // Kéo WinCanvas từ Hierarchy vào đây

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem có phải Nhân vật chính chạm vào không
        if (collision.CompareTag("Player"))
        {
            // Thắng qua LevelManager để lưu tiến trình, mở khóa màn sau và tính sao
            LevelManager levelManager = FindAnyObjectByType<LevelManager>();
            if (levelManager != null)
            {
                if (winCanvas != null) winCanvas.SetActive(true);
                levelManager.WinGame();
                return;
            }

            // Dự phòng khi scene thiếu LevelManager: chỉ hiện Canvas chiến thắng
            if (winCanvas != null)
            {
                winCanvas.SetActive(true);
                Debug.Log("Đã chạm vào chữ lá cờ! Hiện WinCanvas.");

                // (Tùy chọn) Nếu muốn nhân vật dừng di chuyển hoặc đóng băng trò chơi:
                // Time.timeScale = 0f; 
            }
            else
            {
                Debug.LogError("Chưa kéo WinCanvas vào Script TextTrigger!");
            }
        }
    }
}