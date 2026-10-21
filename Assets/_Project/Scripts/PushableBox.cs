using UnityEngine;

// Đảm bảo luôn có Rigidbody2D để code hoạt động
[RequireComponent(typeof(Rigidbody2D))]
public class PushableBox : MonoBehaviour
{
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        // Thiết lập ban đầu: Khóa xoay và khóa vị trí ngang
        LockBox();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Chỉ xử lý nếu đối tượng chạm vào là Người chơi
        if (collision.gameObject.CompareTag("Player"))
        {
            // Lấy điểm chạm đầu tiên mà không tạo rác bộ nhớ
            ContactPoint2D contact = collision.GetContact(0);

            // KIỂM TRA HƯỚNG ĐẨY:
            // normal.x > 0.1: Đang có lực đẩy từ 2 bên hông
            // normal.y < 0.9: Không phải là đang dẫm mạnh lên đầu hộp
            if (Mathf.Abs(contact.normal.x) > 0.1f && Mathf.Abs(contact.normal.y) < 0.9f)
            {
                UnlockBox();

                // CHỐNG BAY LÊN:
                // Nếu hộp có xu hướng bị kéo lên (vận tốc Y > 0), ép nó về 0
                if (_rb.linearVelocity.y > 0.01f)
                {
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            LockBox();
        }
    }

    private void LockBox()
    {
        // Hãm phanh vận tốc ngang và khóa trục X để hộp nằm im
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    private void UnlockBox()
    {
        // Mở khóa trục X để có thể đẩy đi, vẫn giữ khóa xoay (FreezeRotation)
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}