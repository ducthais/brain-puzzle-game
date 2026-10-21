using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class Infinite_Jump_Test
{
    private GameObject playerObj;
    private Rigidbody2D rb;
    private PlayerController playerController;

    // ==========================================
    // SETUP: Khởi tạo Player với đầy đủ Component
    // ==========================================
    [SetUp]
    public void Setup()
    {
        playerObj = new GameObject("Test_Player_Level5");

        // 1. Thêm Vật lý
        rb = playerObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;

        // 2. Thêm các Component giao diện để tránh lỗi Null trong hàm Update()
        playerObj.AddComponent<Animator>();
        playerObj.AddComponent<SpriteRenderer>();

        // 3. Thêm Script điều khiển
        playerController = playerObj.AddComponent<PlayerController>();

        // 4. MẸO QUAN TRỌNG: Đánh lừa code hiểu rằng Scene của Test Runner chính là Scene "level 5"
        // Việc này giúp kích hoạt thành công maxJumps = 3 trong hàm Start()
        playerController.tripleJumpSceneName = SceneManager.GetActiveScene().name;
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(playerObj);
    }

    // =========================================================
    // KỊCH BẢN: KIỂM THỬ GIỚI HẠN 3 CÚ NHẢY LIÊN TIẾP (TRIPLE JUMP)
    // =========================================================
    [UnityTest]
    public IEnumerator TC_Level5_TripleJump_LimitsToThreeJumpsInAir()
    {
        // ARRANGE (Chuẩn bị)
        // Cố tình đưa Player lên cao để mô phỏng trạng thái rơi tự do (isGrounded = false)
        playerObj.transform.position = new Vector3(0f, 10f, 0f);

        // Đợi 1 frame để Unity chạy hàm Start() của PlayerController
        yield return null;

        // ACT & ASSERT (Thực thi và Kiểm chứng theo TCs báo cáo)

        // Nhảy lần 1 (TC_4.2_01 - Kỳ vọng: Nảy lên)
        rb.linearVelocity = Vector2.zero; // Dập tắt quán tính rơi
        playerController.PressJump();
        yield return new WaitForFixedUpdate();
        Assert.IsTrue(rb.linearVelocity.y > 0f, "Lỗi: Nhảy lần 1 thất bại!");

        // Nhảy lần 2 (TC_4.2_01 - Kỳ vọng: Nảy lên tiếp)
        rb.linearVelocity = Vector2.zero;
        playerController.PressJump();
        yield return new WaitForFixedUpdate();
        Assert.IsTrue(rb.linearVelocity.y > 0f, "Lỗi: Nhảy lần 2 thất bại!");

        // Nhảy lần 3 (TC_4.2_01 - Kỳ vọng: Nảy lên nấc cuối cùng)
        rb.linearVelocity = Vector2.zero;
        playerController.PressJump();
        yield return new WaitForFixedUpdate();
        Assert.IsTrue(rb.linearVelocity.y > 0f, "Lỗi: Nhảy lần 3 thất bại!");

        // Nhảy lần 4 (TC_4.2_02 - Kỳ vọng: Bị khóa, rơi xuống)
        rb.linearVelocity = Vector2.zero;
        playerController.PressJump();
        yield return new WaitForFixedUpdate();

        // Tại lần 4, hàm PressJump không kích hoạt nên vận tốc Y bị trọng lực kéo xuống âm hoặc bằng 0
        Assert.IsTrue(rb.linearVelocity.y <= 0f, "Lỗi: Nhân vật vẫn nhảy được ở lần thứ 4 (Sai thiết kế Triple Jump)!");
    }
}