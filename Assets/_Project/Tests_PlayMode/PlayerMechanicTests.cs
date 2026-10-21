using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


public class PlayerMechanicTests
{
    
    public IEnumerator Player_Jump_VelocityChangesCorrectly()
    {
        // Arrange: Khởi tạo một GameObject mô phỏng Player
        GameObject playerObj = new GameObject("TestPlayer");
        var rb = playerObj.AddComponent<Rigidbody2D>();

        // Setup thông số vật lý chuẩn của game
        rb.gravityScale = 2.5f;
        rb.mass = 1f;

        // Đợi 1 frame để Unity khởi tạo xong vật lý
        yield return null;

        // Act: Giả lập lực nhảy tác động lên nhân vật (Jump Force = 12)
        float jumpForce = 12f;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Chờ engine xử lý tính toán vật lý trong FixedUpdate tiếp theo
        yield return new WaitForFixedUpdate();

        // Assert: Kiểm tra xem vận tốc trục Y có thực sự mang giá trị dương (đang bay lên) hay không
        Assert.IsTrue(rb.linearVelocity.y > 0, "Vận tốc trục Y phải lớn hơn 0 ngay sau khi thực hiện lệnh nhảy.");

        // Dọn dẹp Scene sau khi test xong
        Object.Destroy(playerObj);
    }
}