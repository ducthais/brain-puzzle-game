using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class GiantBox : MonoBehaviour
{
    [Tooltip("Kéo đối tượng cây cầu vào đây để hộp theo dõi góc nghiêng")]
    public Transform bridgeTransform;

    [Tooltip("Góc nghiêng tối thiểu (độ) để hộp bắt đầu mở khóa và trượt")]
    public float tiltThreshold = 5f;

    private Rigidbody2D _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        // Luôn khóa xoay để chiếc hộp không bị lăn lộn nhào khi trượt
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        if (bridgeTransform == null) return;

        // Chuyển đổi góc xoay của cầu về dạng [-180, 180]
        float currentAngle = bridgeTransform.eulerAngles.z;
        if (currentAngle > 180f) currentAngle -= 360f;

        // KIỂM TRA ĐỘ NGHIÊNG CỦA CẦU
        if (Mathf.Abs(currentAngle) < tiltThreshold)
        {
            // Nếu cầu đang phẳng: KHÓA TRỤC X (Không cho đẩy trái/phải), vẫn giữ trục Y để hộp chịu trọng lực rơi sát mặt cầu
            _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            // Nếu cầu đã nghiêng đủ độ: MỞ KHÓA TRỤC X để lực hấp dẫn tự kéo hộp trượt đi
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}