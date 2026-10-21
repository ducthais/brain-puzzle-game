using UnityEngine;

public class MysteryBox : MonoBehaviour
{
    [Header("--- KẾT NỐI VỚI MÔI TRƯỜNG ---")]
    [Tooltip("Kéo Nền đất tàng hình (HiddenBridge) vào đây")]
    public GameObject hiddenPlatform;

    [Header("--- TRẠNG THÁI HỘP ---")]
    [Tooltip("Hình ảnh hộp sau khi bị đập (Ví dụ: hộp rỗng, hộp xám)")]
    public Sprite emptyBoxSprite;

    private bool _isHit = false; // Ngăn người chơi đập hộp nhiều lần
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // Tự động TẮT nền đất khi bắt đầu game
        if (hiddenPlatform != null)
        {
            hiddenPlatform.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Nếu hộp đã bị kích hoạt rồi thì bỏ qua
        if (_isHit) return;

        // 1. Kiểm tra xem có phải nhân vật (Player) chạm vào không
        if (collision.gameObject.CompareTag("Player"))
        {
            // 2. Kiểm tra hướng va chạm: Chỉ kích hoạt khi cụng đầu từ dưới lên
            // collision.contacts[0].normal.y > 0.5f nghĩa là Player đang ở dưới hòm
            if (collision.contacts[0].normal.y > 0.5f)
            {
                ActivateBox();
            }
        }
    }

    private void ActivateBox()
    {
        _isHit = true;
        Debug.Log("Kích hoạt niềm tin vô hình!");

        // Hiện nền đất
        if (hiddenPlatform != null)
        {
            hiddenPlatform.SetActive(true);
        }

        // Đổi hình ảnh hòm để báo hiệu đã hết "hàng"
        if (emptyBoxSprite != null && _spriteRenderer != null)
        {
            _spriteRenderer.sprite = emptyBoxSprite;
        }

        // Tùy chọn: Thêm âm thanh kích hoạt nếu bạn có AudioManager
        // if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(âm_thanh_của_bạn);
    }
}