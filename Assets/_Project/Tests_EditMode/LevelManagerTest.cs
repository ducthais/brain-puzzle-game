using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

public class LevelManagerTests
{
    // ==========================================================
    // 1. CODE CŨ CỦA ĐẠI (Test ID Level - Rất an toàn)
    // ==========================================================
    public class LevelData
    {
        public int Id { get; set; }
        public bool IsUnlocked { get; set; }
    }

    [Test]
    public void GetLevel_WhenLevelIdInvalid_ReturnsNullSafely()
    {
        List<LevelData> availableLevels = new List<LevelData>
        {
            new LevelData { Id = 1, IsUnlocked = true },
            new LevelData { Id = 2, IsUnlocked = false }
        };

        int targetLevelId = 99;
        var levelResult = availableLevels.FirstOrDefault(lvl => lvl.Id == targetLevelId);

        Assert.IsNull(levelResult, "Hệ thống phải trả về null an toàn khi ID không tồn tại.");
    }

    // ==========================================================
    // 2. CODE MỚI MỤC 4.5 (Đã sửa hoàn toàn lỗi gán private set)
    // ==========================================================
    private GameObject gameObj;
    private LevelManager levelManager;

    [SetUp]
    public void Setup()
    {
        gameObj = new GameObject("TestRoot");
        levelManager = gameObj.AddComponent<LevelManager>();

        levelManager.hintCanvas1 = new GameObject("HintCanvas1");
        levelManager.hintCanvas2 = new GameObject("HintCanvas2");
        levelManager.hintCanvas1.SetActive(false);
        levelManager.hintCanvas2.SetActive(false);

        if (GameObject.FindObjectOfType<PremiumManager>() == null)
            new GameObject("PremiumManager").AddComponent<PremiumManager>();

        if (GameObject.FindObjectOfType<AdManager>() == null)
            new GameObject("AdManager").AddComponent<AdManager>();
    }

    [TearDown]
    public void Teardown()
    {
        if (gameObj != null) Object.DestroyImmediate(gameObj);

        var pm = GameObject.FindObjectOfType<PremiumManager>();
        if (pm != null) Object.DestroyImmediate(pm.gameObject);

        var am = GameObject.FindObjectOfType<AdManager>();
        if (am != null) Object.DestroyImmediate(am.gameObject);

        Time.timeScale = 1f;

        // Làm sạch bộ nhớ đệm sau khi kết thúc ca test
        PlayerPrefs.DeleteKey("isPremium");
        PlayerPrefs.Save();
    }

    // Test cho TC_BQ_4.5_01: Premium bấm Hint mở thẳng Canvas 2, bỏ qua Ads
    [Test]
    public void Test_ClickHint_PremiumUser_OpensCanvas2Immediately()
    {
        // Thay thế việc gán trực tiếp bằng cách can thiệp vào PlayerPrefs đúng cơ chế của game
        PlayerPrefs.SetInt("isPremium", 1);
        PlayerPrefs.Save();

        levelManager.OnClickHintButton();

        Assert.IsTrue(levelManager.hintCanvas2.activeSelf, "Lỗi: Tài khoản Premium không mở được thẳng bảng gợi ý!");
        Assert.IsFalse(levelManager.hintCanvas1.activeSelf, "Lỗi: Premium lại hiển thị bảng hỏi Ads!");
    }

    // Test cho TC_BQ_4.5_02 & 03: Free bấm Hint bắt buộc phải hiện Canvas 1 hỏi xem Ads
    [Test]
    public void Test_ClickHint_FreeUser_OpensCanvas1First()
    {
        // Gán bằng 0 để cấu hình tài khoản Free thường
        PlayerPrefs.SetInt("isPremium", 0);
        PlayerPrefs.Save();

        levelManager.OnClickHintButton();

        Assert.IsTrue(levelManager.hintCanvas1.activeSelf, "Lỗi: Người chơi Free không hiển thị bảng hỏi xem Ads!");
        Assert.IsFalse(levelManager.hintCanvas2.activeSelf, "Lỗi: Người chơi Free lại xem được trực tiếp gợi ý!");
    }

    // Test cho TC_BQ_4.5_04: Premium bấm nút "Nhảy" qua màn ngay lập tức
    [Test]
    public void Test_ClickNhay_PremiumUser_WinsGameImmediately()
    {
        // Khởi tạo thêm winPanel ảo để kiểm tra chính xác tính năng hiện bảng chiến thắng
        levelManager.winPanel = new GameObject("WinPanel");
        levelManager.winPanel.SetActive(false);

        PlayerPrefs.SetInt("isPremium", 1);
        PlayerPrefs.Save();
        levelManager.hintCanvas2.SetActive(true);

        levelManager.OnClickNhay_Canvas2();

        Assert.IsFalse(levelManager.hintCanvas2.activeSelf, "Lỗi: Bảng Canvas 2 chưa đóng sau khi bấm Nhảy!");
        Assert.IsTrue(levelManager.winPanel.activeSelf, "Lỗi: Hệ thống chưa kích hoạt WinPanel hiển thị cho Premium!");

        // SỬA TẠI ĐÂY: Khi hiện bảng Win, hệ thống game bắt buộc phải đóng băng timeScale = 0f
        Assert.AreEqual(0f, Time.timeScale, "Lỗi: Khi hiện bảng Win thì hệ thống phải đóng băng game (timeScale = 0)!");
    }

    // Test cho TC_BQ_4.5_05: Free bấm nút "Nhảy" phải kích hoạt đóng băng game để chạy phễu Ads
    [Test]
    public void Test_ClickNhay_FreeUser_FreezesTimeScaleForAds()
    {
        PlayerPrefs.SetInt("isPremium", 0);
        PlayerPrefs.Save();
        levelManager.hintCanvas2.SetActive(true);

        levelManager.OnClickNhay_Canvas2();

        Assert.AreEqual(0f, Time.timeScale, "Lỗi: Tài khoản Free bấm Nhảy mà hệ thống không chặn game lại để phát Ads!");
    }
}