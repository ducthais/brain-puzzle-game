using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Mục Tiêu Cần Theo Dõi")]
    public Transform target;

    [Header("Cài đặt Độ Mượt")]
    public float smoothSpeed = 10f;
    public Vector3 offset = new Vector3(0f, -2f, -10f);

    [Header("Khóa Trục ngang")]
    public bool lockXAxis = true;

    [Header("Cài đặt Đi Xuyên Lề Trái (Màn 19)")]
    [Tooltip("Chỉ đuổi theo khi NVC vượt qua tọa độ X này về bên trái. Để -999 nếu muốn đuổi theo ngay.")]
    public float startFollowLeftX = -12f; // <-- CHỐT CHẶN TRỤC NGANG MỚI THÊM

    [Header("Cài đặt Leap of Faith (Trục Y)")]
    public float startFollowY = -1f;

    [Header("Giới Hạn Đáy")]
    public float minY = -5f;

    private float _initialX;
    private float _initialY;

    private void Start()
    {
        _initialX = transform.position.x;
        _initialY = transform.position.y;

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = transform.position;

        // 1. XỬ LÝ TRỤC X (NGANG)
        if (lockXAxis)
        {
            desiredPosition.x = _initialX; // Khóa cứng
        }
        else
        {
            // BÍ QUYẾT Ở ĐÂY: NVC đi quá mốc X (về bên trái) thì Camera mới bừng tỉnh chạy theo
            if (target.position.x < startFollowLeftX)
            {
                desiredPosition.x = target.position.x + offset.x;
            }
            else
            {
                // Vẫn ở trong vùng tầm nhìn ban đầu -> Camera giả vờ đứng im không biết gì
                desiredPosition.x = _initialX;
            }
        }

        // 2. XỬ LÝ TRỤC Y (DỌC)
        if (target.position.y < startFollowY)
        {
            float targetY = target.position.y + offset.y;
            desiredPosition.y = Mathf.Max(targetY, minY);
        }
        else
        {
            desiredPosition.y = _initialY;
        }

        // Lerp mượt mà tạo hiệu ứng điện ảnh
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}