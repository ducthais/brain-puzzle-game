using UnityEngine;
using System;

[RequireComponent(typeof(PlayerDeath))]
public class Level19Modifier : MonoBehaviour
{
    private LevelManager _levelManager;
    private Camera _mainCam;
    private bool _isDead = false;

    private void Start()
    {
        _levelManager = FindAnyObjectByType<LevelManager>();
        _mainCam = Camera.main;

        // 1. Ra lệnh cho Camera cho phép chạy theo chiều ngang
        if (_mainCam != null)
        {
            CameraFollow camFollow = _mainCam.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.lockXAxis = false;
            }
        }

        // 2. Tắt PlayerDeath gốc để không bị giết oan khi vừa chạm lề
        PlayerDeath originalDeathScript = GetComponent<PlayerDeath>();
        if (originalDeathScript != null)
        {
            originalDeathScript.enabled = false;
        }

        Debug.Log("[Level 19] Đã mở khóa Camera trục X và tắt luật chết ngoài viền!");
    }

    private void Update()
    {
        if (_isDead || _mainCam == null) return;

        Vector3 viewPos = _mainCam.WorldToViewportPoint(transform.position);

        // Vẫn giữ luật: Nếu rơi xuống vực sâu quá thì chết
        if (viewPos.y < -0.2f)
        {
            Die("Rơi xuống vực");
        }
    }

    // Vẫn check va chạm với gai/bẫy bình thường
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isDead) return;
        if (collision.gameObject.CompareTag("Trap") || collision.gameObject.CompareTag("DeathZone")) Die("Đạp bẫy");
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (_isDead) return;
        if (collider.gameObject.CompareTag("Trap") || collider.gameObject.CompareTag("DeathZone")) Die("Chạm bẫy");
    }

    private void Die(string reason)
    {
        _isDead = true;
        Debug.Log($"[Level 19] Chết vì: {reason}");

        if (AdManager.Instance != null)
        {
            AdManager.Instance.OnPlayerDeath(() =>
            {
                if (_levelManager != null) _levelManager.ReplayLevel();
            });
        }
        else
        {
            if (_levelManager != null) _levelManager.ReplayLevel();
        }
    }
}