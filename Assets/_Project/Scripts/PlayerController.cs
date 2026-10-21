using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Thông số di chuyển")]
    public float speed = 5f;
    public float jumpForce = 12f;

    [Header("Cài đặt Nhảy Đặc Biệt (Level 5)")]
    public string tripleJumpSceneName = "level 5";
  
    private int maxJumps = 1;
    private int jumpCount = 0;
    private bool isGrounded;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private float currentMoveDirection = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == tripleJumpSceneName)
        {
            maxJumps = 3; 
            Debug.Log("Kích hoạt Triple Jump cho riêng màn này!");
        }
        else
        {
            maxJumps = 1; 
        }
    }

    private void Update()
    {
        // Kiểm tra chạm đất bằng vận tốc rơi (Nếu vận tốc Y dao động từ -0.1 đến 0.1 là đang đứng im trên đất)
        isGrounded = (rb.linearVelocity.y > -0.1f && rb.linearVelocity.y < 0.1f);

        // RESET BỘ ĐẾM NHẢY KHI CHẠM ĐẤT
        if (isGrounded)
        {
            jumpCount = 0;
        }

        anim.SetBool("isJumping", !isGrounded); 
        anim.SetFloat("Speed", Mathf.Abs(currentMoveDirection));

        if (currentMoveDirection > 0) spriteRenderer.flipX = false;
        else if (currentMoveDirection < 0) spriteRenderer.flipX = true;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(currentMoveDirection * speed, rb.linearVelocity.y);
    }

    // ==========================================
    // CÁC HÀM CÔNG KHAI DÀNH CHO VIRTUAL BUTTONS
    // ==========================================
    public void SetMoveDirection(float direction)
    {
        currentMoveDirection = direction;
    }

    public void PressJump()
    {
        if (jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.jumpSound);
        }
    }
}