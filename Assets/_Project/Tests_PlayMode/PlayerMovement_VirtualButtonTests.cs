using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerMovement_VirtualButtonTests
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
        playerObj = new GameObject("Test_Player_Movement");

        // 1. Thêm Vật lý
        rb = playerObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;

        // 2. Thêm các Component hình ảnh để tránh lỗi NullReferenceException trong Update()
        playerObj.AddComponent<Animator>();
        playerObj.AddComponent<SpriteRenderer>();

        // 3. Thêm Script điều khiển và thiết lập tốc độ chuẩn
        playerController = playerObj.AddComponent<PlayerController>();
        playerController.speed = 5f; // Đặt vận tốc chuẩn để so sánh Assert
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(playerObj);
    }

    // =========================================================
    // KỊCH BẢN 1: NHẤN NÚT PHẢI (Truyền giá trị +1)
    // =========================================================
    [UnityTest]
    public IEnumerator TC_41_MoveRight_ChangesVelocityXToPositive()
    {
        // ACT: Mô phỏng UI Button "Phải" gọi hàm truyền tham số 1
        playerController.SetMoveDirection(1f);

        // Đợi 1 frame vật lý để Engine cập nhật rb.linearVelocity trong hàm FixedUpdate
        yield return new WaitForFixedUpdate();

        // ASSERT: Kiểm tra vận tốc trục X phải hướng sang dương và bằng đúng tốc độ speed
        Assert.IsTrue(rb.linearVelocity.x > 0f, "Lỗi: Nhân vật không di chuyển sang phải!");
        Assert.AreEqual(5f, rb.linearVelocity.x, "Lỗi: Vận tốc di chuyển sai lệch so với chỉ số speed!");
    }

    // =========================================================
    // KỊCH BẢN 2: NHẤN NÚT TRÁI (Truyền giá trị -1)
    // =========================================================
    [UnityTest]
    public IEnumerator TC_41_MoveLeft_ChangesVelocityXToNegative()
    {
        // ACT: Mô phỏng UI Button "Trái" gọi hàm truyền tham số -1
        playerController.SetMoveDirection(-1f);

        yield return new WaitForFixedUpdate();

        // ASSERT: Kiểm tra vận tốc trục X phải hướng sang âm và bằng -speed
        Assert.IsTrue(rb.linearVelocity.x < 0f, "Lỗi: Nhân vật không di chuyển sang trái!");
        Assert.AreEqual(-5f, rb.linearVelocity.x, "Lỗi: Vận tốc di chuyển sai lệch so với chỉ số speed!");
    }

    // =========================================================
    // KỊCH BẢN 3: NHẢ NÚT (Truyền giá trị 0)
    // =========================================================
    [UnityTest]
    public IEnumerator TC_41_StopMove_ResetsVelocityXToZero()
    {
        // ARRANGE: Cho nhân vật đang chạy tốc độ cao sang phải trước
        playerController.SetMoveDirection(1f);
        yield return new WaitForFixedUpdate();

        // ACT: Mô phỏng sự kiện PointerUp (nhả nút) truyền tham số 0
        playerController.SetMoveDirection(0f);
        yield return new WaitForFixedUpdate();

        // ASSERT: Vận tốc trục X phải lập tức bị dập tắt về 0
        Assert.AreEqual(0f, rb.linearVelocity.x, "Lỗi: Nhân vật bị trượt đi, không dừng lại ngay khi nhả nút!");
    }
}