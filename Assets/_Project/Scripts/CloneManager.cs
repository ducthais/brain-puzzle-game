using UnityEngine;

public class CloneManager : MonoBehaviour
{
    [Header("Cài đặt Bản sao")]
    [Tooltip("Kéo prefab PlayerClone vào đây")]
    public GameObject clonePrefab; 
    
    [Tooltip("Điểm bắt đầu để đưa Player về")]
    public Transform startPoint;   

    [Header("Tham chiếu Player")]
    [Tooltip("Kéo GameObject Player vào đây")]
    public GameObject player;      

    // Hàm này sẽ được gọi khi ấn nút Reset
    public void CreateCloneAndRespawn()
    
    {
        Debug.Log("🔥 HÀM TẠO CLONE ĐÃ ĐƯỢC CHẠY!"); 

        // 1. Tạo ra bản sao tại đúng vị trí...
        // 1. Tạo ra bản sao tại đúng vị trí và góc quay hiện tại của Player
        Instantiate(clonePrefab, player.transform.position, player.transform.rotation);

        // 2. Dịch chuyển Player thật về lại điểm xuất phát
        player.transform.position = startPoint.position;

        // 3. Reset vận tốc của Player để tránh lỗi văng đi do gia tốc cũ còn dư
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero; 
        }
    }
}