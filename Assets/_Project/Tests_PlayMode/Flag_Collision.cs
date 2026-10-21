using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

public class Flag_Collision
{
    private GameObject levelManagerObj;
    private LevelManager levelManager;

    [SetUp]
    public void SetUp()
    {
        // Dọn dẹp môi trường trước khi test
        Time.timeScale = 1f;
        PlayerPrefs.DeleteAll();

        // Khởi tạo LevelManager giả lập để test
        levelManagerObj = new GameObject("LevelManagerTestObj");
        levelManager = levelManagerObj.AddComponent<LevelManager>();
        levelManager.nextLevelName = "Level 2";
    }

    [TearDown]
    public void TearDown()
    {
        // Dọn rác sau khi test xong
        Object.DestroyImmediate(levelManagerObj);
    }

    [Test]
    public void TC_4_7_01_WinGame_UpdatesPlayerPrefs()
    {
        // Arrange: Giả lập lưu mốc Level cao nhất đang là 1
        PlayerPrefs.SetInt("MaxUnlockedLevel", 1);

        // Act: Kích hoạt hàm WinGame như trong code thực tế
        levelManager.WinGame();

        // Assert: Xác nhận mốc lưu đã được đẩy lên ít nhất là Level 2
        Assert.IsTrue(PlayerPrefs.GetInt("MaxUnlockedLevel") >= 2, "Lỗi: Hàm WinGame chưa lưu dữ liệu vào PlayerPrefs!");
    }

    [Test]
    public void TC_4_7_02_FlagCollision_WhenLocked_DoesNotTriggerWinGame()
    {
        // Arrange: Tạo cờ và thiết lập trạng thái KHÓA (isLocked = true)
        GameObject flagObj = new GameObject("EndFlag");
        YellowFlag flag = flagObj.AddComponent<YellowFlag>();
        flag.isLocked = true;

        // Tạo giả lập nhân vật Player để va chạm
        GameObject playerObj = new GameObject("Player");
        playerObj.tag = "Player";
        BoxCollider2D playerCol = playerObj.AddComponent<BoxCollider2D>();

        // Act: Kích hoạt hàm ẩn OnTriggerEnter2D của YellowFlag bằng Reflection
        MethodInfo onTriggerMethod = typeof(YellowFlag).GetMethod("OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance);
        onTriggerMethod.Invoke(flag, new object[] { playerCol });

        // Assert: Nếu cờ khóa, game không được gọi WinGame (tức là Time.timeScale vẫn phải bằng 1f)
        Assert.AreEqual(1f, Time.timeScale, "Lỗi: Cờ đang khóa nhưng game vẫn kích hoạt WinGame() đóng băng thời gian!");

        // Cleanup
        Object.DestroyImmediate(flagObj);
        Object.DestroyImmediate(playerObj);
    }
}