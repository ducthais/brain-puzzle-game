using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Kiểm thử cơ chế di chuyển / nhảy của PlayerController thật (màn thường: chỉ 1 lần nhảy).
/// </summary>
public class PlayerMechanicTests
{
    private GameObject playerObj;
    private Rigidbody2D rb;
    private PlayerController player;

    [SetUp]
    public void Setup()
    {
        playerObj = new GameObject("TestPlayer");
        rb = playerObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 2.5f;
        rb.mass = 1f;

        // Update() của PlayerController dùng Animator và SpriteRenderer
        playerObj.AddComponent<Animator>();
        playerObj.AddComponent<SpriteRenderer>();
        player = playerObj.AddComponent<PlayerController>();
        player.tripleJumpSceneName = "__khong_phai_scene_test__"; // Đảm bảo chế độ 1 lần nhảy
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(playerObj);
    }

    [UnityTest]
    public IEnumerator Player_Jump_VelocityChangesCorrectly()
    {
        // Đợi 1 frame để Start() chạy
        yield return null;
        rb.linearVelocity = Vector2.zero;

        player.PressJump();

        Assert.AreEqual(player.jumpForce, rb.linearVelocity.y, 0.001f, "Nhảy phải đặt vận tốc Y bằng jumpForce.");

        yield return new WaitForFixedUpdate();
        Assert.IsTrue(rb.linearVelocity.y > 0, "Vận tốc trục Y phải lớn hơn 0 ngay sau khi thực hiện lệnh nhảy.");
    }

    [UnityTest]
    public IEnumerator Player_CannotDoubleJump_InNormalLevel()
    {
        yield return null;
        rb.linearVelocity = Vector2.zero;
        player.PressJump();

        // Để Update() chạy khi đang bay lên (không chạm đất → không reset số lần nhảy)
        yield return null;
        rb.linearVelocity = new Vector2(0f, 3f);

        player.PressJump();

        Assert.AreEqual(3f, rb.linearVelocity.y, 0.001f, "Màn thường không được nhảy lần 2 khi đang ở trên không.");
    }

    [UnityTest]
    public IEnumerator Player_Move_SetsHorizontalVelocity()
    {
        yield return null;

        player.SetMoveDirection(1f);
        yield return new WaitForFixedUpdate();
        Assert.AreEqual(player.speed, rb.linearVelocity.x, 0.001f, "Đi sang phải phải có vận tốc X = speed.");

        player.SetMoveDirection(-1f);
        yield return new WaitForFixedUpdate();
        Assert.AreEqual(-player.speed, rb.linearVelocity.x, 0.001f, "Đi sang trái phải có vận tốc X = -speed.");

        player.SetMoveDirection(0f);
        yield return new WaitForFixedUpdate();
        Assert.AreEqual(0f, rb.linearVelocity.x, 0.001f, "Thả nút phải dừng lại.");
    }
}
