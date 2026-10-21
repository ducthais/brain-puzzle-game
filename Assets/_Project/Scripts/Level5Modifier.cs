using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class Level5Modifier : MonoBehaviour
{
    private PlayerController _player;

    private void Start()
    {
        _player = GetComponent<PlayerController>();
        if (_player != null)
        {
            // _player.maxJumps = 3;
            Debug.Log("[Level 5] Đã kích hoạt Triple Jump!");
        }
    }

    // CẢM BIẾN VẬT LÝ DÀNH RIÊNG CHO MÀN 5 (Bỏ qua Layer mặt đất)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        ReloadJumps(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ReloadJumps(collision.gameObject);
    }

    private void ReloadJumps(GameObject hitObject)
    {
        // Chạm vào bất cứ thứ gì không phải bẫy hay vực thì hồi nhảy
        if (!hitObject.CompareTag("Trap") && !hitObject.CompareTag("DeathZone"))
        {
            if (_player != null)
            {
                // Gọi thẳng vào biến jumpCount đã được Public ở file gốc
                // _player.jumpCount = 0;
            }
        }
    }
}