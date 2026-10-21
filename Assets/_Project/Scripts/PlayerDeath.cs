using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private LevelManager _levelManager;
    private Camera _mainCam;
    private bool _isDead = false;

    private void Start()
    {
        _levelManager = FindAnyObjectByType<LevelManager>();
        _mainCam = Camera.main;
    }

    private void Update()
    {
        if (_isDead) return;
        CheckCameraBounds();
    }

    private void CheckCameraBounds()
    {
        if (_mainCam == null) return;

        Vector3 viewPos = _mainCam.WorldToViewportPoint(transform.position);

        if (viewPos.x < -0.1f || viewPos.x > 1.1f || viewPos.y < -0.2f)
        {
            Die("Rơi khỏi Camera");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isDead) return;

        if (collision.gameObject.CompareTag("Trap") || collision.gameObject.CompareTag("DeathZone"))
        {
            Die("Va chạm chướng ngại vật/Khung tử thần (Collision)");
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (_isDead) return;

        if (collider.gameObject.CompareTag("Trap") || collider.gameObject.CompareTag("DeathZone"))
        {
            Die("Va chạm chướng ngại vật/Khung tử thần (Trigger)");
        }
    }

    private void Die(string reason)
    {
        _isDead = true;
        Debug.Log($"[PlayerDeath] Nhân vật đã chết vì: {reason}");

        // Ghi nhận số lần chết của lượt chơi (dùng để tính sao và phân tích độ khó)
        LevelSession session = LevelSession.Current;
        if (session != null)
        {
            session.RecordDeath();
            GameAnalytics.Reporter.PlayerDeath(session.LevelId, session.Elapsed(Time.realtimeSinceStartup));
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.hurtSound);

        if (AdManager.Instance != null)
        {
            // ĐÃ SỬA: Truyền lệnh nạp lại Scene thành một hành động trì hoãn (Callback)
            AdManager.Instance.OnPlayerDeath(() =>
            {
                _isDead = false; // Reset trạng thái chết khi bắt đầu màn chơi mới
                if (_levelManager != null)
                {
                    _levelManager.ReplayLevel();
                }
            });
        }
        else
        {
            // Dự phòng an toàn nếu không tìm thấy cấu trúc AdManager
            if (_levelManager != null) _levelManager.ReplayLevel();
        }
    }
}