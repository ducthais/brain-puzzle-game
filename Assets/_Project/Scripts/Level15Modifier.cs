using UnityEngine;
using System;

[RequireComponent(typeof(PlayerDeath))]
public class Level15Modifier : MonoBehaviour
{
    private LevelManager _levelManager;
    private Camera _mainCam;
    private bool _isDead = false;

    private void Start()
    {
        _levelManager = FindAnyObjectByType<LevelManager>();
        _mainCam = Camera.main;

        // Tắt PlayerDeath gốc để Modifier nắm trọn quyền sinh sát
        PlayerDeath originalDeathScript = GetComponent<PlayerDeath>();
        if (originalDeathScript != null)
        {
            originalDeathScript.enabled = false;
        }

        Debug.Log("[Level 15] Đã tắt PlayerDeath gốc. Nới lỏng đáy hố vô hạn!");
    }

    private void Update()
    {
        if (_isDead || _mainCam == null) return;

        Vector3 viewPos = _mainCam.WorldToViewportPoint(transform.position);

        // CHỈ KIỂM TRA RƠI KHỎI 2 CẠNH BÊN (X)
        if (viewPos.x < -0.1f || viewPos.x > 1.1f)
        {
            Die("Rơi khỏi 2 cạnh bên");
        }

        // ĐÃ XÓA KIỂM TRA ĐÁY CAMERA (Y): 
        // Giờ nhân vật có lọt ra khỏi camera do rơi quá nhanh cũng không bị chết oan nữa!
    }

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
        Debug.Log($"[Level 15] Chết vì: {reason}");

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