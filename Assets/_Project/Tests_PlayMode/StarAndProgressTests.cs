using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Kiểm thử PlayMode cho luồng tính sao, lưu tiến trình và mở khóa màn của LevelManager.
/// </summary>
public class StarAndProgressTests
{
    private GameObject gameObj;
    private LevelManager levelManager;
    private InMemoryProgressStore inMemoryStore;

    [SetUp]
    public void Setup()
    {
        // 1. Cô lập tiến trình: dùng store trong bộ nhớ, không ảnh hưởng PlayerPrefs thật
        inMemoryStore = new InMemoryProgressStore();
        ProgressService.UseStore(inMemoryStore);

        // 2. Dọn sạch phiên chơi cũ
        LevelSession.Clear();

        // 3. Khởi tạo cụm đối tượng giả lập LevelManager và giao diện
        gameObj = new GameObject("Test_LevelManager_Root");
        levelManager = gameObj.AddComponent<LevelManager>();

        levelManager.winPanel = new GameObject("WinPanel");
        levelManager.winPanel.SetActive(false);

        levelManager.hintCanvas1 = new GameObject("HintCanvas1");
        levelManager.hintCanvas1.SetActive(false);

        levelManager.hintCanvas2 = new GameObject("HintCanvas2");
        levelManager.hintCanvas2.SetActive(false);

        levelManager.nextLevelName = "Level 2";

        Time.timeScale = 1f;
    }

    [TearDown]
    public void Teardown()
    {
        // 1. Thu hồi tài nguyên GameObject đã tạo trong test (hủy panel trước, khi levelManager còn tồn tại)
        if (levelManager != null)
        {
            if (levelManager.winPanel != null) Object.DestroyImmediate(levelManager.winPanel);
            if (levelManager.hintCanvas1 != null) Object.DestroyImmediate(levelManager.hintCanvas1);
            if (levelManager.hintCanvas2 != null) Object.DestroyImmediate(levelManager.hintCanvas2);
        }
        if (gameObj != null) Object.DestroyImmediate(gameObj);

        // 2. Trả lại thời gian thực và khôi phục store mặc định
        Time.timeScale = 1f;
        ProgressService.UseStore(null);
        LevelSession.Clear();
    }

    /// <summary>
    /// Kịch bản: Thắng màn hợp lệ không chết, không xem hint -> Nhận 3 sao và mở khóa Level 2.
    /// (Lưu ý: Trong Test Runner, tên Scene không dạng 'Level N' nên LevelManager mặc định coi là màn 1).
    /// </summary>
    [UnityTest]
    public IEnumerator WinGame_PerfectRun_AwardsThreeStars_AndUnlocksNextLevel()
    {
        // ARRANGE: Bắt đầu một session mới cho màn 1 (0 lần chết, 0 lần xem hint)
        LevelSession.Begin(1, Time.realtimeSinceStartup);
        yield return null;

        // ACT: Người chơi chạm cờ chiến thắng
        levelManager.WinGame();
        yield return null;

        // ASSERT:
        // 1. Số sao đạt được ở lượt chơi này phải là 3
        Assert.AreEqual(3, levelManager.LastStars, "Lỗi: Không chết và không xem hint phải đạt đúng 3 sao!");

        // 2. Mở khóa màn kế tiếp (Level 2)
        Assert.AreEqual(2, ProgressService.MaxUnlockedLevel, "Lỗi: Thắng Level 1 phải mở khóa Level 2!");
        Assert.IsTrue(ProgressService.IsUnlocked(2), "Lỗi: Level 2 phải ở trạng thái đã mở khóa!");
        Assert.IsTrue(ProgressService.IsCompleted(1), "Lỗi: Level 1 phải ở trạng thái đã hoàn thành!");

        // 3. Số sao được lưu lại trong ProgressService
        Assert.AreEqual(3, ProgressService.GetStars(1), "Lỗi: Số sao đã lưu cho Level 1 phải là 3!");

        // 4. Bảng WinPanel phải được bật lên
        Assert.IsTrue(levelManager.winPanel.activeSelf, "Lỗi: WinPanel chưa được kích hoạt sau khi thắng!");
        Assert.AreEqual(0f, Time.timeScale, "Lỗi: Game phải đóng băng thời gian (timeScale = 0) khi hiện WinPanel!");
    }

    /// <summary>
    /// Kịch bản: Thắng màn nhưng đã bị chết ít nhất 1 lần -> Nhận 2 sao.
    /// </summary>
    [UnityTest]
    public IEnumerator WinGame_WithDeath_AwardsTwoStars()
    {
        // ARRANGE: Bắt đầu session màn 1 và ghi nhận 1 lần chết
        LevelSession session = LevelSession.Begin(1, Time.realtimeSinceStartup);
        session.RecordDeath();
        yield return null;

        // ACT: Thắng màn
        levelManager.WinGame();
        yield return null;

        // ASSERT: Bị mất sao không chết -> Nhận 2 sao
        Assert.AreEqual(2, levelManager.LastStars, "Lỗi: Chết 1 lần nhưng không xem hint phải nhận đúng 2 sao!");
        Assert.AreEqual(2, ProgressService.GetStars(1), "Lỗi: Tiến trình lưu sai số sao khi có chết!");
    }

    /// <summary>
    /// Kịch bản: Chơi lại màn cũ với kết quả thấp hơn -> Không bao giờ làm giảm số sao cao nhất đã lưu.
    /// </summary>
    [UnityTest]
    public IEnumerator WinGame_ReplayingWithFewerStars_DoesNotLowerSavedStars()
    {
        // ARRANGE: Người chơi trước đó đã hoàn hảo đạt 3 sao ở Level 1
        ProgressService.CompleteLevel(1, "Level 2", 3);
        Assert.AreEqual(3, ProgressService.GetStars(1));

        // Lần chơi lại này gặp sự cố: vừa chết vừa xem hint (chỉ đáng 1 sao)
        LevelSession session = LevelSession.Begin(1, Time.realtimeSinceStartup);
        session.RecordDeath();
        session.RecordHint();
        yield return null;

        // ACT: Thắng màn ở lần chơi lại
        levelManager.WinGame();
        yield return null;

        // ASSERT:
        // Lượt chơi hiện tại ghi nhận 1 sao
        Assert.AreEqual(1, levelManager.LastStars, "Lỗi: LastStars của lượt chơi hiện tại phải phản ánh đúng 1 sao!");

        // Nhưng kỷ lục lưu trữ trong hệ thống vẫn giữ nguyên 3 sao cao nhất
        Assert.AreEqual(3, ProgressService.GetStars(1), "Lỗi: Tiến trình bị giảm sao khi chơi lại với kết quả thấp hơn!");
        Assert.GreaterOrEqual(ProgressService.MaxUnlockedLevel, 2, "Lỗi: MaxUnlockedLevel bị giảm khi chơi lại màn cũ!");
    }

    /// <summary>
    /// Kịch bản: Người chơi bấm nút 'Nhảy' (bỏ qua màn) trên Hint Canvas 2 -> Thắng nhưng chỉ nhận 1 sao.
    /// </summary>
    [UnityTest]
    public IEnumerator OnClickNhay_Canvas2_SkipsLevelWithOneStar()
    {
        // ARRANGE: Mở sẵn bảng gợi ý Canvas 2
        LevelSession.Begin(1, Time.realtimeSinceStartup);
        levelManager.hintCanvas2.SetActive(true);
        yield return null;

        // ACT: Bấm nút bỏ qua màn
        levelManager.OnClickNhay_Canvas2();
        yield return null;

        // ASSERT:
        // 1. Nút Nhảy phải đóng bảng Hint Canvas 2
        Assert.IsFalse(levelManager.hintCanvas2.activeSelf, "Lỗi: Bảng Hint Canvas 2 chưa đóng khi bấm nút Nhảy!");

        // 2. Nhận đúng 1 sao danh dự (kể cả trước đó chưa chết và chưa xem hint khác)
        Assert.AreEqual(1, levelManager.LastStars, "Lỗi: Bỏ qua màn bằng nút Nhảy bắt buộc chỉ được 1 sao!");
        Assert.AreEqual(1, ProgressService.GetStars(1), "Lỗi: Số sao lưu trữ sau khi bỏ qua màn phải là 1 sao!");

        // 3. Vẫn mở khóa màn kế tiếp
        Assert.AreEqual(2, ProgressService.MaxUnlockedLevel, "Lỗi: Bỏ qua màn vẫn phải mở khóa màn kế tiếp!");
        Assert.IsTrue(levelManager.winPanel.activeSelf, "Lỗi: WinPanel chưa hiện lên sau khi bỏ qua màn!");
    }
}
