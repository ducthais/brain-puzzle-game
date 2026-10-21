using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FallingTrap : MonoBehaviour
{
    [Header("Tốc độ sập (m/s)")]
    public float fallSpeed = 8f;

    private Rigidbody2D rb;
    private bool isStopped = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        rb.linearVelocity = new Vector2(0f, -fallSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        StopTrap(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        StopTrap(collider.gameObject);
    }

    private void StopTrap(GameObject hitObject)
    {
        if (isStopped) return;

        if (!hitObject.CompareTag("Player") && !hitObject.CompareTag("Trap") && !hitObject.CompareTag("DeathZone"))
        {
            ForceStop(); // Gọi hàm dừng
        }
    }

    // ==========================================
    // HÀM MỚI: Cho phép Cờ Vàng (hoặc nút bấm) bắt bẫy dừng lại giữa không trung
    public void ForceStop()
    {
        if (isStopped) return;
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; // Khóa cứng tại chỗ
        isStopped = true;
    }
}