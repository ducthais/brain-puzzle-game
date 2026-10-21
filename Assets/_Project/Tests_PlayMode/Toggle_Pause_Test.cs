using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Toggle_Pause_Test
{
    [UnityTest]
    public IEnumerator TC_4_8_01_TogglePause_SetsTimeScaleToZero()
    {
        // Arrange: Khởi tạo LevelManager và đảm bảo game đang chạy bình thường
        GameObject lmObj = new GameObject("LevelManagerTestObj");
        LevelManager lm = lmObj.AddComponent<LevelManager>();
        Time.timeScale = 1.0f;

        // Chờ 1 frame để Unity thiết lập xong vật lý
        yield return null;

        // Act: Kích hoạt nút Tạm dừng
        lm.TogglePause();

        // Chờ thêm 1 frame để áp dụng sự thay đổi
        yield return null;

        // Assert: Xác nhận trục thời gian đã bị ép về 0
        Assert.AreEqual(0f, Time.timeScale, "Lỗi: Lệnh TogglePause chưa ép được Time.timeScale về 0!");

        // Dọn dẹp trả lại thời gian thực và xóa rác
        Time.timeScale = 1.0f;
        Object.Destroy(lmObj);
    }
}