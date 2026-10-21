using UnityEngine;

public class DropZone : MonoBehaviour
{
    [Tooltip("Kéo đối tượng Lá cờ ngoài Scene thả vào ô này")]
    public YellowFlag endFlag;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi chiếc hộp khổng lồ trượt vào vùng này
        if (collision.CompareTag("GiantBox"))
        {
            Debug.Log("Chiếc hộp đã vào đúng vị trí.");

            // 1. Khóa cứng chiếc hộp tại chỗ
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            // 2. Gửi lệnh mở khóa cho Lá cờ
            if (endFlag != null)
            {
                endFlag.UnlockFlag();
            }
        }
    }
}